namespace Hashgraph.Mirror.Filters;
/// <summary>
/// Retrieve data after the given conensus time value.
/// </summary>
public class TimestampAfterFilter : IMirrorQueryFilter
{
    /// <summary>
    /// Consensus timestamp limiting results.
    /// </summary>
    private readonly ConsensusTimeStamp _timestamp;
    /// <summary>
    /// Constructor, requires a consensus timestamp.
    /// </summary>
    /// <param name="timestamp">
    /// The consensus timestamp representing the moment in time
    /// in the ledger that any data after the value should be returned.
    /// </param>
    public TimestampAfterFilter(ConsensusTimeStamp timestamp)
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
    public string Value => $"gt:{_timestamp}";
}
