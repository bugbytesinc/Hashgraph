using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        // Preliminary Implementation not to rely upon yet
        private async Task<TransactionReceipt> GetReceiptAsync(TransactionID transactionId, ContextStack context)
        {
            var query = new Query
            {
                TransactionGetReceipt = new TransactionGetReceiptQuery
                {
                    TransactionID = transactionId
                }
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, query, instantiateGetTransactionReceiptsAsyncMethod, checkForRetry);
            Validate.ValidatePreCheckResult(transactionId, response.Header.NodeTransactionPrecheckCode);
            return response.Receipt;

            static Func<Query, Task<TransactionGetReceiptResponse>> instantiateGetTransactionReceiptsAsyncMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => (await client.getTransactionReceiptsAsync(query)).TransactionGetReceipt;
            }

            static bool checkForRetry(TransactionGetReceiptResponse response)
            {
                return
                    response.Header.NodeTransactionPrecheckCode == ResponseCodeEnum.Busy ||
                    response.Receipt.Status == ResponseCodeEnum.Unknown;
            }
        }
    }
}
