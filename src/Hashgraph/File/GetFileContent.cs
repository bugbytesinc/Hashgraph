using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        public async Task<ReadOnlyMemory<byte>> GetFileContentAsync(Address file, Action<IContext>? configure = null)
        {
            file = RequireInputParameter.File(file);
            var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transfers = Transactions.CreateCryptoTransferList((payer, -context.FeeLimit), (gateway, context.FeeLimit));
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateCryptoTransferTransactionBody(context, transfers, transactionId, "Get File Contents");
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, payer);
            var query = new Query
            {
                FileGetContents = new FileGetContentsQuery
                {
                    Header = Transactions.CreateProtoQueryHeader(transactionBody, signatures),
                    FileID = Protobuf.ToFileId(file)                    
                }
            };
            var data = await Transactions.ExecuteRequestWithRetryAsync(context, query, getServerMethod, shouldRetry);
            ValidateResult.PreCheck(transactionId, data.Header.NodeTransactionPrecheckCode);
            return new ReadOnlyMemory<byte>(data.FileContents.Contents.ToByteArray());

            static Func<Query, Task<FileGetContentsResponse>> getServerMethod(Channel channel)
            {
                var client = new FileService.FileServiceClient(channel);
                return async (Query query) => (await client.getFileContentAsync(query)).FileGetContents;
            }

            static bool shouldRetry(FileGetContentsResponse response)
            {
                var code = response.Header.NodeTransactionPrecheckCode;
                return
                    code == ResponseCodeEnum.Busy ||
                    code == ResponseCodeEnum.InvalidTransactionStart;
            }
        }
    }
}
