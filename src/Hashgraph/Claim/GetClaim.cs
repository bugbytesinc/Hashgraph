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
