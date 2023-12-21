using Hashgraph.Converters;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace Hashgraph;

/// <summary>
/// Represents a Hedera Network Account Address.
/// </summary>
[JsonConverter(typeof(AddressConverter))]
public sealed record Address
{
    /// <summary>
    /// Internal field to hold an Alias or Moniker if
    /// this address represents such a type of address.
    /// Will be null for <code>shard.realm.num</code>
    /// forms.
    /// </summary>
    private readonly object? _alternate;
    /// <summary>
    /// Enum identifying any special types of address
    /// beyond <code>shard.realm.num</code> that this
    /// addrss may hold, including alias and monikers.
    /// </summary>
    internal AddressType AddressType { get; private init; }
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
        AddressType = AddressType.ShardRealmNum;
        ShardNum = 0;
        RealmNum = 0;
        AccountNum = 0;
        _alternate = null;
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
        AddressType = AddressType.ShardRealmNum;
        ShardNum = shardNum;
        RealmNum = realmNum;
        AccountNum = accountNum;
        _alternate = null;
    }
    /// <summary>
    /// Outputs a string representation of the address, alias
    /// or moiker in the form of <code>shard.realm.num</code> or
    /// <code>shard.realm.alias</code>.
    /// </summary>
    /// <returns>
    /// String representation of this account identifier in its
    /// address, alias or moniker format.
    /// </returns>
    public override string ToString()
    {
        return AddressType switch
        {
            AddressType.Alias => ((Alias)_alternate!).ToString(),
            AddressType.Moniker => ((Moniker)_alternate!).ToString(),
            _ => this == None ? "None" : $"{ShardNum}.{RealmNum}.{AccountNum}"
        };
    }
    /// <summary>
    /// Internal helper constructor creating a form
    /// of address that represents an alias.  The public
    /// way to achive an alias address is to use the implict
    /// cast operator defined in <code>Alias</code>.
    /// </summary>
    /// <param name="alias">
    /// The alias this address wil encapsulate
    /// </param>
    internal Address(Alias alias)
    {
        AddressType = AddressType.Alias;
        ShardNum = alias.ShardNum;
        RealmNum = alias.RealmNum;
        AccountNum = 0;
        _alternate = alias;
    }
    /// <summary>
    /// Internal helper constructor creating a form
    /// of address that represents a moniker (EIP-1014).
    /// The public way to achive a moniker address is to
    /// use the implicit cast operator defined in
    /// <code>Moniker</code>
    /// </summary>
    /// <param name="moniker"></param>
    internal Address(Moniker moniker)
    {
        AddressType = AddressType.Moniker;
        ShardNum = moniker.ShardNum;
        RealmNum = moniker.RealmNum;
        AccountNum = 0;
        _alternate = moniker;
    }
    /// <summary>
    /// Attempts to retrieve the moniker wrapped by this
    /// address instance.  Will return false if this address
    /// does not hold a moniker.
    /// </summary>
    /// <param name="moniker">
    /// Variable receiving the moniker instance if the 
    /// operation is successfull, otherwise <code>null</code>.
    /// </param>
    /// <returns>
    /// <code>True</code> if the address hold a moniker,
    /// otherwise false.
    /// </returns>
    public bool TryGetMoniker([MaybeNullWhen(false)] out Moniker moniker)
    {
        if (AddressType == AddressType.Moniker)
        {
            moniker = (Moniker)_alternate!;
            return true;
        }
        moniker = default;
        return false;
    }
    /// <summary>
    /// Attempts to retrieve the alias wrapped by this
    /// address instance.  Will return false if this address
    /// does not hold an alias.
    /// </summary>
    /// <param name="alias">
    /// Variable receiving the alias instance if the 
    /// operation is successfull, otherwise <code>null</code>.
    /// </param>
    /// <returns>
    /// <code>True</code> if the address holds a moniker,
    /// otherwise false.
    /// </returns>
    public bool TryGetAlias([MaybeNullWhen(false)] out Alias alias)
    {
        if (AddressType == AddressType.Alias)
        {
            alias = (Alias)_alternate!;
            return true;
        }
        alias = null;
        return false;
    }
}
/// <summary>
/// Internal enumerator indicating which type of
/// address this object represents.
/// </summary>
internal enum AddressType
{
    /// <summary>
    /// Traditional hedera shard.realm.num address.
    /// </summary>
    ShardRealmNum,
    /// <summary>
    /// Enumeration (Public Key) Alias form of address,
    /// used to identify crypto accounts.
    /// </summary>
    Alias,
    /// <summary>
    /// EOA 20-byte, EIP-1014 Hedera VM contract form 
    /// of address, used as an alternate way to address
    /// contract instances created via the 
    /// solidity CREATE2 function.
    /// </summary>
    Moniker
}
internal static class AddressExtensions
{
    internal static bool IsNullOrNone([NotNullWhen(false)] this Address? address)
    {
        return address is null || address == Address.None;
    }
}