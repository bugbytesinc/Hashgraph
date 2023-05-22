using System;
using System.Collections.Generic;

namespace Hashgraph.Mirror.Implementation;
/// <summary>
/// Paged list of gossip nodes.
/// </summary>
internal class GossipNodePage : Page<GossipNodeData>
{
    /// <summary>
    /// List of gossip nodes.
    /// </summary>
    public GossipNodeData[]? Nodes { get; set; }
    /// <summary>
    /// Enumerates the list of gossip nodes.
    /// </summary>
    /// <returns>
    /// Enumerator of gossip nodes for this paged list.
    /// </returns>
    public override IEnumerable<GossipNodeData> GetItems()
    {
        return Nodes ?? Array.Empty<GossipNodeData>();
    }
}