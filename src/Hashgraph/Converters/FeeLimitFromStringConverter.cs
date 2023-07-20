using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hashgraph.Converters;

internal class FeeLimitFromStringConverter : JsonConverter<long>
{
    /// <summary>
    /// Convert a JSON string representing the fee limit into a long
    /// </summary>
    /// <param name="reader">reader</param>
    /// <param name="typeToConvert">type to convert</param>
    /// <param name="options">json options</param>
    /// <returns>bytes or an empty value</returns>
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (long.TryParse(reader.GetString(), out var value))
        {
            return value;
        }
        return 0;
    }
    /// <summary>
    /// Converts a timespan into total whole seconds 
    /// represented as a string.
    /// </summary>
    /// <param name="writer">json writer</param>
    /// <param name="timespan">timestamp to convert</param>
    /// <param name="options">json options</param>
    public override void Write(Utf8JsonWriter writer, long feeLimit, JsonSerializerOptions options)
    {
        writer.WriteStringValue(feeLimit.ToString());
    }
}