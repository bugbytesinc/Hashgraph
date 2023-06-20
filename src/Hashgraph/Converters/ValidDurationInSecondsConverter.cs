using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hashgraph.Converters;

internal class ValidDurationInSecondsConverter : JsonConverter<TimeSpan>
{
    /// <summary>
    /// Convert a JSON string representing whole seconds
    /// into a timestamp.
    /// </summary>
    /// <param name="reader">reader</param>
    /// <param name="typeToConvert">type to convert</param>
    /// <param name="options">json options</param>
    /// <returns>bytes or an empty value</returns>
    public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (double.TryParse(reader.GetString(), out var value))
        {
            return TimeSpan.FromSeconds(value);
        }
        return TimeSpan.Zero;
    }
    /// <summary>
    /// Converts a timespan into total whole seconds 
    /// represented as a string.
    /// </summary>
    /// <param name="writer">json writer</param>
    /// <param name="timespan">timestamp to convert</param>
    /// <param name="options">json options</param>
    public override void Write(Utf8JsonWriter writer, TimeSpan timespan, JsonSerializerOptions options)
    {
        writer.WriteStringValue(((int)timespan.TotalSeconds).ToString());
    }
}