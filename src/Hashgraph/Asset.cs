using System;

namespace Hashgraph
{
    /// <summary>
    /// Class representing an asset instance.  An asset instance is an
    /// non fungible token address in addition to an instance serial number.
    /// </summary>
    /// <remarks>
    /// This class consists of both an <see cref="Address"/> representing 
    /// the address of the underlying non fungible token definition and
    /// a serial number representing this instance of the asset token.
    /// This class is immutable once created.
    /// </remarks>
    public sealed record Asset
    {
        /// <summary>
        /// Network Shard Number for Asset (NFT) Type
        /// </summary>
        public long ShardNum { get; private init; }
        /// <summary>
        /// Network Realm Number for Asset (NFT) Type
        /// </summary>
        public long RealmNum { get; private init; }
        /// <summary>
        /// Network Account Number for Asset (NFT) Type
        /// </summary>
        public long AccountNum { get; private init; }
        /// <summary>
        /// Serial number representing the unique instance of the asset.
        /// </summary>
        public long SerialNum { get; private init; }
        /// <summary>
        /// A special designation of an asset that can't be created.
        /// It represents the absence of a valid address and serial number.
        /// </summary>
        public static Asset None { get; } = new Asset();
        /// <summary>
        /// Internal Constructor representing the "None" version of an
        /// asset.
        /// </summary>
        private Asset()
        {
            ShardNum = 0;
            RealmNum = 0;
            AccountNum = 0;
            SerialNum = 0;
        }
        /// <summary>
        /// Public Constructor, an <code>Asset</code> is immutable after creation.
        /// </summary>
        /// <param name="address">
        /// Main Network Node Address
        /// </param>
        /// <param name="serialNum">
        /// Serial number representing the unique instance of the asset.
        /// </param>
        public Asset(Address address, long serialNum) : this(address.ShardNum, address.RealmNum, address.AccountNum, serialNum) { }
        /// <summary>
        /// Public Constructor, an <code>Asset</code> is immutable after creation.
        /// </summary>
        /// <param name="shardNum">
        /// Main Network Node Shard Number
        /// </param>
        /// <param name="realmNum">
        /// Main Network Node Realm Number
        /// </param>
        /// <param name="accountNum">
        /// Main Network Node Account Number
        /// </param>
        /// <param name="serialNum">
        /// Serial number representing the unique instance of the asset.
        /// </param>
        public Asset(long shardNum, long realmNum, long accountNum, long serialNum)
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
            if (serialNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(serialNum), "Account Number cannot be negative.");
            }
            ShardNum = shardNum;
            RealmNum = realmNum;
            AccountNum = accountNum;
            SerialNum = serialNum;
        }
        /// <summary>
        /// Implicit operator for converting an Asset to an Address
        /// </summary>
        /// <param name="gateway">
        /// The Asset object containing the realm, shard and account 
        /// number address information of the associated token definition
        /// to convert into an address object.
        /// </param>
        public static implicit operator Address(Asset asset)
        {
            return new Address(asset.ShardNum, asset.RealmNum, asset.AccountNum);
        }
    }
}