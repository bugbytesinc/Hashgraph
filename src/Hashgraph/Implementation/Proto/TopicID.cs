using Hashgraph;
using System;

namespace Proto;

public sealed partial class TopicID
{
    internal TopicID(Address topic) : this()
    {
        if (topic is null)
        {
            throw new ArgumentNullException(nameof(topic), "Topic Address is missing. Please check that it is not null.");
        }
        else if (topic.AddressType == AddressType.ShardRealmNum)
        {
            ShardNum = topic.ShardNum;
            RealmNum = topic.RealmNum;
            TopicNum = topic.AccountNum;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(topic), "Topic Address does not appear to be in a valid <shard>.<realm>.<num> form.");
        }
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