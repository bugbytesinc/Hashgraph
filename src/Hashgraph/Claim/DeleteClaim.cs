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
        /// Removes the specified claim from the associated account.
        /// </summary>
        /// <param name="address">
        /// Address of the account having the claim to remove.
        /// </param>
        /// <param name="hash">
        /// The hash/id of the claim to remove.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating success.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        /// 
        /// <remarks>Marked Internal Because functionality removed from testnet</remarks>
        internal Task<TransactionReceipt> DeleteClaimAsync(Address address, ReadOnlyMemory<byte> hash, Action<IContext>? configure = null)
        {
            return DeleteClaimImplementationAsync<TransactionReceipt>(address, hash, configure);
        }
        /// <summary>
        /// Removes the specified claim from the associated account.
        /// </summary>
        /// <param name="address">
        /// Address of the account having the claim to remove.
        /// </param>
        /// <param name="hash">
        /// The hash/id of the claim to remove.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating success and transaction details.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        /// 
        /// <remarks>Marked Internal Because functionality removed from testnet</remarks>
        internal Task<TransactionRecord> DeleteClaimWithRecordAsync(Address address, ReadOnlyMemory<byte> hash, Action<IContext>? configure = null)
        {
            return DeleteClaimImplementationAsync<TransactionRecord>(address, hash);
        }
        /// <summary>
        /// Internal implementation of the Delete Claim Methods
        /// </summary>
        private async Task<TResult> DeleteClaimImplementationAsync<TResult>(Address address, ReadOnlyMemory<byte> hash, Action<IContext>? configure = null) where TResult : new()
        {
            address = RequireInputParameter.Address(address);
            hash = RequireInputParameter.Hash(hash);
            var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatory = Transactions.GatherSignatories(context, new Signatory(payer));
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId, "Delete Claim");
            transactionBody.CryptoDeleteClaim = new CryptoDeleteClaimTransactionBody
            {
                AccountIDToDeleteFrom = Protobuf.ToAccountID(address),
                HashToDelete = ByteString.CopyFrom(hash.ToArray())
            };
            var request = await Transactions.SignTransactionAsync(transactionBody, signatory);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck.NodeTransactionPrecheckCode);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to remove Claim, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is TransactionRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                Protobuf.FillRecordProperties(record, rec);
            }
            else if (result is TransactionReceipt rcpt)
            {
                Protobuf.FillReceiptProperties(transactionId, receipt, rcpt);
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Transaction transaction) => await client.deleteClaimAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
