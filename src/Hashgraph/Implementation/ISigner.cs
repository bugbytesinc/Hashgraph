using Proto;
using System;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal interface implemented by objects that 
    /// can sign transactions.  Not intended for public use.
    /// </summary>
    internal interface ISigner
    {        
        SignaturePair[] Sign(ReadOnlyMemory<byte> data);
    }
}
