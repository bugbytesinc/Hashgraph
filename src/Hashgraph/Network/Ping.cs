using Google.Protobuf;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Contacts the the configured gateway with a COST_ASK request
    /// to exercise the communications pipeline from this process thru
    /// to the execution engine on the gossip node.
    /// </summary>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// The time it takes (in miliseconds) to receive a response from
    /// the remote gossip node.
    /// </returns>
    /// <exception cref="PrecheckException">
    /// If the request failed or the gossip node was unreachable.
    /// </exception>
    public async Task<long> PingAsync(Action<IContext>? configure = null)
    {
        await using var context = CreateChildContext(configure);
        var query = new CryptoGetInfoQuery(new Address(0, 0, 98)) as INetworkQuery;
        var envelope = query.CreateEnvelope();
        query.SetHeader(new QueryHeader
        {
            Payment = new Transaction { SignedTransactionBytes = ByteString.Empty },
            ResponseType = ResponseType.CostAnswer
        });
        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var answer = await context.ExecuteNetworkRequestWithRetryAsync(envelope, query.InstantiateNetworkRequestMethod, shouldRetryRequest).ConfigureAwait(false);
        stopwatch.Stop();
        var code = answer.ResponseHeader?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
        if (code != ResponseCodeEnum.Ok)
        {
            throw new PrecheckException($"Ping Failed with Code: {code}", TxId.None, (ResponseCode)code, 0);
        }
        return stopwatch.ElapsedMilliseconds;

        static bool shouldRetryRequest(Response response)
        {
            return ResponseCodeEnum.Busy == response.ResponseHeader?.NodeTransactionPrecheckCode;
        }
    }
}