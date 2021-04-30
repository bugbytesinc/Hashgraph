#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph
{
    /// <summary>
    /// The token balance information associated with an account,
    /// including the amount of coins held, KYC status and Freeze status.
    /// </summary>
    public sealed record TokenBalance : CryptoBalance
    {
        /// <summary>
        /// The Address of the token
        /// </summary>
        public Address Token { get; internal init; }
        /// <summary>
        /// The string symbol representing this token.
        /// </summary>
        public string Symbol { get; internal init; }
        /// <summary>
        /// The KYC status of the token for this account.
        /// </summary>
        public TokenKycStatus KycStatus { get; internal init; }
        /// <summary>
        /// The Frozen status of the token for this account.
        /// </summary>
        public TokenTradableStatus TradableStatus { get; internal init; }
    }
}
