using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Updates the properties or contents of an existing file stored in the network.
        /// </summary>
        /// <param name="updateParameters">
        /// Update parameters indicating the file to update and what properties such 
        /// as the access key or content that should be updated.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating the operation was successful.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> UpdateFileAsync(UpdateFileParams updateParameters, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new FileUpdateTransactionBody(updateParameters), configure, false, updateParameters.Signatory));
        }
        /// <summary>
        /// Updates the properties or contents of an existing file stored in the network.
        /// </summary>
        /// <param name="updateParameters">
        /// Update parameters indicating the file to update and what properties such 
        /// as the access key or content that should be updated.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record describing the details of the operation 
        /// including fees and transaction hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> UpdateFileWithRecordAsync(UpdateFileParams updateParameters, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await ExecuteTransactionAsync(new FileUpdateTransactionBody(updateParameters), configure, true, updateParameters.Signatory));
        }
    }
}
