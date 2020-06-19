using Hashgraph;

namespace Proto
{
    public sealed partial class FileID
    {
        internal FileID(Address address) : this()
        {
            ShardNum = address.ShardNum;
            RealmNum = address.RealmNum;
            FileNum = address.AccountNum;
        }
        internal Address ToAddress()
        {
            return new Address(ShardNum, RealmNum, FileNum);
        }
    }
}
