using Google.Protobuf;
using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    internal static class Transactions
    {
        internal static TransactionID GetOrCreateProtoTransactionID(ContextStack context)
        {
            var preExistingTransaction = context.Transaction;
            if (preExistingTransaction == null)
            {
                var transactionId = CreateNewProtoTransactionID(context.Payer, DateTime.UtcNow);
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

        internal static TransactionID CreateNewProtoTransactionID(Account payer, DateTime dateTime)
        {
            return new TransactionID
            {
                TransactionValidStart = Protobuf.toProtoTimestamp(dateTime),
                AccountID = Protobuf.ToAccountID(payer)
            };
        }
        internal static TransferList CreateProtoTransferList(params (Address address, long amount)[] list)
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
        internal static TransactionBody CreateProtoTransactionBody(ContextStack context, TransferList transfers, TransactionID transactionId, string defaultMemo)
        {
            return new TransactionBody
            {
                TransactionID = transactionId,
                NodeAccountID = Protobuf.ToAccountID(context.Gateway),
                TransactionFee = (ulong)context.Fee,
                TransactionValidDuration = Protobuf.ToDuration(context.TransactionDuration),
                Memo = context.Memo ?? defaultMemo ?? "",
                CryptoTransfer = new CryptoTransferTransactionBody
                {
                    Transfers = transfers
                }
            };
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

        internal static async Task<TResponse> ExecuteQueryWithRetryAsync<TResponse>(ContextStack context, Query query, Func<Channel, Func<Query, Task<TResponse>>> instantiateQueryNetworkMethod, Func<TResponse, ResponseHeader> extractHeaderFromResponse)
        {
            var channel = new Channel(context.Gateway.Url, ChannelCredentials.Insecure);
            try
            {
                var queryNetwork = instantiateQueryNetworkMethod(channel);
                var response = await queryNetwork(query);
                var header = extractHeaderFromResponse(response);
                if (ResponseContainsRetryableErrorCode(header))
                {
                    var maxRetries = context.BusyRetryCount;
                    var retryStandoff = context.BusyRetryDelay;
                    for (var retryCount = 0; retryCount < maxRetries && ResponseContainsRetryableErrorCode(header); retryCount++)
                    {
                        await Task.Delay(retryStandoff);
                        response = await queryNetwork(query);
                        header = extractHeaderFromResponse(response);
                    }
                }
                return response;
            }
            finally
            {
                await channel.ShutdownAsync();
            }
        }

        private static bool ResponseContainsRetryableErrorCode(ResponseHeader header)
        {
            return
                header.NodeTransactionPrecheckCode == ResponseCodeEnum.Busy ||
                header.NodeTransactionPrecheckCode == ResponseCodeEnum.InvalidTransactionStart;
        }
    }
}
