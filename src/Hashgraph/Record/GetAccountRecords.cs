using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Retrieves the account records associated with an account that are presently
        /// held within the network because they exceeded the receive or send threshold
        /// values for autogeneration of records.
        /// </summary>
        /// <param name="address">
        /// The Hedera Network Address to retrieve associated records.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A detailed description of the account.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        public async Task<TransactionRecord[]> GetAccountRecordsAsync(Address address, Action<IContext>? configure = null)
        {
            address = RequireInputParameter.Address(address);
            var context = CreateChildContext(configure);
            var query = new Query
            {
                CryptoGetAccountRecords = new CryptoGetAccountRecordsQuery
                {
                    Header = Transactions.CreateAskCostHeader(),
                    AccountID = Protobuf.ToAccountID(address)
                }
            };
            var response = await Transactions.ExecuteUnsignedAskRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
            long cost = (long)response.CryptoGetAccountRecords.Header.Cost;
            if (cost > 0)
            {
                var transactionId = Transactions.GetOrCreateTransactionID(context);
                query.CryptoGetAccountRecords.Header = await Transactions.CreateAndSignQueryHeaderAsync(context, cost, "Get Account Records", transactionId);
                response = await Transactions.ExecuteSignedRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
                var precheckCode = getResponseHeader(response)?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
                if (precheckCode != ResponseCodeEnum.Ok)                
                {
                    throw new TransactionException("Unable to retrieve transaction records.", Protobuf.FromTransactionId(transactionId), (ResponseCode) precheckCode );
                }
            }
            return response.CryptoGetAccountRecords.Records.Select(record =>
            {
                var result = new TransactionRecord();
                Protobuf.FillRecordProperties(record, result);
                return result;
            }).ToArray();

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => (await client.getAccountRecordsAsync(query));
            }

            static ResponseHeader? getResponseHeader(Response response)
            {
                return response.CryptoGetAccountRecords?.Header;
            }
        }
    }
}
