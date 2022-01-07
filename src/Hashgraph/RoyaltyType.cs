using System.ComponentModel;

namespace Hashgraph;

/// <summary>
/// The known types of Royalties applied to token and
/// asset transfers.
/// </summary>
public enum RoyaltyType
{
    /// <summary>
    /// A single Fixed Royalty applied to the transaction as
    /// a whole when transferring an asset or token.
    /// </summary>
    /// <remarks>
    /// Applies to both Fungible Tokens and Assets (NFTs).
    /// </remarks>
    [Description("Fixed Royalty Fee")] Fixed = 0,
    /// <summary>
    /// A Royalty computed from value given in exchange for
    /// receiving an Asset (NFT).
    /// </summary>
    /// <remarks>
    /// Only applies to assets (NFTs).
    /// </remarks>
    [Description("Royalty Royalty Fee")] Asset = 1,
    /// <summary>
    /// A Royalty computed from the amount of Fungible token
    /// exchanged, can be in the form as a deduction of the
    /// token echanged, or an exise amount taken from the 
    /// sender of the fungible token.
    /// </summary>
    /// <remarks>
    /// Only applies to Fungible Tokens.
    /// </remarks>
    [Description("Fractional Royalty Fee")] Token = 2,
}