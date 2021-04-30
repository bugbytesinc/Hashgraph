using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Creates a new network account with a given initial balance
        /// and other values as indicated in the create parameters.
        /// </summary>
        /// <param name="createParameters">
        /// The account creation parameters, includes the initial balance,
        /// public key and values associated with the new account.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction recipt with a description of the newly created account.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<CreateAccountReceipt> CreateAccountAsync(CreateAccountParams createParameters, Action<IContext>? configure = null)
        {
            return new CreateAccountReceipt(await ExecuteTransactionAsync(new CryptoCreateTransactionBody(createParameters), configure, false, createParameters.Signatory).ConfigureAwait(false));
        }
        /// <summary>
        /// Creates a new network account with a given initial balance
        /// and other values as indicated in the create parameters.
        /// </summary>
        /// <param name="createParameters">
        /// The account creation parameters, includes the initial balance,
        /// public key and values associated with the new account.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record with a description of the newly created account
        /// and record information.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<CreateAccountRecord> CreateAccountWithRecordAsync(CreateAccountParams createParameters, Action<IContext>? configure = null)
        {
            return new CreateAccountRecord(await ExecuteTransactionAsync(new CryptoCreateTransactionBody(createParameters), configure, true, createParameters.Signatory).ConfigureAwait(false));
        }
    }
}
