namespace Hashgraph.Mirror;
/// <summary>
/// A links object typically returned from the mirror node when more 
/// paged data is available.
/// </summary>
public class Links
{
    /// <summary>
    /// The URL of a link to call to retrieve the next set of paged data.
    /// </summary>
    public string? Next { get; set; }
}
