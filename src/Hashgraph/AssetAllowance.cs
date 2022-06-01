using Proto;
using System;
using System.Collections.Generic;

namespace Hashgraph;

/// <summary>
/// Represents an allowance allocation permitting a
/// agent account privleges of spending the specified
/// amount assets from the owning account.
/// </summary>
public sealed record AssetAllowance
{
    /// <summary>
    /// The address of the asset's token definition
    /// having the allocated allowance.
    /// </summary>
    public Address Token { get; private init; }
    /// <summary>
    /// The Account owner holding the assets that
    /// may be spent by the delegate.
    /// </summary>
    public Address Owner { get; private init; }
    /// <summary>
    /// The account that may spend the allocated
    /// allowance of assets.
    /// </summary>
    public Address Agent { get; private init; }
    /// <summary>
    /// The explicit list of serial numbers that
    /// can be spent by the delegate.  If the value
    /// is <code>null</code> then all assets of the
    /// token class may be spend.  If the list is 
    /// empty, it means all of the identified assets
    /// with specific serial numbers have already been
    /// removed from the account.
    /// </summary>
    public IReadOnlyCollection<long>? SerialNumbers { get; private init; }
    /// <summary>
    /// Represents an allowance allocation permitting a
    /// agent account privleges of spending the specified
    /// amount assets from the owning account.
    /// </summary>
    /// <param name="token">
    /// The address of the asset's token definition
    /// having the allocated allowance.
    /// </param>
    /// <param name="owner">
    /// The Account owner holding the assets that
    /// may be spent by the delegate.
    /// </param>
    /// <param name="agent">
    /// The account that may spend the allocated
    /// allowance of assets.
    /// </param>
    /// <param name="serialNumbers">
    /// The explicit list of serial numbers that
    /// can be spent by the delegate.  If the value
    /// is <code>null</code> then all assets of the
    /// token class may be spend.
    /// </param>
    /// <exception cref="ArgumentException">
    /// If any of the addresses are null or empty.
    /// </exception>
    public AssetAllowance(Address token, Address owner, Address agent, IReadOnlyCollection<long>? serialNumbers = null)
    {
        if (token.IsNullOrNone())
        {
            throw new ArgumentException(nameof(token), "The allowance asset token cannot be null or empty.");
        }
        if (owner.IsNullOrNone())
        {
            throw new ArgumentException(nameof(owner), "The allowance owner account cannot be null or empty.");
        }
        if (agent.IsNullOrNone())
        {
            throw new ArgumentException(nameof(agent), "The allowance agent account cannot be null or empty.");
        }
        Token = token;
        Owner = owner;
        Agent = agent;
        SerialNumbers = serialNumbers;
    }
    /// <summary>
    /// Represents an allowance allocation permitting a
    /// agent account privleges of spending the specified
    /// asset from the owning account.
    /// </summary>
    /// <remarks>
    /// Convenience constructor for a singular asset form.
    /// </remarks>
    /// <param name="asset">
    /// Single asset to grant the allowance.
    /// </param>
    /// <param name="owner">
    /// The Account owner holding the assets that
    /// may be spent by the delegate.
    /// </param>
    /// <param name="agent">
    /// The account that may spend the allocated
    /// allowance of assets.
    /// </param>
    /// <exception cref="ArgumentException">
    /// If any of the addresses are null or empty.
    /// </exception>
    public AssetAllowance(Asset asset, Address owner, Address agent)
    {
        if (asset is null || Asset.None.Equals(asset))
        {
            throw new ArgumentException(nameof(asset), "The allowance asset cannot be null or empty.");
        }
        if (owner.IsNullOrNone())
        {
            throw new ArgumentException(nameof(owner), "The allowance owner account cannot be null or empty.");
        }
        if (agent.IsNullOrNone())
        {
            throw new ArgumentException(nameof(agent), "The allowance agent account cannot be null or empty.");
        }
        Token = asset;
        Owner = owner;
        Agent = agent;
        SerialNumbers = new[] { asset.SerialNum };
    }
    /// <summary>
    /// Internal helper constructor for creating the
    /// allowance from protobuf object.
    /// </summary>
    internal AssetAllowance(GrantedNftAllowance allowance, Address owner)
    {
        if (allowance is not null)
        {
            Token = allowance.TokenId.AsAddress();
            Owner = owner;
            Agent = allowance.Spender.AsAddress();
            SerialNumbers = Array.Empty<long>();
        }
        else
        {
            Token = Address.None;
            Owner = Address.None;
            Agent = Address.None;
            SerialNumbers = Array.Empty<long>();
        }
    }
}