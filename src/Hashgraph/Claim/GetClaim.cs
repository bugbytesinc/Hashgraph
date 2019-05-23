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
        public async Task<Claim> GetClaimAsync(Address address, ReadOnlyMemory<byte> hash, Action<IContext>? configure = null)
        {
            address = RequireInputParameter.Address(address);
            hash = RequireInputParameter.Hash(hash);
            var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transfers = Transactions.CreateCryptoTransferList((payer, -context.FeeLimit), (gateway, context.FeeLimit));
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateCryptoTransferTransactionBody(context, transfers, transactionId, "Get Claim Info");
            var query = new Query
            {
                CryptoGetClaim = new CryptoGetClaimQuery
                {
                    Header = Transactions.SignQueryHeader(transactionBody, payer),
                    AccountID = Protobuf.ToAccountID(address),
                    Hash = ByteString.CopyFrom(hash.ToArray())
                }
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, query, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, response.CryptoGetClaim.Header.NodeTransactionPrecheckCode);
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
