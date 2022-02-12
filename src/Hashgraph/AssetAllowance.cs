using Proto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hashgraph;

/// <summary>
/// Represents an allowance allocation permitting a
/// delegate account privleges of spending the specified
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
    public Address Delegate { get; private init; }
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
    internal AssetAllowance(Proto.NftAllowance allowance)
    {
        if (allowance is not null)
        {
            Token = allowance.TokenId.AsAddress();
            Owner = allowance.Owner.AsAddress();
            Delegate = allowance.Spender.AsAddress();
            SerialNumbers = allowance.ApprovedForAll.GetValueOrDefault() ?
                null :
                    (allowance.SerialNumbers is null ?
                        Array.Empty<long>() :
                        new ReadOnlyCollection<long>(allowance.SerialNumbers));
        }
        else
        {
            Token = Address.None;
            Owner = Address.None;
            Delegate = Address.None;
            SerialNumbers = Array.Empty<long>();
        }
    }
}