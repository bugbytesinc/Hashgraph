using Grpc.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Hashgraph.Implementation
{
    internal class ContextStack : IContext
    {
        private readonly ContextStack? _parent;
        private readonly Dictionary<string, object?> _map;
        private readonly ConcurrentDictionary<string, Channel> _channels;

        public Gateway Gateway { get => get<Gateway>(nameof(Gateway)); set => set(nameof(Gateway), value); }
        public Account Payer { get => get<Account>(nameof(Payer)); set => set(nameof(Payer), value); }
        public long Fee { get => get<long>(nameof(Fee)); set => set(nameof(Fee), value); }
        public TimeSpan TransactionDuration { get => get<TimeSpan>(nameof(TransactionDuration)); set => set(nameof(TransactionDuration), value); }
        public ulong CreateAccountCreateRecordSendThreshold { get => get<ulong>(nameof(CreateAccountCreateRecordSendThreshold)); set => set(nameof(CreateAccountCreateRecordSendThreshold), value); }
        public ulong CreateAcountRequireSignatureReceiveThreshold { get => get<ulong>(nameof(CreateAcountRequireSignatureReceiveThreshold)); set => set(nameof(CreateAcountRequireSignatureReceiveThreshold), value); }
        public bool CreateAccountAlwaysRequireReceiveSignature { get => get<bool>(nameof(CreateAccountAlwaysRequireReceiveSignature)); set => set(nameof(CreateAccountAlwaysRequireReceiveSignature), value); }
        public TimeSpan CreateAccountAutoRenewPeriod { get => get<TimeSpan>(nameof(CreateAccountAutoRenewPeriod)); set => set(nameof(CreateAccountAutoRenewPeriod), value); }
        public int RetryCount { get => get<int>(nameof(RetryCount)); set => set(nameof(RetryCount), value); }
        public TimeSpan RetryDelay { get => get<TimeSpan>(nameof(RetryDelay)); set => set(nameof(RetryDelay), value); }
        public string Memo { get => get<string>(nameof(Memo)); set => set(nameof(Memo), value); }
        public bool GenerateRecord { get => get<bool>(nameof(GenerateRecord)); set => set(nameof(GenerateRecord), value); }
        public Transaction Transaction { get => get<Transaction>(nameof(Transaction)); set => set(nameof(Transaction), value); }
        public Action<Transaction> OnTransactionCreated { get => get<Action<Transaction>>(nameof(OnTransactionCreated)); set => set(nameof(OnTransactionCreated), value); }

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
                case nameof(Fee):
                case nameof(RetryCount):
                case nameof(RetryDelay):
                case nameof(TransactionDuration):
                case nameof(CreateAccountCreateRecordSendThreshold):
                case nameof(CreateAcountRequireSignatureReceiveThreshold):
                case nameof(CreateAccountAlwaysRequireReceiveSignature):
                case nameof(CreateAccountAutoRenewPeriod):
                case nameof(Memo):
                case nameof(GenerateRecord):
                case nameof(Transaction):
                case nameof(OnTransactionCreated):
                    _map.Remove(name);
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"'{name}' is not a valid property to reset.");
            }
        }
        public Channel GetChannel()
        {
            return _channels.GetOrAdd(Gateway.Url, url => new Channel(Gateway.Url, ChannelCredentials.Insecure));
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
