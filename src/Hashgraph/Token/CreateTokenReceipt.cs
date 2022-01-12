using Hashgraph.Implementation;
using Proto;

namespace Hashgraph;

/// <summary>
/// Receipt produced from creating a new token.
/// </summary>
public sealed record CreateTokenReceipt : TransactionReceipt
{
    /// <summary>
    /// The newly created token address.
    /// </summary>
    /// <remarks>
    /// The value will be <code>None</code> if the create topic
    /// method was scheduled as a pending transaction.
    /// </remarks>
    public Address Token { get; internal init; }
    /// <summary>
    /// Internal Constructor of the receipt.
    /// </summary>
    internal CreateTokenReceipt(NetworkResult result) : base(result)
    {
        Token = result.Receipt.TokenID.AsAddress();
    }
}