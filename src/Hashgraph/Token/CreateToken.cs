#pragma warning disable CS8604
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
        public async Task<CreateTokenReceipt> CreateTokenAsync(CreateTokenParams createParameters, Action<IContext>? configure = null)
        {
            return new CreateTokenReceipt(await CreateTokenImplementationAsync(createParameters, configure, false));
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
        public async Task<CreateTokenRecord> CreateTokenWithRecordAsync(CreateTokenParams createParameters, Action<IContext>? configure = null)
        {
            return new CreateTokenRecord(await CreateTokenImplementationAsync(createParameters, configure, true));
        }
        /// <summary>
        /// Internal implementation of the Create Token service.
        /// </summary>
        private async Task<NetworkResult> CreateTokenImplementationAsync(CreateTokenParams createParameters, Action<IContext>? configure, bool includeRecord)
        {
            createParameters = RequireInputParameter.CreateParameters(createParameters);
            await using var context = CreateChildContext(configure);
            var transactionBody = new TransactionBody
            {
                TokenCreation = new TokenCreateTransactionBody
                {
                    Name = createParameters.Name,
                    Symbol = createParameters.Symbol,
                    InitialSupply = createParameters.Circulation,
                    Decimals = createParameters.Decimals,
                    Treasury = new AccountID(createParameters.Treasury),
                    AdminKey = createParameters.Administrator.IsNullOrNone() ? null : new Key(createParameters.Administrator),
                    KycKey = createParameters.GrantKycEndorsement.IsNullOrNone() ? null : new Key(createParameters.GrantKycEndorsement),
                    FreezeKey = createParameters.SuspendEndorsement.IsNullOrNone() ? null : new Key(createParameters.SuspendEndorsement),
                    WipeKey = createParameters.ConfiscateEndorsement.IsNullOrNone() ? null : new Key(createParameters.ConfiscateEndorsement),
                    SupplyKey = createParameters.SupplyEndorsement.IsNullOrNone() ? null : new Key(createParameters.SupplyEndorsement),
                    FreezeDefault = createParameters.InitializeSuspended,
                    Expiry = new Timestamp(createParameters.Expiration),
                    AutoRenewAccount = createParameters.RenewAccount.IsNullOrNone() ? null : new AccountID(createParameters.RenewAccount),
                    AutoRenewPeriod = createParameters.RenewPeriod.HasValue ? new Duration(createParameters.RenewPeriod.Value) : null,
                    Memo = createParameters.Memo
                }
            };
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Unable to create Token, status: {0}", createParameters.Signatory);
        }
    }
}
