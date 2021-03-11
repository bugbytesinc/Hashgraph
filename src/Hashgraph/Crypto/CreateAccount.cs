using Hashgraph.Implementation;
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
            return new CreateAccountReceipt(await CreateAccountImplementationAsync(createParameters, configure, false));
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
            return new CreateAccountRecord(await CreateAccountImplementationAsync(createParameters, configure, true));
        }
        /// <summary>
        /// Internal implementation for Create Account
        /// Returns either a receipt or record or throws
        /// an exception.
        /// </summary>
        private async Task<NetworkResult> CreateAccountImplementationAsync(CreateAccountParams createParameters, Action<IContext>? configure, bool includeRecord)
        {
            var publicKey = RequireInputParameter.KeysFromEndorsements(createParameters);
            await using var context = CreateChildContext(configure);
            var transactionBody = new TransactionBody
            {
                CryptoCreateAccount = new CryptoCreateTransactionBody
                {
                    Key = publicKey,
                    InitialBalance = createParameters.InitialBalance,
                    ReceiverSigRequired = createParameters.RequireReceiveSignature,
                    AutoRenewPeriod = new Duration(createParameters.AutoRenewPeriod),
                    ProxyAccountID = createParameters.Proxy is null ? null : new AccountID(createParameters.Proxy),
                    Memo = createParameters.Memo ?? string.Empty
                }
            };
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Unable to create account, status: {0}", createParameters.Signatory);
        }
    }
}
