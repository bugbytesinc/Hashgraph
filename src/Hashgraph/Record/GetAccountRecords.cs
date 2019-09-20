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
        /// held within the network.
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
            var response = await Transactions.ExecuteUnsignedAskRequestWithRetryAsync(context, query, getRequestMethod, getResponseCode);
            long cost = (long)response.CryptoGetAccountRecords.Header.Cost;
            if (cost > 0)
            {
                query.CryptoGetAccountRecords.Header = Transactions.CreateAndSignQueryHeader(context, cost, "Get Account Records", out var transactionId);
                response = await Transactions.ExecuteSignedRequestWithRetryAsync(context, query, getRequestMethod, getResponseCode);
                ValidateResult.PreCheck(transactionId, getResponseCode(response));
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

            static ResponseCodeEnum getResponseCode(Response response)
            {
                return response.CryptoGetAccountRecords?.Header?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
            }
        }
    }
}
