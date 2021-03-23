using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Retrieves version information from the node.
        /// </summary>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// Version information regarding the gossip network node.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// 
        /// NOTE: Marked Internal because this is not yet implemented in testnet.
        internal async Task<VersionInfo> GetVersionInfoAsync(Action<IContext>? configure = null)
        {
            return new VersionInfo(await ExecuteQueryAsync(new NetworkGetVersionInfoQuery(), configure));
        }
    }
}
