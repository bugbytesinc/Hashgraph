using System;

namespace Hashgraph.Implementation
{
    internal interface IData
    {
        ReadOnlySpan<byte> Data { get; }
    }
}
