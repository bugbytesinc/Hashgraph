namespace Proto
{
    public sealed partial class AccountAmount
    {
        internal AccountAmount(Hashgraph.Address account, long amount) : this()
        {
            AccountID = new AccountID(account);
            Amount = amount;
        }
    }
}
