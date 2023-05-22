#pragma warning disable CS8618
using Hashgraph.Converters;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;

/// <summary>
/// Represents a detailed holding of a token by an account.
/// </summary>
public class TokenHoldingData
{
    /// <summary>
    /// The address of the token.
    /// </summary>
    [JsonPropertyName("token_id")]
    public Address Token { get; set; }
    /// <summary>
    /// Was this token holding a result of an 
    /// automatic association.
    /// </summary>
    [JsonPropertyName("automatic_association")]
    public bool AutoAssociated { get; set; }
    /// <summary>
    /// The balance of account’s holdings of token in tinytokens.
    /// </summary>
    [JsonPropertyName("balance")]
    public long Balance { get; set; }
    /// <summary>
    /// The date when this holding was established.
    /// </summary>
    [JsonPropertyName("created_timestamp")]
    public ConsensusTimeStamp Created { get; set; }
    /// <summary>
    /// Status of the token related to freezing (if applicable)
    /// </summary>
    [JsonPropertyName("freeze_status")]
    [JsonConverter(typeof(FreezeStatusConverter))]
    public TokenTradableStatus FreezeStatus { get; set; } = default!;
    /// <summary>
    /// Status of the KYC status of the holding (if applicable)
    /// </summary>
    [JsonPropertyName("kyc_status")]
    [JsonConverter(typeof(TokenKycStatusConverter))]
    public TokenKycStatus KycStatus { get; set; } = default!;
}
