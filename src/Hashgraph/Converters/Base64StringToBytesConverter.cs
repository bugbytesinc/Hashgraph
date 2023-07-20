using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hashgraph.Converters;

/// <summary>
/// 64 base encoded memo converter
/// </summary>
public class Base64StringToBytesConverter : JsonConverter<ReadOnlyMemory<byte>>
{
    /// <summary>
    /// Convert a JSON string encoded as base 64 into bytes
    /// </summary>
    /// <param name="reader">reader</param>
    /// <param name="typeToConvert">type to convert</param>
    /// <param name="options">json options</param>
    /// <returns>bytes or an empty value</returns>
    public override ReadOnlyMemory<byte> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var valueInBase64 = reader.GetString();
        if (!string.IsNullOrWhiteSpace(valueInBase64))
        {
            try
            {
                return Convert.FromBase64String(valueInBase64);
            }
            catch
            {
                // Punt.
            }
        }
        return ReadOnlyMemory<byte>.Empty;
    }
    /// <summary>
    /// Converts an array of bytes into a 64 bit encoded string.
    /// </summary>
    /// <param name="writer">json writer</param>
    /// <param name="bytes">timestamp to convert</param>
    /// <param name="options">json options</param>
    public override void Write(Utf8JsonWriter writer, ReadOnlyMemory<byte> bytes, JsonSerializerOptions options)
    {
        writer.WriteStringValue(Convert.ToBase64String(bytes.Span));
    }
}
