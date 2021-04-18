using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Deletes an account from the network returning the remaining 
        /// crypto balance to the specified account.  Must be signed 
        /// by the account being deleted.
        /// </summary>
        /// <param name="addressToDelete">
        /// The address for account that will be deleted.
        /// </param>
        /// <param name="transferToAddress">
        /// The account that will receive any remaining balance from the deleted account.
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
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> DeleteAccountAsync(Address addressToDelete, Address transferToAddress, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new CryptoDeleteTransactionBody(addressToDelete, transferToAddress), configure, false).ConfigureAwait(false));
        }
        /// <summary>
        /// Deletes an account from the network returning the remaining 
        /// crypto balance to the specified account.  Must be signed 
        /// by the account being deleted.
        /// </summary>
        /// <param name="addressToDelete">
        /// The address for account that will be deleted.
        /// </param>
        /// <param name="transferToAddress">
        /// The account that will receive any remaining balance from the deleted account.
        /// </param>
        /// <param name="signatory">
        /// The signatory containing any additional private keys or callbacks
        /// to meet the requirements for deleting the account and transferring 
        /// the remaining crypto balance.
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
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> DeleteAccountAsync(Address addressToDelete, Address transferToAddress, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new CryptoDeleteTransactionBody(addressToDelete, transferToAddress), configure, false, signatory).ConfigureAwait(false));
        }
    }
}
