#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

using System;

namespace Hashgraph.Extensions
{
    /// <summary>
    /// Information regarding a node from the signed
    /// address book.
    /// </summary>
    public sealed record NodeInfo
    {
        /// <summary>
        /// Identifier of the node (non-sequential)
        /// </summary>
        public long Id { get; internal init; }
        /// <summary>
        /// The RSA public key of the node.
        /// </summary>
        public string RsaPublicKey { get; internal init; }
        /// <summary>
        /// The crypto account associated with this node.
        /// </summary>
        public Address Address { get; internal init; }
        /// <summary>
        /// Hash of the X509 certificate for gRPC traffict to this node.
        /// </summary>
        public ReadOnlyMemory<byte> CertificateHash { get; internal init; }
        /// <summary>
        /// List of public ip addresses and ports exposed by this node.
        /// </summary>
        public Endpoint[] Endpoints { get; internal init; }
        /// <summary>
        /// A Description of the node.
        /// </summary>
        public string Description { get; internal set; }
        /// <summary>
        /// The amount of tinybars staked to the node.
        /// </summary>
        public long Stake { get; internal set; }
    }
}
