﻿using Proto;
using System;
using System.Linq;

namespace Hashgraph;

/// <summary>
/// The information returned from the GetTokenInfo Client 
/// method call.  It represents the details concerning an 
/// Hedera Non Fungable Token.
/// </summary>
public sealed record AssetInfo
{
    /// <summary>
    /// The identifer of the asset (NFT Instance).
    /// </summary>
    public Asset Asset { get; private init; }
    /// <summary>
    /// The account currently owning the asset.
    /// </summary>
    public Address Owner { get; private init; }
    /// <summary>
    /// The account that has the rights to spend
    /// this asset via an allowance grant.
    /// </summary>
    public Address Agent { get; private init; }
    /// <summary>
    /// The Consensus Timestamp for when this asset was created (minted).
    /// </summary>
    public ConsensusTimeStamp Created { get; private init; }
    /// <summary>
    /// The metadata associated with this asset, limited to 100 bytes.
    /// </summary>
    public ReadOnlyMemory<byte> Metadata { get; private init; }
    /// <summary>
    /// Identification of the Ledger (Network) this 
    /// asset information was retrieved from.
    /// </summary>
    public ReadOnlyMemory<byte> Ledger { get; private init; }
    /// <summary>
    /// Equality implementation
    /// </summary>
    /// <param name="other">
    /// The other <code>AssetInfo</code> object to compare.
    /// </param>
    /// <returns>
    /// True if asset, owner, created and metadata are the same.
    /// </returns>
    public bool Equals(AssetInfo? other)
    {
        return other is not null &&
            Asset.Equals(other.Asset) &&
            Owner.Equals(other.Owner) &&
            Created.Equals(other.Created) &&
            Metadata.Span.SequenceEqual(other.Metadata.Span) &&
            Ledger.Span.SequenceEqual(other.Ledger.Span) &&
            Agent.Equals(other.Agent);
    }
    /// <summary>
    /// Equality implementation.
    /// </summary>
    /// <returns>
    /// A unique hash of the contents of this <code>AssetInfo</code> 
    /// object.  Only consistent within the current instance of 
    /// the application process.
    /// </returns>
    public override int GetHashCode()
    {
        return $"AssetInfo.{Asset.GetHashCode()}.{Owner.GetHashCode()}.{Created.GetHashCode()}.{Hex.FromBytes(Metadata)}.{Hex.FromBytes(Ledger)}.{Agent.GetHashCode()}".GetHashCode();
    }
    private AssetInfo(TokenNftInfo info)
    {
        Asset = info.NftID.AsAsset();
        Owner = info.AccountID.AsAddress();
        Created = info.CreationTime.ToConsensusTimeStamp();
        Metadata = info.Metadata.Memory;
        Ledger = info.LedgerId.Memory;
        Agent = info.SpenderId.AsAddress();
    }
    internal AssetInfo(Response response) : this(response.TokenGetNftInfo.Nft)
    {
    }
}