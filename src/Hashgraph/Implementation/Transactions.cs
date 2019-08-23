using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph
{
    /// <summary>
    /// Internal helper class providing protobuf message construction 
    /// helpers and communication with the remote GRPC server.
    /// </summary>
    internal static class Transactions
    {
        internal static TransactionID GetOrCreateTransactionID(ContextStack context)
        {
            var preExistingTransaction = context.Transaction;
            if (preExistingTransaction is null)
            {
                var (seconds, nanos) = Epoch.UniqueSecondsAndNanos(context.AdjustForLocalClockDrift);
                return new TransactionID
                {
                    AccountID = Protobuf.ToAccountID(RequireInContext.Payer(context)),
                    TransactionValidStart = new Proto.Timestamp
                    {
                        Seconds = seconds,
                        Nanos = nanos
                    }
                };
            }
            else
            {
                return Protobuf.ToTransactionID(preExistingTransaction);
            }
        }
        internal static TransferList CreateCryptoTransferList(params (Address address, long amount)[] list)
        {
            var netRequests = new Dictionary<Address, long>();
            foreach (var transfer in list)
            {
                if (netRequests.TryGetValue(transfer.address, out long value))
                {
                    netRequests[transfer.address] = value + transfer.amount;
                }
                else
                {
                    netRequests[transfer.address] = transfer.amount;
                }
            }
            var transfers = new TransferList();
            foreach (var transfer in netRequests)
            {
                if (transfer.Value != 0)
                {
                    transfers.AccountAmounts.Add(new AccountAmount
                    {
                        AccountID = Protobuf.ToAccountID(transfer.Key),
                        Amount = transfer.Value
                    });
                }
            }
            return transfers;
        }
        internal static TransactionBody CreateTransactionBody(ContextStack context, TransactionID transactionId, string defaultMemo)
        {
            return new TransactionBody
            {
                TransactionID = transactionId,
                NodeAccountID = Protobuf.ToAccountID(RequireInContext.Gateway(context)),
                TransactionFee = (ulong)context.FeeLimit,
                TransactionValidDuration = Protobuf.ToDuration(context.TransactionDuration),
                Memo = context.Memo ?? defaultMemo ?? "",
            };
        }
        internal static QueryHeader CreateAndSignQueryHeader(ContextStack context, long queryFee, string defaultMemo, out TransactionID transactionId)
        {
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var fee = RequireInContext.QueryFee(context, queryFee);
            transactionId = GetOrCreateTransactionID(context);
            TransactionBody transactionBody = new TransactionBody
            {
                TransactionID = transactionId,
                NodeAccountID = Protobuf.ToAccountID(gateway),
                TransactionFee = (ulong)context.FeeLimit,
                TransactionValidDuration = Protobuf.ToDuration(context.TransactionDuration),
                Memo = context.Memo ?? defaultMemo ?? "",
            };
            if (fee > 0)
            {
                var transfers = CreateCryptoTransferList((payer, -fee), (gateway, fee));
                transactionBody.CryptoTransfer = new CryptoTransferTransactionBody { Transfers = transfers };
            }
            return SignQueryHeader(transactionBody, payer);
        }
        internal static Proto.Transaction SignTransaction(TransactionBody transactionBody, params ISigner[] signers)
        {
            var map = new Dictionary<ByteString, SignaturePair>();
            var bytes = transactionBody.ToByteArray();
            foreach (var signer in signers)
            {
                foreach (var signature in signer.Sign(bytes))
                {
                    map[signature.PubKeyPrefix] = signature;
                }
            }
            var signatures = new SignatureMap();
            foreach (var signature in map.Values)
            {
                signatures.SigPair.Add(signature);
            }
            return new Proto.Transaction
            {
                BodyBytes = ByteString.CopyFrom(bytes),
                SigMap = signatures
            };
        }
        internal static QueryHeader SignQueryHeader(TransactionBody transactionBody, params ISigner[] signers)
        {
            return new QueryHeader
            {
                Payment = SignTransaction(transactionBody, signers)
            };
        }
        internal static Task<TResponse> ExecuteRequestWithRetryAsync<TRequest, TResponse>(ContextStack context, TRequest request, Func<Channel, Func<TRequest, Task<TResponse>>> instantiateRequestMethod, Func<TResponse, ResponseCodeEnum> getResponseCode) where TRequest : IMessage where TResponse : IMessage
        {
            var trackTimeDrift = context.AdjustForLocalClockDrift && context.Transaction is null;
            var startingInstant = trackTimeDrift ? Epoch.UniqueClockNanos() : 0;

            return ExecuteRequestWithRetryAsync(context, request, instantiateRequestMethod, shouldRetryRequest);

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
        internal static async Task<TResponse> ExecuteRequestWithRetryAsync<TRequest, TResponse>(ContextStack context, TRequest request, Func<Channel, Func<TRequest, Task<TResponse>>> instantiateRequestMethod, Func<TResponse, bool> shouldRetryRequest) where TRequest : IMessage where TResponse : IMessage
        {
            try
            {
                var maxRetries = context.RetryCount;
                var retryDelay = context.RetryDelay;
                var callOnSendingHandlers = InstantiateOnSendingRequestHandler(context);
                var callOnResponseReceivedHandlers = InstantiateOnResponseReceivedHandler(context);
                var sendRequest = instantiateRequestMethod(context.GetChannel());
                callOnSendingHandlers(request);
                for (var retryCount = 0; retryCount < maxRetries; retryCount++)
                {
                    try
                    {
                        var tenativeResponse = await sendRequest(request);
                        callOnResponseReceivedHandlers(retryCount, tenativeResponse);
                        if (!shouldRetryRequest(tenativeResponse))
                        {
                            return tenativeResponse;
                        }
                    }
                    catch (RpcException rpcex) when (rpcex.StatusCode == StatusCode.Unavailable)
                    {
                        callOnResponseReceivedHandlers(retryCount, new StringValue { Value = $"Unable to communicate with network: {rpcex.Status}" });
                    }
                    await Task.Delay(retryDelay * (retryCount + 1));
                }
                var finalResponse = await sendRequest(request);
                callOnResponseReceivedHandlers(maxRetries, finalResponse);
                return finalResponse;
            }
            catch (RpcException rpcex)
            {
                throw new PrecheckException($"Unable to communicate with network: {rpcex.Status}", new TxId(), ResponseCode.Unknown, rpcex);
            }
        }

        private static Action<IMessage> InstantiateOnSendingRequestHandler(ContextStack context)
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
        private static Action<int, IMessage> InstantiateOnResponseReceivedHandler(ContextStack context)
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
    }
}
