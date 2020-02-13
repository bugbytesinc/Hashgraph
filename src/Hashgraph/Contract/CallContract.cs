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
        /// Calls a smart contract returning a receipt indicating success.  
        /// This can be used for contract calls that do not return data. 
        /// If the contract returns data, call the <see cref="CallContractWithRecordAsync"/> 
        /// call instead to retrieve the information returned from the function call.
        /// </summary>
        /// <param name="callParameters">
        /// An object identifying the function to call, any input parameters and the 
        /// amount of gas that may be used to execute the request.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A contract transaction receipt indicating success, it does not
        /// include any output parameters sent from the contract.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> CallContractAsync(CallContractParams callParameters, Action<IContext>? configure = null)
        {
            return CallContractImplementationAsync<TransactionReceipt>(callParameters, configure);
        }
        /// <summary>
        /// Calls a smart contract returning if successful.  The CallContractReceipt 
        /// will include the details of the results from the call, including the 
        /// output parameters returned by the function call.
        /// </summary>
        /// <param name="callParameters">
        /// An object identifying the function to call, any input parameters and the 
        /// amount of gas that may be used to execute the request.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A contract transaction record indicating success, it also
        /// any output parameters sent from the contract function.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<CallContractRecord> CallContractWithRecordAsync(CallContractParams callParameters, Action<IContext>? configure = null)
        {
            return CallContractImplementationAsync<CallContractRecord>(callParameters, configure);
        }
        /// <summary>
        /// Internal implementation of the contract call method.
        /// </summary>
        private async Task<TResult> CallContractImplementationAsync<TResult>(CallContractParams callParmeters, Action<IContext>? configure) where TResult : new()
        {
            callParmeters = RequireInputParameter.CallContractParameters(callParmeters);
            await using var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatory = Transactions.GatherSignatories(context, callParmeters.Signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId);
            transactionBody.ContractCall = new ContractCallTransactionBody
            {
                ContractID = Protobuf.ToContractID(callParmeters.Contract),
                Gas = callParmeters.Gas,
                Amount = callParmeters.PayableAmount,
                FunctionParameters = Abi.EncodeFunctionWithArguments(callParmeters.FunctionName, callParmeters.FunctionArgs).ToByteString()
            };
            var request = await Transactions.SignTransactionAsync(transactionBody, signatory);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Contract call failed, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is CallContractRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                Protobuf.FillRecordProperties(record, rec);
                rec.CallResult = Protobuf.FromContractCallResult(record.ContractCallResult);
            }
            else if (result is TransactionReceipt rcpt)
            {
                Protobuf.FillReceiptProperties(transactionId, receipt, rcpt);
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new SmartContractService.SmartContractServiceClient(channel);
                return async (Transaction transaction) => await client.contractCallMethodAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
