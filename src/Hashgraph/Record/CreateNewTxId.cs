using Hashgraph.Implementation;
using System;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Creates a new Transaction Id
        /// </summary>
        /// <param name="configure"></param>
        /// <returns></returns>
        public TxId CreateNewTxId(Action<IContext>? configure = null)
        {
            return Protobuf.FromTransactionId(Transactions.GetOrCreateTransactionID(CreateChildContext(configure)));
        }
    }
}
