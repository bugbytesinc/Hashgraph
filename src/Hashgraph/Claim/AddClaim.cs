
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
        /// Adds a claim to the given account.
        /// </summary>
        /// <param name="claim">
        /// The details of the claim attachment request.
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
        internal Task<TransactionReceipt> AddClaimAsync(Claim claim, Action<IContext>? configure = null)
        {
            return AddClaimImplementationAsync<TransactionReceipt>(claim, configure);
        }
        /// <summary>
        /// Adds a claim to the given account, returning a transaction record.
        /// </summary>
        /// <param name="claim">
        /// The details of the claim attachment request.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating success and information on fees.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        /// 
        /// <remarks>Marked Internal Because functionality removed from testnet</remarks>
        internal Task<TransactionRecord> AddClaimWithRecordAsync(Claim claim, Action<IContext>? configure = null)
        {
            return AddClaimImplementationAsync<TransactionRecord>(claim, configure);
        }
        /// <summary>
        /// Internal implementation of the Add Claim service.
        /// </summary>
        private async Task<TResult> AddClaimImplementationAsync<TResult>(Claim claim, Action<IContext>? configure) where TResult : new()
        {
            claim = RequireInputParameter.AddParameters(claim);
            var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId, "Add Claim");
            transactionBody.CryptoAddClaim = new CryptoAddClaimTransactionBody
            {
                Claim = new Proto.Claim
                {
                    AccountID = Protobuf.ToAccountID(claim.Address),
                    Hash = ByteString.CopyFrom(claim.Hash.ToArray()),
                    Keys = Protobuf.ToPublicKeyList(claim.Endorsements),
                    ClaimDuration = Protobuf.ToDuration(claim.ClaimDuration)
                }
            };
            var request = Transactions.SignTransaction(transactionBody, payer);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck.NodeTransactionPrecheckCode);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to attach claim, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
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
                var client = new FileService.FileServiceClient(channel);
                return async (Transaction transaction) => await client.createFileAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
