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
        internal Address ToAddress()
        {
            return new Address(ShardNum, RealmNum, AccountNum);
        }
    }
}
