using System;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal interface implemented by objects that 
    /// can sign transactions.  Not intended for public use.
    /// </summary>
    internal interface ISigner
    {
        byte[] Sign(ReadOnlyMemory<byte> data);
    }
}
