#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

namespace Hashgraph.Extensions
{
    /// <summary>
    /// Information regarding a node from the signed
    /// address book.
    /// </summary>
    public class NodeInfo
    {
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
    }
}
