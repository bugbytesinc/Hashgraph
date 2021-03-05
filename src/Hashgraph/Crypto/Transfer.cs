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
        public async Task<TransactionReceipt> TransferAsync(Address fromAddress, Address toAddress, long amount, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await TransferImplementationAsync(fromAddress, toAddress, amount, null, configure, false));
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
        public async Task<TransactionReceipt> TransferAsync(Address fromAddress, Address toAddress, long amount, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await TransferImplementationAsync(fromAddress, toAddress, amount, signatory, configure, false));
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
        public async Task<TransactionRecord> TransferWithRecordAsync(Address fromAddress, Address toAddress, long amount, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await TransferImplementationAsync(fromAddress, toAddress, amount, null, configure, true));
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
        public async Task<TransactionRecord> TransferWithRecordAsync(Address fromAddress, Address toAddress, long amount, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await TransferImplementationAsync(fromAddress, toAddress, amount, signatory, configure, true));
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
        public async Task<TransactionReceipt> TransferAsync(TransferParams transfers, Action<IContext>? configure = null)
        {
            var (cryptoTransfers, tokenTransfers) = RequireInputParameter.CryptoAndTransferList(transfers);
            return new TransactionReceipt(await TransferImplementationAsync(cryptoTransfers, tokenTransfers, transfers.Signatory, configure, false));
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
        public async Task<TransactionRecord> TransferWithRecordAsync(TransferParams transfers, Action<IContext>? configure = null)
        {
            var (cryptoTransfers, tokenTransfers) = RequireInputParameter.CryptoAndTransferList(transfers);
            return new TransactionRecord(await TransferImplementationAsync(cryptoTransfers, tokenTransfers, transfers.Signatory, configure, true));
        }
        /// <summary>
        /// Internal implementation for Transfer Crypto.
        /// Returns either a receipt or record or throws
        /// an exception.
        /// </summary>
        private Task<NetworkResult> TransferImplementationAsync(Address fromAddress, Address toAddress, long amount, Signatory? signatory, Action<IContext>? configure, bool includeRecord)
        {
            fromAddress = RequireInputParameter.FromAddress(fromAddress);
            toAddress = RequireInputParameter.ToAddress(toAddress);
            amount = RequireInputParameter.Amount(amount);
            var cryptoTransfers = RequireInputParameter.CryptoTransferList(new[] { KeyValuePair.Create(fromAddress, -amount), KeyValuePair.Create(toAddress, amount) });
            return TransferImplementationAsync(cryptoTransfers, null, signatory, configure, includeRecord);
        }
        /// <summary>
        /// Internal implementation for Multi Account Transfer Crypto and Tokens.
        /// Returns either a receipt or record or throws an exception.
        /// </summary>
        private async Task<NetworkResult> TransferImplementationAsync(TransferList? cryptoTransfers, IEnumerable<TokenTransferList>? tokenTransfers, Signatory? signatory, Action<IContext>? configure, bool includeRecord)
        {
            await using var context = CreateChildContext(configure);
            var transactionBody = new TransactionBody
            {
                CryptoTransfer = new CryptoTransferTransactionBody()
            };
            if (cryptoTransfers != null)
            {
                transactionBody.CryptoTransfer.Transfers = cryptoTransfers;
            }
            if (tokenTransfers != null)
            {
                transactionBody.CryptoTransfer.TokenTransfers.AddRange(tokenTransfers);
            }
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Unable to execute transfers, status: {0}", signatory);
        }
    }
}
