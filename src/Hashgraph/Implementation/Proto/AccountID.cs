using Hashgraph;

namespace Proto
{
    public sealed partial class AccountID
    {
        internal AccountID(Address address) : this()
        {
            ShardNum = address.ShardNum;
            RealmNum = address.RealmNum;
            AccountNum = address.AccountNum;
        }
    }

    internal static class AccountIDExtensions
    {
        internal static Address AsAddress(this AccountID? accountId)
        {
            if(accountId is not null)
            {
                return new Address(accountId.ShardNum, accountId.RealmNum, accountId.AccountNum);
            }
            return Address.None;
        }
    }
}
