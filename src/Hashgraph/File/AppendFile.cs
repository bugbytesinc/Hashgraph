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
        public async Task<TransactionRecord> AppendFileAsync(AppendFileParams appendParameters, Action<IContext>? configure = null)
        {
            appendParameters = RequireInputParameter.AppendParameters(appendParameters);
            var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var appendFileBody = new FileAppendTransactionBody
            {
                FileID = Protobuf.ToFileId(appendParameters.File),
                Contents = ByteString.CopyFrom(appendParameters.Contents.ToArray())
            };
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateEmptyTransactionBody(context, transactionId, "Append File Content");
            transactionBody.FileAppend = appendFileBody;
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
                throw new TransactionException($"Unable to append to file, status: {record.Receipt.Status}", Protobuf.FromTransactionRecord<TransactionRecord>(record, transactionId));
            }
            return Protobuf.FromTransactionRecord<TransactionRecord>(record, transactionId);

            static Func<Transaction, Task<TransactionResponse>> getServerMethod(Channel channel)
            {
                var client = new FileService.FileServiceClient(channel);
                return async (Transaction transaction) => await client.appendContentAsync(transaction);
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
