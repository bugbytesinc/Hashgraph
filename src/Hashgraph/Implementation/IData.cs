using System;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal interface providing access to an object’s 
    /// underlying serialized protobuf data.  
    /// Not intended for public use.
    /// </summary>
    internal interface IData
    {
        ReadOnlyMemory<byte> Data { get; }
    }
}
