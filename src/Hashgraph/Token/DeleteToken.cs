using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Deletes a token instance from the network. 
        /// Must be signed by the admin key.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token instance that will be deleted.
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
        public Task<TransactionReceipt> DeleteTokenAsync(Address token, Action<IContext>? configure = null)
        {
            return DeleteTokenImplementationAsync(token, null, configure);
        }
        /// <summary>
        /// Deletes a token from the network. Must be signed by the admin key.
        /// </summary>
        /// <param name="token">
        /// The identifier (Address/Symbol) of the token instance that will be deleted.
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
        public Task<TransactionReceipt> DeleteTokenAsync(Address token, Signatory signatory, Action<IContext>? configure = null)
        {
            return DeleteTokenImplementationAsync(token, signatory, configure);
        }
        /// <summary>
        /// Internal implementation of delete token method.
        /// </summary>
        private async Task<TransactionReceipt> DeleteTokenImplementationAsync(Address token, Signatory? signatory, Action<IContext>? configure)
        {
            token = RequireInputParameter.Token(token);
            await using var context = CreateChildContext(configure);
            var transactionBody = new TransactionBody
            {
                TokenDeletion = new TokenDeleteTransactionBody
                {
                    Token = new TokenID(token)
                }
            };
            return new TransactionReceipt(await transactionBody.SignAndExecuteWithRetryAsync(context, false, "Unable to Delete Token, status: {0}", signatory));
        }
    }
}
