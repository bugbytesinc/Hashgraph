using System;

namespace Hashgraph
{
    public interface IContext
    {
        Gateway Gateway { get; set; }
        Account Payer { get; set; }
        long Fee { get; set; }
        TimeSpan TransactionDuration { get; set; }
        string Memo { get; set; }
        int BusyRetryCount { get; set; }
        TimeSpan BusyRetryDelay { get; set; }
        Transaction Transaction { get; set; }
        Action<Transaction> OnTransactionCreated { get; set; }
        void Reset(string name);
    }
}
