using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hashgraph.Converters;
/// <summary>
/// Response Code Converter
/// </summary>
public class RepsponseCodeConverter : JsonConverter<ResponseCode>
{
    /// <summary>
    /// Map of the response code text value to enum value.
    /// </summary>
    private static readonly Dictionary<string, ResponseCode> _mapDesc;
    /// <summary>
    /// Map of the response code enum value to string text value.
    /// </summary>
    private static readonly Dictionary<ResponseCode, string> _mapCode;
    /// <summary>
    /// Converts a JSON string value to a Response Code value.
    /// </summary>
    /// <param name="reader">reader</param>
    /// <param name="typeToConvert">type to convert</param>
    /// <param name="options">json options</param>
    /// <returns>Response Code Enum, or -500 if not convertable.</returns>
    public override ResponseCode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (_mapDesc.TryGetValue(reader.GetString()!, out ResponseCode code))
        {
            return code;
        }
        return (ResponseCode)(-500);
    }
    /// <summary>
    /// Converts a Resonse Code value into its Text String representation.
    /// </summary>
    /// <param name="writer">json writer</param>
    /// <param name="timeStamp">timestamp to convert</param>
    /// <param name="options">json options</param>
    public override void Write(Utf8JsonWriter writer, ResponseCode code, JsonSerializerOptions options)
    {
        if (_mapCode.TryGetValue(code, out string? desc))
        {
            writer.WriteStringValue(desc);
        }
        else
        {
            writer.WriteStringValue(string.Empty);
        }
    }
    /// <summary>
    /// Static setup helper function that creates the mappings
    /// between text values and enum values.
    /// </summary>
    static RepsponseCodeConverter()
    {
        _mapDesc = new Dictionary<string, ResponseCode>();
        _mapCode = new Dictionary<ResponseCode, string>();
        foreach (var field in typeof(ResponseCode).GetFields())
        {
            if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
            {
                if (!string.IsNullOrWhiteSpace(attr.Description))
                {
                    if (field.GetValue(null) is ResponseCode code)
                    {
                        _mapDesc[attr.Description] = code;
                    }
                }
            }
        }
    }
}