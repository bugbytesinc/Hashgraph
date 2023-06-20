using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Hashgraph;

/// <summary>
/// Internal helper class providing protobuf message construction 
/// helpers and communication with the remote GRPC server.
/// </summary>
internal static class GossipContextStackExtensions
{
    internal static ISignatory GatherSignatories(this GossipContextStack context, params Signatory?[] extraSignatories)
    {
        var signatories = new List<Signatory>(1 + extraSignatories.Length);
        var contextSignatory = context.Signatory;
        if (contextSignatory is not null)
        {
            signatories.Add(contextSignatory);
        }
        foreach (var extraSignatory in extraSignatories)
        {
            if (extraSignatory is not null)
            {
                signatories.Add(extraSignatory);
            }
        }
        return signatories.Count == 1 ?
            signatories[0] :
            new Signatory(signatories.ToArray());
    }
    internal static TransactionID GetOrCreateTransactionID(this GossipContextStack context)
    {
        var preExistingTransaction = context.Transaction;
        if (preExistingTransaction is null)
        {
            var payer = context.Payer;
            return payer is null
                ? throw new InvalidOperationException("The Payer address has not been configured. Please check that 'Payer' is set in the Client context.")
                : CreateTransactionID(context, payer);
        }
        else if (preExistingTransaction.Pending)
        {
            throw new ArgumentException("Can not set the context's Transaction ID's Pending field of a transaction to true.", nameof(context.Transaction));
        }
        else
        {
            return new TransactionID(preExistingTransaction);
        }
    }
    internal static TransactionID CreateTransactionID(this GossipContextStack context, Address payer)
    {
        var (seconds, nanos) = Epoch.UniqueSecondsAndNanos(context.AdjustForLocalClockDrift);
        return new TransactionID
        {
            AccountID = new AccountID(payer),
            TransactionValidStart = new Proto.Timestamp
            {
                Seconds = seconds,
                Nanos = nanos
            }
        };
    }
    internal static Task<TResponse> ExecuteSignedRequestWithRetryImplementationAsync<TRequest, TResponse>(this GossipContextStack context, TRequest request, Func<GrpcChannel, Func<TRequest, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TResponse>>> instantiateRequestMethod, Func<TResponse, ResponseCodeEnum> getResponseCode) where TRequest : IMessage where TResponse : IMessage
    {
        var trackTimeDrift = context.AdjustForLocalClockDrift && context.Transaction is null;
        var startingInstant = trackTimeDrift ? Epoch.UniqueClockNanos() : 0;

        return ExecuteNetworkRequestWithRetryAsync(context, request, instantiateRequestMethod, shouldRetryRequest);

        bool shouldRetryRequest(TResponse response)
        {
            var code = getResponseCode(response);
            if (trackTimeDrift && code == ResponseCodeEnum.InvalidTransactionStart)
            {
                Epoch.AddToClockDrift(Epoch.UniqueClockNanos() - startingInstant);
            }
            return
                code == ResponseCodeEnum.Busy ||
                code == ResponseCodeEnum.InvalidTransactionStart;
        }
    }
    internal static async Task<TResponse> ExecuteNetworkRequestWithRetryAsync<TRequest, TResponse>(this GossipContextStack context, TRequest request, Func<GrpcChannel, Func<TRequest, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TResponse>>> instantiateRequestMethod, Func<TResponse, bool> shouldRetryRequest) where TRequest : IMessage where TResponse : IMessage
    {
        try
        {
            var retryCount = 0;
            var maxRetries = context.RetryCount;
            var retryDelay = context.RetryDelay;
            var callOnSendingHandlers = InstantiateOnSendingRequestHandler(context);
            var callOnResponseReceivedHandlers = InstantiateOnResponseReceivedHandler(context);
            var sendRequest = instantiateRequestMethod(context.GetChannel());
            callOnSendingHandlers(request);
            for (; retryCount < maxRetries; retryCount++)
            {
                try
                {
                    var tenativeResponse = await sendRequest(request, null, null, default);
                    callOnResponseReceivedHandlers(retryCount, tenativeResponse);
                    if (!shouldRetryRequest(tenativeResponse))
                    {
                        return tenativeResponse;
                    }
                }
                catch (RpcException rpcex) when (request is Query query && query.QueryCase == Query.QueryOneofCase.TransactionGetReceipt)
                {
                    var channel = context.GetChannel();
                    var message = channel.State == ConnectivityState.Connecting ?
                        $"Unable to communicate with network node {channel.Target} while retrieving receipt, it may be down or not reachable." :
                        $"Unable to communicate with network node {channel.Target} while retrieving receipt: {rpcex.Status}";
                    callOnResponseReceivedHandlers(retryCount, new StringValue { Value = message });
                }
                catch (RpcException rpcex) when (rpcex.StatusCode == StatusCode.Unavailable || rpcex.StatusCode == StatusCode.Unknown || rpcex.StatusCode == StatusCode.Cancelled)
                {
                    var channel = context.GetChannel();
                    var message = channel.State == ConnectivityState.Connecting ?
                        $"Unable to communicate with network node {channel.Target}, it may be down or not reachable." :
                        $"Unable to communicate with network node {channel.Target}: {rpcex.Status}";
                    callOnResponseReceivedHandlers(retryCount, new StringValue { Value = message });

                    if (request is Transaction transaction)
                    {
                        // If this was a transaction, it may have actully successfully been processed, in which case 
                        // the receipt will already be in the system.  Check to see if it is there.
                        await Task.Delay(retryDelay * retryCount).ConfigureAwait(false);
                        var receiptResponse = await CheckForReceipt(transaction).ConfigureAwait(false);
                        callOnResponseReceivedHandlers(retryCount, receiptResponse);
                        if (receiptResponse.NodeTransactionPrecheckCode != ResponseCodeEnum.ReceiptNotFound &&
                            receiptResponse is TResponse tenativeResponse &&
                            !shouldRetryRequest(tenativeResponse))
                        {
                            return tenativeResponse;
                        }
                    }
                    else if (request is Query query)
                    {
                        // If this was a query, it may not have made it to the node and we can retry.  However,
                        // if we receive a duplicate transaction error, it means the node accepted the payment
                        // and terminated the connection before returning results, in which case funds are lost.
                        // For that scenario, re-throw the original exception and it will be caught and translated
                        // into a PrecheckException with the appropriate error message.
                        var retryQueryResponse = await RetryQuery();
                        if ((retryQueryResponse as Response)?.ResponseHeader?.NodeTransactionPrecheckCode == ResponseCodeEnum.DuplicateTransaction)
                        {
                            throw;
                        }
                        if (!shouldRetryRequest(retryQueryResponse))
                        {
                            return retryQueryResponse;
                        }
                    }
                }
                await Task.Delay(retryDelay * (retryCount + 1)).ConfigureAwait(false);
            }
            var finalResponse = await sendRequest(request, null, null, default);
            callOnResponseReceivedHandlers(maxRetries, finalResponse);
            return finalResponse;

            async Task<TransactionResponse> CheckForReceipt(Transaction transaction)
            {
                // In the case we submitted a transaction, the receipt may actually
                // be in the system.  Unpacking the transaction is not necessarily efficient,
                // however we are here due to edge case error condition due to poor network 
                // performance or grpc connection issues already.
                if (transaction != null)
                {
                    var transactionId = ExtractTransactionID(transaction);
                    var query = new Query
                    {
                        TransactionGetReceipt = new TransactionGetReceiptQuery
                        {
                            TransactionID = transactionId
                        }
                    };
                    for (; retryCount < maxRetries; retryCount++)
                    {
                        try
                        {
                            var client = new CryptoService.CryptoServiceClient(context.GetChannel());
                            var receipt = await client.getTransactionReceiptsAsync(query);
                            return new TransactionResponse { NodeTransactionPrecheckCode = receipt.TransactionGetReceipt.Header.NodeTransactionPrecheckCode };
                        }
                        catch (RpcException rpcex) when (rpcex.StatusCode == StatusCode.Unavailable)
                        {
                            var channel = context.GetChannel();
                            var message = channel.State == ConnectivityState.Connecting ?
                                $"Unable to communicate with network node {channel.Target}, it may be down or not reachable." :
                                $"Unable to communicate with network node {channel.Target}: {rpcex.Status}";
                            callOnResponseReceivedHandlers(retryCount, new StringValue { Value = message });
                        }
                        await Task.Delay(retryDelay * (retryCount + 1)).ConfigureAwait(false);
                    }
                }
                return new TransactionResponse { NodeTransactionPrecheckCode = ResponseCodeEnum.Unknown };
            }

            async Task<TResponse> RetryQuery()
            {
                while (true)
                {
                    try
                    {
                        await Task.Delay(retryDelay * retryCount).ConfigureAwait(false);
                        return await sendRequest(request, null, null, default);
                    }
                    catch (RpcException rpcex) when (rpcex.StatusCode == StatusCode.Unavailable && retryCount < maxRetries - 1)
                    {
                        var channel = context.GetChannel();
                        var message = channel.State == ConnectivityState.Connecting ?
                            $"Unable to communicate with network node {channel.Target}, it may be down or not reachable." :
                            $"Unable to communicate with network node {channel.Target}: {rpcex.Status}";
                        callOnResponseReceivedHandlers(retryCount, new StringValue { Value = message });
                    }
                    retryCount++;
                }
            }
        }
        catch (RpcException rpcex) when (request is Query query)
        {
            var channel = context.GetChannel();
            var message = rpcex.StatusCode == StatusCode.Unavailable && channel.State == ConnectivityState.Connecting ?
                $"Unable to communicate with network node {channel.Target}, it may be down or not reachable, or accepted payment and terminated the connection before returning Query results." :
                $"Unable to communicate with network node {channel.Target}: {rpcex.Status}, it may have accepted payment and terminated the connection before returning Query results.";
            throw new PrecheckException(message, TxId.None, ResponseCode.RpcError, 0, rpcex);
        }
        catch (RpcException rpcex)
        {
            var transactionId = (request is Transaction transaction) ? ExtractTransactionID(transaction) : null;
            var channel = context.GetChannel();
            var message = rpcex.StatusCode == StatusCode.Unavailable && channel.State == ConnectivityState.Connecting ?
                $"Unable to communicate with network node {channel.Target}, it may be down or not reachable." :
                $"Unable to communicate with network node {channel.Target}: {rpcex.Status}";
            throw new PrecheckException(message, transactionId.AsTxId(), ResponseCode.RpcError, 0, rpcex);
        }
    }

