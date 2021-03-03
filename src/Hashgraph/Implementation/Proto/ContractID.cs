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
    }

    internal static class ContractIDExtensions
    {
        internal static Address AsAddress(this ContractID? id)
        {
            if (id is not null)
            {
                return new Address(id.ShardNum, id.RealmNum, id.ContractNum);
            }
            return Address.None;
        }
    }
}
