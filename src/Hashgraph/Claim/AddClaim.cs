
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
        public Task<TransactionReceipt> AddClaimAsync(Claim claim, Action<IContext>? configure = null)
        {
            return AddClaimImplementationAsync<TransactionReceipt>(claim, configure);
        }
        public Task<TransactionRecord> AddClaimWithRecordAsync(Claim claim, Action<IContext>? configure = null)
        {
            return AddClaimImplementationAsync<TransactionRecord>(claim, configure);
        }
        public async Task<TResult> AddClaimImplementationAsync<TResult>(Claim claim, Action<IContext>? configure) where TResult : new()
        {
            claim = RequireInputParameter.AddParameters(claim);
            var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateEmptyTransactionBody(context, transactionId, "Add Claim");
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
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, response.NodeTransactionPrecheckCode);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to attach claim, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is TransactionReceipt rcpt)
            {
                Protobuf.FillReceiptProperties(transactionId, receipt, rcpt);
            }
            if (result is TransactionRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                Protobuf.FillRecordProperties(transactionId, receipt, record, rec);
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
