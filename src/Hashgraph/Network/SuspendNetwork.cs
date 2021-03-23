using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Set the period of time where the network will suspend will stop creating events 
        /// and accepting transactions. This can be used to safely shut down 
        /// the platform for maintenance and for upgrades if the file information is included.
        /// </summary>
        /// <param name="suspendParameters">
        /// The details of the suspend request, includes the time to wait before suspension, 
        /// the duration of the suspension and optionally to include an update file.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Submit Message Receipt indicating success.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> SuspendNetworkAsync(SuspendNetworkParams suspendParameters, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new FreezeTransactionBody(suspendParameters), configure, false));
        }
    }
}
