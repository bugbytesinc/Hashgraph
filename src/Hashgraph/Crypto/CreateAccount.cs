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
        /// Creates a new network account with a given initial balance.
        /// </summary>
        /// <param name="publicKey">
        /// The public Ed25519 key corresponding to the private key authorized 
        /// to sign transactions on behalf of this new account.  The key 
        /// length is expected to be 44 bytes long and start with the prefix 
        /// of 0x302a300506032b6570032100.
        /// </param>
        /// <param name="initialBalance">
        /// The initial balance that will be transferred from the 
        /// <see cref="IContext.Payer"/> account to the new account 
        /// upon creation.
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
        public async Task<CreateAccountRecord> CreateAccountAsync(ReadOnlyMemory<byte> publicKey, ulong initialBalance, Action<IContext>? configure = null)
        {
            Require.PublicKeyArgument(publicKey);
            Require.InitialBalanceArgument(initialBalance);
            var context = CreateChildContext(configure);
            Require.GatewayInContext(context);
            var payer = Require.PayerInContext(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateEmptyTransactionBody(context, transactionId, "Create Account");
            // Create Account requires just the 32 bits of the public key, without the prefix.
            var publicKeyWithoutPrefix = publicKey.ToArray().TakeLast(32).ToArray();
            transactionBody.CryptoCreateAccount = new CryptoCreateTransactionBody
            {
                Key = new Key { Ed25519 = ByteString.CopyFrom(publicKeyWithoutPrefix) },
                InitialBalance = initialBalance,
                SendRecordThreshold = context.CreateAccountCreateRecordSendThreshold,
                ReceiveRecordThreshold = context.CreateAcountRequireSignatureReceiveThreshold,
                ReceiverSigRequired = context.CreateAccountAlwaysRequireReceiveSignature,
                AutoRenewPeriod = Protobuf.ToDuration(context.CreateAccountAutoRenewPeriod)
            };
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, payer);
            var request = new Transaction
            {
                Body = transactionBody,
                Sigs = signatures
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, request, instantiateCreateAccountAsyncMethod, checkForRetry);
            Validate.ValidatePreCheckResult(transactionId, response.NodeTransactionPrecheckCode);
            var record = await GetFastRecordAsync(transactionId, context);
            if (record.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to create account, status: {record.Receipt.Status}", Protobuf.FromTransactionRecord<TransactionRecord>(record, transactionId));
            }
            var result = Protobuf.FromTransactionRecord<CreateAccountRecord>(record, transactionId);
            result.Address = Protobuf.FromAccountID(record.Receipt.AccountID);
            return result;

            static Func<Proto.Transaction, Task<TransactionResponse>> instantiateCreateAccountAsyncMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Proto.Transaction transaction) => await client.createAccountAsync(transaction);
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
