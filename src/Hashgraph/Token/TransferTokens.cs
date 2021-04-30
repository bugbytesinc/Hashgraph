using Proto;
using System;
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
        public async Task<TransactionReceipt> TransferTokensAsync(Address token, Address fromAddress, Address toAddress, long amount, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new CryptoTransferTransactionBody(token, fromAddress, toAddress, amount), configure, false).ConfigureAwait(false));
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
        public async Task<TransactionReceipt> TransferTokensAsync(Address token, Address fromAddress, Address toAddress, long amount, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new CryptoTransferTransactionBody(token, fromAddress, toAddress, amount), configure, false, signatory).ConfigureAwait(false));
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
        public async Task<TransactionRecord> TransferTokensWithRecordAsync(Address token, Address fromAddress, Address toAddress, long amount, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await ExecuteTransactionAsync(new CryptoTransferTransactionBody(token, fromAddress, toAddress, amount), configure, true).ConfigureAwait(false));
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
        public async Task<TransactionRecord> TransferTokensWithRecordAsync(Address token, Address fromAddress, Address toAddress, long amount, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await ExecuteTransactionAsync(new CryptoTransferTransactionBody(token, fromAddress, toAddress, amount), configure, true, signatory).ConfigureAwait(false));
        }
    }
}
