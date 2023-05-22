#pragma warning disable CS8618
using System.Text.Json.Serialization;

namespace Hashgraph.Mirror;

/// <summary>
/// Represents a fraction.
/// </summary>
public class FractionData
{
    /// <summary>
    /// Numerator
    /// </summary>
    [JsonPropertyName("numerator")]
    public long Numerator { get; set; }
    /// <summary>
    /// Denominator
    /// </summary>
    [JsonPropertyName("denominator")]
    public long Denominator { get; set; }
}
