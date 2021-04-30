#pragma warning disable CS8653
#pragma warning disable CS8600 
#pragma warning disable CS8601 
#pragma warning disable CS8603
#pragma warning disable IDE1006
using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal Base Implementation of the <see cref="IContext"/> and 
    /// <see cref="IMirrorContext"/> used for configuring
    /// <see cref="Client"/> and <see cref="MirrorClient"/>objects.  Maintains 
    /// a stack of parent objects and coordinates values returned for 
    /// various contexts.  Not intended for public use.
    /// </summary>
    internal abstract class ContextStack<TContext> : IAsyncDisposable where TContext : class
    {
        private readonly ContextStack<TContext>? _parent;
        private readonly Dictionary<string, object?> _map;
        private readonly ConcurrentDictionary<string, Channel> _channels;
        private int _refCount;

        public ContextStack(ContextStack<TContext>? parent)
        {
            _parent = parent;
            _map = new Dictionary<string, object?>();
            if (parent == null)
            {
                // Root Context, holds the channels and is
                // only accessible via other contexts
                // so the ref count starts at 0
                _refCount = 0;
                _channels = new ConcurrentDictionary<string, Channel>();
            }
            else
            {
                // Not the root context, will be held
                // by a client or call context. Ref count
                // starts at 1 for convenience
                _refCount = 1;
                _channels = parent._channels;
                parent.addRef();
            }
        }
        protected abstract bool IsValidPropertyName(string name);
        protected abstract string GetChannelUrl();
        protected abstract Channel ConstructNewChannel(string url);
        public void Reset(string name)
        {
            if (IsValidPropertyName(name))
            {
                _map.Remove(name);
            }
            else
            {
                throw new ArgumentOutOfRangeException($"'{name}' is not a valid property to reset.");
            }
        }
        public Channel GetChannel()
        {
            return _channels.GetOrAdd(GetChannelUrl(), ConstructNewChannel);
        }
        public ValueTask DisposeAsync()
        {
            // Note: there still may be internal stacked references to this
            // object, it does not actually release resources unless it is root.
            // This all comes down to maintaining a map of urls to open grpc
            // channels.  Opening a chanel is an expensive operation.  The
            // map is shared thru the whole entire tree of child contexts.
            return removeRef();
        }
        // Value is forced to be set, but shouldn't be used
        // if method returns false, ignore nullable warnings
        private bool TryGet<T>(string name, out T value)
        {
            for (ContextStack<TContext> ctx = this; ctx != null; ctx = ctx._parent)
            {
                if (ctx._map.TryGetValue(name, out object asObject))
                {
                    if (asObject is T t)
                    {
                        value = t;
                    }
                    else
                    {
                        value = default;
                    }
                    return true;
                }
            }
            value = default;
            return false;
        }
        public IEnumerable<T> GetAll<T>(string name)
        {
            for (ContextStack<TContext>? ctx = this; ctx != null; ctx = ctx._parent)
            {
                if (ctx._map.TryGetValue(name, out object? asObject) && asObject is T t)
                {
                    yield return t;
                }
            }
        }

        // Value should default to value type default (0)
        // if it is not found, or Null for Reference Types
        protected T get<T>(string name)
        {
            if (TryGet(name, out T value))
            {
                return value;
            }
            return default;
        }

        protected void set<T>(string name, T value)
        {
            _map[name] = value;
        }
        private void addRef()
        {
            _parent?.addRef();
            Interlocked.Increment(ref _refCount);
        }
        private async ValueTask removeRef()
        {
            var count = Interlocked.Decrement(ref _refCount);
            if (_parent == null)
            {
                if (count == 0)
                {
                    await Task.WhenAll(_channels.Values.Select(channel => channel.ShutdownAsync()).ToArray()).ConfigureAwait(false);
                    _channels.Clear();
                }
            }
            else
            {
                await _parent.removeRef().ConfigureAwait(false);
            }
        }
    }
}
