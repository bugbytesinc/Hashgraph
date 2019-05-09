using Google.Protobuf;
using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph
{
    internal static class Transactions
    {
        internal static TransactionID GetOrCreateTransactionID(ContextStack context)
        {
            var preExistingTransaction = context.Transaction;
            if (preExistingTransaction is null)
            {
                var transactionId = CreateNewTransactionID(Require.PayerInContext(context), DateTime.UtcNow);
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
                NodeAccountID = Protobuf.ToAccountID(Require.GatewayInContext(context)),
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

        internal static SignatureList SignProtoTransactionBody(TransactionBody transactionBody, params ISigner[] signers)
        {
            var signatures = new SignatureList();
            var bytes = transactionBody.ToByteArray();
            foreach (var signer in signers)
            {
                signatures.Sigs.Add(new Signature { Ed25519 = ByteString.CopyFrom(signer.Sign(bytes)) });
            }
            return signatures;
        }
        internal static QueryHeader CreateProtoQueryHeader(TransactionBody transactionBody, SignatureList signatures)
        {
            return new QueryHeader
            {
                Payment = new Proto.Transaction
                {
                    Body = transactionBody,
                    Sigs = signatures
                }
            };
        }

        internal static async Task<TResponse> ExecuteRequestWithRetryAsync<TRequest, TResponse>(ContextStack context, TRequest request, Func<Channel, Func<TRequest, Task<TResponse>>> instantiateRequestMethod, Func<TResponse, bool> checkForRetry) where TRequest : IMessage where TResponse : IMessage
        {
            var callOnSendingHandlers = InstantiateOnSendingRequestHandler(context);
            var callOnResponseReceivedHandlers = InstantiateOnResponseReceivedHandler(context);
            var sendRequest = instantiateRequestMethod(context.GetChannel());
            callOnSendingHandlers(request);
            var response = await sendRequest(request);
            callOnResponseReceivedHandlers(0, response);
            var retryable = checkForRetry(response);
            if (retryable)
            {
                var maxRetries = context.RetryCount;
                var retryDelay = context.RetryDelay;
                for (var retryCount = 0; retryCount < maxRetries && retryable; retryCount++)
                {
                    await Task.Delay(retryDelay * (retryCount + 1));
                    response = await sendRequest(request);
                    callOnResponseReceivedHandlers(retryCount + 1, response);
                    retryable = checkForRetry(response);
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
