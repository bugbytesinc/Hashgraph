using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Continues/Un-Pauses all accounts' ability to send or
        /// receive the specified token (unless they have been
        /// suspended/frozen)
        /// </summary>
        /// <param name="token">
        /// The identifier (Address) of the token to continue/un-pause
        /// to re-enable transfers between accounts.
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
        public async Task<TransactionReceipt> ContinueTokenAsync(Address token, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new TokenUnpauseTransactionBody(token), configure, false).ConfigureAwait(false));
        }
        /// <summary>
        /// Continues/Un-Pauses all accounts' ability to send or
        /// receive the specified token (unless they have been
        /// </summary>
        /// <param name="token">
        /// The identifier (Address) of the token to continue/un-pause
        /// to re-enable transfers between accounts.
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
        public async Task<TransactionReceipt> ContinueTokenAsync(Address token, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new TokenUnpauseTransactionBody(token), configure, false, signatory).ConfigureAwait(false));
        }
        /// <summary>
        /// Continues/Un-Pauses all accounts' ability to send or
        /// receive the specified token (unless they have been
        /// </summary>
        /// <param name="token">
        /// The identifier (Address) of the token to continue/un-pause
        /// to re-enable transfers between accounts.
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
        public async Task<TransactionRecord> ContinueTokenWithRecordAsync(Address token, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await ExecuteTransactionAsync(new TokenUnpauseTransactionBody(token), configure, true).ConfigureAwait(false));
        }
        /// <summary>
        /// Continues/Un-Pauses all accounts' ability to send or
        /// receive the specified token (unless they have been
        /// </summary>
        /// <param name="token">
        /// The identifier (Address) of the token to continue/un-pause
        /// to re-enable transfers between accounts.
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
        public async Task<TransactionRecord> ContinueTokenWithRecordAsync(Address token, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await ExecuteTransactionAsync(new TokenUnpauseTransactionBody(token), configure, true, signatory).ConfigureAwait(false));
        }
    }
}
