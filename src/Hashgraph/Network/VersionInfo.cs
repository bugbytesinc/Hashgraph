#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph
{
    /// <summary>
    /// Contains version information identifying the Hedera
    /// Services version and API Protobuf version implemented
    /// by the node being queried.
    /// </summary>
    public sealed class VersionInfo
    {
        /// <summary>
        /// Hedera API Protobuf version supported by this node.
        /// </summary>
        public SemanticVersion ApiProtobufVersion { get; internal set; }
        /// <summary>
        /// Hedera Services Version implemented by this node.
        /// </summary>
        public SemanticVersion HederaServicesVersion { get; internal set; }
    }
}
