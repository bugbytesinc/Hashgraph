using Google.Protobuf;
using Grpc.Core;
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
        public Task<CreateContractReceipt> CreateContractAsync(CreateContractParams createParameters, Action<IContext>? configure = null)
        {
            return CreateContractImplementationAsync<CreateContractReceipt>(createParameters, configure);
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
        public Task<CreateContractRecord> CreateContractWithRecordAsync(CreateContractParams createParameters, Action<IContext>? configure = null)
        {
            return CreateContractImplementationAsync<CreateContractRecord>(createParameters, configure);
        }
        /// <summary>
        /// Internal Create Contract Implementation
        /// </summary>
        public async Task<TResult> CreateContractImplementationAsync<TResult>(CreateContractParams createParameters, Action<IContext>? configure) where TResult : new()
        {
            createParameters = RequireInputParameter.CreateParameters(createParameters);
            var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatory = Transactions.GatherSignatories(context, createParameters.Signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId, "Create Contract");
            transactionBody.ContractCreateInstance = new ContractCreateTransactionBody
            {
                FileID = Protobuf.ToFileId(createParameters.File),
                AdminKey = createParameters.Administrator is null ? null : Protobuf.ToPublicKey(createParameters.Administrator),
                Gas = createParameters.Gas,
                InitialBalance = createParameters.InitialBalance,
                AutoRenewPeriod = Protobuf.ToDuration(createParameters.RenewPeriod),
                ConstructorParameters = ByteString.CopyFrom(Abi.EncodeArguments(createParameters.Arguments).ToArray()),
                Memo = context.Memo ?? "Create Contract"
            };
            var request = await Transactions.SignTransactionAsync(transactionBody, signatory);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck.NodeTransactionPrecheckCode);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to create contract, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is CreateContractRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                Protobuf.FillRecordProperties(record, rec);
                rec.Contract = Protobuf.FromContractID(receipt.ContractID);
            }
            else if (result is CreateContractReceipt rcpt)
            {
                Protobuf.FillReceiptProperties(transactionId, receipt, rcpt);
                rcpt.Contract = Protobuf.FromContractID(receipt.ContractID);
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new SmartContractService.SmartContractServiceClient(channel);
                return async (Transaction transaction) => await client.createContractAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
