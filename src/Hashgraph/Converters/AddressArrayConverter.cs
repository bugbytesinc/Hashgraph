using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hashgraph.Converters;
/// <summary>
/// Account Address Array JSON Converter
/// </summary>
public class AddressArrayConverter : JsonConverter<Address[]>
{
    /// <summary>
    /// Convert a JSON string array into an Account Address Array
    /// </summary>
    /// <param name="reader">reader</param>
    /// <param name="typeToConvert">type to convert</param>
    /// <param name="options">json options</param>
    /// <returns>ConsensusTimestamp object</returns>
    public override Address[] Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException();
        }
        reader.Read();
        var converter = new AddressConverter();
        var list = new List<Address>();
        while (reader.TokenType != JsonTokenType.EndArray)
        {
            list.Add(converter.Read(ref reader, typeof(Address), options!));
            reader.Read();
        }
        return list.ToArray();
    }
    /// <summary>
    /// Converts aacount address array into its string array representation.
    /// </summary>
    /// <param name="writer">json writer</param>
    /// <param name="addresses">address array to convert</param>
    /// <param name="options">json options</param>
    public override void Write(Utf8JsonWriter writer, Address[] addresses, JsonSerializerOptions options)
    {
        var converter = new AddressConverter();
        writer.WriteStartArray();
        foreach (var address in addresses)
        {
            converter.Write(writer, address, options);
        }
        writer.WriteEndArray();
    }
}