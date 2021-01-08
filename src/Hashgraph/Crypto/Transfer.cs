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
        /// Transfer tinybars from one account to another.
        /// </summary>
        /// <param name="fromAddress">
        /// The address to transfer the tinybars from.  Ensure that
        /// a signatory either in the context or passed with this
        /// call can fulfill the signing requrements to transfer 
        /// crypto out of the account identified by this address.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the tinybars.
        /// </param>
        /// <param name="amount">
        /// The amount of tinybars to transfer.
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
        public Task<TransactionReceipt> TransferAsync(Address fromAddress, Address toAddress, long amount, Action<IContext>? configure = null)
        {
            return TransferImplementationAsync<TransactionReceipt>(fromAddress, toAddress, amount, null, configure);
        }
        /// <summary>
        /// Transfer tinybars from one account to another.
        /// </summary>
        /// <param name="fromAddress">
        /// The address to transfer the tinybars from.  Ensure that
        /// a signatory either in the context or passed with this
        /// call can fulfill the signing requrements to transfer 
        /// crypto out of the account identified by this address.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the tinybars.
        /// </param>
        /// <param name="amount">
        /// The amount of tinybars to transfer.
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
        public Task<TransactionReceipt> TransferAsync(Address fromAddress, Address toAddress, long amount, Signatory signatory, Action<IContext>? configure = null)
        {
            return TransferImplementationAsync<TransactionReceipt>(fromAddress, toAddress, amount, signatory, configure);
        }
        /// <summary>
        /// Transfer tinybars from one account to another.
        /// </summary>
        /// <param name="fromAddress">
        /// The address to transfer the tinybars from.  Ensure that
        /// a signatory either in the context can fulfill the signing 
        /// requrements to transfer crypto out of the account identified 
        /// by this address.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the tinybars.
        /// </param>
        /// <param name="amount">
        /// The amount of tinybars to transfer.
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
        public Task<TransactionRecord> TransferWithRecordAsync(Address fromAddress, Address toAddress, long amount, Action<IContext>? configure = null)
        {
            return TransferImplementationAsync<TransactionRecord>(fromAddress, toAddress, amount, null, configure);
        }
        /// <summary>
        /// Transfer tinybars from one account to another.
        /// </summary>
        /// <param name="fromAddress">
        /// The address to transfer the tinybars from.  Ensure that
        /// a signatory either in the context or passed with this
        /// call can fulfill the signing requrements to transfer 
        /// crypto out of the account identified by this address.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the tinybars.
        /// </param>
        /// <param name="amount">
        /// The amount of tinybars to transfer.
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
        public Task<TransactionRecord> TransferWithRecordAsync(Address fromAddress, Address toAddress, long amount, Signatory signatory, Action<IContext>? configure = null)
        {
            return TransferImplementationAsync<TransactionRecord>(fromAddress, toAddress, amount, signatory, configure);
        }
        /// <summary>
        /// Transfer tinybars from an arbitray set of accounts to
        /// another arbitrary set of accounts.
        /// </summary>
        /// <param name="transfers">
        /// A dictionary mapping how much crypto to transfer
        /// from and to each address.  Negative values send 
        /// crypto out of the account, positive values receive
        /// crypto into the account.  The value of all the 
        /// transfer values in the dictionary must sum to zero.
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
        [Obsolete("TransferAsync accepting only a dictionary of crypto transfers is depricated, please use TransferAsync accepting the TransferParams instead.")]
        public Task<TransactionReceipt> TransferAsync(IEnumerable<KeyValuePair<Address, long>> transfers, Action<IContext>? configure = null)
        {
            var cryptoTransfers = RequireInputParameter.CryptoTransferList(transfers);
            return TransferImplementationAsync<TransactionReceipt>(cryptoTransfers, null, null, configure);
        }
        /// <summary>
        /// Transfer tinybars from an arbitray set of accounts to
        /// another arbitrary set of accounts.
        /// </summary>
        /// <param name="transfers">
        /// A dictionary mapping how much crypto to transfer
        /// from and to each address.  Negative values send 
        /// crypto out of the account, positive values receive
        /// crypto into the account.  The value of all the 
        /// transfer values in the dictionary must sum to zero.
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
        [Obsolete("TransferAsync accepting only a dictionary of crypto transfers is depricated, please use TransferAsync accepting the TransferParams instead.")]
        public Task<TransactionReceipt> TransferAsync(IEnumerable<KeyValuePair<Address, long>> transfers, Signatory signatory, Action<IContext>? configure = null)
        {
            var cryptoTransfers = RequireInputParameter.CryptoTransferList(transfers);
            return TransferImplementationAsync<TransactionReceipt>(cryptoTransfers, null, signatory, configure);
        }
        /// <summary>
        /// Transfer tinybars from an arbitray set of accounts to
        /// another arbitrary set of accounts.
        /// </summary>
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
        [Obsolete("TransferWithRecordAsync accepting only a dictionary of crypto transfers is depricated, please use TransferWithRecordAsync accepting the TransferParams instead.")]
        public Task<TransactionRecord> TransferWithRecordAsync(IEnumerable<KeyValuePair<Address, long>> transfers, Action<IContext>? configure = null)
        {
            var cryptoTransfers = RequireInputParameter.CryptoTransferList(transfers);
            return TransferImplementationAsync<TransactionRecord>(cryptoTransfers, null, null, configure);
        }
        /// <summary>
        /// Transfer tinybars from an arbitray set of accounts to
        /// another arbitrary set of accounts.
        /// </summary>
        /// <param name="transfers">
        /// A dictionary mapping how much crypto to transfer
        /// from and to each address.  Negative values send 
        /// crypto out of the account, positive values receive
        /// crypto into the account.  The value of all the 
        /// transfer values in the dictionary must sum to zero.
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
        [Obsolete("TransferWithRecordAsync accepting only a dictionary of crypto transfers is depricated, please use TransferWithRecordAsync accepting the TransferParams instead.")]
        public Task<TransactionRecord> TransferWithRecordAsync(IEnumerable<KeyValuePair<Address, long>> transfers, Signatory signatory, Action<IContext>? configure = null)
        {
            var cryptoTransfers = RequireInputParameter.CryptoTransferList(transfers);
            return TransferImplementationAsync<TransactionRecord>(cryptoTransfers, null, signatory, configure);
        }
        /// <summary>
        /// Transfer cryptocurrency and tokens in the same transaction atomically among multiple hedera accounts and contracts.
        /// </summary>
        /// <param name="transfers">
        /// A transfers parameter object holding lists of crypto and token transfers to perform.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer receipt indicating success of the consensus operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionReceipt> TransferAsync(TransferParams transfers, Action<IContext>? configure = null)
        {
            var (cryptoTransfers, tokenTransfers) = RequireInputParameter.CryptoAndTransferList(transfers);
            return TransferImplementationAsync<TransactionReceipt>(cryptoTransfers, tokenTransfers, transfers.Signatory, configure);
        }
        /// <summary>
        /// Transfer cryptocurrency and tokens in the same transaction atomically among multiple hedera accounts and contracts.
        /// </summary>
        /// <param name="transfers">
        /// A transfers parameter object holding lists of crypto and token transfers to perform.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer receipt indicating success of the consensus operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public Task<TransactionRecord> TransferWithRecordAsync(TransferParams transfers, Action<IContext>? configure = null)
        {
            var (cryptoTransfers, tokenTransfers) = RequireInputParameter.CryptoAndTransferList(transfers);
            return TransferImplementationAsync<TransactionRecord>(cryptoTransfers, tokenTransfers, transfers.Signatory, configure);
        }
        /// <summary>
        /// Internal implementation for Transfer Crypto.
        /// Returns either a receipt or record or throws
        /// an exception.
        /// </summary>
        private async Task<TResult> TransferImplementationAsync<TResult>(Address fromAddress, Address toAddress, long amount, Signatory? signatory, Action<IContext>? configure) where TResult : new()
        {
            fromAddress = RequireInputParameter.FromAddress(fromAddress);
            toAddress = RequireInputParameter.ToAddress(toAddress);
            amount = RequireInputParameter.Amount(amount);
            var cryptoTransfers = RequireInputParameter.CryptoTransferList(new[] { KeyValuePair.Create(fromAddress, -amount), KeyValuePair.Create(toAddress, amount) });
            return await TransferImplementationAsync<TResult>(cryptoTransfers, null, signatory, configure);
        }
        /// <summary>
        /// Internal implementation for Multi Account Transfer Crypto and Tokens.
        /// Returns either a receipt or record or throws an exception.
        /// </summary>
        private async Task<TResult> TransferImplementationAsync<TResult>(TransferList? cryptoTransfers, IEnumerable<TokenTransferList>? tokenTransfers, Signatory? signatory, Action<IContext>? configure) where TResult : new()
        {
            await using var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var signatories = Transactions.GatherSignatories(context, signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId);
            transactionBody.CryptoTransfer = new CryptoTransferTransactionBody();
            if (cryptoTransfers != null)
            {
                transactionBody.CryptoTransfer.Transfers = cryptoTransfers;
            }
            if (tokenTransfers != null)
            {
                transactionBody.CryptoTransfer.TokenTransfers.AddRange(tokenTransfers);
            }
            var precheck = await Transactions.SignAndSubmitTransactionWithRetryAsync(transactionBody, signatories, context, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to execute transfers, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
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
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Transaction request) => await client.cryptoTransferAsync(request);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
