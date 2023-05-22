using Hashgraph.Converters;
using System.Text.Json.Serialization;

namespace Hashgraph;

/// <summary>
/// KYC Status of a Token
/// </summary>
public enum TokenKycStatus
{
    /// <summary>
    /// KYC Does not apply to this token
    /// </summary>
    NotApplicable = 0,
    /// <summary>
    /// KYC has been granted.
    /// </summary>
    Granted = 1,
    /// <summary>
    /// KYC has been revoked.
    /// </summary>
    Revoked = 2
}