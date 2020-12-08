using Hashgraph.Implementation;

namespace Hashgraph
{
    /// <summary>
    /// Represents a Hedera Network Account Address.
    /// </summary>
    public sealed record Address
    {
        /// <summary>
        /// Network Shard Number for Account
        /// </summary>
        public long ShardNum { get; private init; }
        /// <summary>
        /// Network Realm Number for Account
        /// </summary>
        public long RealmNum { get; private init; }
        /// <summary>
        /// Network Account Number for Account
        /// </summary>
        public long AccountNum { get; private init; }
        /// <summary>
        /// A special designation of an address that can't be created.
        /// It represents the absence of a valid address.  The network will
        /// intrepret as "no account/file/topic/token/contract" when applied 
        /// to change parameters. (typically the value null is intepreted 
        /// as "make no change"). In this way, it is possible to remove a 
        /// auto-renew account from a topic.
        /// </summary>
        public static Address None { get; } = new Address();
        /// <summary>
        /// Internal Constructor representing the "None" version of an
        /// address.  This is a special construct that is used by the network
        /// to represent "removing" auto-renew address from a topic.
        /// </summary>
        private Address()
        {
            ShardNum = 0;
            RealmNum = 0;
            AccountNum = 0;
        }
        /// <summary>
        /// Public Constructor, an <code>Address</code> is immutable after creation.
        /// </summary>
        /// <param name="shardNum">
        /// Network Shard Number
        /// </param>
        /// <param name="realmNum">
        /// Network Realm Number
        /// </param>
        /// <param name="accountNum">
        /// Network Account Number
        /// </param>
        public Address(long shardNum, long realmNum, long accountNum)
        {
            ShardNum = RequireInputParameter.ShardNumber(shardNum);
            RealmNum = RequireInputParameter.RealmNumber(realmNum);
            AccountNum = RequireInputParameter.AcountNumber(accountNum);
        }
    }
}
