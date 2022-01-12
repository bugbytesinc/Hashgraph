using Hashgraph.Implementation;
using Proto;

namespace Hashgraph;

/// <summary>
/// Receipt produced from creating a new contract.
/// </summary>
public sealed record CreateTopicReceipt : TransactionReceipt
{
    /// <summary>
    /// The newly created or associated topic instance address.
    /// </summary>
    /// <remarks>
    /// The value will be <code>None</code> if the create topic
    /// method was scheduled as a pending transaction.
    /// </remarks>
    public Address Topic { get; internal init; }
    /// <summary>
    /// Internal Constructor of the receipt.
    /// </summary>
    internal CreateTopicReceipt(NetworkResult result) : base(result)
    {
        Topic = result.Receipt.TopicID.AsAddress();
    }
}