    private static Action<IMessage> InstantiateOnSendingRequestHandler(this GossipContextStack context)
    {
        var handlers = context.GetAll<Action<IMessage>>(nameof(context.OnSendingRequest)).Where(h => h != null).ToArray();
        if (handlers.Length > 0)
        {
            return (IMessage request) => ExecuteHandlers(handlers, request);
        }
        else
        {
            return NoOp;
        }
        static void ExecuteHandlers(Action<IMessage>[] handlers, IMessage request)
        {
            var data = new ReadOnlyMemory<byte>(request.ToByteArray());
            foreach (var handler in handlers)
            {
                handler(request);
            }
        }
        static void NoOp(IMessage request)
        {
        }
    }
    private static Action<int, IMessage> InstantiateOnResponseReceivedHandler(this GossipContextStack context)
    {
        var handlers = context.GetAll<Action<int, IMessage>>(nameof(context.OnResponseReceived)).Where(h => h != null).ToArray();
        if (handlers.Length > 0)
        {
            return (int tryNumber, IMessage response) => ExecuteHandlers(handlers, tryNumber, response);
        }
        else
        {
            return NoOp;
        }
        static void ExecuteHandlers(Action<int, IMessage>[] handlers, int tryNumber, IMessage response)
        {
            foreach (var handler in handlers)
            {
                handler(tryNumber, response);
            }
        }
        static void NoOp(int tryNumber, IMessage response)
        {
        }
    }
    /// <summary>
    /// Internal Helper function to retrieve receipt record provided by 
    /// the network following network consensus regarding a query or transaction.
    /// </summary>
    internal static async Task<Proto.TransactionReceipt> GetReceiptAsync(this GossipContextStack context, TransactionID transactionId)
    {
        var query = new TransactionGetReceiptQuery(transactionId, false, false) as INetworkQuery;
        var response = await context.ExecuteNetworkRequestWithRetryAsync(query.CreateEnvelope(), query.InstantiateNetworkRequestMethod, shouldRetry).ConfigureAwait(false);
        var responseCode = response.TransactionGetReceipt.Header.NodeTransactionPrecheckCode;
        switch (responseCode)
        {
            case ResponseCodeEnum.Ok:
                break;
            case ResponseCodeEnum.Busy:
                throw new ConsensusException("Network failed to respond to request for a transaction receipt, it is too busy. It is possible the network may still reach concensus for this transaction.", transactionId.AsTxId(), (ResponseCode)responseCode);
            case ResponseCodeEnum.Unknown:
            case ResponseCodeEnum.ReceiptNotFound:
                throw new TransactionException($"Network failed to return a transaction receipt, Status Code Returned: {responseCode}", transactionId, responseCode);
        }
        var status = response.TransactionGetReceipt.Receipt.Status;
        switch (status)
        {
            case ResponseCodeEnum.Unknown:
                throw new ConsensusException("Network failed to reach concensus within the configured retry time window, It is possible the network may still reach concensus for this transaction.", transactionId.AsTxId(), (ResponseCode)status);
            case ResponseCodeEnum.TransactionExpired:
                throw new ConsensusException("Network failed to reach concensus before transaction request expired.", transactionId.AsTxId(), (ResponseCode)status);
            case ResponseCodeEnum.ReceiptNotFound:
                throw new ConsensusException("Network failed to find a receipt for given transaction.", transactionId.AsTxId(), (ResponseCode)status);
            default:
                return response.TransactionGetReceipt.Receipt;
        }

        static bool shouldRetry(Response response)
        {
            return
                response.TransactionGetReceipt?.Header?.NodeTransactionPrecheckCode == ResponseCodeEnum.Busy ||
                response.TransactionGetReceipt?.Receipt?.Status == ResponseCodeEnum.Unknown;
        }
    }

    private static TransactionID ExtractTransactionID(Transaction transaction)
    {
        var signedTransaction = SignedTransaction.Parser.ParseFrom(transaction.SignedTransactionBytes);
        var transactionBody = TransactionBody.Parser.ParseFrom(signedTransaction.BodyBytes);
        return transactionBody.TransactionID;
    }
}