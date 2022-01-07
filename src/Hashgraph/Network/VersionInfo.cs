using Proto;

namespace Hashgraph;

/// <summary>
/// Contains version information identifying the Hedera
/// Services version and API Protobuf version implemented
/// by the node being queried.
/// </summary>
public sealed record VersionInfo
{
    /// <summary>
    /// Hedera API Protobuf version supported by this node.
    /// </summary>
    public SemanticVersion ApiProtobufVersion { get; private init; }
    /// <summary>
    /// Hedera Services Version implemented by this node.
    /// </summary>
    public SemanticVersion HederaServicesVersion { get; private init; }
    /// <summary>
    /// Internal constructor from raw results
    /// </summary>
    internal VersionInfo(Response response)
    {
        var info = response.NetworkGetVersionInfo;
        ApiProtobufVersion = new SemanticVersion(info.HapiProtoVersion);
        HederaServicesVersion = new SemanticVersion(info.HederaServicesVersion);
    }
}