using Hashgraph.Converters;
using System.Text.Json.Serialization;

namespace Hashgraph;

/// <summary>
/// The type of token.
/// </summary>
[JsonConverter(typeof(TokenTypeConverter))]
public enum TokenType
{
    /// <summary>
    /// Fungible Token
    /// </summary>
    Fungible = 0,
    /// <summary>
    /// Asset Token (Non Fungible, non Divisible)
    /// </summary>
    Asset = 1
}