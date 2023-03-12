using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror.Mappers;

/// <summary>
/// Account Address JSON Converter
/// </summary>
/// /// <remarks>
/// This may be moved into the Hashgraph
/// Project in the future to facilitate
/// Easier integration with JSON Serialization
/// </remarks>
public class AddressConverter : JsonConverter<Address>
{
    /// <summary>
    /// Convert a JSON string into an Account Address
    /// </summary>
    /// <param name="reader">reader</param>
    /// <param name="typeToConvert">type to convert</param>
    /// <param name="options">json options</param>
    /// <returns>ConsensusTimestamp object</returns>
    public override Address Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (!string.IsNullOrWhiteSpace(value))
        {
            var parts = value.Split('.');
            if (parts.Length == 3)
            {
                if (uint.TryParse(parts[0], out uint shard) &&
                    uint.TryParse(parts[1], out uint realm) &&
                    uint.TryParse(parts[2], out uint number))
                {
                    return new Address(shard, realm, number);
                }
            }
        }
        return Address.None;
    }
    /// <summary>
    /// Converts aacount address object into its string representation.
    /// </summary>
    /// <param name="writer">json writer</param>
    /// <param name="address">address to convert</param>
    /// <param name="options">json options</param>
    public override void Write(Utf8JsonWriter writer, Address address, JsonSerializerOptions options)
    {
        writer.WriteStringValue(address.ToString());
    }
}
