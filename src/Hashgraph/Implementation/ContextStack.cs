using Google.Protobuf;
using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal Implementation of the <see cref="IContext"/> used for configuring
    /// <see cref="Client"/> objects.  Maintains a stack of parent objects 
    /// and coordinates values returned for various contexts.  Not intended for
    /// public use.
    /// </summary>
    internal class ContextStack : IContext
    {
        private readonly ContextStack? _parent;
        private readonly Dictionary<string, object?> _map;
        private readonly ConcurrentDictionary<string, Channel> _channels;

        public Gateway? Gateway { get => get<Gateway>(nameof(Gateway)); set => set(nameof(Gateway), value); }
        public Account? Payer { get => get<Account>(nameof(Payer)); set => set(nameof(Payer), value); }
        public Signatory? Signatory { get => get<Signatory>(nameof(Signatory)); set => set(nameof(Signatory), value); }
        public long FeeLimit { get => get<long>(nameof(FeeLimit)); set => set(nameof(FeeLimit), value); }
        public TimeSpan TransactionDuration { get => get<TimeSpan>(nameof(TransactionDuration)); set => set(nameof(TransactionDuration), value); }
        public int RetryCount { get => get<int>(nameof(RetryCount)); set => set(nameof(RetryCount), value); }
        public TimeSpan RetryDelay { get => get<TimeSpan>(nameof(RetryDelay)); set => set(nameof(RetryDelay), value); }
        public string? Memo { get => get<string>(nameof(Memo)); set => set(nameof(Memo), value); }
        public bool AdjustForLocalClockDrift { get => get<bool>(nameof(AdjustForLocalClockDrift)); set => set(nameof(AdjustForLocalClockDrift), value); }
        public TxId? Transaction { get => get<TxId>(nameof(Transaction)); set => set(nameof(Transaction), value); }
        public Action<IMessage>? OnSendingRequest { get => get<Action<IMessage>>(nameof(OnSendingRequest)); set => set(nameof(OnSendingRequest), value); }
        public Action<int, IMessage>? OnResponseReceived { get => get<Action<int, IMessage>>(nameof(OnResponseReceived)); set => set(nameof(OnResponseReceived), value); }
        

        public ContextStack(ContextStack? parent)
        {
            _parent = parent;
            _map = new Dictionary<string, object?>();
            _channels = parent?._channels ?? new ConcurrentDictionary<string, Channel>();
        }
        public void Reset(string name)
        {
            switch (name)
            {
                case nameof(Gateway):
                case nameof(Payer):
                case nameof(Signatory):
                case nameof(FeeLimit):
                case nameof(RetryCount):
                case nameof(RetryDelay):
                case nameof(TransactionDuration):
                case nameof(Memo):
                case nameof(AdjustForLocalClockDrift):
                case nameof(Transaction):
                case nameof(OnSendingRequest):
                case nameof(OnResponseReceived):
                    _map.Remove(name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"'{name}' is not a valid property to reset.");
            }
        }
        public Channel GetChannel()
        {
            var url = Gateway?.Url;
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException("The Network Gateway Node has not been configured.");
            }
            return _channels.GetOrAdd(url, url => new Channel(url, ChannelCredentials.Insecure));
        }


        // Value is forced to be set, but shouldn't be used
        // if method returns false, ignore nullable warnings
#nullable disable
        public bool TryGet<T>(string name, out T value)
        {
            for (ContextStack ctx = this; ctx != null; ctx = ctx._parent)
            {
                if (ctx._map.TryGetValue(name, out object asObject))
                {
                    if (asObject is T)
                    {
                        value = (T)asObject;
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
#nullable restore

        public IEnumerable<T> GetAll<T>(string name)
        {
            for (ContextStack? ctx = this; ctx != null; ctx = ctx._parent)
            {
                if (ctx._map.TryGetValue(name, out object? asObject) && asObject is T)
                {
                    yield return (T)asObject;
                }
            }
        }

        // Value should default to value type default (0)
        // if it is not found, or Null for Reference Types
#nullable disable
        private T get<T>(string name)
        {
            if (TryGet(name, out T value))
            {
                return value;
            }
            return default;
        }
#nullable restore

        private void set<T>(string name, T value)
        {
            _map[name] = value;
        }
    }
}
