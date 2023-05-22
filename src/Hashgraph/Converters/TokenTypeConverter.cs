using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hashgraph.Converters;

/// <summary>
/// Token Type Converter
/// </summary>
public class TokenTypeConverter : JsonConverter<TokenType>
{
    /// <summary>
    /// Convert a JSON string into a Token Type
    /// </summary>
    /// <param name="reader">reader</param>
    /// <param name="typeToConvert">type to convert</param>
    /// <param name="options">json options</param>
    /// <returns>TokenType Enum</returns>
    public override TokenType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.GetString() switch
        {
            "FUNGIBLE_COMMON" => TokenType.Fungible,
            "NON_FUNGIBLE_UNIQUE" => TokenType.Asset,
            _ => TokenType.Fungible
        };
    }
    /// <summary>
    /// Converts a token type object into its string representation.
    /// </summary>
    /// <param name="writer">json writer</param>
    /// <param name="timeStamp">timestamp to convert</param>
    /// <param name="options">json options</param>
    public override void Write(Utf8JsonWriter writer, TokenType timeStamp, JsonSerializerOptions options)
    {
        writer.WriteStringValue(timeStamp switch
        {
            TokenType.Fungible => "FUNGIBLE_COMMON",
            TokenType.Asset => "NON_FUNGIBLE_UNIQUE",
            _ => "FUNGIBLE_COMMON"
        });
    }
}
