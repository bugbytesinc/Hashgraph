using System;
using System.Collections.Generic;

namespace Hashgraph.Implementation
{
    internal class ContextStack : IContext
    {
        private readonly ContextStack? _parent;
        private readonly Dictionary<string, object?> _map;

        public Gateway Gateway { get => get<Gateway>(nameof(Gateway)); set => set(nameof(Gateway), value); }
        public Account Payer { get => get<Account>(nameof(Payer)); set => set(nameof(Payer), value); }
        public long Fee { get => get<long>(nameof(Fee)); set => set(nameof(Fee), value); }
        public TimeSpan TransactionDuration { get => get<TimeSpan>(nameof(TransactionDuration)); set => set(nameof(TransactionDuration), value); }
        public int BusyRetryCount { get => get<int>(nameof(BusyRetryCount)); set => set(nameof(BusyRetryCount), value); }
        public TimeSpan BusyRetryDelay { get => get<TimeSpan>(nameof(BusyRetryDelay)); set => set(nameof(BusyRetryDelay), value); }
        public string Memo { get => get<string>(nameof(Memo)); set => set(nameof(Memo), value); }
        public Transaction Transaction { get => get<Transaction>(nameof(Transaction)); set => set(nameof(Transaction), value); }
        public Action<Transaction> OnTransactionCreated { get => get<Action<Transaction>>(nameof(OnTransactionCreated)); set => set(nameof(OnTransactionCreated), value); }

        public ContextStack(ContextStack? parent)
        {
            _parent = parent;
            _map = new Dictionary<string, object?>();
        }
        public void Reset(string name)
        {
            switch (name)
            {
                case nameof(Gateway):
                case nameof(Payer):
                case nameof(Fee):
                case nameof(BusyRetryCount):
                case nameof(BusyRetryDelay):
                case nameof(TransactionDuration):
                case nameof(Memo):
                case nameof(Transaction):
                case nameof(OnTransactionCreated):
                    _map.Remove(name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"'{name}' is not a valid property to reset.");
            }
        }

        // Value is forced to be set, but shouldn't be used
        // if method returns false, ignore nullable warnings
#nullable disable
        public bool TryGet<T>(string name, out T value)
        {
            for (ContextStack ctx = this; ctx != null; ctx = ctx._parent)
            {
                if (ctx._map.TryGetValue(name, out object asObject) && asObject is T)
                {
                    value = (T)asObject;
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
