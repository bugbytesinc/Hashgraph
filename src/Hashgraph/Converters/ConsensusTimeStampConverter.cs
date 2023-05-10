using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hashgraph.Converters;

/// <summary>
/// Consensus Timestamp JSON Converter
/// </summary>
public class ConsensusTimeStampConverter : JsonConverter<ConsensusTimeStamp>
{
    /// <summary>
    /// Convert a JSON string into a Consensus Timestamp
    /// </summary>
    /// <param name="reader">reader</param>
    /// <param name="typeToConvert">type to convert</param>
    /// <param name="options">json options</param>
    /// <returns>ConsensusTimestamp object</returns>
    public override ConsensusTimeStamp Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (decimal.TryParse(reader.GetString(), out decimal epoch))
        {
            return new ConsensusTimeStamp(epoch);
        }
        return ConsensusTimeStamp.MinValue;
    }
    /// <summary>
    /// Converts a consensus timestamp object into its string representation.
    /// </summary>
    /// <param name="writer">json writer</param>
    /// <param name="timeStamp">timestamp to convert</param>
    /// <param name="options">json options</param>
    public override void Write(Utf8JsonWriter writer, ConsensusTimeStamp timeStamp, JsonSerializerOptions options)
    {
        writer.WriteStringValue(timeStamp.ToString());
    }
}
