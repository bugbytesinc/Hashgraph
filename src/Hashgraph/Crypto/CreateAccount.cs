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
        public async Task<Address> CreateAccountAsync(ReadOnlyMemory<byte> publicKey, ulong initialBalance, Action<IContext>? configure = null)
        {
            Require.PublicKeyArgument(publicKey);
            Require.InitialBalanceArgument(initialBalance);
            var context = CreateChildContext(configure);
            Require.GatewayInContext(context);
            Require.PayerInContext(context);
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
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, context.Payer);
            var request = new Proto.Transaction
            {
                Body = transactionBody,
                Sigs = signatures
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, request, instantiateExecuteCreateAccountAsyncMethod, checkForRetry);
            Validate.ValidatePreCheckResult(response.NodeTransactionPrecheckCode);
            var record = await GetFastRecordAsync(transactionId, context);
            if (record.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new PrecheckException($"Account was created, but unable to get receipt with new Account Address.  Code {record.Receipt.Status}", PrecheckResponse.Ok);
            }
            return Protobuf.FromAccountID(record.Receipt.AccountID);

            static Func<Proto.Transaction, Task<TransactionResponse>> instantiateExecuteCreateAccountAsyncMethod(Channel channel)
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
