﻿using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Deletes a contract instance from the network returning the remaining 
        /// crypto balance to the specified address.  Must be signed 
        /// by the admin key.
        /// </summary>
        /// <param name="contractToDelete">
        /// The Contract instance that will be deleted.
        /// </param>
        /// <param name="transferToAddress">
        /// The address that will receive any remaining balance from the deleted Contract.
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
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        ///
        /// <remarks>Marked Internal Because functionality removed from testnet</remarks>
        internal async Task<TransactionReceipt> DeleteContractAsync(Address contractToDelete, Address transferToAddress, Action<IContext>? configure = null)
        {
            contractToDelete = RequireInputParameter.ContractToDelete(contractToDelete);
            transferToAddress = RequireInputParameter.TransferToAddress(transferToAddress);
            var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId, "Delete Contract");
            transactionBody.ContractDeleteInstance = new ContractDeleteTransactionBody
            {
                ContractID = Protobuf.ToContractID(contractToDelete),
                TransferAccountID = Protobuf.ToAccountID(transferToAddress)
            };
            var request = Transactions.SignTransaction(transactionBody, payer);
            var precheck = await Transactions.ExecuteRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck.NodeTransactionPrecheckCode);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to delete contract, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
            }
            var result = new TransactionReceipt();
            Protobuf.FillReceiptProperties(transactionId, receipt, result);
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new SmartContractService.SmartContractServiceClient(channel);
                return async (Transaction transaction) => await client.deleteContractAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
