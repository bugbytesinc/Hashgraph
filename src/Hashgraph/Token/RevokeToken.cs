using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Revokes KYC status from the associated account's relating to the specified token.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token to revoke KYC status.
        /// </param>
        /// <param name="address">
        /// Address of the account to suspend.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> RevokeTokenKycAsync(Address token, Address address, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await RevokeTokenKycImplementationAsync(token, address, null, configure, false));
        }
        /// <summary>
        /// Revokes KYC status from the associated account's relating to the specified token.
        /// receive the specified token.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token to revoke KYC status.
        /// </param>
        /// <param name="address">
        /// Address of the account to suspend.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// associated with this token (if not already added in the context).
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> RevokeTokenKycAsync(Address token, Address address, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await RevokeTokenKycImplementationAsync(token, address, signatory, configure, false));
        }
        /// <summary>
        /// Revokes KYC status from the associated account's relating to the specified token.
        /// receive the specified token.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token to revoke KYC status.
        /// </param>
        /// <param name="address">
        /// Address of the account to suspend.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> RevokeTokenKycWithRecordAsync(Address token, Address address, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await RevokeTokenKycImplementationAsync(token, address, null, configure, true));
        }
        /// <summary>
        /// Revokes KYC status from the associated account's relating to the specified token.
        /// receive the specified token.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token to revoke KYC status.
        /// </param>
        /// <param name="address">
        /// Address of the account to suspend.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// associated with this token (if not already added in the context).
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> RevokeTokenKycWithRecordAsync(Address token, Address address, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await RevokeTokenKycImplementationAsync(token, address, signatory, configure, true));
        }
        /// <summary>
        /// Internal implementation of delete token method.
        /// </summary>
        private async Task<NetworkResult> RevokeTokenKycImplementationAsync(Address token, Address address, Signatory? signatory, Action<IContext>? configure, bool includeRecord)
        {
            token = RequireInputParameter.Token(token);
            address = RequireInputParameter.Address(address);
            await using var context = CreateChildContext(configure);
            var transactionBody = new TransactionBody
            {
                TokenRevokeKyc = new TokenRevokeKycTransactionBody
                {
                    Token = new TokenID(token),
                    Account = new AccountID(address)
                }
            };
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Unable to Revoke Token, status: {0}", signatory);
        }
    }
}
