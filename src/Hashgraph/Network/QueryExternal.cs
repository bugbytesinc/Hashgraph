using Google.Protobuf;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Submits the given query represented in the protbuf encoded byte message to
    /// the network, returning the <see cref="Proto.Response"/> protobuf encoded as bytes.
    /// </summary>
    /// <remarks>
    /// Payment information attached to the query will be ignored, the algorithm will attempt 
    /// to query for free first, then sign with the Payer/Signatory pair contained within the 
    /// configuration if the query requires a payment.  Additionally, querying for a receipt 
    /// is a speial case, it is never charged a query fee and the algorithm, when it recognizes 
    /// a receipt query, will wait for consensus if necessary before returning results.
    /// </remarks>
    /// <param name="queryBytes">
    /// The encoded protobuf bytes of the query to perform.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// The bytes of the <see cref="Proto.Response"/> representing the query results.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// If the bytes do not represent a valid Protobuf Encoded Query.
    /// </exception>
    public async Task<ReadOnlyMemory<byte>> QueryExternalAsync(ReadOnlyMemory<byte> queryBytes, Action<IContext>? configure = null)
    {
        try
        {
            if (queryBytes.IsEmpty)
            {
                throw new ArgumentException("Missing Query Bytes (was empty).", nameof(queryBytes));
            }
            var envelope = Query.Parser.ParseFrom(queryBytes.Span);
            var query = envelope.GetNetworkQuery();
            if (query is null)
            {
                throw new ArgumentException("The Query did not contain a request.", nameof(queryBytes));
            }
            await using var context = CreateChildContext(configure);
            query.SetHeader(new QueryHeader
            {
                Payment = new Transaction { SignedTransactionBytes = ByteString.Empty },
                ResponseType = ResponseType.CostAnswer
            });
            var response = envelope.QueryCase == Query.QueryOneofCase.TransactionGetReceipt ?
                await context.ExecuteNetworkRequestWithRetryAsync(envelope, query.InstantiateNetworkRequestMethod, shouldRetryReceiptQuery).ConfigureAwait(false) :
                await context.ExecuteNetworkRequestWithRetryAsync(envelope, query.InstantiateNetworkRequestMethod, shouldRetryGenericQuery).ConfigureAwait(false);
            if (response.ResponseHeader?.NodeTransactionPrecheckCode == ResponseCodeEnum.Ok && response.ResponseHeader?.Cost > 0)
            {
                var transactionId = context.GetOrCreateTransactionID();
                query.SetHeader(await context.CreateSignedQueryHeader((long)response.ResponseHeader.Cost, transactionId).ConfigureAwait(false));
                response = await context.ExecuteSignedRequestWithRetryImplementationAsync(envelope, query.InstantiateNetworkRequestMethod, getResponseCode);
            }
            return response.ToByteArray();
        }
        catch (InvalidProtocolBufferException ipbe)
        {
            throw new ArgumentException("Query Bytes not recognized as valid Protobuf.", nameof(queryBytes), ipbe);
        }
        static bool shouldRetryReceiptQuery(Response response)
        {
            return
                response.TransactionGetReceipt?.Header?.NodeTransactionPrecheckCode == ResponseCodeEnum.Busy ||
                response.TransactionGetReceipt?.Receipt?.Status == ResponseCodeEnum.Unknown;
        }
        static bool shouldRetryGenericQuery(Response response)
        {
            return ResponseCodeEnum.Busy == response.ResponseHeader?.NodeTransactionPrecheckCode;
        }
        static ResponseCodeEnum getResponseCode(Response response)
        {
            return response.ResponseHeader?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
        }
    }
}
