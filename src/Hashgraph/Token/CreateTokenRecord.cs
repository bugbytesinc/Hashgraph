using Hashgraph.Implementation;
using Proto;

namespace Hashgraph;

/// <summary>
/// Record produced from creating a new token.
/// </summary>
public sealed record CreateTokenRecord : TransactionRecord
{
    /// <summary>
    /// The newly created token address.
    /// </summary>
    /// <remarks>
    /// The value will be <code>None</code> if the create token
    /// method was scheduled as a pending transaction.
    /// </remarks>
    public Address Token { get; internal init; }
    /// <summary>
    /// Internal Constructor of the record.
    /// </summary>
    internal CreateTokenRecord(NetworkResult result) : base(result)
    {
        Token = result.Receipt.TokenID.AsAddress();
    }
}