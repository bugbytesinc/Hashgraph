#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

using System;

namespace Hashgraph.Extensions
{
    /// <summary>
    /// Information regarding a node from the signed
    /// address book.
    /// </summary>
    public class NodeInfo
    {
        /// <summary>
        /// Identifier of the node (non-sequential)
        /// </summary>
        public long Id { get; internal set; }
        /// <summary>
        /// The ip address of the Node with separator & octets
        /// </summary>
        public string IpAddress { get; internal set; }
        /// <summary>
        /// The port number of the grpc server for the node
        /// </summary>
        public int Port { get; internal set; }
        /// <summary>
        /// The memo field of the node
        /// </summary>
        public string Memo { get; internal set; }
        /// <summary>
        /// The RSA public key of the node.
        /// </summary>
        public string RsaPublicKey { get; internal set; }
        /// <summary>
        /// The crypto account associated with this node.
        /// </summary>
        public Address Address { get; internal set; }
        /// <summary>
        /// Hash of the X509 certificate for gRPC traffict to this node.
        /// </summary>
        public ReadOnlyMemory<byte> CertificateHash { get; internal set; }
    }
}
