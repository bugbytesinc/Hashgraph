using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Retrieves the transaction record for a given transaction ID that was
        /// successfully processed, otherwise the first one to reach consensus.
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
        /// <remarks>
        /// Generally there is only one record per transaction, but in certain cases
        /// where there is a transaction ID collision (deliberate or accidental) there
        /// may be more, the <see cref="GetAllTransactionRecordsAsync(TxId, Action{IContext}?)"/>
        /// method may be used to retreive all records.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="TransactionException">If the network has no record of the transaction or request has invalid or had missing data.</exception>
        public async Task<TransactionRecord> GetTransactionRecordAsync(TxId transaction, Action<IContext>? configure = null)
        {
            transaction = RequireInputParameter.Transaction(transaction);
            await using var context = CreateChildContext(configure);
            var transactionId = new TransactionID(transaction);
            // For the public version of this method, we do not know
            // if the transaction in question has come to consensus so
            // we need to get the receipt first (and wait if necessary).
            // The Receipt status returned does notmatter in this case.  
            // We may be retrieving a failed record (the status would not equal OK).
            await WaitForConsensusReceipt(context, transactionId);
            var record = await GetTransactionRecordAsync(context, transactionId);
            return record.ToTransactionRecord();

        }
        /// <summary>
        /// Retrieves all records having the given transaction ID, including duplicates
        /// that were rejected or produced errors during execution.  Typically there is
        /// only one record per transaction, but in some cases, deliberate or accidental
        /// there may be more than one for a given transaction ID.
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
        /// An collection of all the transaction records known to the system
        /// at the time of query having the identified transaction id.
        /// </returns>
        public async Task<ReadOnlyCollection<TransactionRecord>> GetAllTransactionRecordsAsync(TxId transaction, Action<IContext>? configure = null)
        {
            transaction = RequireInputParameter.Transaction(transaction);
            await using var context = CreateChildContext(configure);
            var transactionId = new TransactionID(transaction);
            // For the public version of this method, we do not know
            // if the transaction in question has come to consensus so
            // we need to get the receipt first (and wait if necessary).
            // The Receipt status returned does notmatter in this case.  
            // We may be retrieving a failed record (the status would not equal OK).
            await WaitForConsensusReceipt(context, transactionId);
            var record = await GetTransactionRecordResponseAsync(context, transactionId, true);
            return record.DuplicateTransactionRecords.ToTransactionRecordList(record.TransactionRecord);
        }
        /// <summary>
        /// Internal Helper function used to wait for conesnsus regardless of the reported
        /// transaction outcome. We do not know if the transaction in question has come 
        /// to consensus so we need to get the receipt first (and wait if necessary).
        /// The Receipt status returned does notmatter in this case.  
        /// We may be retrieving a failed record (the status would not equal OK).
        private async Task WaitForConsensusReceipt(GossipContextStack context, TransactionID transactionId)
        {
            var query = new Query
            {
                TransactionGetReceipt = new TransactionGetReceiptQuery
                {
                    TransactionID = transactionId
                }
            };
            await Transactions.ExecuteNetworkRequestWithRetryAsync(context, query, getServerMethod, shouldRetry);

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
        private async Task<Proto.TransactionRecord> GetTransactionRecordAsync(GossipContextStack context, TransactionID transactionRecordId)
        {
            return (await GetTransactionRecordResponseAsync(context, transactionRecordId, false)).TransactionRecord;
        }
        /// <summary>
        /// Internal Helper function to retrieve the transaction record or list of duplicate records provided 
        /// by the network following network consensus regarding a query or transaction.
        /// </summary>
        private async Task<Proto.TransactionGetRecordResponse> GetTransactionRecordResponseAsync(GossipContextStack context, TransactionID transactionRecordId, bool includeDuplicates)
        {
            var query = new Query
            {
                TransactionGetRecord = new TransactionGetRecordQuery
                {
                    Header = Transactions.CreateAskCostHeader(),
                    TransactionID = transactionRecordId,
                    IncludeDuplicates = includeDuplicates
                }
            };
            var response = await Transactions.ExecuteUnsignedAskRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
            long cost = (long)response.TransactionGetRecord.Header.Cost;
            if (cost > 0)
            {
                var transactionId = Transactions.GetOrCreateTransactionID(context);
                query.TransactionGetRecord.Header = await Transactions.CreateAndSignQueryHeaderAsync(context, cost, transactionId);
                response = await Transactions.ExecuteSignedRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
                var precheckCode = getResponseHeader(response)?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
                // Note if we are retrieving the list, Not found is OK too.
                if (precheckCode != ResponseCodeEnum.Ok && !(includeDuplicates && precheckCode == ResponseCodeEnum.RecordNotFound))
                {
                    throw new TransactionException("Unable to retrieve transaction record.", transactionRecordId.ToTxId(), (ResponseCode)precheckCode);
                }
            }
            return response.TransactionGetRecord;

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => { return await client.getTxRecordByTxIDAsync(query); };
            }

            static ResponseHeader? getResponseHeader(Response response)
            {
                return response.TransactionGetRecord?.Header;
            }
        }
    }
}
