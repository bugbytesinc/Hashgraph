using Google.Protobuf;
using System;

namespace Hashgraph
{
    public interface IContext
    {
        Gateway? Gateway { get; set; }
        Account? Payer { get; set; }
        long FeeLimit { get; set; }
        TimeSpan TransactionDuration { get; set; }
        ulong CreateAccountCreateRecordSendThreshold { get; set; }
        ulong CreateAcountRequireSignatureReceiveThreshold { get; set; }
        bool CreateAccountAlwaysRequireReceiveSignature { get; set; }
        TimeSpan CreateAccountAutoRenewPeriod { get; set; }
        string? Memo { get; set; }
        bool GenerateRecord { get; set; }
        int RetryCount { get; set; }
        TimeSpan RetryDelay { get; set; }
        TxId? Transaction { get; set; }
        Action<TxId>? OnTransactionCreated { get; set; }
        Action<IMessage>? OnSendingRequest { get; set; }
        Action<int,IMessage>? OnResponseReceived { get; set; }
        void Reset(string name);
    }
}
