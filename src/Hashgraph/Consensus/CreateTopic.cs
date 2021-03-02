using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Creates a new topic instance with the given create parameters.
        /// </summary>
        /// <param name="createParameters">
        /// Details regarding the topic to instantiate.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt with a description of the newly created topic.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<CreateTopicReceipt> CreateTopicAsync(CreateTopicParams createParameters, Action<IContext>? configure = null)
        {
            return new CreateTopicReceipt(await CreateTopicImplementationAsync(createParameters, configure, false));
        }
        /// <summary>
        /// Creates a new topic instance with the given create parameters.
        /// </summary>
        /// <param name="createParameters">
        /// Details regarding the topic to instantiate.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record with a description of the newly created topic.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<CreateTopicRecord> CreateTopicWithRecordAsync(CreateTopicParams createParameters, Action<IContext>? configure = null)
        {
            return new CreateTopicRecord(await CreateTopicImplementationAsync(createParameters, configure, true));
        }
        /// <summary>
        /// Internal implementation of the Create ConsensusTopic service.
        /// </summary>
        private async Task<NetworkResult> CreateTopicImplementationAsync(CreateTopicParams createParameters, Action<IContext>? configure, bool includeRecord)
        {
            createParameters = RequireInputParameter.CreateParameters(createParameters);
            await using var context = CreateChildContext(configure);
            var transactionBody = new TransactionBody
            {
                ConsensusCreateTopic = new ConsensusCreateTopicTransactionBody
                {
                    Memo = createParameters.Memo,
                    AdminKey = createParameters.Administrator is null ? null : new Key(createParameters.Administrator),
                    SubmitKey = createParameters.Participant is null ? null : new Key(createParameters.Participant),
                    AutoRenewPeriod = new Duration(createParameters.RenewPeriod),
                    AutoRenewAccount = createParameters.RenewAccount is null ? null : new AccountID(createParameters.RenewAccount)
                }
            };
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Unable to create Consensus Topic, status: {0}", createParameters.Signatory);
        }
    }
}
