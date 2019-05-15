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
        public async Task<TransactionRecord> UpdateFileAsync(UpdateFileParams updateParameters, Action<IContext>? configure = null)
        {
            updateParameters = RequireInputParameter.UpdateParameters(updateParameters);
            var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var updateFileBody = new FileUpdateTransactionBody
            {
                FileID = Protobuf.ToFileId(updateParameters.File)
            };
            if (!(updateParameters.Endorsements is null))
            {
                updateFileBody.Keys = Protobuf.ToPublicKeyList(updateParameters.Endorsements);
            }
            if (updateParameters.Expiration.HasValue)
            {
                updateFileBody.ExpirationTime = Protobuf.ToTimestamp(updateParameters.Expiration.Value);
            }
            if (updateParameters.Contents.HasValue)
            {
                updateFileBody.Contents = ByteString.CopyFrom(updateParameters.Contents.Value.ToArray());
            }
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateEmptyTransactionBody(context, transactionId, "Update File");
            transactionBody.FileUpdate = updateFileBody;
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
                throw new TransactionException($"Unable to update file, status: {record.Receipt.Status}", Protobuf.FromTransactionRecord<TransactionRecord>(record, transactionId));
            }
            return Protobuf.FromTransactionRecord<FileTransactionRecord>(record, transactionId);

            static Func<Transaction, Task<TransactionResponse>> getServerMethod(Channel channel)
            {
                var client = new FileService.FileServiceClient(channel);
                return async (Transaction transaction) => await client.updateFileAsync(transaction);
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
