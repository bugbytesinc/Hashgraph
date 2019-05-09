using System;

namespace Hashgraph.Implementation
{
    internal interface ISigner
    {
        byte[] Sign(ReadOnlyMemory<byte> data);
    }
}
