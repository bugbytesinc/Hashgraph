using System;

namespace Hashgraph
{
    public interface IContext
    {
        Gateway Gateway { get; set; }
        Account Payer { get; set; }
        long Fee { get; set; }
        TimeSpan TransactionDuration { get; set; }
        ulong CreateAccountCreateRecordSendThreshold { get; set; }
        ulong CreateAcountRequireSignatureReceiveThreshold { get; set; }
        bool CreateAccountAlwaysRequireReceiveSignature { get; set; }
        TimeSpan CreateAccountAutoRenewPeriod { get; set; }
        string Memo { get; set; }
        bool GenerateRecord { get; set; }
        int RetryCount { get; set; }
        TimeSpan RetryDelay { get; set; }
        Transaction Transaction { get; set; }
        Action<Transaction> OnTransactionCreated { get; set; }
        void Reset(string name);
    }
}
