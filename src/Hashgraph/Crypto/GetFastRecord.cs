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
        /// Internal Helper function to retrieve the “fast” transaction record provided 
        /// by the network following network consensus regarding a query or transaction.
        /// </summary>
        private async Task<Proto.TransactionRecord> GetFastRecordAsync(TransactionID transactionId, ContextStack context)
        {
            var query = new Query
            {
                TransactionGetFastRecord = new TransactionGetFastRecordQuery
                {
                    TransactionID = transactionId
                }
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, query, instantiateGetTransactionReceiptsAsyncMethod, checkForRetry);
            if (response.TransactionRecord is null)
            {
                var expiration = Protobuf.FromTimestamp(transactionId.TransactionValidStart).Add(context.TransactionDuration);
                if(expiration < DateTime.UtcNow)
                {
                    throw new ConsensusException("Network failed to reach concensus before transaction request exired.", Protobuf.FromTransactionId(transactionId), (ResponseCode)response.Header.NodeTransactionPrecheckCode);
                }
                throw new PrecheckException("Failed to receive response from server within the given retry interval.", Protobuf.FromTransactionId(transactionId), (ResponseCode)response.Header.NodeTransactionPrecheckCode);
            }
            switch(response.TransactionRecord.Receipt.Status)
            {
                case ResponseCodeEnum.TransactionExpired:
                    throw new ConsensusException("Network failed to reach concensus before transaction request exired.", Protobuf.FromTransactionId(transactionId), (ResponseCode)response.TransactionRecord.Receipt.Status);
                case ResponseCodeEnum.RecordNotFound:
                    throw new ConsensusException("Network failed to find a record for given transaction.", Protobuf.FromTransactionId(transactionId), (ResponseCode)response.TransactionRecord.Receipt.Status);
                default:
                    return response.TransactionRecord;
            }

            static Func<Query, Task<TransactionGetFastRecordResponse>> instantiateGetTransactionReceiptsAsyncMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => (await client.getFastTransactionRecordAsync(query)).TransactionGetFastRecord;
            }

            static bool checkForRetry(TransactionGetFastRecordResponse response)
            {
                return
                    response.Header.NodeTransactionPrecheckCode == ResponseCodeEnum.Busy ||
                    response.TransactionRecord?.Receipt?.Status == ResponseCodeEnum.Unknown;
            }
        }
    }
}
