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
        /// Retrieves the transaction record for a given transaction ID.
        /// </summary>
        /// <param name="transaction">
        /// Transaction identifier of the record
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record with the specified id, or an exception if not found.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="TransactionException">If the network has no record of the transaction or request has invalid or had missing data.</exception>
        public async Task<TransactionRecord> GetTransactionRecordAsync(TxId transaction, Action<IContext>? configure = null)
        {
            transaction = RequireInputParameter.Transaction(transaction);
            var context = CreateChildContext(configure);
            var transactionId = Protobuf.ToTransactionID(transaction);
            // For the public version of this method, we do not know
            // if the transaction in question has come to consensus so
            // we need to get the receipt first (and wait if necessary).
            var query = new Query
            {
                TransactionGetReceipt = new TransactionGetReceiptQuery
                {
                    TransactionID = transactionId
                }
            };
            await Transactions.ExecuteNetworkRequestWithRetryAsync(context, query, getServerMethod, shouldRetry);
            // The Receipt status returned does notmatter in this case.  
            // We may be retrieving a failed record (the status would not equal OK).
            var record = await GetTransactionRecordAsync(context, transactionId);
            var result = new TransactionRecord();
            Protobuf.FillRecordProperties(record, result);
            return result;

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
        /// <summary>
        /// Internal Helper function to retrieve the transaction record provided 
        /// by the network following network consensus regarding a query or transaction.
        /// </summary>
        private async Task<Proto.TransactionRecord> GetTransactionRecordAsync(ContextStack context, TransactionID transactionRecordId)
        {
            var query = new Query
            {
                TransactionGetRecord = new TransactionGetRecordQuery
                {
                    Header = Transactions.CreateAskCostHeader(),
                    TransactionID = transactionRecordId
                }
            };
            var response = await Transactions.ExecuteUnsignedAskRequestWithRetryAsync(context, query, getRequestMethod, getResponseCode);
            long cost = (long)response.TransactionGetRecord.Header.Cost;
            if (cost > 0)
            {
                query.TransactionGetRecord.Header = Transactions.CreateAndSignQueryHeader(context, cost, "Get Transaction Record", out var transactionId);
                response = await Transactions.ExecuteSignedRequestWithRetryAsync(context, query, getRequestMethod, getResponseCode);
                var responseCode = getResponseCode(response);
                if (responseCode != ResponseCodeEnum.Ok)
                {
                    throw new TransactionException("Unable to retrieve transaction record.", Protobuf.FromTransactionId(transactionRecordId), (ResponseCode)responseCode);
                }
            }
            return response.TransactionGetRecord.TransactionRecord;

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => { return await client.getTxRecordByTxIDAsync(query); };
            }

            static ResponseCodeEnum getResponseCode(Response response)
            {
                return response.TransactionGetRecord?.Header?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
            }
        }
    }
}
