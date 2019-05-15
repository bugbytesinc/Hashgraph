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
        public async Task<FileTransactionRecord> CreateFileAsync(CreateFileParams createParameters, Action<IContext>? configure = null)
        {
            createParameters = RequireInputParameter.CreateParameters(createParameters);
            var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateEmptyTransactionBody(context, transactionId, "Create File");
            transactionBody.FileCreate = new FileCreateTransactionBody
            {
                ExpirationTime = Protobuf.ToTimestamp(createParameters.Expiration),
                Keys = Protobuf.ToPublicKeyList(createParameters.Endorsements),
                Contents = ByteString.CopyFrom(createParameters.Contents.ToArray()),
                ShardID = Protobuf.ToShardID(gateway.ShardNum),
                RealmID = Protobuf.ToRealmID(gateway.RealmNum, gateway.ShardNum)
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
                throw new TransactionException($"Unable to create file, status: {record.Receipt.Status}", Protobuf.FromTransactionRecord<TransactionRecord>(record, transactionId));
            }
            var result = Protobuf.FromTransactionRecord<FileTransactionRecord>(record, transactionId);
            result.File = Protobuf.FromFileID(record.Receipt.FileID);
            return result;

            static Func<Transaction, Task<TransactionResponse>> getServerMethod(Channel channel)
            {
                var client = new FileService.FileServiceClient(channel);
                return async (Transaction transaction) => await client.createFileAsync(transaction);
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
