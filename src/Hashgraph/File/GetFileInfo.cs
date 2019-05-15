using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        public async Task<FileInfo> GetFileInfoAsync(Address file, Action<IContext>? configure = null)
        {
            file = RequireInputParameter.File(file);
            var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transfers = Transactions.CreateCryptoTransferList((payer, -context.FeeLimit), (gateway, context.FeeLimit));
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateCryptoTransferTransactionBody(context, transfers, transactionId, "Get File Info");
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, payer);
            var query = new Query
            {
                FileGetInfo = new FileGetInfoQuery
                {
                    Header = Transactions.CreateProtoQueryHeader(transactionBody, signatures),
                    FileID = Protobuf.ToFileId(file)                    
                }
            };
            var data = await Transactions.ExecuteRequestWithRetryAsync(context, query, getServerMethod, shouldRetry);
            ValidateResult.PreCheck(transactionId, data.Header.NodeTransactionPrecheckCode);
            return Protobuf.FromFileInfo(data.FileInfo);

            static Func<Query, Task<FileGetInfoResponse>> getServerMethod(Channel channel)
            {
                var client = new FileService.FileServiceClient(channel);
                return async (Query query) => (await client.getFileInfoAsync(query)).FileGetInfo;
            }

            static bool shouldRetry(FileGetInfoResponse response)
            {
                var code = response.Header.NodeTransactionPrecheckCode;
                return
                    code == ResponseCodeEnum.Busy ||
                    code == ResponseCodeEnum.InvalidTransactionStart;
            }
        }
    }
}
