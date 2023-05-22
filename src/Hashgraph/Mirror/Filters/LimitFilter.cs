namespace Hashgraph.Mirror.Filters;
/// <summary>
/// Indicates the number of results to return in a page.
/// </summary>
public class LimitFilter : IMirrorQueryFilter
{
    /// <summary>
    /// The number of records to return in a page.
    /// </summary>
    private readonly int _limit;
    /// <summary>
    /// Constructor requres the page size limit.
    /// </summary>
    /// <param name="limit">
    /// The number of records to return in a page.
    /// </param>
    public LimitFilter(int limit)
    {
        _limit = limit;
    }
    /// <summary>
    /// The query parameter name recognized by the remote mirror node.
    /// </summary>
    public string Name => "limit";

    /// <summary>
    /// The value of the query parameter sent to the mirror node.
    /// </summary>
    public string Value => _limit.ToString();
}
