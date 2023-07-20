namespace Hashgraph.Mirror.Filters;
/// <summary>
/// Filter specifying the order of results
/// that should be returned by the mirror node.
/// </summary>
public class OrderByFilter : IMirrorQueryFilter
{
    /// <summary>
    /// OrderBy instance that requests that results be returned in ascending order.
    /// </summary>
    public static OrderByFilter Ascending = new OrderByFilter(true);
    /// <summary>
    /// OrderBy instance that requests that results be returned in descending order.
    /// </summary>
    public static OrderByFilter Descending = new OrderByFilter(false);
    /// <summary>
    /// Flag indicating the sorting order 
    /// is ascending (otherwise descending)
    /// </summary>
    private readonly bool _ascending;
    /// <summary>
    /// Private constructor taking argument
    /// of ascending or not.
    /// </summary>
    /// <param name="ascending">
    /// true of the desired order is ascending
    /// </param>
    private OrderByFilter(bool ascending)
    {
        _ascending = ascending;
    }
    /// <summary>
    /// The query parameter name recognized by the remote mirror node.
    /// </summary>
    public string Name => "order";
    /// <summary>
    /// The value of the query parameter sent to the mirror node.
    /// </summary>
    public string Value => _ascending ? "asc" : "desc";
}