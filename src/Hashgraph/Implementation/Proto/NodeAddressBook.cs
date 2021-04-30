using Hashgraph.Extensions;
using System;
using System.Linq;

namespace Proto
{
    public sealed partial class NodeAddressBook
    {
        internal Hashgraph.Extensions.NodeInfo[] ToNodeInfoArray()
        {
            return NodeAddress.Select(node => new Hashgraph.Extensions.NodeInfo
            {
                Id = node.NodeId,
                RsaPublicKey = node.RSAPubKey,
                Address = node.NodeAccountId.AsAddress(),
                CertificateHash = new ReadOnlyMemory<byte>(node.NodeCertHash.ToArray()),
                Endpoints = node.ServiceEndpoint?.Select(e => new Endpoint(e)).ToArray() ?? Array.Empty<Endpoint>(),
                Description = node.Description,
                Stake = node.Stake,
            }).ToArray();
        }
    }
}
