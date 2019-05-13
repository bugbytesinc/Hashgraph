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
        /// A transaction record with a description of the newly created account.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<CreateAccountRecord> CreateAccountAsync(CreateAccountParams createParameters, Action<IContext>? configure = null)
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
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, payer);
            var request = new Transaction
            {
                Body = transactionBody,
                Sigs = signatures
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, request, instantiateCreateAccountAsyncMethod, checkForRetry);
            ValidateResult.PreCheck(transactionId, response.NodeTransactionPrecheckCode);
            var record = await GetFastRecordAsync(transactionId, context);
            if (record.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to create account, status: {record.Receipt.Status}", Protobuf.FromTransactionRecord<TransactionRecord>(record, transactionId));
            }
            var result = Protobuf.FromTransactionRecord<CreateAccountRecord>(record, transactionId);
            result.Address = Protobuf.FromAccountID(record.Receipt.AccountID);
            return result;

            static Func<Transaction, Task<TransactionResponse>> instantiateCreateAccountAsyncMethod(Channel channel)
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
