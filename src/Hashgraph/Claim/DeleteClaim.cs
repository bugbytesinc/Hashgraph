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
        public Task<TransactionReceipt> DeleteClaimAsync(Address address, ReadOnlyMemory<byte> hash, Action<IContext>? configure = null)
        {
            return DeleteClaimImplementationAsync<TransactionReceipt>(address, hash, configure);
        }
        public Task<TransactionRecord> DeleteClaimWithRecordAsync(Address address, ReadOnlyMemory<byte> hash, Action<IContext>? configure = null)
        {
            return DeleteClaimImplementationAsync<TransactionRecord>(address, hash);
        }
        public async Task<TResult> DeleteClaimImplementationAsync<TResult>(Address address, ReadOnlyMemory<byte> hash, Action<IContext>? configure = null) where TResult : new()
        {
            address = RequireInputParameter.Address(address);
            hash = RequireInputParameter.Hash(hash);
            var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateEmptyTransactionBody(context, transactionId, "Delete Claim");
            transactionBody.CryptoDeleteClaim = new CryptoDeleteClaimTransactionBody
            {
                AccountIDToDeleteFrom = Protobuf.ToAccountID(address),
                HashToDelete = ByteString.CopyFrom(hash.ToArray())
            };
            var request = Transactions.SignTransaction(transactionBody, payer);
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, response.NodeTransactionPrecheckCode);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to remove Claim, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is TransactionReceipt rcpt)
            {
                Protobuf.FillReceiptProperties(transactionId, receipt, rcpt);
            }
            else if (result is TransactionRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                Protobuf.FillRecordProperties(transactionId, receipt, record, rec);
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
