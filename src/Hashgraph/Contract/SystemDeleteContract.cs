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
        /// Removes a contract from the network via Administrative Delete
        /// </summary>
        /// <param name="contractToDelete">
        /// The address of the contract to delete.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating success of the contract deletion.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> SystemDeleteContractAsync(Address contractToDelete, Action<IContext>? configure = null)
        {
            return SystemDeleteContractImplementationAsync<TransactionReceipt>(contractToDelete, null, configure);
        }
        /// <summary>
        /// Removes a contract from the network via Administrative Delete
        /// </summary>
        /// <param name="contractToDelete">
        /// The address of the contract to delete.
        /// </param>
        /// <param name="signatory">
        /// Typically private key, keys or signing callback method that 
        /// are needed to delete the contract as per the requirements in the
        /// associated Endorsement.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating success of the contract deletion.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> SytemDeleteContractAsync(Address contractToDelete, Signatory signatory, Action<IContext>? configure = null)
        {
            return SystemDeleteContractImplementationAsync<TransactionReceipt>(contractToDelete, signatory, configure);
        }
        /// <summary>
        /// Removes a contract from the network via Administrative Delete
        /// </summary>
        /// <param name="contractToDelete">
        /// The address of the contract to delete.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating success of the contract deletion,
        /// fees & other transaction details.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionRecord> SystemDeleteContractWithRecordAsync(Address contractToDelete, Action<IContext>? configure = null)
        {
            return SystemDeleteContractImplementationAsync<TransactionRecord>(contractToDelete, null, configure);
        }
        /// <summary>
        /// Removes a contract from the network via Administrative Delete
        /// </summary>
        /// <param name="contractToDelete">
        /// The address of the contract to delete.
        /// </param>
        /// <param name="signatory">
        /// Typically private key, keys or signing callback method that 
        /// are needed to delete the contract as per the requirements in the
        /// associated Endorsement.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating success of the contract deletion,
        /// fees & other transaction details.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionRecord> SystemDeleteContractWithRecordAsync(Address contractToDelete, Signatory signatory, Action<IContext>? configure = null)
        {
            return SystemDeleteContractImplementationAsync<TransactionRecord>(contractToDelete, signatory, configure);
        }
        /// <summary>
        /// Internal helper function implementing the contract delete functionality.
        /// </summary>
        public async Task<TResult> SystemDeleteContractImplementationAsync<TResult>(Address contractToDelete, Signatory? signatory, Action<IContext>? configure = null) where TResult : new()
        {
            contractToDelete = RequireInputParameter.ContractToDelete(contractToDelete);
            await using var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatories = Transactions.GatherSignatories(context, signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId);
            transactionBody.SystemDelete = new SystemDeleteTransactionBody
            {
                ContractID = new ContractID(contractToDelete)
            };
            var precheck = await Transactions.SignAndSubmitTransactionWithRetryAsync(transactionBody, signatories, context, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to delete contract, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is TransactionRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                record.FillProperties(rec);
            }
            else if (result is TransactionReceipt rcpt)
            {
                receipt.FillProperties(transactionId, rcpt);
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new SmartContractService.SmartContractServiceClient(channel);
                return async (Transaction transaction) => await client.systemDeleteAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
