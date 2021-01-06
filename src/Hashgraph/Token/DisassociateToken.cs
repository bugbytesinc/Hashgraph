using Grpc.Core;
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
        /// Removes Storage associated with the Account for maintaining token balances 
        /// for this account.
        /// </summary>
        /// <remarks>
        /// Since this action modifies the account's records, 
        /// it must be signed by the account's key.
        /// </remarks>
        /// <param name="token">
        /// The Address of the token that will be dissociated.
        /// </param>
        /// <param name="account">
        /// Address of the account that will be dissociated.
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
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token has already been dissociated.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> DissociateTokenAsync(Address token, Address account, Action<IContext>? configure = null)
        {
            var list = new TokenID[] { new TokenID(RequireInputParameter.Token(token)) };
            return DissociateTokenImplementationAsync<TransactionReceipt>(list, account, null, configure);
        }
        /// <summary>
        /// Removes Storage associated with the Account for maintaining token balances 
        /// for this account.
        /// </summary>
        /// <remarks>
        /// Since this action modifies the account's records, 
        /// it must be signed by the account's key.
        /// </remarks>
        /// <param name="tokens">
        /// The Address of the tokens that will be dissociated.
        /// </param>
        /// <param name="account">
        /// Address of the account that will be dissociated.
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
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the token has already been dissociated.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> DissociateTokensAsync(IEnumerable<Address> tokens, Address account, Action<IContext>? configure = null)
        {
            var list = RequireInputParameter.Tokens(tokens);
            return DissociateTokenImplementationAsync<TransactionReceipt>(list, account, null, configure);
        }
        /// <summary>
        /// Removes Storage associated with the Account for maintaining token balances 
        /// for this account.
        /// </summary>
        /// <remarks>
        /// Since this action modifies the account's records, 
        /// it must be signed by the account's key.
        /// </remarks>
        /// <param name="token">
        /// The Address of the token that will be dissociated.
        /// </param>
        /// <param name="account">
        /// Address of the account that will be dissociated.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// dissociated with this token (if not already added in the context).
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
        public Task<TransactionReceipt> DissociateTokenAsync(Address token, Address account, Signatory signatory, Action<IContext>? configure = null)
        {
            var list = new TokenID[] { new TokenID(RequireInputParameter.Token(token)) };
            return DissociateTokenImplementationAsync<TransactionReceipt>(list, account, signatory, configure);
        }
        /// <summary>
        /// Removes Storage associated with the Account for maintaining token balances 
        /// for this account.
        /// </summary>
        /// <remarks>
        /// Since this action modifies the account's records, 
        /// it must be signed by the account's key.
        /// </remarks>
        /// <param name="tokens">
        /// The Address of the tokens that will be dissociated.
        /// </param>
        /// <param name="account">
        /// Address of the account that will be dissociated.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// dissociated with this token (if not already added in the context).
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
        public Task<TransactionReceipt> DissociateTokensAsync(IEnumerable<Address> tokens, Address account, Signatory signatory, Action<IContext>? configure = null)
        {
            var list = RequireInputParameter.Tokens(tokens);
            return DissociateTokenImplementationAsync<TransactionReceipt>(list, account, signatory, configure);
        }
        /// <summary>
        /// Removes Storage associated with the Account for maintaining token balances 
        /// for this account.
        /// </summary>
        /// <remarks>
        /// Since this action modifies the account's records, 
        /// it must be signed by the account's key.
        /// </remarks>
        /// <param name="token">
        /// The Address of the token that will be dissociated.
        /// </param>
        /// <param name="account">
        /// Address of the account that will be dissociated.
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
        public Task<TransactionRecord> DissociateTokenWithRecordAsync(Address token, Address account, Action<IContext>? configure = null)
        {
            var list = new TokenID[] { new TokenID(RequireInputParameter.Token(token)) };
            return DissociateTokenImplementationAsync<TransactionRecord>(list, account, null, configure);
        }
        /// <summary>
        /// Removes Storage associated with the Account for maintaining token balances 
        /// for this account.
        /// </summary>
        /// <remarks>
        /// Since this action modifies the account's records, 
        /// it must be signed by the account's key.
        /// </remarks>
        /// <param name="tokens">
        /// The Address of the tokens that will be dissociated.
        /// </param>
        /// <param name="account">
        /// Address of the account that will be dissociated.
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
        public Task<TransactionRecord> DissociateTokensWithRecordAsync(IEnumerable<Address> tokens, Address account, Action<IContext>? configure = null)
        {
            var list = RequireInputParameter.Tokens(tokens);
            return DissociateTokenImplementationAsync<TransactionRecord>(list, account, null, configure);
        }
        /// <summary>
        /// Removes Storage associated with the Account for maintaining token balances 
        /// for this account.
        /// </summary>
        /// <remarks>
        /// Since this action modifies the account's records, 
        /// it must be signed by the account's key.
        /// </remarks>
        /// <param name="token">
        /// The Address of the token that will be dissociated.
        /// </param>
        /// <param name="account">
        /// Address of the account that will be dissociated.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// dissociated with this token (if not already added in the context).
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
        public Task<TransactionRecord> DissociateTokenWithRecordAsync(Address token, Address account, Signatory signatory, Action<IContext>? configure = null)
        {
            var list = new TokenID[] { new TokenID(RequireInputParameter.Token(token)) };
            return DissociateTokenImplementationAsync<TransactionRecord>(list, account, signatory, configure);
        }
        /// <summary>
        /// Removes Storage associated with the Account for maintaining token balances 
        /// for this account.
        /// </summary>
        /// <remarks>
        /// Since this action modifies the account's records, 
        /// it must be signed by the account's key.
        /// </remarks>
        /// <param name="tokens">
        /// The Address of the tokens that will be dissociated.
        /// </param>
        /// <param name="account">
        /// Address of the account that will be dissociated.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// dissociated with this token (if not already added in the context).
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
        public Task<TransactionRecord> DissociateTokensWithRecordAsync(IEnumerable<Address> tokens, Address account, Signatory signatory, Action<IContext>? configure = null)
        {
            var list = RequireInputParameter.Tokens(tokens);
            return DissociateTokenImplementationAsync<TransactionRecord>(list, account, signatory, configure);
        }
        /// <summary>
        /// Internal implementation of dissociate method.
        /// </summary>
        private async Task<TResult> DissociateTokenImplementationAsync<TResult>(TokenID[] tokens, Address account, Signatory? signatory, Action<IContext>? configure) where TResult : new()
        {
            account = RequireInputParameter.Account(account);
            await using var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatories = Transactions.GatherSignatories(context, signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId);
            transactionBody.TokenDissociate = new TokenDissociateTransactionBody
            {
                Account = new AccountID(account)
            };
            transactionBody.TokenDissociate.Tokens.AddRange(tokens);
            var request = await Transactions.SignTransactionAsync(transactionBody, signatories, context.SignaturePrefixTrimLimit);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to Dissociate Token from Account, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
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
                return async (Transaction transaction) => await client.dissociateTokensAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
