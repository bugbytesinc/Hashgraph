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
        private async Task<Proto.TransactionReceipt> GetReceiptAsync(ContextStack context, TransactionID transactionId)
        {
            var query = new Query
            {
                TransactionGetReceipt = new TransactionGetReceiptQuery
                {
                    TransactionID = transactionId
                }
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, query, getServerMethod, shouldRetry);
            var responseCode = response.TransactionGetReceipt.Header.NodeTransactionPrecheckCode;
            switch (responseCode)
            {
                case ResponseCodeEnum.Ok:
                    break;
                case ResponseCodeEnum.Busy:
                    throw new ConsensusException("Network failed to respond to request for a transaction receipt, it is too busy. It is possible the network may still reach concensus for this transaction.", Protobuf.FromTransactionId(transactionId), (ResponseCode)responseCode);
                case ResponseCodeEnum.Unknown:
                    throw new TransactionException($"Network failed return a transaction receipt, Status Code Returned: {response.TransactionGetReceipt.Header.NodeTransactionPrecheckCode}", Protobuf.FromTransactionId(transactionId), (ResponseCode)responseCode);
            }
            var status = response.TransactionGetReceipt.Receipt.Status;
            switch (status)
            {
                case ResponseCodeEnum.Unknown:
                    throw new ConsensusException("Network failed to reach concensus within the configured retry time window, It is possible the network may still reach concensus for this transaction.", Protobuf.FromTransactionId(transactionId), (ResponseCode)status);
                case ResponseCodeEnum.TransactionExpired:
                    throw new ConsensusException("Network failed to reach concensus before transaction request expired.", Protobuf.FromTransactionId(transactionId), (ResponseCode)status);
                case ResponseCodeEnum.RecordNotFound:
                    throw new ConsensusException("Network failed to find a receipt for given transaction.", Protobuf.FromTransactionId(transactionId), (ResponseCode)status);
                default:
                    return response.TransactionGetReceipt.Receipt;
            }

            static Func<Query, Task<Response>> getServerMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => (await client.getTransactionReceiptsAsync(query));
            }

            static bool shouldRetry(Response response)
            {
                return
                    response.TransactionGetReceipt?.Header?.NodeTransactionPrecheckCode == ResponseCodeEnum.Busy ||
                    response.TransactionGetReceipt?.Receipt?.Status == ResponseCodeEnum.Unknown;
            }
        }
    }
}
