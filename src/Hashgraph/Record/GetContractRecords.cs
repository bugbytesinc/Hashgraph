#pragma warning disable CS0612
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
                    ContractID = new ContractID(contract)
                }
            };
            var response = await query.SignAndExecuteWithRetryAsync(context);
            return response.ContractGetRecordsResponse.Records.Select(record => record.ToTransactionRecord()).ToArray();
        }
    }
}
