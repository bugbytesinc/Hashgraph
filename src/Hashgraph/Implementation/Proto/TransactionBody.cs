using Hashgraph.Implementation;
using System.Diagnostics.CodeAnalysis;

namespace Proto;

public sealed partial class TransactionBody
{
    internal bool TryGetNetworkTransaction([NotNullWhen(true)] out INetworkTransaction networkTransaction)
    {
        networkTransaction = data_ as INetworkTransaction;
        return networkTransaction != null;
    }
}