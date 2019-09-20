using Google.Protobuf;
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
        /// Retrieves information regarding a claim attached to an account.
        /// </summary>
        /// <param name="address">
        /// Address of the account having the claim information to retrieve.
        /// </param>
        /// <param name="hash">
        /// The hash/id of the claim information to retrieve.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// The claim details, if found. Throws an exception if not.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        /// 
        /// <remarks>Marked Internal Because functionality removed from testnet</remarks>
        internal async Task<Claim> GetClaimAsync(Address address, ReadOnlyMemory<byte> hash, Action<IContext>? configure = null)
        {
            address = RequireInputParameter.Address(address);
            hash = RequireInputParameter.Hash(hash);
            var context = CreateChildContext(configure);
            var query = new Query
            {
                CryptoGetClaim = new CryptoGetClaimQuery
                {
                    Header = Transactions.CreateAskCostHeader(),
                    AccountID = Protobuf.ToAccountID(address),
                    Hash = ByteString.CopyFrom(hash.ToArray())
                }
            };
            var response = await Transactions.ExecuteUnsignedAskRequestWithRetryAsync(context, query, getRequestMethod, getResponseCode);
            long cost = (long)response.CryptoGetClaim.Header.Cost;
            if (cost > 0)
            {
                query.CryptoGetClaim.Header = Transactions.CreateAndSignQueryHeader(context, cost, "Get Claim Info", out var transactionId);
                response = await Transactions.ExecuteSignedRequestWithRetryAsync(context, query, getRequestMethod, getResponseCode);
                ValidateResult.PreCheck(transactionId, getResponseCode(response));
            }
            return Protobuf.FromClaim(response.CryptoGetClaim.Claim);

            static Func<Query, Task<Response>> getRequestMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Query query) => (await client.getClaimAsync(query));
            }

            static ResponseCodeEnum getResponseCode(Response response)
            {
                return response.CryptoGetClaim?.Header?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
            }
        }
    }
}
