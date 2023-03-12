using Hashgraph.Implementation;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Hashgraph;

/// <summary>
/// Represents 20-byte EVM address Hedera Virtual Machine Address.
/// </summary>
/// <remarks>
/// Every contract has an VM address determined by its<code>shard.realm.num</code> id.
/// 
/// This address is as follows:
/// 
///     The first 4 bytes are the big-endian representation of the shard.
///     The next 8 bytes are the big-endian representation of the realm.
///     The final 8 bytes are the big-endian representation of the number.
/// 
/// Contracts created via CREATE2 have an <b>additional, primary address</b> that is 
/// derived from the<a href="https://eips.ethereum.org/EIPS/eip-1014"> EIP-1014</a> 
/// specification, and does not have a simple relation to a<tt> shard.realm.num</tt> id. 
/// 
/// (Please do note that CREATE2 contracts can also be referenced by the three-part
/// EVM address described above.)
/// </remarks>
public sealed record Moniker
{
    /// <summary>
    /// Network Shard Number for Account, should match the
    /// encoding of the <code>Bytes</code> parameter if it
    /// represents a shard.realm.num format (and not a 
    /// EIP-1014 generated value).
    /// </summary>
    public long ShardNum { get; private init; }
    /// <summary>
    /// Network Realm Number for Account, should match the
    /// encoding of the <code>Bytes</code> parameter if it
    /// represents a shard.realm.num format (and not a 
    /// EIP-1014 generated value).
    /// </summary>
    public long RealmNum { get; private init; }
    /// <summary>
    /// The 20-byte EVM address of the contract.
    /// 
    /// Every contract has an VM address determined by its<code>shard.realm.num</code> id.
    /// 
    /// This address is as follows:
    /// 
    ///     The first 4 bytes are the big-endian representation of the shard.
    ///     The next 8 bytes are the big-endian representation of the realm.
    ///     The final 8 bytes are the big-endian representation of the number.
    ///     
    /// In the above format, the shard and realm should match the encoded values.
    /// 
    /// Contracts created via CREATE2 have an <b>additional, primary address</b> that is 
    /// derived from the<a href="https://eips.ethereum.org/EIPS/eip-1014"> EIP-1014</a> 
    /// specification, and does not have a simple relation to a<tt> shard.realm.num</tt> id.
    /// (therefore shard and realm values do not match the encoded bytes)
    /// </summary>
    public ReadOnlyMemory<byte> Bytes { get; private init; }
    /// <summary>
    /// A special designation of an alias that can't be created.
    /// It represents the absence of a valid alias.  The network will
    /// intrepret as "no account/file/topic/token/contract" when applied 
    /// to change parameters. (typically the value null is intepreted 
    /// as "make no change"). In this way, it is possible to remove a 
    /// auto-renew account from a topic.
    /// </summary>
    public static Moniker None { get; } = new Moniker();
    /// <summary>
    /// Internal Constructor representing the "None" version of an
    /// alias.  This is a special construct that is used by the network
    /// to represent "removing" auto-renew address from a topic.
    /// </summary>
    private Moniker()
    {
        ShardNum = 0;
        RealmNum = 0;
        Bytes = ReadOnlyMemory<byte>.Empty;
    }
    /// <summary>
    /// Constructor from ECDSASecp256K1 Endorsement, converts the public
    /// key into the appropriate 20-byte public key hash, with shard and
    /// realm of zero.
    /// </summary>
    /// <param name="endorsement">
    /// An ECDSASecp256K1 public key.  The moniker will automatically 
    /// convert the public key into the matching 20-byte eth hash.
    /// </param>
    public Moniker(Endorsement endorsement) : this(0, 0, evmAddressFromEndorsement(endorsement))
    {
    }
    /// <summary>
    /// Constructor from ECDSASecp256K1 Endorsement, converts the public
    /// key into the appropriate 20-byte public key hash, with specified
    /// shard and realm.
    /// </summary>
    /// <param name="shardNum">
    /// Shard Number
    /// </param>
    /// <param name="realmNum">
    /// Realm Number
    /// </param>
    /// <param name="endorsement">
    /// An ECDSASecp256K1 public key.  The moniker will automatically 
    /// convert the public key into the matching 20-byte eth hash.
    /// </param>
    public Moniker(long shardNum, long realmNum, Endorsement endorsement) : this(shardNum, realmNum, evmAddressFromEndorsement(endorsement))
    {
    }
    /// <summary>
    /// Public Constructor, takes an array of bytes  as the 
    /// constructor and sets the Shard and Realm to values of zero.
    /// </summary>
    /// <param name="bytes">
    /// The bytes representing the moniker address, if originates from
    /// shard.realm.num form the encoding is follows: 
    /// 
    ///     The first 4 bytes are the big-endian representation of the shard.
    ///     The next 8 bytes are the big-endian representation of the realm.
    ///     The final 8 bytes are the big-endian representation of the number.
    /// </param>
    public Moniker(ReadOnlyMemory<byte> bytes) : this(0, 0, bytes)
    {
    }
    /// <summary>
    /// Public Constructor, a <code>Moniker</code> is imutable after
    /// construction.
    /// </summary>
    /// <param name="shardNum">
    /// Network Shard Number
    /// </param>
    /// <param name="realmNum">
    /// Network Realm Number
    /// </param>
    /// <param name="bytes">
    /// The bytes representing the moniker address, if originates from
    /// shard.realm.num form the encoding is follows: 
    /// 
    ///     The first 4 bytes are the big-endian representation of the shard.
    ///     The next 8 bytes are the big-endian representation of the realm.
    ///     The final 8 bytes are the big-endian representation of the number.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// when any of the inputs are out of range, such as the bytes not having
    /// a length of 20.
    /// </exception>
    public Moniker(long shardNum, long realmNum, ReadOnlyMemory<byte> bytes)
    {
        if (shardNum < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(shardNum), "Shard Number cannot be negative.");
        }
        if (realmNum < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(realmNum), "Realm Number cannot be negative.");
        }
        if (bytes.Length != 20)
        {
            throw new ArgumentOutOfRangeException(nameof(bytes), "The encoded bytes must have a length of 20.");
        }
        ShardNum = shardNum;
        RealmNum = realmNum;
        Bytes = bytes;
    }
    /// <summary>
    /// Equality implementation
    /// </summary>
    /// <param name="other">
    /// The other <code>Moniker</code> object to compare.
    /// </param>
    /// <returns>
    /// True if asset, owner, created and metadata are the same.
    /// </returns>
    public bool Equals(Moniker? other)
    {
        return other is not null &&
            ShardNum == other.ShardNum &&
            RealmNum == other.RealmNum &&
            Bytes.Span.SequenceEqual(other.Bytes.Span);
    }
    /// <summary>
    /// Equality implementation.
    /// </summary>
    /// <returns>
    /// A unique hash of the contents of this <code>Moniker</code> 
    /// object.  Only consistent within the current instance of 
    /// the application process.
    /// </returns>
    public override int GetHashCode()
    {
        return $"Moniker.{ShardNum}.{RealmNum}.{Hex.FromBytes(Bytes)}".GetHashCode();
    }
    /// <summary>
    /// Outputs a string representation of the moniker 
    /// in the form <code>shard.realm.moniker</code>.
    /// </summary>
    /// <returns>
    /// String representation of this account identifier in its
    /// moniker format.
    /// </returns>
    public override string ToString()
    {
        if (this == None)
        {
            return "None";
        }
        return $"{ShardNum}.{RealmNum}.{Hex.FromBytes(Bytes)}";
    }
    /// <summary>
    /// Implicit operator for converting a byte array into a encoded address. 
    /// It sets the shard and realm values to zero.
    /// </summary>
    /// <param name="handle">
    /// The bytes representing an Ed25519 key.
    /// </param>
    public static implicit operator Moniker(ReadOnlyMemory<byte> handle)
    {
        return new Moniker(handle);
    }
    /// <summary>
    /// Implicitly converts an moniker to an <code>Address</code>
    /// from an <code>Moniker</code> instance.
    /// </summary>
    /// <param name="alias">
    /// Moniker identifying the contract instance.
    /// </param>
    public static implicit operator Address(Moniker moniker)
    {
        return new Address(moniker);
    }
    /// <summary>
    /// Computes the EVM Hash Address from an endorsment containing
    /// a ECDSASecp256K1 public key.
    /// </summary>
    /// <param name="endorsement">
    /// The endorsement, must be of type ECDSASecp256K1
    /// </param>
    /// <returns>
    /// The bytes representing the equivalent EVM Address.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// If the endorsement is not of type ECDSASecp256K1
    /// </exception>
    private static ReadOnlyMemory<byte> evmAddressFromEndorsement(Endorsement endorsement)
    {
        if (endorsement.Type == KeyType.ECDSASecp256K1)
        {
            var publicKey = EcdsaSecp256k1Util.PublicParamsFromDerOrRaw(endorsement.PublicKey).Q.GetEncoded(false);
            var digest = new KeccakDigest(256);
            digest.BlockUpdate(publicKey, 1, publicKey.Length - 1);
            var hash = new byte[digest.GetDigestSize()];
            digest.DoFinal(hash, 0);
            return hash.AsMemory()[^20..];
        }
        throw new ArgumentException("Can only compute a Moniker address from an Endorsment of type ECDSASecp256K1.");
    }
}

internal static class MonikerExtensions
{
    internal static bool IsNullOrNone([NotNullWhen(false)] this Moniker? moniker)
    {
        return moniker is null || moniker == Moniker.None;
    }
}