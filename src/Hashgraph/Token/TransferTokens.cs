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
        /// Transfer tokens from one account to another.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token to transfer.
        /// </param>
        /// <param name="fromAddress">
        /// The address to transfer the tokens from.  Ensure that
        /// a signatory either in the context or passed with this
        /// call can fulfill the signing requrements to transfer 
        /// tokens out of the account identified by this address.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the tokens.
        /// </param>
        /// <param name="amount">
        /// The amount of tokens to transfer.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer receipt indicating success of the operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> TransferTokensAsync(Address token, Address fromAddress, Address toAddress, long amount, Action<IContext>? configure = null)
        {
            return TransferTokenImplementationAsync<TransactionReceipt>(token, fromAddress, toAddress, amount, null, configure);
        }
        /// <summary>
        /// Transfer tokens from one account to another.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token to transfer.
        /// </param>
        /// <param name="fromAddress">
        /// The address to transfer the tokens from.  Ensure that
        /// a signatory either in the context or passed with this
        /// call can fulfill the signing requrements to transfer 
        /// crypto out of the account identified by this address.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the tokens.
        /// </param>
        /// <param name="amount">
        /// The amount of tokens to transfer.
        /// </param>
        /// <param name="signatory">
        /// The signatory containing any additional private keys or callbacks
        /// to meet the requirements for the sending and receiving accounts.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer receipt indicating success of the operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> TransferTokensAsync(Address token, Address fromAddress, Address toAddress, long amount, Signatory signatory, Action<IContext>? configure = null)
        {
            return TransferTokenImplementationAsync<TransactionReceipt>(token, fromAddress, toAddress, amount, signatory, configure);
        }
        /// <summary>
        /// Transfer tokens from one account to another.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token to transfer.
        /// </param>
        /// <param name="fromAddress">
        /// The address to transfer the tokens from.  Ensure that
        /// a signatory either in the context can fulfill the signing 
        /// requrements to transfer crypto out of the account identified 
        /// by this address.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the tokens.
        /// </param>
        /// <param name="amount">
        /// The amount of tokens to transfer.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer record describing the details of the concensus transaction.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionRecord> TransferTokensWithRecordAsync(Address token, Address fromAddress, Address toAddress, long amount, Action<IContext>? configure = null)
        {
            return TransferTokenImplementationAsync<TransactionRecord>(token, fromAddress, toAddress, amount, null, configure);
        }
        /// <summary>
        /// Transfer tokens from one account to another.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token to transfer.
        /// </param>
        /// <param name="fromAddress">
        /// The address to transfer the tokens from.  Ensure that
        /// a signatory either in the context or passed with this
        /// call can fulfill the signing requrements to transfer 
        /// crypto out of the account identified by this address.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the tokens.
        /// </param>
        /// <param name="amount">
        /// The amount of tokens to transfer.
        /// </param>
        /// <param name="signatory">
        /// The signatory containing any additional private keys or callbacks
        /// to meet the requirements for the sending and receiving accounts.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer record describing the details of the concensus transaction.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionRecord> TransferTokensWithRecordAsync(Address token, Address fromAddress, Address toAddress, long amount, Signatory signatory, Action<IContext>? configure = null)
        {
            return TransferTokenImplementationAsync<TransactionRecord>(token, fromAddress, toAddress, amount, signatory, configure);
        }
        /// <summary>
        /// Transfer tokens from an arbitray set of accounts to
        /// another arbitrary set of accounts.
        /// </summary>
        /// <param name="transfers">
        /// A list of transfers of tokens to and from account
        /// addresses. Negative values send coins out of the account, 
        /// positive values receive coins into the account.  The value 
        /// of all the transfer values for a given token in the list
        /// must sum to zero.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer receipt indicating success of the operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> TransferTokensAsync(IEnumerable<TokenTransfer> transfers, Action<IContext>? configure = null)
        {
            return TransferTokenImplementationAsync<TransactionReceipt>(transfers, null, configure);
        }
        /// <summary>
        /// Transfer tokens from an arbitray set of accounts to
        /// another arbitrary set of accounts.
        /// </summary>
        /// <param name="transfers">
        /// A list of transfers of tokens to and from account
        /// addresses. Negative values send coins out of the account, 
        /// positive values receive coins into the account.  The value 
        /// of all the transfer values for a given token in the list
        /// must sum to zero.
        /// </param>
        /// <param name="signatory">
        /// The signatory containing any additional private keys or callbacks
        /// to meet the requirements for the sending and receiving accounts.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer receipt indicating success of the operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> TransferTokensAsync(IEnumerable<TokenTransfer> transfers, Signatory signatory, Action<IContext>? configure = null)
        {
            return TransferTokenImplementationAsync<TransactionReceipt>(transfers, signatory, configure);
        }
        /// <summary>
        /// Transfer tokens from an arbitray set of accounts to
        /// another arbitrary set of accounts.
        /// </summary>
        /// <param name="transfers">
        /// A list of transfers of tokens to and from account
        /// addresses. Negative values send coins out of the account, 
        /// positive values receive coins into the account.  The value 
        /// of all the transfer values for a given token in the list
        /// must sum to zero.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer record describing the details of the concensus transaction.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionRecord> TransferTokensWithRecordAsync(IEnumerable<TokenTransfer> transfers, Action<IContext>? configure = null)
        {
            return TransferTokenImplementationAsync<TransactionRecord>(transfers, null, configure);
        }
        /// <summary>
        /// Transfer tokens from an arbitray set of accounts to
        /// another arbitrary set of accounts.
        /// </summary>
        /// <param name="transfers">
        /// A list of transfers of tokens to and from account
        /// addresses. Negative values send coins out of the account, 
        /// positive values receive coins into the account.  The value 
        /// of all the transfer values for a given token in the list
        /// must sum to zero.
        /// </param>
        /// <param name="signatory">
        /// The signatory containing any additional private keys or callbacks
        /// to meet the requirements for the sending and receiving accounts.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer record describing the details of the concensus transaction.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionRecord> TransferTokensWithRecordAsync(IEnumerable<TokenTransfer> transfers, Signatory signatory, Action<IContext>? configure = null)
        {
            return TransferTokenImplementationAsync<TransactionRecord>(transfers, signatory, configure);
        }
        /// <summary>
        /// Internal implementation for Single Transfer Crypto
        /// Returns either a receipt or record or throws
        /// an exception.
        /// </summary>
        private async Task<TResult> TransferTokenImplementationAsync<TResult>(Address token, Address fromAddress, Address toAddress, long amount, Signatory? signatory, Action<IContext>? configure) where TResult : new()
        {
            token = RequireInputParameter.Token(token);
            fromAddress = RequireInputParameter.FromAddress(fromAddress);
            toAddress = RequireInputParameter.ToAddress(toAddress);
            amount = RequireInputParameter.Amount(amount);
            var tokenTransfers = Transactions.CreateTokenTransfers(fromAddress, toAddress, token, amount);
            return await TransferTokenImplementationAsync<TResult>(tokenTransfers, signatory, configure);
        }
        /// <summary>
        /// Internal implementation for Multi Account Transfer Crypto.
        /// Returns either a receipt or record or throws an exception.
        /// </summary>
        private Task<TResult> TransferTokenImplementationAsync<TResult>(IEnumerable<TokenTransfer> transfers, Signatory? signatory, Action<IContext>? configure) where TResult : new()
        {
            var tokenTransfers = Transactions.CreateTokenTransfers(transfers);
            return TransferTokenImplementationAsync<TResult>(tokenTransfers, signatory, configure);
        }
        /// <summary>
        /// Internal implementation for Multi Account Transfer Crypto.
        /// Returns either a receipt or record or throws an exception.
        /// </summary>
        private async Task<TResult> TransferTokenImplementationAsync<TResult>(TokenTransfersTransactionBody transfers, Signatory? signatory, Action<IContext>? configure) where TResult : new()
        {
            await using var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var signatories = Transactions.GatherSignatories(context, signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId);
            transactionBody.TokenTransfers = transfers;
            var request = await Transactions.SignTransactionAsync(transactionBody, signatories);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to execute token transfers, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
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
                return async (Transaction request) => await client.transferTokensAsync(request);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
