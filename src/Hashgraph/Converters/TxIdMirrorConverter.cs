using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hashgraph.Converters;

/// <summary>
/// TxId Converter for reading and writing JSON
/// when interacting with a mirror node.
/// </summary>
public class TxIdMirrorConverter : JsonConverter<TxId>
{
    /// <summary>
    /// Converts a JSON string (mirror node version) to a transaction id.
    /// </summary>
    /// <param name="reader">reader</param>
    /// <param name="typeToConvert">type to convert</param>
    /// <param name="options">json options</param>
    /// <returns>A transaction id as a TxID or None if empty</returns>
    public override TxId? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var valueAsString = reader.GetString();
        if (!string.IsNullOrWhiteSpace(valueAsString))
        {
            var parts = valueAsString.Split('-');
            if (parts.Length > 1 &&
                parts.Length < 4 &&
                TryParseAddress(parts[0], out var address) &&
                long.TryParse(parts[1], out var seconds) &&
                int.TryParse((parts.Length == 3 ? parts[2] : null) ?? "0", out var nanos))
            {
                return new TxId(address, seconds, nanos);
            }
        }
        return TxId.None;
    }
    /// <summary>
    /// Converts a transction id (as TxId) into its mirror
    /// node string representaiton.
    /// </summary>
    /// <param name="writer">json writer</param>
    /// <param name="timeStamp">timestamp to convert</param>
    /// <param name="options">json options</param>
    public override void Write(Utf8JsonWriter writer, TxId txId, JsonSerializerOptions options)
    {
        if (txId == null || txId == TxId.None)
        {
            writer.WriteStringValue(string.Empty);
        }
        else
        {
            writer.WriteStringValue($"{txId.Address}-{txId.ValidStartSeconds}-{txId.ValidStartNanos:000000000}");
        }
    }
    /// <summary>
    /// Helper function for parsing the address
    /// portion of a transaction ID
    /// </summary>
    /// <param name="addressAsString">string form of the addres</param>
    /// <param name="address">contains the parsed address if successful.</param>
    /// <returns>true if able to parse the string into an address</returns>
    private bool TryParseAddress(string addressAsString, [NotNullWhen(true)] out Address? address)
    {
        var parts = addressAsString.Split('.');
        if (parts.Length == 3)
        {
            if (uint.TryParse(parts[0], out uint shard) &&
                uint.TryParse(parts[1], out uint realm) &&
                uint.TryParse(parts[2], out uint number))
            {
                address = new Address(shard, realm, number);
                return true;
            }
        }
        address = default;
        return false;
    }
}