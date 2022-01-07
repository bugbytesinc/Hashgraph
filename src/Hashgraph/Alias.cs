using Google.Protobuf;
using System;

namespace Hashgraph;

/// <summary>
/// Represents a Hedera Network Account Alias.
/// </summary>
public sealed record Alias
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
    /// An Endorsement (public key) representing an alternate
    /// identifier for the account.  Must be a simple key
    /// endorsement, not a multi-key or n-of-m key.
    /// </summary>
    public Endorsement Endorsement { get; private init; }
    /// <summary>
    /// A special designation of an alias that can't be created.
    /// It represents the absence of a valid alias.  The network will
    /// intrepret as "no account/file/topic/token/contract" when applied 
    /// to change parameters. (typically the value null is intepreted 
    /// as "make no change"). In this way, it is possible to remove a 
    /// auto-renew account from a topic.
    /// </summary>
    public static Alias None { get; } = new Alias();
    /// <summary>
    /// Internal Constructor representing the "None" version of an
    /// alias.  This is a special construct that is used by the network
    /// to represent "removing" auto-renew address from a topic.
    /// </summary>
    private Alias()
    {
        ShardNum = 0;
        RealmNum = 0;
        Endorsement = Endorsement.None;
    }
    /// <summary>
    /// Public Constructor, takes an <code>Endorsement</code> as the 
    /// constructor and sets the Shard and Realm to values of zero.
    /// </summary>
    /// <param name="endorsement">
    /// The endorsment holding the public key that is the alias 
    /// associated with the target account.
    /// </param>
    public Alias(Endorsement endorsement) : this(0, 0, endorsement)
    {
    }
    /// <summary>
    /// Public Constructor, an <code>Alias</code> is imutable after
    /// construction.
    /// </summary>
    /// <param name="shardNum">
    /// Network Shard Number
    /// </param>
    /// <param name="realmNum">
    /// Network Realm Number
    /// </param>
    /// <param name="endorsement">
    /// The endorsment holding the public key that is the alias 
    /// associated with the target account.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// when any of the inputs are out of range.
    /// </exception>
    public Alias(long shardNum, long realmNum, Endorsement endorsement)
    {
        if (shardNum < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(shardNum), "Shard Number cannot be negative.");
        }
        if (realmNum < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(realmNum), "Realm Number cannot be negative.");
        }
        if (endorsement == Endorsement.None)
        {
            throw new ArgumentOutOfRangeException(nameof(endorsement), "Endorsement can not be empty.");
        }
        if (endorsement.Type != KeyType.Ed25519 && endorsement.Type != KeyType.ECDSASecp256K1)
        {
            throw new ArgumentOutOfRangeException(nameof(endorsement), "Unsupported key type, Endorsment must be a simple Ed25519 or ECDSA Secp 256K1 key type.");
        }
        ShardNum = shardNum;
        RealmNum = realmNum;
        Endorsement = endorsement;
    }
    /// <summary>
    /// Implicit operator for converting a byte array into an alias.  It assumes
    /// the bytes represent an Ed25519 or ECDSA Secp 256K1  key and sets the shard 
    /// and realm values to zero.
    /// </summary>
    /// <param name="publicKey">
    /// The bytes representing an Ed25519 key.
    /// </param>
    public static implicit operator Alias(ReadOnlyMemory<byte> publicKey)
    {
        return new Alias(new Endorsement(publicKey));
    }
    /// <summary>
    /// Implicit operator for converting an endorsment to an aliaas.  It
    /// set the shard and realm ot zero.
    /// </summary>
    /// <param name="endorsement">
    /// The endorsment holding the public key that is the alias 
    /// associated with the target account.
    /// </param>
    public static implicit operator Alias(Endorsement endorsement)
    {
        return new Alias(endorsement);
    }
}
internal static class AliasExtensions
{
    internal static bool IsNullOrNone(this Alias? alias)
    {
        return alias is null || alias == Alias.None;
    }
    internal static Alias ToAlias(this ByteString? bytes, long shard, long realm)
    {
        if (bytes is not null && !bytes.IsEmpty)
        {
            try
            {
                return new Alias(shard, realm, Proto.Key.Parser.ParseFrom(bytes).ToEndorsement());
            }
            catch
            {
                // For now we punt.
            }
        }
        return Alias.None;
    }
}