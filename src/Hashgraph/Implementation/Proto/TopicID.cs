using Hashgraph;

namespace Proto
{
    public sealed partial class TopicID
    {
        internal TopicID(Address address) : this()
        {
            ShardNum = address.ShardNum;
            RealmNum = address.RealmNum;
            TopicNum = address.AccountNum;
        }
    }

    internal static class TopicIDExtensions
    {
        internal static Address AsAddress(this TopicID? id)
        {
            if (id is not null)
            {
                return new Address(id.ShardNum, id.RealmNum, id.TopicNum);
            }
            return Address.None;
        }
    }
}
