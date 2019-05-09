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
                throw new TransactionException($"Unable to create account, status: {record.Receipt.Status}", Protobuf.FromTransactionRecord<TransactionRecord>(record));
            }
            var result = Protobuf.FromTransactionRecord<CreateAccountRecord>(record);
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

#pragma warning disable CS8618 // Non-nullable field is uninitialized.
        public class CreateAccountRecord : TransactionRecord
        {
            public Address Address { get; internal set; }
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    }
}
