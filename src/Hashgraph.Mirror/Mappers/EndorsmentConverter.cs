using Google.Protobuf;
using Org.BouncyCastle.Crypto.Parameters;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror.Mappers;

/// <summary>
/// Endorsment JSON Converter
/// </summary>
/// <remarks>
/// This may be moved into the Hashgraph
/// Project in the future to facilitate
/// Easier integration with JSON Serialization
/// </remarks>
public class EndorsementConverter : JsonConverter<Endorsement>
{
    /// <summary>
    /// Convert a JSON string into a Consensus Timestamp
    /// </summary>
    /// <param name="reader">reader</param>
    /// <param name="typeToConvert">type to convert</param>
    /// <param name="options">json options</param>
    /// <returns>ConsensusTimestamp object</returns>
    public override Endorsement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string? type = null;
        string? data = null;
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();
                switch (propertyName)
                {
                    case "_type":
                        type = reader.GetString();
                        break;

                    case "key":
                        data = reader.GetString();
                        break;
                }
            }
        }
        if (string.IsNullOrWhiteSpace(data) || string.IsNullOrWhiteSpace(type))
        {
            throw new JsonException();
        }
        var bytes = Hex.ToBytes(data);
        switch (type)
        {
            case "ED25519":
                return new Endorsement(KeyType.Ed25519, bytes);
            case "ECDSA_SECP256K1":
                return new Endorsement(KeyType.ECDSASecp256K1, bytes);
            case "ProtobufEncoded":
                return Proto.Key.Parser.ParseFrom(bytes.ToArray()).ToEndorsement();
        }
        throw new JsonException();
    }
    /// <summary>
    /// Converts a consensus timestamp object into its string representation.
    /// </summary>
    /// <param name="writer">json writer</param>
    /// <param name="endorsement">timestamp to convert</param>
    /// <param name="options">json options</param>
    public override void Write(Utf8JsonWriter writer, Endorsement endorsement, JsonSerializerOptions options)
    {
        string type;
        byte[] data;
        switch (endorsement.Type)
        {
            case KeyType.Ed25519:
                type = "ED25519";
                data = ((Ed25519PublicKeyParameters)endorsement._data).GetEncoded();
                break;
            case KeyType.ECDSASecp256K1:
                type = "ECDSA_SECP256K1";
                data = ((ECPublicKeyParameters)endorsement._data).Q.GetEncoded(true);
                break;
            default:
                type = "ProtobufEncoded";
                data = new Proto.Key(endorsement).ToByteArray();
                break;
        }
        writer.WriteStartObject();
        writer.WritePropertyName("_type");
        writer.WriteStringValue(type);
        writer.WritePropertyName("key");
        writer.WriteStringValue(Hex.FromBytes(data));
        writer.WriteEndObject();
    }
}
