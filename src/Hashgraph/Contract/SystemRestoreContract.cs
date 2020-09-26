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
        /// Restores a contract to the network via Administrative Restore
        /// </summary>
        /// <param name="contractToRestore">
        /// The address of the contract to restore.
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
        public Task<TransactionReceipt> SystemRestoreContractAsync(Address contractToRestore, Action<IContext>? configure = null)
        {
            return SystemRestoreContractImplementationAsync<TransactionReceipt>(contractToRestore, null, configure);
        }
        /// <summary>
        /// Restores a contract to the network via Administrative Restore
        /// </summary>
        /// <param name="contractToRestore">
        /// The address of the contract to restore.
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
        public Task<TransactionReceipt> SytemRestoreContractAsync(Address contractToRestore, Signatory signatory, Action<IContext>? configure = null)
        {
            return SystemRestoreContractImplementationAsync<TransactionReceipt>(contractToRestore, signatory, configure);
        }
        /// <summary>
        /// Restores a contract to the network via Administrative Restore
        /// </summary>
        /// <param name="contractToRestore">
        /// The address of the contract to restore.
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
        public Task<TransactionRecord> SystemRestoreContractWithRecordAsync(Address contractToRestore, Action<IContext>? configure = null)
        {
            return SystemRestoreContractImplementationAsync<TransactionRecord>(contractToRestore, null, configure);
        }
        /// <summary>
        /// Restores a contract to the network via Administrative Restore
        /// </summary>
        /// <param name="contractToRestore">
        /// The address of the contract to restore.
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
        public Task<TransactionRecord> SystemRestoreContractWithRecordAsync(Address contractToRestore, Signatory signatory, Action<IContext>? configure = null)
        {
            return SystemRestoreContractImplementationAsync<TransactionRecord>(contractToRestore, signatory, configure);
        }
        /// <summary>
        /// Internal helper function implementing the contract delete functionality.
        /// </summary>
        public async Task<TResult> SystemRestoreContractImplementationAsync<TResult>(Address contractToRestore, Signatory? signatory, Action<IContext>? configure = null) where TResult : new()
        {
            contractToRestore = RequireInputParameter.ContractToRestore(contractToRestore);
            await using var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatories = Transactions.GatherSignatories(context, signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId);
            transactionBody.SystemUndelete = new SystemUndeleteTransactionBody
            {
                ContractID = new ContractID(contractToRestore)
            };
            var request = await Transactions.SignTransactionAsync(transactionBody, signatories);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to restore contract, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
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
                return async (Transaction transaction) => await client.systemUndeleteAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
