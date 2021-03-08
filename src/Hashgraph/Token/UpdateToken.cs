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
        public async Task<TransactionReceipt> UpdateTokenAsync(UpdateTokenParams updateParameters, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await UpdateTokenImplementationAsync(updateParameters, configure, false));
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
        public async Task<TransactionRecord> UpdateTokenWithRecordAsync(UpdateTokenParams updateParameters, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await UpdateTokenImplementationAsync(updateParameters, configure, true));
        }
        /// <summary>
        /// Internal implementation of the update Token functionality.
        /// </summary>
        private async Task<NetworkResult> UpdateTokenImplementationAsync(UpdateTokenParams updateParameters, Action<IContext>? configure, bool includeRecord)
        {
            updateParameters = RequireInputParameter.UpdateParameters(updateParameters);
            await using var context = CreateChildContext(configure);
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
                updateTokenBody.Expiry = new Timestamp(updateParameters.Expiration.Value);
            }
            if (updateParameters.RenewPeriod.HasValue)
            {
                updateTokenBody.AutoRenewPeriod = new Duration(updateParameters.RenewPeriod.Value);
            }
            if (!(updateParameters.RenewAccount is null))
            {
                updateTokenBody.AutoRenewAccount = new AccountID(updateParameters.RenewAccount);
            }
            if (!(updateParameters.Memo is null))
            {
                updateTokenBody.Memo = updateParameters.Memo;
            }
            var transactionBody = new TransactionBody
            {
                TokenUpdate = updateTokenBody
            };
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Unable to update Token, status: {0}", updateParameters.Signatory);
        }
    }
}
