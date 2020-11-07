using Google.Protobuf;
using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Set the period of time where the network will suspend will stop creating events 
        /// and accepting transactions. This can be used to safely shut down 
        /// the platform for maintenance and for upgrades if the file information is included.
        /// </summary>
        /// <param name="suspendParameters">
        /// The details of the suspend request, includes the time to wait before suspension, 
        /// the duration of the suspension and optionally to include an update file.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Submit Message Receipt indicating success.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> SuspendNetworkAsync(SuspendNetworkParams suspendParameters, Action<IContext>? configure = null)
        {
            suspendParameters = RequireInputParameter.SuspendNetworkParams(suspendParameters);
            await using var context = CreateChildContext(configure);
            var gateway = RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatories = Transactions.GatherSignatories(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId);
            var startDate = DateTime.UtcNow.Add(suspendParameters.Starting);
            var endDate = startDate.Add(suspendParameters.Duration);
            transactionBody.Freeze = new FreezeTransactionBody
            {
                StartHour = startDate.Hour,
                StartMin = startDate.Minute,
                EndHour = endDate.Hour,
                EndMin = endDate.Minute,
            };
            if (!suspendParameters.UpdateFile.IsNullOrNone())
            {
                transactionBody.Freeze.UpdateFile = new FileID(suspendParameters.UpdateFile);
                transactionBody.Freeze.FileHash = ByteString.CopyFrom(suspendParameters.UpdateFileHash.Span);
            }
            var request = await Transactions.SignTransactionAsync(transactionBody, signatories);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Failed to submit suspend/freeze command, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
            }
            return receipt.FillProperties(transactionId, new TransactionReceipt());

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new FreezeService.FreezeServiceClient(channel);
                return async (Transaction transaction) => await client.freezeAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
