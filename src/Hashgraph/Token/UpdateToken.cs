using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Updates the changeable properties of a Hedera Fungable Token.
        /// </summary>
        /// <param name="updateParameters">
        /// The Token update parameters, includes a required 
        /// <see cref="Address"/> or <code>Symbol</code> reference to the Token 
        /// to update plus a number of changeable properties of the Token.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating success of the operation.
        /// of the request.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> UpdateTokenAsync(UpdateTokenParams updateParameters, Action<IContext>? configure = null)
        {
            return UpdateTokenImplementationAsync<TransactionReceipt>(updateParameters, configure);
        }
        /// <summary>
        /// Updates the changeable properties of a hedera network Token.
        /// </summary>
        /// <param name="updateParameters">
        /// The Token update parameters, includes a required 
        /// <see cref="Address"/> or <code>Symbol</code> reference to the Token 
        /// to update plus a number of changeable properties of the Token.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record containing the details of the results.
        /// of the request.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionRecord> UpdateTokenWithRecordAsync(UpdateTokenParams updateParameters, Action<IContext>? configure = null)
        {
            return UpdateTokenImplementationAsync<TransactionRecord>(updateParameters, configure);
        }
        /// <summary>
        /// Internal implementation of the update Token functionality.
        /// </summary>
        private async Task<TResult> UpdateTokenImplementationAsync<TResult>(UpdateTokenParams updateParameters, Action<IContext>? configure) where TResult : new()
        {
            updateParameters = RequireInputParameter.UpdateParameters(updateParameters);
            await using var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatory = Transactions.GatherSignatories(context, updateParameters.Signatory);
            var updateTokenBody = new TokenUpdateTransactionBody
            {
                Token = new TokenID(updateParameters.Token)
            };
            if (!(updateParameters.Treasury is null))
            {
                updateTokenBody.Treasury = new AccountID(updateParameters.Treasury);
            }
            if (!(updateParameters.Administrator is null))
            {
                updateTokenBody.AdminKey = new Key(updateParameters.Administrator);
            }
            if (!(updateParameters.GrantKycEndorsement is null))
            {
                updateTokenBody.KycKey = new Key(updateParameters.GrantKycEndorsement);
            }
            if (!(updateParameters.SuspendEndorsement is null))
            {
                updateTokenBody.FreezeKey = new Key(updateParameters.SuspendEndorsement);
            }
            if (!(updateParameters.ConfiscateEndorsement is null))
            {
                updateTokenBody.WipeKey = new Key(updateParameters.ConfiscateEndorsement);
            }
            if (!(updateParameters.SupplyEndorsement is null))
            {
                updateTokenBody.SupplyKey = new Key(updateParameters.SupplyEndorsement);
            }
            if (!string.IsNullOrWhiteSpace(updateParameters.Symbol))
            {
                updateTokenBody.Symbol = updateParameters.Symbol;
            }
            if (!string.IsNullOrWhiteSpace(updateParameters.Name))
            {
                updateTokenBody.Name = updateParameters.Name;
            }
            if (updateParameters.Expiration.HasValue)
            {
                updateTokenBody.Expiry = (ulong)Epoch.FromDate(updateParameters.Expiration.Value).seconds;
            }
            if (updateParameters.RenewPeriod.HasValue)
            {
                updateTokenBody.AutoRenewPeriod = (ulong)updateParameters.RenewPeriod.Value.TotalSeconds;
            }
            if (!(updateParameters.RenewAccount is null))
            {
                updateTokenBody.AutoRenewAccount = new AccountID(updateParameters.RenewAccount);
            }
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId);
            transactionBody.TokenUpdate = updateTokenBody;
            var request = await Transactions.SignTransactionAsync(transactionBody, signatory);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to update Token, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is TransactionRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                record.FillProperties(rec);
            }
            else if (result is TransactionReceipt rcpt)
            {
                receipt.FillProperties(transactionId, rcpt);
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new TokenService.TokenServiceClient(channel);
                return async (Transaction transaction) => await client.updateTokenAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
