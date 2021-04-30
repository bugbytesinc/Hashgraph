using System;

namespace Hashgraph
{
    /// <summary>
    /// Class representing the Network Address and Node 
    /// Account address for gaining access to the Hedera Network.
    /// </summary>
    /// <remarks>
    /// This class consists of both an <see cref="Address"/> representing 
    /// the main node within the network and the host name and port of the 
    /// network address where the public network endpoint is located.
    /// This class is immutable once created.
    /// </remarks>
    public sealed record Gateway
    {
        /// <summary>
        /// The URL and port of the public Hedera Network access point.
        /// </summary>
        public string Url { get; private init; }
        /// <summary>
        /// Network Shard Number for Gateway Account
        /// </summary>
        public long ShardNum { get; private init; }
        /// <summary>
        /// Network Realm Number for Gateway Account
        /// </summary>
        public long RealmNum { get; private init; }
        /// <summary>
        /// Network Account Number for Gateway Account
        /// </summary>
        public long AccountNum { get; private init; }
        /// <summary>
        /// Public Constructor, a <code>Gateway</code> is immutable after creation.
        /// </summary>
        /// <param name="url">
        /// The URL and port of the public Hedera Network access point.
        /// </param>
        /// <param name="address">
        /// Main Network Node Address
        /// </param>
        public Gateway(string url, Address address) : this(url, address.ShardNum, address.RealmNum, address.AccountNum) { }
        /// <summary>
        /// Public Constructor, a <code>Gateway</code> is immutable after creation.
        /// </summary>
        /// <param name="url">
        /// The URL and port of the public Hedera Network access point.
        /// </param>
        /// <param name="shardNum">
        /// Main Network Node Shard Number
        /// </param>
        /// <param name="realmNum">
        /// Main Network Node Realm Number
        /// </param>
        /// <param name="accountNum">
        /// Main Network Node Account Number
        /// </param>
        public Gateway(string url, long shardNum, long realmNum, long accountNum)
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
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentOutOfRangeException(nameof(url), "URL is required.");
            }
            Url = url;
            ShardNum = shardNum;
            RealmNum = realmNum;
            AccountNum = accountNum;
        }
        /// <summary>
        /// Implicit operator for converting a Gateway to an Address
        /// </summary>
        /// <param name="gateway">
        /// The Gateway object containing the realm, shard and account 
        /// number address information to convert into an address object.
        /// </param>
        public static implicit operator Address(Gateway gateway)
        {
            return new Address(gateway.ShardNum, gateway.RealmNum, gateway.AccountNum);
        }
    }
}