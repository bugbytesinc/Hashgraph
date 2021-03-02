using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Deletes a topic instance from the network. Must be signed 
        /// by the admin key.
        /// </summary>
        /// <param name="topicToDelete">
        /// The Topic instance that will be deleted.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the topic is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> DeleteTopicAsync(Address topicToDelete, Action<IContext>? configure = null)
        {
            return await DeleteTopicImplementationAsync(topicToDelete, null, configure);
        }
        /// <summary>
        /// Deletes a topic instance from the network. Must be signed 
        /// by the admin key.
        /// </summary>
        /// <param name="topicToDelete">
        /// The Topic instance that will be deleted.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key/callback matching the administrative endorsements
        /// associated with this topic (if not already added in the context).
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the topic is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> DeleteTopicAsync(Address topicToDelete, Signatory signatory, Action<IContext>? configure = null)
        {
            return await DeleteTopicImplementationAsync(topicToDelete, signatory, configure);
        }
        /// <summary>
        /// Internal implementation of delete topic method.
        /// </summary>
        private async Task<TransactionReceipt> DeleteTopicImplementationAsync(Address topicToDelete, Signatory? signatory, Action<IContext>? configure)
        {
            topicToDelete = RequireInputParameter.AddressToDelete(topicToDelete);
            await using var context = CreateChildContext(configure);
            var transactionBody = new TransactionBody
            {
                ConsensusDeleteTopic = new ConsensusDeleteTopicTransactionBody
                {
                    TopicID = new TopicID(topicToDelete)
                }
            };
            return new TransactionReceipt(await transactionBody.SignAndExecuteWithRetryAsync(context, false, "Unable to Delete Topic, status: {0}", signatory));
        }
    }
}
