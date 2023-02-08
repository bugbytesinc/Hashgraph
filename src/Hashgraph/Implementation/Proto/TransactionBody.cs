using Hashgraph.Implementation;
using System.Diagnostics.CodeAnalysis;

namespace Proto;

public sealed partial class TransactionBody
{
    internal bool TryGetNetworkTransaction([NotNullWhen(true)] out INetworkTransaction networkTransaction)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        networkTransaction = data_ as INetworkTransaction;
#pragma warning restore CS8601 // Possible null reference assignment.
        return networkTransaction != null;
    }
}