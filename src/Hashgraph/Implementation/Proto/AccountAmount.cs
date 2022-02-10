using Hashgraph;

namespace Proto;

public sealed partial class AccountAmount
{
    internal AccountAmount(Address psudoAddress, long amount) : this()
    {
        AccountID = new AccountID(psudoAddress);
        Amount = amount;
    }
}