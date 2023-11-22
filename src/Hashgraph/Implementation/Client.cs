using Google.Protobuf;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Hashgraph.Test")]

namespace Hashgraph;

/// <summary>
/// Hedera Network Client
/// </summary>
/// <remarks>
/// This component facilitates interaction with the Hedera Network.  
/// It manages the communication channels with the network and 
/// serialization of requests and responses.  This library generally 
/// shields the client code from directly interacting with the 
/// underlying protobuf communication layer but does provide hooks 
/// allowing advanced low-level manipulation of messages if necessary.
/// </remarks>
public sealed partial class Client
{
    private async Task<NetworkResult> ExecuteTransactionAsync(INetworkTransaction transaction, Action<IContext>? configure, bool includeRecord, params Signatory?[] extraSignatories)
    {
        var result = new NetworkResult();
        await using var context = CreateChildContext(configure);
        var gateway = context.Gateway;
        if (gateway is null)
        {
            throw new InvalidOperationException("The Network Gateway Node has not been configured. Please check that 'Gateway' is set in the Client context.");
        }
        var signatory = context.GatherSignatories(extraSignatories);
        var schedule = signatory.GetSchedule();
        if (schedule is not null)
        {
            var scheduledTransactionBody = transaction.CreateSchedulableTransactionBody();
            scheduledTransactionBody.TransactionFee = (ulong)context.FeeLimit;
            transaction = new ScheduleCreateTransactionBody
            {
                ScheduledTransactionBody = scheduledTransactionBody,
                AdminKey = schedule.Administrator is null ? null : new Key(schedule.Administrator),
                PayerAccountID = schedule.PendingPayer is null ? null : new AccountID(schedule.PendingPayer),
                ExpirationTime = schedule.Expiration is null ? null : new Timestamp(schedule.Expiration.Value),
                WaitForExpiry = schedule.DelayExecution,
                Memo = schedule.Memo ?? ""
            };
        }
        var transactionBody = transaction.CreateTransactionBody();
        transactionBody.TransactionFee = (ulong)context.FeeLimit;
        transactionBody.NodeAccountID = new AccountID(gateway);
        transactionBody.TransactionID = result.TransactionID = context.GetOrCreateTransactionID();
        transactionBody.TransactionValidDuration = new Duration(context.TransactionDuration);
        transactionBody.Memo = context.Memo ?? "";
        var invoice = new Invoice(transactionBody, context.SignaturePrefixTrimLimit);
        await signatory.SignAsync(invoice).ConfigureAwait(false);
        var signedTransaction = new Transaction
        {
            SignedTransactionBytes = invoice.GenerateSignedTransactionFromSignatures().ToByteString()
        };
        var precheck = await context.ExecuteSignedRequestWithRetryImplementationAsync(signedTransaction, transaction.InstantiateNetworkRequestMethod, getResponseCode).ConfigureAwait(false);
        if (precheck.NodeTransactionPrecheckCode != ResponseCodeEnum.Ok)
        {
            var responseCode = (ResponseCode)precheck.NodeTransactionPrecheckCode;
            throw new PrecheckException($"Transaction Failed Pre-Check: {responseCode}", result.TransactionID.AsTxId(), responseCode, precheck.Cost);
        }
        result.Receipt = await context.GetReceiptAsync(result.TransactionID).ConfigureAwait(false);
        transaction.CheckReceipt(result);
        if (includeRecord)
        {
            result.Record = (await ExecuteQueryInContextAsync(new TransactionGetRecordQuery(result.TransactionID, false, false), context, 0).ConfigureAwait(false)).TransactionGetRecord.TransactionRecord;
        }
        return result;

        static ResponseCodeEnum getResponseCode(TransactionResponse response)
        {
            return response.NodeTransactionPrecheckCode;
        }
    }
    private async Task<Response> ExecuteQueryAsync(INetworkQuery query, Action<IContext>? configure, long supplementalCost = 0)
    {
        await using var context = CreateChildContext(configure);
        return await ExecuteQueryInContextAsync(query, context, supplementalCost).ConfigureAwait(false);

    }
    private static async Task<Response> ExecuteQueryInContextAsync(INetworkQuery query, GossipContextStack context, long supplementalCost)
    {
        var envelope = query.CreateEnvelope();
        query.SetHeader(new QueryHeader
        {
            Payment = new Transaction { SignedTransactionBytes = ByteString.Empty },
            ResponseType = ResponseType.CostAnswer
        });
        var response = await executeAskQuery().ConfigureAwait(false);
        ulong cost = response.ResponseHeader?.Cost ?? 0UL;
        if (cost > 0)
        {
            var transactionId = context.GetOrCreateTransactionID();
            query.SetHeader(await context.CreateSignedQueryHeader((long)cost + supplementalCost, transactionId).ConfigureAwait(false));
            response = await executeSignedQuery().ConfigureAwait(false);
            query.CheckResponse(transactionId, response);
        }
        return response;

        async Task<Response> executeAskQuery()
        {
            var answer = await context.ExecuteNetworkRequestWithRetryAsync(envelope, query.InstantiateNetworkRequestMethod, shouldRetryRequest).ConfigureAwait(false);
            var code = answer.ResponseHeader?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
            if (code != ResponseCodeEnum.Ok)
            {
                if (code == ResponseCodeEnum.NotSupported)
                {
                    // This may be a backdoor call that must be signed by a superuser account.
                    // It will not answer a COST_ASK without a signature.  Try signing with an
                    // empty transfer instead, this is not the most efficient, but we're already
                    // in a failure mode and performance is already broken.
                    var transactionId = context.GetOrCreateTransactionID();
                    query.SetHeader(await context.CreateSignedQueryHeader(0, transactionId).ConfigureAwait(false));
                    answer = await executeSignedQuery().ConfigureAwait(false);
                    // If we get a valid repsonse back, it turns out that we needed to identify
                    // ourselves with the signature, the rest of the process can proceed as normal.
                    // If it was a failure then we fall back to the original NOT_SUPPORTED error
                    // we received on the first attempt.
                    if (answer.ResponseHeader?.NodeTransactionPrecheckCode == ResponseCodeEnum.Ok)
                    {
                        return answer;
                    }
                }
                throw new PrecheckException($"Transaction Failed Pre-Check: {code}", TxId.None, (ResponseCode)code, 0);
            }
            return answer;

            static bool shouldRetryRequest(Response response)
            {
                return ResponseCodeEnum.Busy == response.ResponseHeader?.NodeTransactionPrecheckCode;
            }
        }

        Task<Response> executeSignedQuery()
        {
            return context.ExecuteSignedRequestWithRetryImplementationAsync(envelope, query.InstantiateNetworkRequestMethod, getResponseCode);

            static ResponseCodeEnum getResponseCode(Response response)
            {
                return response.ResponseHeader?.NodeTransactionPrecheckCode ?? ResponseCodeEnum.Unknown;
            }
        }
    }
}
