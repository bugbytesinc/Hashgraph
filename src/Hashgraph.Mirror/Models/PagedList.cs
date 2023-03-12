namespace Hashgraph.Mirror;
/// <summary>
/// Generic paged list object that follows the pattern for all paged items 
/// returned from a mirror node.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class PagedList<T>
{
    /// <summary>
    /// A links object containing the next URL to call to retrieve the next 
    /// page of items in the list.
    /// </summary>
    public Links? Links { get; set; }
    /// <summary>
    /// Generic iterator object used by low level functions to convert paged 
    /// information into IAsyncEnumerable helper functions.
    /// </summary>
    /// <returns>
    /// Enumerable of the items in the list.
    /// </returns>
    public abstract IEnumerable<T> GetItems();
}
