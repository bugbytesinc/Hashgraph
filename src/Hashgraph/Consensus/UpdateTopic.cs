using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Updates the changeable properties of a Hedera Network Topic.
        /// </summary>
        /// <param name="updateParameters">
        /// The Topic update parameters, includes a required 
        /// <see cref="Address"/> reference to the Topic to update plus
        /// a number of changeable properties of the Topic.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating success of the operation.
        /// of the request.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> UpdateTopicAsync(UpdateTopicParams updateParameters, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await UpdateTopicImplementationAsync(updateParameters, configure, false));
        }
        /// <summary>
        /// Updates the changeable properties of a hedera network Topic.
        /// </summary>
        /// <param name="updateParameters">
        /// The Topic update parameters, includes a required 
        /// <see cref="Address"/> reference to the Topic to update plus
        /// a number of changeable properties of the Topic.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record containing the details of the results.
        /// of the request.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> UpdateTopicWithRecordAsync(UpdateTopicParams updateParameters, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await UpdateTopicImplementationAsync(updateParameters, configure, true));
        }
        /// <summary>
        /// Internal implementation of the update Topic functionality.
        /// </summary>
        private async Task<NetworkResult> UpdateTopicImplementationAsync(UpdateTopicParams updateParameters, Action<IContext>? configure, bool includeRecord)
        {
            updateParameters = RequireInputParameter.UpdateParameters(updateParameters);
            await using var context = CreateChildContext(configure);
            var updateTopicBody = new ConsensusUpdateTopicTransactionBody
            {
                TopicID = new TopicID(updateParameters.Topic)
            };
            if (updateParameters.Memo != null)
            {
                updateTopicBody.Memo = updateParameters.Memo;
            }
            if (!(updateParameters.Administrator is null))
            {
                updateTopicBody.AdminKey = new Key(updateParameters.Administrator);
            }
            if (!(updateParameters.Participant is null))
            {
                updateTopicBody.SubmitKey = new Key(updateParameters.Participant);
            }
            if (updateParameters.RenewPeriod.HasValue)
            {
                updateTopicBody.AutoRenewPeriod = new Duration(updateParameters.RenewPeriod.Value);
            }
            if (!(updateParameters.RenewAccount is null))
            {
                updateTopicBody.AutoRenewAccount = new AccountID(updateParameters.RenewAccount);
            }
            var transactionBody = new TransactionBody
            {
                ConsensusUpdateTopic = updateTopicBody
            };
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Unable to update Topic, status: {0}", updateParameters.Signatory);
        }
    }
}
