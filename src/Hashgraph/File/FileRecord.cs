using Hashgraph.Implementation;
using Proto;

namespace Hashgraph;

/// <summary>
/// A transaction record containing information concerning the newly created file.
/// </summary>
public sealed record FileRecord : TransactionRecord
{
    /// <summary>
    /// The address of the newly created file.
    /// </summary>
    /// <remarks>
    /// The value will be <code>None</code> if the create file
    /// method was scheduled as a pending transaction.
    /// </remarks>
    public Address File { get; internal init; }
    /// <summary>
    /// Internal Constructor of the record.
    /// </summary>
    internal FileRecord(NetworkResult record) : base(record)
    {
        File = record.Receipt.FileID.AsAddress();
    }
}