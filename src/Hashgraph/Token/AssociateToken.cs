using Hashgraph.Implementation;
using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Provisions Storage associated with the Account for maintaining token balances 
        /// for this account.
        /// </summary>
        /// <remarks>
        /// Since this action will result in higher account renewal costs, it must be signed
        /// by the account's key.
        /// </remarks>
        /// <param name="token">
        /// The identifier Address of the token to associate with the account.
        /// </param>
        /// <param name="account">
        /// Address of the account to provision token balance storage.
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
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token has already been associated.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> AssociateTokenAsync(Address token, Address account, Action<IContext>? configure = null)
        {
            var list = new TokenID[] { new TokenID(RequireInputParameter.Token(token)) };
            return new TransactionReceipt(await AssociateTokenImplementationAsync(list, account, null, configure, false));
        }
        /// <summary>
        /// Provisions Storage associated with the Account for maintaining 
        /// token balances for the listed tokens.
        /// </summary>
        /// <remarks>
        /// Since this action will result in higher account renewal costs, it must be signed
        /// by the account's key.
        /// </remarks>
        /// <param name="tokens">
        /// The identifier Addresses of the tokens to associate with the account.
        /// </param>
        /// <param name="account">
        /// Address of the account to provision token balance storage.
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
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token has already been associated.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> AssociateTokensAsync(IEnumerable<Address> tokens, Address account, Action<IContext>? configure = null)
        {
            var list = RequireInputParameter.Tokens(tokens);
            return new TransactionReceipt(await AssociateTokenImplementationAsync(list, account, null, configure, false));
        }
        /// <summary>
        /// Provisions Storage associated with the Account for maintaining token balances 
        /// for this account.
        /// </summary>
        /// <remarks>
        /// Since this action will result in higher account renewal costs, it must be signed
        /// by the account's key.
        /// </remarks>
        /// <param name="token">
        /// The identifier Address of the token to associate with the account.
        /// </param>
        /// <param name="account">
        /// Address of the account to provision token balance storage.
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
        public async Task<TransactionReceipt> AssociateTokenAsync(Address token, Address account, Signatory signatory, Action<IContext>? configure = null)
        {
            var list = new TokenID[] { new TokenID(RequireInputParameter.Token(token)) };
            return new TransactionReceipt(await AssociateTokenImplementationAsync(list, account, signatory, configure, false));
        }
        /// <summary>
        /// Provisions Storage associated with the Account for maintaining 
        /// token balances for the listed tokens.
        /// </summary>
        /// <remarks>
        /// Since this action will result in higher account renewal costs, it must be signed
        /// by the account's key.
        /// </remarks>
        /// <param name="tokens">
        /// The identifier Addresses of the tokens to associate with the account.
        /// </param>
        /// <param name="account">
        /// Address of the account to provision token balance storage.
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
        public async Task<TransactionReceipt> AssociateTokensAsync(IEnumerable<Address> tokens, Address account, Signatory signatory, Action<IContext>? configure = null)
        {
            var list = RequireInputParameter.Tokens(tokens);
            return new TransactionReceipt(await AssociateTokenImplementationAsync(list, account, signatory, configure, false));
        }
        /// <summary>
        /// Provisions Storage associated with the Account for maintaining token balances 
        /// for this account.
        /// </summary>
        /// <remarks>
        /// Since this action will result in higher account renewal costs, it must be signed
        /// by the account's key.
        /// </remarks>
        /// <param name="token">
        /// The identifier Address of the token to associate with the account.
        /// </param>
        /// <param name="account">
        /// Address of the account to provision token balance storage.
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
        public async Task<TransactionRecord> AssociateTokenWithRecordAsync(Address token, Address account, Action<IContext>? configure = null)
        {
            var list = new TokenID[] { new TokenID(RequireInputParameter.Token(token)) };
            return new TransactionRecord(await AssociateTokenImplementationAsync(list, account, null, configure, true));
        }
        /// <summary>
        /// Provisions Storage associated with the Account for maintaining 
        /// token balances for the listed tokens.
        /// </summary>
        /// <remarks>
        /// Since this action will result in higher account renewal costs, it must be signed
        /// by the account's key.
        /// </remarks>
        /// <param name="tokens">
        /// The identifier Addresses of the tokens to associate with the account.
        /// </param>
        /// <param name="account">
        /// Address of the account to provision token balance storage.
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
        public async Task<TransactionRecord> AssociateTokensWithRecordAsync(IEnumerable<Address> tokens, Address account, Action<IContext>? configure = null)
        {
            var list = RequireInputParameter.Tokens(tokens);
            return new TransactionRecord(await AssociateTokenImplementationAsync(list, account, null, configure, true));
        }
        /// <summary>
        /// Provisions Storage associated with the Account for maintaining token balances 
        /// for this account.
        /// </summary>
        /// <remarks>
        /// Since this action will result in higher account renewal costs, it must be signed
        /// by the account's key.
        /// </remarks>
        /// <param name="token">
        /// The identifier Address of the token to associate with the account.
        /// </param>
        /// <param name="account">
        /// Address of the account to provision token balance storage.
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
        public async Task<TransactionRecord> AssociateTokenWithRecordAsync(Address token, Address account, Signatory signatory, Action<IContext>? configure = null)
        {
            var list = new TokenID[] { new TokenID(RequireInputParameter.Token(token)) };
            return new TransactionRecord(await AssociateTokenImplementationAsync(list, account, signatory, configure, true));
        }
        /// <summary>
        /// Provisions Storage associated with the Account for maintaining 
        /// token balances for the listed tokens.
        /// </summary>
        /// <remarks>
        /// Since this action will result in higher account renewal costs, it must be signed
        /// by the account's key.
        /// </remarks>
        /// <param name="tokens">
        /// The identifier Addresses of the tokens to associate with the account.
        /// </param>
        /// <param name="account">
        /// Address of the account to provision token balance storage.
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
        public async Task<TransactionRecord> AssociateTokensWithRecordAsync(IEnumerable<Address> tokens, Address account, Signatory signatory, Action<IContext>? configure = null)
        {
            var list = RequireInputParameter.Tokens(tokens);
            return new TransactionRecord(await AssociateTokenImplementationAsync(list, account, signatory, configure, true));
        }
        /// <summary>
        /// Internal implementation of associate method.
        /// </summary>
        private async Task<NetworkResult> AssociateTokenImplementationAsync(TokenID[] tokens, Address account, Signatory? signatory, Action<IContext>? configure, bool includeRecord)
        {
            account = RequireInputParameter.Account(account);
            await using var context = CreateChildContext(configure);
            var transactionBody = new TransactionBody
            {
                TokenAssociate = new TokenAssociateTransactionBody
                {
                    Account = new AccountID(account)
                }
            };
            transactionBody.TokenAssociate.Tokens.AddRange(tokens);
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Unable to associate Token with Account, status: {0}", signatory);
        }
    }
}
