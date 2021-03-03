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
    }

    internal static class FileIDExtensions
    {
        internal static Address AsAddress(this FileID? id)
        {
            if (id is not null)
            {
                return new Address(id.ShardNum, id.RealmNum, id.FileNum);
            }
            return Address.None;
        }
    }
}
