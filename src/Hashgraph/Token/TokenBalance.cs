#pragma warning disable CS8618 // Non-nullable field is uninitialized.

using Proto;

namespace Hashgraph;

/// <summary>
/// The token balance information associated with an account,
/// including the amount of coins held, KYC status and Freeze status.
/// </summary>
public sealed record TokenBalance : CryptoBalance
{
    /// <summary>
    /// The Address of the token
    /// </summary>
    public Address Token { get; }
    /// <summary>
    /// The string symbol representing this token.
    /// </summary>
    public string Symbol { get; }
    /// <summary>
    /// The KYC status of the token for this account.
    /// </summary>
    public TokenKycStatus KycStatus { get; }
    /// <summary>
    /// The Frozen status of the token for this account.
    /// </summary>
    public TokenTradableStatus TradableStatus { get; }
    /// <summary>
    /// True if this token was associated automatically by
    /// the network via autoassociaiton via becomming a
    /// token or assset treasury.
    /// </summary>
    public bool AutoAssociated { get; }
    /// <summary>
    /// Internal Helper Function to create a token balance
    /// from raw protobuf response.
    /// </summary>
    internal TokenBalance(TokenRelationship entry)
    {
        Token = entry.TokenId.AsAddress();
        Symbol = entry.Symbol;
        Balance = entry.Balance;
        KycStatus = (TokenKycStatus)entry.KycStatus;
        TradableStatus = (TokenTradableStatus)entry.FreezeStatus;
        Decimals = entry.Decimals;
        AutoAssociated = entry.AutomaticAssociation;
    }
}