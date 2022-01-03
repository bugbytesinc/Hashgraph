using System;

namespace Hashgraph
{
    /// <summary>
    /// Represents a Hedera Network Account Address
    /// or Alias record pointer.
    /// </summary>
    /// <remarks>
    /// When identifying a crypto account in the
    /// Hedera System, the nework will accept a
    /// <code>shard.realm.num</code> id for the 
    /// crypto account or an <code>shard.realm.alias</code>
    /// form where the alias is the bytes representing
    /// a public key.
    /// </remarks>
    public sealed record AddressOrAlias
    {
        /// <summary>
        /// Network Shard Number for the Crypto Account
        /// </summary>
        public long ShardNum { get; private init; }
        /// <summary>
        /// Network Realm Number for the Crypto Account
        /// </summary>
        public long RealmNum { get; private init; }
        /// <summary>
        /// Network Account Number for the Crypto Account
        /// if this record pointer was created in the
        /// <code>shard.realm.num</code> format.  If
        /// created with a key instead (the Endorsment
        /// is not null) this value will be zero.
        /// </summary>
        public long AccountNum { get; private init; }
        /// <summary>
        /// The network alias value for the Crypto Account
        /// if the record pointer was created with a key.
        /// If it was created as a <code>shard.realm.num</code>
        /// then this property will be <code>null</code>.
        /// </summary>
        public Endorsement? Endorsement { get; private init; }
        /// <summary>
        /// A special designation of an identifier that can't be created.
        /// It represents the absence of a valid address or alias.  The 
        /// network will intrepret as "no account" when applied 
        /// to change parameters. (typically the value null is intepreted 
        /// as "make no change"). In this way, it is possible to remove a 
        /// auto-renew account from a topic.
        /// </summary>
        public static AddressOrAlias None { get; } = new AddressOrAlias();
        /// <summary>
        /// Constructs an account identifier from the <code>shard.realm.num</code> 
        /// format (an <code>Address</code>).
        /// </summary>
        /// <param name="address">
        /// Address of the crypto account.
        /// </param>
        public AddressOrAlias(Address address)
        {
            ShardNum = address.ShardNum;
            RealmNum = address.RealmNum;
            AccountNum = address.AccountNum;
            Endorsement = null;
        }
        /// <summary>
        /// Constructs an account identifer from a Gateway identifier
        /// (which is the paring of a url and an account).
        /// </summary>
        /// <param name="gateway">
        /// The gateway identifier.
        /// </param>
        public AddressOrAlias(Gateway gateway)
        {
            ShardNum = gateway.ShardNum;
            RealmNum = gateway.RealmNum;
            AccountNum = gateway.AccountNum;
            Endorsement = null;
        }
        /// <summary>
        /// Constructs an account identifier from the <code>shard.realm.alias</code>
        /// format (an <code>Alias</code>).
        /// </summary>
        /// <param name="alias">
        /// The alias representation of a crypto account.
        /// </param>
        public AddressOrAlias(Alias alias)
        {
            ShardNum = alias.ShardNum;
            RealmNum = alias.RealmNum;
            AccountNum = 0;
            Endorsement = alias.Endorsement;
        }
        /// <summary>
        /// Internal Constructor representing the "None" version of an
        /// address.  This is a special construct that is used by the network
        /// to represent "removing" auto-renew address from a topic.
        /// </summary>
        private AddressOrAlias()
        {
            ShardNum = 0;
            RealmNum = 0;
            AccountNum = 0;
            Endorsement = null;
        }
        /// <summary>
        /// Outputs a string representation of the address or
        /// alias in the form of <code>shard.realm.num</code> or
        /// <code>shard.realm.alias</code>.
        /// </summary>
        /// <returns>
        /// String representation of this account identifier in its
        /// address or alias format.
        /// </returns>
        public override string ToString()
        {
            if (this == None)
            {
                return "None";
            }
            else if (Endorsement is null)
            {
                return $"{ShardNum}.{RealmNum}.{AccountNum}";
            }
            return $"{ShardNum}.{RealmNum}.{Endorsement}";

        }
        /// <summary>
        /// Implicitly converts an address to an <code>AddressOrAlias</code>
        /// from an <code>Address</code> instance.
        /// </summary>
        /// <param name="address">
        /// Address of the crypto account
        /// </param>
        public static implicit operator AddressOrAlias(Address address)
        {
            return new AddressOrAlias(address);
        }
        /// <summary>
        /// Implictly converts a gateway to an <code>AddressOrAlias</code>
        /// from a <code>Gateway</code> object instance.
        /// </summary>
        /// <param name="gateway">
        /// Gateway object instance (which is an Address in of itself).
        /// </param>
        public static implicit operator AddressOrAlias(Gateway gateway)
        {
            return new AddressOrAlias(gateway);
        }
        /// <summary>
        /// Implicitly converts an alias to an <code>AddressOrAlias</code>
        /// from an <code>Alias</code> instance.
        /// </summary>
        /// <param name="alias">
        /// Alias identifying the crypto account.
        /// </param>
        public static implicit operator AddressOrAlias(Alias alias)
        {
            return new AddressOrAlias(alias);
        }
        /// <summary>
        /// Implictly converts a public key value to an <code>AddressOrAlias</code>
        /// from a byte sequence.  The bytes must represent an Ed25519 public key
        /// value.  The shard and realm values will be set to their defaults of zero.
        /// </summary>
        /// <param name="publicKey">
        /// Bytes representing an Ed25519 public key.
        /// </param>
        public static implicit operator AddressOrAlias(ReadOnlyMemory<byte> publicKey)
        {
            return new Alias(publicKey);
        }
        /// <summary>
        /// Implicitly converts an <code>Endorsement</code> to an
        /// <code>AddressOrAlias</code>.  The <code>Endorsement</code> must
        /// be a simple key, complex key formats (such as lists) are not
        /// supported as aliases.
        /// </summary>
        /// <param name="endorsement">
        /// The endorsment representing a simple public key value.
        /// </param>
        public static implicit operator AddressOrAlias(Endorsement endorsement)
        {
            return new Alias(endorsement);
        }
    }
    internal static class AddressOrAliasExtensions
    {
        internal static bool IsNullOrNone(this AddressOrAlias? addressOrAlias)
        {
            return addressOrAlias is null || addressOrAlias == AddressOrAlias.None;
        }
    }
}
