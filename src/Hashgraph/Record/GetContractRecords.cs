#pragma warning disable CS0612
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
        /// DEPRICATED: THIS METHOD WILL BE REMOVED WHEN NETWORK RETURNS NOT-IMPLEMENTED
        /// Retrieves the records associated with an contract that are presently
        /// held within the network.
        /// </summary>
        /// <param name="contract">
        /// The Hedera Network Contract Address to retrieve associated records.
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
        internal async Task<TransactionRecord[]> GetContractRecordsAsync(Address contract, Action<IContext>? configure = null)
        {
            contract = RequireInputParameter.Contract(contract);
            await using var context = CreateChildContext(configure);
            var query = new Query
            {
                ContractGetRecords = new ContractGetRecordsQuery
                {
                    Header = Transactions.CreateAskCostHeader(),
                    ContractID = new ContractID(contract)
                }
            };
            var response = await Transactions.ExecuteUnsignedAskRequestWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
            long cost = (long)response.ContractGetRecordsResponse.Header.Cost;
            if (cost > 0)
            {
                var transactionId = Transactions.GetOrCreateTransactionID(context);
                query.QueryHeader = await Transactions.CreateAndSignQueryHeaderAsync(context, cost, transactionId);
                response = await Transactions.ExecuteSignedQueryWithRetryAsync(context, query, getRequestMethod, getResponseHeader);
                ValidateResult.ResponseHeader(transactionId, getResponseHeader(response));
            }
            return response.ContractGetRecordsResponse.Records.Select(record => record.ToTransactionRecord()).ToArray();

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new SmartContractService.SmartContractServiceClient(channel);
                return async (Query query) => (await client.getTxRecordByContractIDAsync(query));
            }

            static ResponseHeader? getResponseHeader(Response response)
            {
                return response.ContractGetRecordsResponse?.Header;
            }
        }
    }
}
