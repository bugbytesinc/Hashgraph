using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Adds a signature to a pending transaction record. The Scheduled Transaction executes 
        /// this signature completes the list of required signatures for execution.
        /// </summary>
        /// <param name="signingParams">
        /// The parameters containing the details of the request, including the ID of the
        /// transaciton to sign, the bytes of the pending transaction and any signatories.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Receipt indicating success.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> SignPendingTransactionAsync(SignPendingTransactionParams signingParams, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await SignPendingTransactionImplementationAsync(signingParams, configure, false));
        }
        /// <summary>
        /// Adds a signature to a pending transaction record. The Scheduled Transaction executes 
        /// this signature completes the list of required signatures for execution.
        /// </summary>
        /// <param name="pending">
        /// The identifier (Address/Schedule ID) of the pending transaction to sign.
        /// </param>
        /// <param name="signatory">
        /// The signatory containing any additional private keys or callbacks
        /// to meet the key signing requirements for participants.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Record for the transaction indicating success.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> SignPendingTransactionWithRecordAsync(SignPendingTransactionParams signingParams, Action<IContext>? configure = null)
        {
            return new SubmitMessageRecord(await SignPendingTransactionImplementationAsync(signingParams, configure, true));
        }
        /// <summary>
        /// Internal implementation of the sign pending transaction call
        /// </summary>
        private async Task<NetworkResult> SignPendingTransactionImplementationAsync(SignPendingTransactionParams signingParams, Action<IContext>? configure, bool includeRecord)
        {
            signingParams = RequireInputParameter.SigningParams(signingParams);
            var pendingTransaction = TransactionBody.Parser.ParseFrom(signingParams.TransactionBody.ToArray());
            await using var context = CreateChildContext(configure);
            var signaturePrefixTrimLimit = context.SignaturePrefixTrimLimit;
            var sigMap =
                await Invoice.TryGenerateSignatureMapAsync(pendingTransaction, signingParams.Signatory, signaturePrefixTrimLimit) ??
                await Invoice.TryGenerateSignatureMapAsync(pendingTransaction, context.Signatory, signaturePrefixTrimLimit);
            var transactionBody = new TransactionBody
            {
                ScheduleSign = new ScheduleSignTransactionBody
                {
                    ScheduleID = new ScheduleID(signingParams.Pending),                    
                    SigMap = sigMap
                }
            };
            return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Failed to Sign Pending Transaction, status: {0}");
        }
    }
}
