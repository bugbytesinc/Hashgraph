using Grpc.Core;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        // Premature Implementation of this feature, does not
        // seem to be implemented on testnet at this moment.
        // Marked as private for now until we can confirm
        // it works and incorporate into automated test.
        private async Task<DeleteAccountRecord> DeleteAccountAsync(Address accountToDelete, Address transferAccount, Action<IContext>? configure = null)
        {
            Require.AccountToDeleteArgument(accountToDelete);
            Require.TransferAccountArgument(transferAccount);
            var context = CreateChildContext(configure);
            Require.GatewayInContext(context);
            var payer = Require.PayerInContext(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateEmptyTransactionBody(context, transactionId, "Delete Account");
            transactionBody.CryptoDelete = new CryptoDeleteTransactionBody
            {
                DeleteAccountID = Protobuf.ToAccountID(accountToDelete),
                TransferAccountID = Protobuf.ToAccountID(transferAccount)
            };
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, payer);
            var request = new Proto.Transaction
            {
                Body = transactionBody,
                Sigs = signatures
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, request, instantiateCryptoDeleteAsyncMethod, checkForRetry);
            Validate.ValidatePreCheckResult(transactionId, response.NodeTransactionPrecheckCode);
            var record = await GetFastRecordAsync(transactionId, context);
            if (record.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to delete account, status: {record.Receipt.Status}", Protobuf.FromTransactionRecord<TransactionRecord>(record));
            }
            var result = Protobuf.FromTransactionRecord<DeleteAccountRecord>(record);
            result.Address = Protobuf.FromAccountID(record.Receipt.AccountID);
            return result;

            static Func<Proto.Transaction, Task<TransactionResponse>> instantiateCryptoDeleteAsyncMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Proto.Transaction transaction) => await client.cryptoDeleteAsync(transaction);
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
        public class DeleteAccountRecord : TransactionRecord
        {
            public Address Address { get; internal set; }
        }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
    }
}
