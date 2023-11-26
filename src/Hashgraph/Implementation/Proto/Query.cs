using Hashgraph.Implementation;

namespace Proto;

public sealed partial class Query
{
    internal INetworkQuery? GetNetworkQuery()
    {
        return query_ as INetworkQuery;
    }
}