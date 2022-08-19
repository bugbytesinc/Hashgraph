using Hashgraph.Implementation;

namespace Hashgraph;

/// <summary>
/// A transaction record containing information concerning the 
/// newly created ranged psudo random number.
/// </summary>
public sealed record RangedPsudoRandomNumberRecord : TransactionRecord
{
    /// <summary>
    /// The 32 bit generated number from a ranged PRNG transaction.
    public int Number { get; internal init; }
    /// <summary>
    /// Internal Constructor of the record.
    /// </summary>
    internal RangedPsudoRandomNumberRecord(NetworkResult record) : base(record)
    {
        Number = record.Record!.PrngNumber;
    }
}