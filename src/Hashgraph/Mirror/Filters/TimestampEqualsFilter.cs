namespace Hashgraph.Mirror.Filters;
/// <summary>
/// Retrieve data matching the given consensus timestamp
/// in the hedera ledger.
/// </summary>
public class TimestampEqualsFilter : IMirrorQueryFilter
{
    /// <summary>
    /// Consensus timestamp to match.
    /// </summary>
    private readonly ConsensusTimeStamp _timestamp;
    /// <summary>
    /// Constructor, requires a consensus timestamp.
    /// </summary>
    /// <param name="timestamp">
    /// The consensus timestamp representing the moment in time
    /// in the ledger to match against.
    /// </param>
    public TimestampEqualsFilter(ConsensusTimeStamp timestamp)
    {
        _timestamp = timestamp;
    }
    /// <summary>
    /// The query parameter name recognized by the remote mirror node.
    /// </summary>
    public string Name => "timestamp";

    /// <summary>
    /// The value of the query parameter sent to the mirror node.
    /// </summary>
    public string Value => _timestamp.ToString();
}
