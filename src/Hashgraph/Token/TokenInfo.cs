#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// The information returned from the GetTokenInfo Client 
    /// method call.  It represents the details concerning a 
    /// Hedera Fungable Token.
    /// </summary>
    public sealed record TokenInfo
    {
        /// <summary>
        /// The Hedera address of this token.
        /// </summary>
        public Address Token { get; internal init; }
        /// <summary>
        /// The string symbol representing this token.
        /// </summary>
        public string Symbol { get; internal init; }
        /// <summary>
        /// Name of this token
        /// </summary>
        public string Name { get; internal init; }
        /// <summary>
        /// The treasury account holding uncirculated tokens.
        /// </summary>
        public Address Treasury { get; internal init; }
        /// <summary>
        /// The total balance of tokens in all accounts (the whole denomination).
        /// </summary>
        public ulong Circulation { get; internal init; }
        /// <summary>
        /// The number of decimal places which each token may be subdivided.
        /// </summary>
        public uint Decimals { get; internal init; }
        /// <summary>
        /// Administrator key for signing transactions modifying this token's properties.
        /// </summary>
        public Endorsement? Administrator { get; internal init; }
        /// <summary>
        /// Administrator key for signing transactions updating the grant or revoke 
        /// KYC status of an account.
        /// </summary>
        public Endorsement? GrantKycEndorsement { get; internal init; }
        /// <summary>
        /// Administrator key for signing transactions for freezing or unfreezing an 
        /// account's ability to transfer tokens.
        /// </summary>
        public Endorsement? SuspendEndorsement { get; internal init; }
        /// <summary>
        /// Administrator key for signing transaction that completely remove tokens
        /// from an crypto address.
        /// </summary>
        public Endorsement? ConfiscateEndorsement { get; internal init; }
        /// <summary>
        /// Administrator key for signing transactions for minting or unminting 
        /// tokens in the treasury account.
        /// </summary>
        public Endorsement? SupplyEndorsement { get; internal init; }
        /// <summary>
        /// The current default suspended/frozen status of the token.
        /// </summary>
        public TokenTradableStatus TradableStatus { get; internal init; }
        /// <summary>
        /// The current default KYC status of the token.
        /// </summary>
        public TokenKycStatus KycStatus { get; internal init; }
        /// <summary>
        /// Expiration date for the token.  Will renew as determined by the
        /// renew period and balance of auto renew account.
        /// </summary>
        public DateTime Expiration { get; internal init; }
        /// <summary>
        /// Interval of the topic and auto-renewal period. If
        /// the associated renewal account does not have sufficient funds to 
        /// renew at the expiration time, it will be renewed for a period 
        /// of time the remaining funds can support.  If no funds remain, the
        /// topic instance will be deleted.
        /// </summary>
        public TimeSpan? RenewPeriod { get; internal init; }
        /// <summary>
        /// Optional address of the account supporting the auto renewal of 
        /// the token at expiration time.  The topic lifetime will be
        /// extended by the RenewPeriod at expiration time if this account
        /// contains sufficient funds.  The private key associated with
        /// this account must sign the transaction if RenewAccount is
        /// specified.
        /// </summary>
        /// <remarks>
        /// If specified, an Administrator Endorsement must also be specified.
        /// </remarks>
        public Address? RenewAccount { get; internal init; }
        /// <summary>
        /// Flag indicating the token has been deleted.
        /// </summary>
        public bool Deleted { get; internal init; }
    }
}
