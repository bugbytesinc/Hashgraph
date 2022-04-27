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
    /// The account that may spend the allocated
    /// allowance of assets.
    /// </summary>
    public Address Agent { get; private init; }
    /// <summary>
    /// The explicit list of serial numbers that
    /// can be spent by the delegate.  If the value
    /// is <code>null</code> then all assets of the
    /// token class may be spent.  If the list is 
    /// empty, it means all of the identified assets
    /// with specific serial numbers have already been
    /// removed from the account.
    /// </summary>
    public IReadOnlyCollection<long>? SerialNumbers { get; private init; }
    /// <summary>
    /// Internal helper constructor for creating the
    /// allowance from protobuf object.
    /// </summary>
    internal AssetAllowance(Proto.GrantedNftAllowance allowance)
    {
        if (allowance is not null)
        {
            Token = allowance.TokenId.AsAddress();
            Agent = allowance.Spender.AsAddress();
            // This is a in HAPI, not sure if it
            // will be fixed.
            //SerialNumbers = allowance.ApprovedForAll ?
            //    null :
            //        (allowance.SerialNumbers is null ?
            //            Array.Empty<long>() :
            //            new ReadOnlyCollection<long>(allowance.SerialNumbers));
        }
        else
        {
            Token = Address.None;
            Agent = Address.None;
            SerialNumbers = Array.Empty<long>();
        }
    }
}