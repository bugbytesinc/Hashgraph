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
        /// Internal Helper function to retrieve the transaction record provided 
        /// by the network following network consensus regarding a query or transaction.
        /// </summary>
        private async Task<Proto.TransactionRecord> GetTransactionRecordAsync(ContextStack context, TransactionID transactionRecordId)
        {
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transfers = Transactions.CreateCryptoTransferList((payer, -context.FeeLimit), (gateway, context.FeeLimit));
            var queryTransactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateCryptoTransferTransactionBody(context, transfers, queryTransactionId, "Get Transaction Record");
            var query = new Query
            {
                TransactionGetRecord = new TransactionGetRecordQuery
                {
                    Header = Transactions.SignQueryHeader(transactionBody, payer),
                    TransactionID = transactionRecordId
                }
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, query, getServerMethod, getResponseCode);
            ValidateResult.PreCheck(queryTransactionId, getResponseCode(response));
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
