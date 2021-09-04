using Proto;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hashgraph
{
    /// <summary>
    /// The information returned from the GetTokenInfo Client 
    /// method call.  It represents the details concerning 
    /// Tokens and Assets.
    /// </summary>
    public sealed record TokenInfo
    {
        /// <summary>
        /// The Hedera address of this token.
        /// </summary>
        public Address Token { get; private init; }
        /// <summary>
        /// The type of token this represents, fungible
        /// token or Asset (NFT).
        /// </summary>
        public TokenType Type { get; private init; }
        /// <summary>
        /// The string symbol representing this token.
        /// </summary>
        public string Symbol { get; private init; }
        /// <summary>
        /// Name of this token
        /// </summary>
        public string Name { get; private init; }
        /// <summary>
        /// The treasury account holding uncirculated tokens.
        /// </summary>
        public Address Treasury { get; private init; }
        /// <summary>
        /// The total balance of tokens in all accounts (the whole denomination).
        /// </summary>
        public ulong Circulation { get; private init; }
        /// <summary>
        /// The number of decimal places which each token may be subdivided.
        /// </summary>
        public uint Decimals { get; private init; }
        /// <summary>
        /// The maximum number of tokens allowed in circulation at a given time.
        /// The value of 0 is unbounded.
        /// </summary>
        public long Ceiling { get; private init; }
        /// <summary>
        /// Administrator key for signing transactions modifying this token's properties.
        /// </summary>
        public Endorsement? Administrator { get; private init; }
        /// <summary>
        /// Administrator key for signing transactions updating the grant or revoke 
        /// KYC status of an account.
        /// </summary>
        public Endorsement? GrantKycEndorsement { get; private init; }
        /// <summary>
        /// Administrator key for signing transactions for freezing or unfreezing an 
        /// account's ability to transfer tokens.
        /// </summary>
        public Endorsement? SuspendEndorsement { get; private init; }
        /// <summary>
        /// Administrator key for signing transaction that completely remove tokens
        /// from an crypto address.
        /// </summary>
        public Endorsement? ConfiscateEndorsement { get; private init; }
        /// <summary>
        /// Administrator key for signing transactions for minting or unminting 
        /// tokens in the treasury account.
        /// </summary>
        public Endorsement? SupplyEndorsement { get; private init; }
        /// <summary>
        /// Administrator key for signing transactions updating the royalties
        /// (custom transfer fees) associated with this token.
        /// </summary>
        public Endorsement? RoyaltiesEndorsement { get; set; }
        /// <summary>
        /// The current default suspended/frozen status of the token.
        /// </summary>
        public TokenTradableStatus TradableStatus { get; private init; }
        /// <summary>
        /// The current default KYC status of the token.
        /// </summary>
        public TokenKycStatus KycStatus { get; private init; }
        /// <summary>
        /// The list of fixed royalties assessed on transactions
        /// by the network when transferring this token.
        /// </summary>
        public ReadOnlyCollection<IRoyalty> Royalties { get; internal init; }
        /// <summary>
        /// Expiration date for the token.  Will renew as determined by the
        /// renew period and balance of auto renew account.
        /// </summary>
        public DateTime Expiration { get; private init; }
        /// <summary>
        /// Interval of the topic and auto-renewal period. If
        /// the associated renewal account does not have sufficient funds to 
        /// renew at the expiration time, it will be renewed for a period 
        /// of time the remaining funds can support.  If no funds remain, the
        /// topic instance will be deleted.
        /// </summary>
        public TimeSpan? RenewPeriod { get; private init; }
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
        public Address? RenewAccount { get; private init; }
        /// <summary>
        /// Flag indicating the token has been deleted.
        /// </summary>
        public bool Deleted { get; private init; }
        /// <summary>
        /// The memo associated with the token instance.
        /// </summary>
        public string Memo { get; private init; }

        internal TokenInfo(Response response)
        {
            var info = response.TokenGetInfo.TokenInfo;
            Token = info.TokenId.AsAddress();
            Type = (TokenType)info.TokenType;
            Symbol = info.Symbol;
            Name = info.Name;
            Treasury = info.Treasury.AsAddress();
            Circulation = info.TotalSupply;
            Decimals = info.Decimals;
            Ceiling = info.MaxSupply;
            Administrator = info.AdminKey?.ToEndorsement();
            GrantKycEndorsement = info.KycKey?.ToEndorsement();
            SuspendEndorsement = info.FreezeKey?.ToEndorsement();
            ConfiscateEndorsement = info.WipeKey?.ToEndorsement();
            SupplyEndorsement = info.SupplyKey?.ToEndorsement();
            RoyaltiesEndorsement = info.FeeScheduleKey?.ToEndorsement();
            Royalties = info.CustomFees.Select(fee => fee.ToRoyalty()).ToList().AsReadOnly();
            TradableStatus = (TokenTradableStatus)info.DefaultFreezeStatus;
            KycStatus = (TokenKycStatus)info.DefaultKycStatus;
            Expiration = info.Expiry.ToDateTime();
            RenewPeriod = info.AutoRenewPeriod?.ToTimeSpan();
            RenewAccount = info.AutoRenewAccount?.AsAddress();
            Deleted = info.Deleted;
            Memo = info.Memo;
        }
    }
}
