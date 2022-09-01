using Hashgraph.Implementation;
using System;

namespace Hashgraph;

/// <summary>
/// A transaction record containing information concerning the 
/// newly created unbounded 384 bit psudo random number.
/// </summary>
public sealed record BytesPsudoRandomNumberRecord : TransactionRecord
{
    /// <summary>
    /// The unbounded 384 bit psudo random number.
    public ReadOnlyMemory<byte> Bytes { get; internal init; }
    /// <summary>
    /// Internal Constructor of the record.
    /// </summary>
    internal BytesPsudoRandomNumberRecord(NetworkResult record) : base(record)
    {
        Bytes = record.Record!.PrngBytes.Memory;
    }
}