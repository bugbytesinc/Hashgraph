using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Deletes a contract instance from the network returning the remaining 
        /// crypto balance to the specified address.  Must be signed 
        /// by the admin key.
        /// </summary>
        /// <param name="contractToDelete">
        /// The Contract instance that will be deleted.
        /// </param>
        /// <param name="transferToAddress">
        /// The address that will receive any remaining balance from the deleted Contract.
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
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the contract is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> DeleteContractAsync(Address contractToDelete, AddressOrAlias transferToAddress, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new ContractDeleteTransactionBody(contractToDelete, transferToAddress), configure, false).ConfigureAwait(false));
        }
        /// <summary>
        /// Deletes a contract instance from the network returning the remaining 
        /// crypto balance to the specified address.  Must be signed 
        /// by the admin key.
        /// </summary>
        /// <param name="contractToDelete">
        /// The Contract instance that will be deleted.
        /// </param>
        /// <param name="transferToAddress">
        /// The address that will receive any remaining balance from the deleted Contract.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key/callback matching the administrative endorsements
        /// associated with this contract (if not already added in the context).
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
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the contract is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> DeleteContractAsync(Address contractToDelete, Address transferToAddress, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new ContractDeleteTransactionBody(contractToDelete, transferToAddress), configure, false, signatory).ConfigureAwait(false));
        }
    }
}
