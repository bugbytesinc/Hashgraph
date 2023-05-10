using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hashgraph.Converters;

/// <summary>
/// Consensus Timestamp JSON Converter
/// </summary>
public class PauseStatusConverter : JsonConverter<TokenTradableStatus>
{
    /// <summary>
    /// Convert a JSON string into a pause tradeable status
    /// </summary>
    /// <param name="reader">reader</param>
    /// <param name="typeToConvert">type to convert</param>
    /// <param name="options">json options</param>
    /// <returns>ConsensusTimestamp object</returns>
    public override TokenTradableStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString() switch
        {
            "PAUSED" => TokenTradableStatus.Suspended,
            "UNPAUSED" => TokenTradableStatus.Tradable,
            "NOT_APPLICABLE" => TokenTradableStatus.NotApplicable,
            _ => TokenTradableStatus.NotApplicable
        };
    }
    /// <summary>
    /// Converts a consensus timestamp object into its string representation.
    /// </summary>
    /// <param name="writer">json writer</param>
    /// <param name="timeStamp">timestamp to convert</param>
    /// <param name="options">json options</param>
    public override void Write(Utf8JsonWriter writer, TokenTradableStatus status, JsonSerializerOptions options)
    {
        writer.WriteStringValue(status switch
        {
            TokenTradableStatus.Suspended => "PAUSED",
            TokenTradableStatus.Tradable => "UNPAUSED",
            _ => "NOT_APPLICABLE"
        });
    }
}
