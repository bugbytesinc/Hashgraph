using Google.Protobuf;
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
                var transactionId = CreateNewTransactionID(RequireInContext.Payer(context), DateTime.UtcNow);
                var transaction = Protobuf.FromTransactionId(transactionId);
                foreach (var handler in context.GetAll<Action<TxId>>(nameof(context.OnTransactionCreated)))
                {
                    handler(transaction);
                }
                return transactionId;
            }
            else
            {
                return Protobuf.ToTransactionID(preExistingTransaction);
            }
        }
        internal static TransactionID CreateNewTransactionID(Account payer, DateTime dateTime)
        {
            return new TransactionID
            {
                TransactionValidStart = Protobuf.ToTimestamp(dateTime),
                AccountID = Protobuf.ToAccountID(payer)
            };
        }
        internal static TransferList CreateCryptoTransferList(params (Address address, long amount)[] list)
        {
            var transfers = new TransferList();
            foreach (var transfer in list)
            {
                transfers.AccountAmounts.Add(new AccountAmount
                {
                    AccountID = Protobuf.ToAccountID(transfer.address),
                    Amount = transfer.amount
                });
            }
            return transfers;
        }
        internal static TransactionBody CreateEmptyTransactionBody(ContextStack context, TransactionID transactionId, string defaultMemo)
        {
            return new TransactionBody
            {
                TransactionID = transactionId,
                NodeAccountID = Protobuf.ToAccountID(RequireInContext.Gateway(context)),
                TransactionFee = (ulong)context.FeeLimit,
                TransactionValidDuration = Protobuf.ToDuration(context.TransactionDuration),
                GenerateRecord = context.GenerateRecord,
                Memo = context.Memo ?? defaultMemo ?? "",
            };
        }
        internal static TransactionBody CreateCryptoTransferTransactionBody(ContextStack context, TransferList transfers, TransactionID transactionId, string defaultMemo)
        {
            var body = CreateEmptyTransactionBody(context, transactionId, defaultMemo);
            body.CryptoTransfer = new CryptoTransferTransactionBody { Transfers = transfers };
            return body;
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
            return ExecuteRequestWithRetryAsync(context, request, instantiateRequestMethod, shouldRetryRequest);

            bool shouldRetryRequest(TResponse response)
            {
                var code = getResponseCode(response);
                return
                    code == ResponseCodeEnum.Busy ||
                    code == ResponseCodeEnum.InvalidTransactionStart;
            }
        }
        internal static async Task<TResponse> ExecuteRequestWithRetryAsync<TRequest, TResponse>(ContextStack context, TRequest request, Func<Channel, Func<TRequest, Task<TResponse>>> instantiateRequestMethod, Func<TResponse, bool> shouldRetryRequest) where TRequest : IMessage where TResponse : IMessage
        {
            var callOnSendingHandlers = InstantiateOnSendingRequestHandler(context);
            var callOnResponseReceivedHandlers = InstantiateOnResponseReceivedHandler(context);
            var sendRequest = instantiateRequestMethod(context.GetChannel());
            callOnSendingHandlers(request);
            var response = await sendRequest(request);
            callOnResponseReceivedHandlers(0, response);
            var shouldRetry = shouldRetryRequest(response);
            if (shouldRetry)
            {
                var maxRetries = context.RetryCount;
                var retryDelay = context.RetryDelay;
                for (var retryCount = 0; retryCount < maxRetries && shouldRetry; retryCount++)
                {
                    await Task.Delay(retryDelay * (retryCount + 1));
                    response = await sendRequest(request);
                    callOnResponseReceivedHandlers(retryCount + 1, response);
                    shouldRetry = shouldRetryRequest(response);
                }
            }
            return response;
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
