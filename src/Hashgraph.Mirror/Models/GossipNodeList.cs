namespace Hashgraph.Mirror;
/// <summary>
/// Paged list of gossip nodes.
/// </summary>
public class GossipNodeList : PagedList<GossipNode>
{
    /// <summary>
    /// List of gossip nodes.
    /// </summary>
    public GossipNode[]? Nodes { get; set; }
    /// <summary>
    /// Enumerates the list of gossip nodes.
    /// </summary>
    /// <returns>
    /// Enumerator of gossip nodes for this paged list.
    /// </returns>
    public override IEnumerable<GossipNode> GetItems()
    {
        return Nodes ?? Array.Empty<GossipNode>();
    }
}