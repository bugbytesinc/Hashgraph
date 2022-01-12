using Hashgraph.Implementation;
using Proto;

namespace Hashgraph;

/// <summary>
/// A transaction record containing information concerning the newly created account.
/// </summary>
public sealed record CreateAccountRecord : TransactionRecord
{
    /// <summary>
    /// The address of the newly created account.
    /// </summary>
    /// <remarks>
    /// The value will be <code>None</code> if the create acocunt
    /// method was scheduled as a pending transaction.
    /// </remarks>
    public Address Address { get; internal init; }
    /// <summary>
    /// Internal Constructor of the record.
    /// </summary>
    internal CreateAccountRecord(NetworkResult result) : base(result)
    {
        Address = result.Receipt.AccountID.AsAddress();
    }
}