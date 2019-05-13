using Google.Protobuf;
using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Updates the changeable properties of a hedera network account.
        /// </summary>
        /// <param name="updateParameters">
        /// The account update parameters, includes a required 
        /// <see cref="Address"/> reference to the account to update plus
        /// a number of changeable properties of the account.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record containing the details of the results
        /// of the request.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> UpdateAccountAsync(UpdateAccountParams updateParameters, Action<IContext>? configure = null)
        {
            updateParameters = RequireInputParameter.UpdateParameters(updateParameters);
            var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var updateAccountBody = new CryptoUpdateTransactionBody
            {
                AccountIDToUpdate = Protobuf.ToAccountID(updateParameters.Account)
            };
            var privateKey = updateParameters.PrivateKey.HasValue ? Keys.ImportPrivateEd25519KeyFromBytes(updateParameters.PrivateKey.Value) : null;
            if (privateKey != null)
            {
                var publicKeyWithoutPrefix = privateKey.Export(NSec.Cryptography.KeyBlobFormat.PkixPublicKey).TakeLast(32).ToArray();
                updateAccountBody.Key = new Key { Ed25519 = ByteString.CopyFrom(publicKeyWithoutPrefix) };
            }
            if (updateParameters.SendThresholdCreateRecord.HasValue)
            {
                updateAccountBody.SendRecordThreshold = updateParameters.SendThresholdCreateRecord.Value;
            }
            if (updateParameters.ReceiveThresholdCreateRecord.HasValue)
            {
                updateAccountBody.ReceiveRecordThreshold = updateParameters.ReceiveThresholdCreateRecord.Value;
            }
            if (updateParameters.AutoRenewPeriod.HasValue)
            {
                updateAccountBody.AutoRenewPeriod = Protobuf.ToDuration(updateParameters.AutoRenewPeriod.Value);
            }
            if (updateParameters.Expiration.HasValue)
            {
                updateAccountBody.ExpirationTime = Protobuf.ToTimestamp(updateParameters.Expiration.Value);
            }
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateEmptyTransactionBody(context, transactionId, "Update Account");
            transactionBody.CryptoUpdateAccount = updateAccountBody;
            var signatures = privateKey is null ?
                Transactions.SignProtoTransactionBody(transactionBody, payer, updateParameters.Account) :
                Transactions.SignProtoTransactionBody(transactionBody, payer, updateParameters.Account, new Signer(privateKey));
            var request = new Transaction
            {
                Body = transactionBody,
                Sigs = signatures
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, request, instantiateExecuteCryptoGetInfoAsyncMethod, checkForRetry);
            ValidateResult.PreCheck(transactionId, response.NodeTransactionPrecheckCode);
            var record = await GetFastRecordAsync(transactionId, context);
            var result = Protobuf.FromTransactionRecord<TransactionRecord>(record, transactionId);
            if (record.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to update account, status: {record.Receipt.Status}", result);
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> instantiateExecuteCryptoGetInfoAsyncMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Transaction transaction) => await client.createAccountAsync(transaction);
            }

            static bool checkForRetry(TransactionResponse response)
            {
                var code = response.NodeTransactionPrecheckCode;
                return
                    code == ResponseCodeEnum.Busy ||
                    code == ResponseCodeEnum.InvalidTransactionStart;
            }
        }
    }
}
