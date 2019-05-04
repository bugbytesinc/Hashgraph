using System;

namespace Hashgraph.Implementation
{
    internal interface ISigner
    {
        byte[] Sign(ReadOnlySpan<byte> data);
    }
}
