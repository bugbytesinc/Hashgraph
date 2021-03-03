namespace Proto
{
    public sealed partial class ScheduleInfo
    {
        internal Hashgraph.PendingTransactionInfo ToPendingTransactionInfo()
        {
            return new Hashgraph.PendingTransactionInfo
            {
                Pending = ScheduleID.ToAddress(),
                Creator = CreatorAccountID.AsAddress(),
                Payer = PayerAccountID.AsAddress(),
                TransactionBody = TransactionBody.ToByteArray(),
                Endorsements = Signatories.ToEndorsements(),
                Administrator = AdminKey?.ToEndorsement(),
                Memo = Memo,
                Expiration = ExpirationTime.ToDateTime()
            };
        }
    }
}
