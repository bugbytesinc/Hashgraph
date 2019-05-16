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
        /// Retrieves the details regarding a file stored on the network.
        /// </summary>
        /// <param name="file">
        /// Address of the file to query.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// The details of the network file, excluding content.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        public async Task<FileInfo> GetFileInfoAsync(Address file, Action<IContext>? configure = null)
        {
            file = RequireInputParameter.File(file);
            var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transfers = Transactions.CreateCryptoTransferList((payer, -context.FeeLimit), (gateway, context.FeeLimit));
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateCryptoTransferTransactionBody(context, transfers, transactionId, "Get File Info");
            var query = new Query
            {
                FileGetInfo = new FileGetInfoQuery
                {
                    Header = Transactions.SignQueryHeader(transactionBody, payer),
                    FileID = Protobuf.ToFileId(file)
                }
            };
            var data = await Transactions.ExecuteRequestWithRetryAsync(context, query, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, data.FileGetInfo.Header.NodeTransactionPrecheckCode);
            return Protobuf.FromFileInfo(data.FileGetInfo.FileInfo);

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new FileService.FileServiceClient(channel);
                return async (Query query) => (await client.getFileInfoAsync(query));
            }

            static ResponseCodeEnum getResponseCode(Response response)
            {
                return response.FileGetInfo?.Header?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
            }
        }
    }
}
