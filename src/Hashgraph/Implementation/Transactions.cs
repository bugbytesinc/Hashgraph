using Google.Protobuf;
using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Hashgraph
{
    internal static class Transactions
    {
        internal static TransactionID GetOrCreateTransactionID(ContextStack context)
        {
            var preExistingTransaction = context.Transaction;
            if (preExistingTransaction == null)
            {
                var transactionId = CreateNewTransactionID(context.Payer, DateTime.UtcNow);
                var transaction = Protobuf.FromTransactionId(transactionId);
                foreach (var handler in context.GetAll<Action<Transaction>>(nameof(context.OnTransactionCreated)))
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
                TransactionValidStart = Protobuf.toProtoTimestamp(dateTime),
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
                NodeAccountID = Protobuf.ToAccountID(context.Gateway),
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
            var bytes = Protobuf.toProtoBytes(transactionBody);
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

        internal static async Task<TResponse> ExecuteRequestWithRetryAsync<TRequest, TResponse>(ContextStack context, TRequest request, Func<Channel, Func<TRequest, Task<TResponse>>> instantiateRequestMethod, Func<TResponse, bool> checkForRetry)
        {
            // Temporary: we're echoing messages
            // to the debug stream until we 
            // design more thoughtful diagnostic hooks.
            Debug.WriteLine($"Sending: {request}");
            var sendRequest = instantiateRequestMethod(context.GetChannel());
            var response = await sendRequest(request);
            Debug.WriteLine($"Received: {response}");
            var retryable = checkForRetry(response);
            if (retryable)
            {
                var maxRetries = context.RetryCount;
                var retryDelay = context.RetryDelay;
                for (var retryCount = 0; retryCount < maxRetries && retryable; retryCount++)
                {
                    await Task.Delay(retryDelay*(retryCount+1));
                    response = await sendRequest(request);
                    Debug.WriteLine($"Received (try {retryCount+1}): {response}");
                    retryable = checkForRetry(response);
                }
            }
            return response;
        }
    }
}
