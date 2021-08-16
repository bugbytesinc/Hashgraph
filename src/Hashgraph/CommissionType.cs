using System.ComponentModel;

namespace Hashgraph
{
    /// <summary>
    /// The known types of Royalties, Fees and Commissions applied to transfer.
    /// </summary>
    public enum CommissionType
    {
        /// <summary>
        /// A Fixed Commission Fee assessed per transaction involving
        /// the token or asset.
        /// </summary>
        /// <remarks>
        /// Applies to both Fungible Tokens and Assets (NFTs).
        /// </remarks>
        [Description("Fixed Commission Fee")] Fixed = 0,
        /// <summary>
        /// A Royalty Commission Fee based on overall value transferred 
        /// in exchange for the asset.
        /// </summary>
        /// <remarks>
        /// Only applies to assets (NFTs).
        /// </remarks>
        [Description("Royalty Commission Fee")] Value = 1,
        /// <summary>
        /// A Porpotional Commission Fee based on the amount of Token Transferred.
        /// </summary>
        /// <remarks>
        /// Only applies to Fungible Tokens.
        /// </remarks>
        [Description("Fractional Commission Fee")] Fractional = 2,
    }
}
