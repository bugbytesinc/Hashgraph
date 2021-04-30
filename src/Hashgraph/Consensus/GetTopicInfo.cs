using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Retrieves detailed information regarding a Topic Instance.
        /// </summary>
        /// <param name="topic">
        /// The Hedera Network Address of the Topic instance to retrieve.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A detailed description of the contract instance.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        public async Task<TopicInfo> GetTopicInfoAsync(Address topic, Action<IContext>? configure = null)
        {
            return new TopicInfo(await ExecuteQueryAsync(new ConsensusGetTopicInfoQuery(topic), configure).ConfigureAwait(false));
        }
    }
}
