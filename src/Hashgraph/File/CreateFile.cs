using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Creates a new file with the given content.
        /// </summary>
        /// <param name="createParameters">
        /// File creation parameters specifying contents and ownership of the file.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt with a description of the newly created file.
        /// and record information.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<FileReceipt> CreateFileAsync(CreateFileParams createParameters, Action<IContext>? configure = null)
        {
            return new FileReceipt(await ExecuteTransactionAsync(new FileCreateTransactionBody(createParameters), configure, false, createParameters.Signatory).ConfigureAwait(false));
        }
        /// <summary>
        /// Creates a new file with the given content.
        /// </summary>
        /// <param name="createParameters">
        /// File creation parameters specifying contents and ownership of the file.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record with a description of the newly created file & fees.
        /// and record information.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<FileRecord> CreateFileWithRecordAsync(CreateFileParams createParameters, Action<IContext>? configure = null)
        {
            return new FileRecord(await ExecuteTransactionAsync(new FileCreateTransactionBody(createParameters), configure, true, createParameters.Signatory).ConfigureAwait(false));
        }
    }
}
