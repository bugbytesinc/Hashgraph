#pragma warning disable CS8618
using Hashgraph.Converters;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;
/// <summary>
/// Token information retrieved from a mirror node.
/// </summary>
public class TokenData
{
    /// <summary>
    /// The Hedera address of this token.
    /// </summary>
    [JsonPropertyName("token_id")]
    public Address Token { get; set; }
    /// <summary>
    /// The string symbol representing this token.
    /// </summary>
    [JsonPropertyName("symbol")]
    public string Symbol { get; set; }
    /// <summary>
    /// Name of this token
    /// </summary>
    [JsonPropertyName("name")]
    public string Name { get; set; }
    /// <summary>
    /// The memo associated with the token instance.
    /// </summary>
    [JsonPropertyName("memo")]
    public string Memo { get; set; }
    /// <summary>
    /// The treasury account holding uncirculated tokens.
    /// </summary>
    [JsonPropertyName("treasury_account_id")]
    public Address Treasury { get; set; }
    /// <summary>
    /// The number of token decimals (if this is a fungible token).
    /// </summary>
    [JsonPropertyName("decimals")]
    public int Decimals { get; set; }
    /// <summary>
    /// The total supply of the token (in tinytokens).
    /// </summary>
    [JsonPropertyName("total_supply")]
    public long Circulation { get; set; }
    /// <summary>
    /// The maximum number of tokens allowed in circulation at a given time.
    /// The value of 0 is unbounded.
    /// </summary>
    [JsonPropertyName("max_supply")]
    public long Ceiling { get; set; }
    /// <summary>
    /// The type of token this represents, fungible
    /// token or Asset (NFT).
    /// </summary>
    [JsonPropertyName("type")]
    public TokenType Type { get; set; }
    /// <summary>
    /// The last time this token was modified.
    /// </summary>
    [JsonPropertyName("modified_timestamp")]
    public ConsensusTimeStamp Modified { get; set; }
    /// <summary>
    /// Optional address of the account supporting the auto renewal of 
    /// the token at expiration time.  The topic lifetime will be
    /// extended by the RenewPeriod at expiration time if this account
    /// contains sufficient funds.  The private key associated with
    /// this account must sign the transaction if RenewAccount is
    /// specified.
    /// </summary>
    [JsonPropertyName("auto_renew_account")]
    public Address? RenewAccount { get; set; }
    /// <summary>
    /// Interval of the topic and auto-renewal period. If
    /// the associated renewal account does not have sufficient funds to 
    /// renew at the expiration time, it will be renewed for a period 
    /// of time the remaining funds can support.  If no funds remain, the
    /// topic instance will be deleted.
    /// </summary>
    [JsonPropertyName("auto_renew_period")]
    public long? RenewPeriodInSeconds { get; set; }
    /// <summary>
    /// Administrator key for signing transactions modifying this token's properties.
    /// </summary>
    [JsonPropertyName("admin_key")]
    public Endorsement? Administrator { get; set; }
    /// <summary>
    /// Administrator key for signing transactions for minting or unminting 
    /// tokens in the treasury account.
    /// </summary>
    [JsonPropertyName("supply_key")]
    public Endorsement? SupplyEndorsement { get; set; }
    /// <summary>
    /// Administrator key for signing transactions updating the royalties
    /// (custom transfer fees) associated with this token.
    /// </summary>
    [JsonPropertyName("fee_schedule_key")]
    public Endorsement? RoyaltiesEndorsement { get; set; }
    /// <summary>
    /// Administrator key for signing transactions for freezing or unfreezing an 
    /// account's ability to transfer tokens.
    /// </summary>
    [JsonPropertyName("freeze_key")]
    public Endorsement? SuspendEndorsement { get; set; }
    /// <summary>
    /// Administrator key for signing transactions updating the grant or revoke 
    /// KYC status of an account.
    /// </summary>
    [JsonPropertyName("kyc_key")]
    public Endorsement? GrantKycEndorsement { get; set; }
    /// <summary>
    /// Administrator key for signing transactions that can pasue or continue
    /// the exchange of all assets across all accounts on the network.
    /// </summary>
    [JsonPropertyName("pause_key")]
    public Endorsement? PauseEndorsement { get; set; }
    /// <summary>
    /// Administrator key for signing transaction that completely remove tokens
    /// from an crypto address.
    /// </summary>
    [JsonPropertyName("wipe_key")]
    public Endorsement? ConfiscateEndorsement { get; set; }
    /// <summary>
    /// The current default suspended/frozen status of the token.
    /// </summary>
    [JsonPropertyName("freeze_default")]
    public bool SuspenedByDefault { get; set; }
    /// <summary>
    /// The current paused/frozen status of the token for all accounts.
    /// </summary>
    [JsonPropertyName("pause_status")]
    [JsonConverter(typeof(PauseStatusConverter))]
    public TokenTradableStatus PauseStatus { get; set; }
    /// <summary>
    /// The number of tokens in circulation upon
    /// creation of this token.
    /// </summary>
    [JsonPropertyName("initial_supply")]
    public long InitialSupply { get; set; }
    /// <summary>
    /// The supply type, finite or infinite.
    /// </summary>
    [JsonPropertyName("supply_type")]
    public string SupplyType { get; set; } = default!;
    /// <summary>
    /// The consensus timestamp of the token's creation.
    /// </summary>
    [JsonPropertyName("created_timestamp")]
    public ConsensusTimeStamp Created { get; set; }
    /// <summary>
    /// Expiration date for the token.  Will renew as determined by the
    /// renew period and balance of auto renew account.
    /// </summary>
    [JsonPropertyName("expiry_timestamp")]
    [JsonConverter(typeof(ConsensusTimeStampFromLongConverter))]
    public ConsensusTimeStamp Expiration { get; set; }
    /// <summary>
    /// Flag indicating the token has been deleted.
    /// </summary>
    [JsonPropertyName("deleted")]
    public bool? Deleted { get; set; }
    /// <summary>
    /// The list of royalties assessed on transactions
    /// by the network when transferring this token.
    /// </summary>
    [JsonPropertyName("custom_fees")]
    public CustomFeeData Royalties { get; set; }
}
