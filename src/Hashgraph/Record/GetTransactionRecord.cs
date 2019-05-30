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
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> GetTransactionRecordAsync(TxId transaction, Action<IContext>? configure = null)
        {
            transaction = RequireInputParameter.Transaction(transaction);
            var context = CreateChildContext(configure);
            var transactionId = Protobuf.ToTransactionID(transaction);
            var record = await GetTransactionRecordAsync(context, transactionId);
            var result = new TransactionRecord();
            Protobuf.FillRecordProperties(transactionId, record, result);
            return result;
        }
        /// <summary>
        /// Internal Helper function to retrieve the transaction record provided 
        /// by the network following network consensus regarding a query or transaction.
        /// </summary>
        private async Task<Proto.TransactionRecord> GetTransactionRecordAsync(ContextStack context, TransactionID transactionRecordId)
        {
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transfers = Transactions.CreateCryptoTransferList((payer, -context.FeeLimit), (gateway, context.FeeLimit));
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateCryptoTransferTransactionBody(context, transfers, transactionId, "Get Transaction Record");
            var query = new Query
            {
                TransactionGetRecord = new TransactionGetRecordQuery
                {
                    Header = Transactions.SignQueryHeader(transactionBody, payer),
                    TransactionID = transactionRecordId
                }
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, query, getServerMethod, getResponseCode);
            var responseCode = getResponseCode(response);
            if(responseCode != ResponseCodeEnum.Ok)
            {
                throw new TransactionException("Unable to retrieve transaction record.", Protobuf.FromTransactionId(transactionRecordId), (ResponseCode) responseCode);
            }
            ValidateResult.PreCheck(transactionId, getResponseCode(response));
            return response.TransactionGetRecord.TransactionRecord;

            static Func<Query, Task<Response>> getServerMethod(Channel channel)
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
