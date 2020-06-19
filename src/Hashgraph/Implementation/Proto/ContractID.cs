using Hashgraph;

namespace Proto
{
    public sealed partial class ContractID
    {
        internal ContractID(Address address) : this()
        {
            ShardNum = address.ShardNum;
            RealmNum = address.RealmNum;
            ContractNum = address.AccountNum;
        }
        internal Address ToAddress()
        {
            return new Address(ShardNum, RealmNum, ContractNum);
        }
    }
}
