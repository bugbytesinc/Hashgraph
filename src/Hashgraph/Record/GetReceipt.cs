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
        /// Internal Helper function to retrieve receipt record provided by 
        /// the network following network consensus regarding a query or transaction.
        /// </summary>
        private async Task<TransactionReceipt> GetReceiptAsync(TransactionID transactionId, ContextStack context)
        {
            var query = new Query
            {
                TransactionGetReceipt = new TransactionGetReceiptQuery
                {
                    TransactionID = transactionId
                }
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, query, getServerMethod, shouldRetry);
            ValidateResult.PreCheck(transactionId, response.Header.NodeTransactionPrecheckCode);
            return response.Receipt;

            static Func<Query, Task<TransactionGetReceiptResponse>> getServerMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => (await client.getTransactionReceiptsAsync(query)).TransactionGetReceipt;
            }

            static bool shouldRetry(TransactionGetReceiptResponse response)
            {
                return
                    response.Header.NodeTransactionPrecheckCode == ResponseCodeEnum.Busy ||
                    response.Receipt.Status == ResponseCodeEnum.Unknown;
            }
        }
    }
}
