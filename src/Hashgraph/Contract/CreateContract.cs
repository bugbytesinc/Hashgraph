using Google.Protobuf;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Creates a new contract instance with the given create parameters.
        /// </summary>
        /// <param name="createParameters">
        /// Details regarding the contract to instantiate.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt with a description of the newly created contract.
        /// and receipt information.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<CreateContractReceipt> CreateContractAsync(CreateContractParams createParameters, Action<IContext>? configure = null)
        {
            return new CreateContractReceipt(await CreateContractImplementationAsync(createParameters, configure, false));
        }
        /// <summary>
        /// Creates a new contract instance with the given create parameters 
        /// returning a detailed record.
        /// </summary>
        /// <param name="createParameters">
        /// Details regarding the contract to instantiate.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record with a description of the newly created contract.
        /// and receipt information.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<CreateContractRecord> CreateContractWithRecordAsync(CreateContractParams createParameters, Action<IContext>? configure = null)
        {
            return new CreateContractRecord(await CreateContractImplementationAsync(createParameters, configure, true));
        }
        /// <summary>
        /// Internal Create Contract Implementation
        /// </summary>
        private async Task<NetworkResult> CreateContractImplementationAsync(CreateContractParams createParameters, Action<IContext>? configure, bool includeRecord)
        {
            createParameters = RequireInputParameter.CreateParameters(createParameters);
            await using var context = CreateChildContext(configure);
            var transactionBody = new TransactionBody
            {
                ContractCreateInstance = new ContractCreateTransactionBody
                {
                    FileID = new FileID(createParameters.File),
                    AdminKey = createParameters.Administrator is null ? null : new Key(createParameters.Administrator),
                    Gas = createParameters.Gas,
                    InitialBalance = createParameters.InitialBalance,
                    AutoRenewPeriod = new Duration(createParameters.RenewPeriod),
                    ConstructorParameters = ByteString.CopyFrom(Abi.EncodeArguments(createParameters.Arguments).ToArray()),
                    Memo = createParameters.Memo ?? ""
                }
            };
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Unable to create contract, status: {0}", createParameters.Signatory);
        }
    }
}
