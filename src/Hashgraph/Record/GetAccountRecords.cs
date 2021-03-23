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
            var response = await ExecuteQueryAsync(new CryptoGetAccountRecordsQuery(address), configure);
            return TransactionRecordExtensions.Create(response.CryptoGetAccountRecords.Records, null).ToArray();
        }
    }
}
