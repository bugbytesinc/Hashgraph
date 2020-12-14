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
        /// Removes the holdings of given token from the associated 
        /// account and returns them to the treasury. Must be signed by 
        /// the confiscate/wipe admin key.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token to confiscate.
        /// </param>
        /// <param name="address">
        /// Address of the account holding the tokens to remove.
        /// </param>
        /// <param name="amount">
        /// The amount of coins to confiscate (of the divisible denomination)
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
        public Task<TokenReceipt> ConfiscateTokensAsync(Address token, Address address, ulong amount, Action<IContext>? configure = null)
        {
            return ConfiscateTokensImplementationAsync<TokenReceipt>(token, address, amount, null, configure);
        }
        /// <summary>
        /// Removes the holdings of given token from the associated 
        /// account and returns them to the treasury. Must be signed by 
        /// the confiscate/wipe admin key.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token to confiscate.
        /// </param>
        /// <param name="address">
        /// Address of the account holding the tokens to remove.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// associated with this token (if not already added in the context).
        /// </param>
        /// <param name="amount">
        /// The amount of coins to confiscate (of the divisible denomination)
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
        public Task<TokenReceipt> ConfiscateTokensAsync(Address token, Address address, ulong amount, Signatory signatory, Action<IContext>? configure = null)
        {
            return ConfiscateTokensImplementationAsync<TokenReceipt>(token, address, amount, signatory, configure);
        }
        /// <summary>
        /// Removes the holdings of given token from the associated 
        /// account and returns them to the treasury. Must be signed by 
        /// the confiscate/wipe admin key.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token to confiscate.
        /// </param>
        /// <param name="address">
        /// Address of the account holding the tokens to remove.
        /// </param>
        /// <param name="amount">
        /// The amount of coins to confiscate (of the divisible denomination)
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
        public Task<TokenRecord> ConfiscateTokensWithRecordAsync(Address token, Address address, ulong amount, Action<IContext>? configure = null)
        {
            return ConfiscateTokensImplementationAsync<TokenRecord>(token, address, amount, null, configure);
        }
        /// <summary>
        /// Removes the holdings of given token from the associated 
        /// account and returns them to the treasury. Must be signed by 
        /// the confiscate/wipe admin key.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token to confiscate.
        /// </param>
        /// <param name="address">
        /// Address of the account holding the tokens to remove.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// associated with this token (if not already added in the context).
        /// </param>
        /// <param name="amount">
        /// The amount of coins to confiscate (of the divisible denomination)
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
        public Task<TokenRecord> ConfiscateTokensWithRecordAsync(Address token, Address address, ulong amount, Signatory signatory, Action<IContext>? configure = null)
        {
            return ConfiscateTokensImplementationAsync<TokenRecord>(token, address, amount, signatory, configure);
        }
        /// <summary>
        /// Internal implementation of delete token method.
        /// </summary>
        private async Task<TResult> ConfiscateTokensImplementationAsync<TResult>(Address token, Address address, ulong amount, Signatory? signatory, Action<IContext>? configure) where TResult : new()
        {
            token = RequireInputParameter.Token(token);
            address = RequireInputParameter.Address(address);
            amount = RequireInputParameter.ConfiscateAmount(amount);
            await using var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatories = Transactions.GatherSignatories(context, signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId);
            transactionBody.TokenWipe = new TokenWipeAccountTransactionBody
            {
                Token = new TokenID(token),
                Account = new AccountID(address),
                Amount = amount
            };
            var request = await Transactions.SignTransactionAsync(transactionBody, signatories);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to Confiscate Token, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is TokenRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                record.FillProperties(rec);
            }
            else if (result is TokenReceipt rcpt)
            {
                receipt.FillProperties(transactionId, rcpt);
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new TokenService.TokenServiceClient(channel);
                return async (Transaction transaction) => await client.wipeTokenAccountAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
