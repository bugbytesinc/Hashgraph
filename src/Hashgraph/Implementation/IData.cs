using System;

namespace Hashgraph.Implementation
{
    internal interface IData
    {
        ReadOnlyMemory<byte> Data { get; }
    }
}
