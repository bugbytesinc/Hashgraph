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
        internal Address ToAddress()
        {
            return new Address(ShardNum, RealmNum, TopicNum);
        }
    }
}
