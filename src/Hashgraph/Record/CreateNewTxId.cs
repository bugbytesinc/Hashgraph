using Proto;
using System;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Creates a new Transaction Id
    /// </summary>
    /// <param name="configure"></param>
    /// <returns></returns>
    public TxId CreateNewTxId(Action<IContext>? configure = null)
    {
        var context = CreateChildContext(configure);
        var result = context.GetOrCreateTransactionID().AsTxId();
        _ = context.DisposeAsync();
        return result;
    }
}