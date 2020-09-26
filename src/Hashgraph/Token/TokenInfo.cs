#pragma warning disable CS8618 // Non-nullable field is uninitialized.

using System;

namespace Hashgraph
{
    /// <summary>
    /// The information returned from the GetTokenInfo Client 
    /// method call.  It represents the details concerning a 
    /// Hedera Fungable Token.
    /// </summary>
    public sealed class TokenInfo
    {
        /// <summary>
        /// The Hedera address of this token.
        /// </summary>
        public Address Token { get; internal set; }
        /// <summary>
        /// The string symbol representing this token.
        /// </summary>
        public string Symbol { get; internal set; }
        /// <summary>
        /// The treasury account holding uncirculated tokens.
        /// </summary>
        public Address Treasury { get; internal set; }
        /// <summary>
        /// The total balance of tokens in all accounts (the whole denomination).
        /// </summary>
        public ulong Circulation { get; internal set; }
        /// <summary>
        /// The number of decimal places which each token may be subdivided.
        /// </summary>
        public uint Decimals { get; internal set; }
        /// <summary>
        /// Administrator key for signing transactions modifying this token's properties.
        /// </summary>
        public Endorsement? Administrator { get; internal set; }
        /// <summary>
        /// Administrator key for signing transactions updating the grant or revoke 
        /// KYC status of an account.
        /// </summary>
        public Endorsement? GrantKycEndorsement { get; internal set; }
        /// <summary>
        /// Administrator key for signing transactions for freezing or unfreezing an 
        /// account's ability to transfer tokens.
        /// </summary>
        public Endorsement? SuspendEndorsement { get; internal set; }
        /// <summary>
        /// Administrator key for signing transaction that completely remove tokens
        /// from an crypto address.
        /// </summary>
        public Endorsement? ConfiscateEndorsement { get; internal set; }
        /// <summary>
        /// Administrator key for signing transactions for minting or unminting 
        /// tokens in the treasury account.
        /// </summary>
        public Endorsement? SupplyEndorsement { get; internal set; }
        /// <summary>
        /// The current default suspended/frozen status of the token.
        /// </summary>
        public TokenTradableStatus TradableStatus { get; internal set; }
        /// <summary>
        /// The current default KYC status of the token.
        /// </summary>
        public TokenKycStatus KycStatus { get; internal set; }
        /// <summary>
        /// Flag indicating the token has been deleted.
        /// </summary>
        public bool Deleted { get; internal set; }
    }
}
