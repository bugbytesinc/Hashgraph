#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph
{
    /// <summary>
    /// The token balance information associated with an account,
    /// including the amount of coins held, KYC status and Freeze status.
    /// </summary>
    public sealed class TokenBalance
    {
        /// <summary>
        /// The Address of the token
        /// </summary>
        public Address Token { get; internal set; }
        /// <summary>
        /// The string symbol representing this token.
        /// </summary>
        public string Symbol { get; internal set; }
        /// <summary>
        /// The balance of tokens held by the associated account
        /// in the smallest denomination.
        /// </summary>
        public ulong Balance { get; internal set; }
        /// <summary>
        /// The KYC status of the token for this account.
        /// </summary>
        public TokenKycStatus KycStatus { get; internal set; }
        /// <summary>
        /// The Frozen status of the token for this account.
        /// </summary>
        public TokenTradableStatus TradableStatus { get; internal set; }
    }
}
