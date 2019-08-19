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
        /// Retrieves the network address associated with the specified smart contract id.
        /// </summary>
        /// <param name="smartContractId">
        /// The smart contract ID to look up.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// The network address associated with the smart contract ID.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        public async Task<Address> GetAddressFromSmartContractIdAsync(string smartContractId, Action<IContext>? configure = null)
        {
            smartContractId = RequireInputParameter.SmartContractId(smartContractId);
            var context = CreateChildContext(configure);
            var query = new Query
            {
                GetBySolidityID = new GetBySolidityIDQuery
                {
                    Header = Transactions.CreateAndSignQueryHeader(context, QueryFees.GetAddressFromSmartContractId, "Get Contract By Solidity ID", out var transactionId),
                    SolidityID = smartContractId
                }
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, query, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, getResponseCode(response));
            var data = response.GetBySolidityID;
            if (data.ContractID != null)
            {
                return Protobuf.FromContractID(data.ContractID);
            }
            if (data.AccountID != null)
            {
                return Protobuf.FromAccountID(data.AccountID);
            }
            if (data.FileID != null)
            {
                return Protobuf.FromFileID(data.FileID);
            }
            throw new TransactionException($"Address from Smart Contract ID {smartContractId} was not found.", Protobuf.FromTransactionId(transactionId), ResponseCode.Unknown);

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new SmartContractService.SmartContractServiceClient(channel);
                return async (Query query) => (await client.getBySolidityIDAsync(query));
            }

            static ResponseCodeEnum getResponseCode(Response response)
            {
                return response.GetBySolidityID?.Header?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
            }
        }
    }
}
