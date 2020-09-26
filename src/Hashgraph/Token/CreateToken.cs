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
        /// Creates a new token with the given create parameters.
        /// </summary>
        /// <param name="createParameters">
        /// Details regarding the token to instantiate.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt with a description of the newly created token.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<CreateTokenReceipt> CreateTokenAsync(CreateTokenParams createParameters, Action<IContext>? configure = null)
        {
            return CreateTokenImplementationAsync<CreateTokenReceipt>(createParameters, configure);
        }
        /// <summary>
        /// Creates a new token instance with the given create parameters.
        /// </summary>
        /// <param name="createParameters">
        /// Details regarding the token to instantiate.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record with a description of the newly created token.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<CreateTokenRecord> CreateTokenWithRecordAsync(CreateTokenParams createParameters, Action<IContext>? configure = null)
        {
            return CreateTokenImplementationAsync<CreateTokenRecord>(createParameters, configure);
        }
        /// <summary>
        /// Internal implementation of the Create Token service.
        /// </summary>
        private async Task<TResult> CreateTokenImplementationAsync<TResult>(CreateTokenParams createParameters, Action<IContext>? configure) where TResult : new()
        {
            createParameters = RequireInputParameter.CreateParameters(createParameters);
            await using var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatory = Transactions.GatherSignatories(context, createParameters.Signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId);
            transactionBody.TokenCreation = new TokenCreation
            {
                //Name = createParameters.Name,
                Symbol = createParameters.Symbol,
                Float = createParameters.Circulation,
                Divisibility = createParameters.Decimals,
                Treasury = new AccountID(createParameters.Treasury),
                AdminKey = createParameters.Administrator is null ? null : new Key(createParameters.Administrator),
                KycKey = createParameters.GrantKycEndorsement is null ? null : new Key(createParameters.GrantKycEndorsement),
                FreezeKey = createParameters.SuspendEndorsement is null ? null : new Key(createParameters.SuspendEndorsement),
                WipeKey = createParameters.ConfiscateEndorsement is null ? null : new Key(createParameters.ConfiscateEndorsement),
                SupplyKey = createParameters.SupplyEndorsement is null ? null : new Key(createParameters.SupplyEndorsement),
                FreezeDefault = createParameters.InitializeSuspended,
                //Expiry = (ulong)Epoch.FromDate(createParameters.Expiration).seconds,
                //AutoRenewAccount = createParameters.RenewAccount is null ? null : new AccountID(createParameters.RenewAccount),
                //AutoRenewPeriod = (ulong)createParameters.RenewPeriod.TotalSeconds
            };
            var request = await Transactions.SignTransactionAsync(transactionBody, signatory);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to create Token, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is CreateTokenRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                record.FillProperties(rec);
            }
            else if (result is CreateTokenReceipt rcpt)
            {
                receipt.FillProperties(transactionId, rcpt);
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new TokenService.TokenServiceClient(channel);
                return async (Transaction transaction) => await client.createTokenAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
