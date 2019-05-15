using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        public async Task<TransactionRecord> DeleteFileAsync(Address fileToDelete, Action<IContext>? configure = null)
        {
            fileToDelete = RequireInputParameter.FileToDelete(fileToDelete);
            var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateEmptyTransactionBody(context, transactionId, "Delete File");
            transactionBody.FileDelete = new FileDeleteTransactionBody
            {
                FileID = Protobuf.ToFileId(fileToDelete)
            };
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, payer);
            var request = new Transaction
            {
                Body = transactionBody,
                Sigs = signatures
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, request, getServerMethod, shouldRetry);
            ValidateResult.PreCheck(transactionId, response.NodeTransactionPrecheckCode);
            var record = await GetFastRecordAsync(transactionId, context);
            if (record.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to delete file, status: {record.Receipt.Status}", Protobuf.FromTransactionRecord<TransactionRecord>(record, transactionId));
            }
            return Protobuf.FromTransactionRecord<TransactionRecord>(record, transactionId);

            static Func<Transaction, Task<TransactionResponse>> getServerMethod(Channel channel)
            {
                var client = new FileService.FileServiceClient(channel);
                return async (Transaction transaction) => await client.deleteFileAsync(transaction);
            }

            static bool shouldRetry(TransactionResponse response)
            {
                var code = response.NodeTransactionPrecheckCode;
                return
                    code == ResponseCodeEnum.Busy ||
                    code == ResponseCodeEnum.InvalidTransactionStart;
            }
        }
    }
}
