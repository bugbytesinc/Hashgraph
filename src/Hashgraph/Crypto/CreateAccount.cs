using Google.Protobuf;
using Grpc.Core;
using Hashgraph.Implementation;
using NSec.Cryptography;
using Proto;
using System;
using System.Linq;
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
        public Task<AccountReceipt> CreateAccountAsync(CreateAccountParams createParameters, Action<IContext>? configure = null)
        {
            return CreateAccountImplementationAsync<AccountReceipt>(createParameters, configure);
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
        public Task<AccountRecord> CreateAccountWithRecordAsync(CreateAccountParams createParameters, Action<IContext>? configure = null)
        {
            return CreateAccountImplementationAsync<AccountRecord>(createParameters, configure);
        }
        /// <summary>
        /// Internal implementation for Create Account
        /// Returns either a receipt or record or throws
        /// an exception.
        /// </summary>
        private async Task<TResult> CreateAccountImplementationAsync<TResult>(CreateAccountParams createParameters, Action<IContext>? configure) where TResult : new()
        {
            createParameters = RequireInputParameter.CreateParameters(createParameters);
            var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateEmptyTransactionBody(context, transactionId, "Create Account");
            // Create Account requires just the 32 bits of the public key, without the prefix.
            var publicKeyWithoutPrefix = Keys.ImportPublicEd25519KeyFromBytes(createParameters.PublicKey).Export(KeyBlobFormat.PkixPublicKey).TakeLast(32).ToArray();
            transactionBody.CryptoCreateAccount = new CryptoCreateTransactionBody
            {
                Key = new Proto.Key { Ed25519 = ByteString.CopyFrom(publicKeyWithoutPrefix) },
                InitialBalance = createParameters.InitialBalance,
                SendRecordThreshold = createParameters.SendThresholdCreateRecord,
                ReceiveRecordThreshold = createParameters.ReceiveThresholdCreateRecord,
                ReceiverSigRequired = createParameters.RequireReceiveSignature,
                AutoRenewPeriod = Protobuf.ToDuration(createParameters.AutoRenewPeriod),
            };
            var request = Transactions.SignTransaction(transactionBody, payer);
            var precheck = await Transactions.ExecuteRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck.NodeTransactionPrecheckCode);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to create account, status: {receipt.Status}", Protobuf.FromTransactionId(transactionId), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is AccountReceipt arcpt)
            {
                Protobuf.FillReceiptProperties(transactionId, receipt, arcpt);
                arcpt.Address = Protobuf.FromAccountID(receipt.AccountID);
            }
            else if (result is AccountRecord arec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                Protobuf.FillRecordProperties(transactionId, record, arec);
                arec.Address = Protobuf.FromAccountID(receipt.AccountID);
            }
            return result;

            static Func<Transaction, Task<TransactionResponse>> getRequestMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Transaction transaction) => await client.createAccountAsync(transaction);
            }

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}
