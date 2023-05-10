using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hashgraph.Converters;

/// <summary>
/// Kyc Status JSON Converter
/// </summary>
public class TokenKycStatusConverter : JsonConverter<TokenKycStatus>
{
    /// <summary>
    /// Convert a JSON string into a pause kyc status
    /// </summary>
    /// <param name="reader">reader</param>
    /// <param name="typeToConvert">type to convert</param>
    /// <param name="options">json options</param>
    /// <returns>token kyc status object</returns>
    public override TokenKycStatus Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString() switch
        {
            "GRANTED" => TokenKycStatus.Granted,
            "REVOKED" => TokenKycStatus.Revoked,
            "NOT_APPLICABLE" => TokenKycStatus.NotApplicable,
            _ => TokenKycStatus.NotApplicable
        };
    }
    /// <summary>
    /// Converts a consensus timestamp object into its string representation.
    /// </summary>
    /// <param name="writer">json writer</param>
    /// <param name="timeStamp">timestamp to convert</param>
    /// <param name="options">json options</param>
    public override void Write(Utf8JsonWriter writer, TokenKycStatus status, JsonSerializerOptions options)
    {
        writer.WriteStringValue(status switch
        {
            TokenKycStatus.Granted => "GRANTED",
            TokenKycStatus.Revoked => "REVOKED",
            TokenKycStatus.NotApplicable => "NOT_APPLICABLE",
            _ => "NOT_APPLICABLE"
        });
    }
}
