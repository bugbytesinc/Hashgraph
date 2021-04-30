using System;

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
            if (shardNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(shardNum), "Shard Number cannot be negative.");
            }
            if (realmNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(realmNum), "Realm Number cannot be negative.");
            }
            if (accountNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(accountNum), "Account Number cannot be negative.");
            }
            ShardNum = shardNum;
            RealmNum = realmNum;
            AccountNum = accountNum;
        }
    }
    internal static class AddressExtensions
    {
        internal static bool IsNullOrNone(this Address? address)
        {
            return address is null || address == Address.None;
        }
    }
}
