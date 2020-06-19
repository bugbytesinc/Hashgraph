using System;
using System.Linq;

namespace Proto
{
    public sealed partial class NodeAddressBook
    {
        internal Hashgraph.Extensions.NodeInfo[] ToNodeInfoArray()
        {
            return NodeAddress.Select(a => new Hashgraph.Extensions.NodeInfo
            {
                Id = a.NodeId,
                IpAddress = a.IpAddress.ToStringUtf8(),
                Port = a.Portno,
                Memo = a.Memo.ToStringUtf8(),
                RsaPublicKey = a.RSAPubKey,
                Address = a.NodeAccountId == null ? Hashgraph.Address.None : a.NodeAccountId.ToAddress(),
                CertificateHash = new ReadOnlyMemory<byte>(a.NodeCertHash.ToArray())
            }).ToArray();
        }
    }
}
