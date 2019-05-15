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
        private async Task<AccountTransactionRecord> DeleteAccountAsync(Address addressToDelete, Address transferToAddress, Action<IContext>? configure = null)
        {
            addressToDelete = RequireInputParameter.AddressToDelete(addressToDelete);
            transferToAddress = RequireInputParameter.TransferToAddress(transferToAddress);
            var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateEmptyTransactionBody(context, transactionId, "Delete Account");
            transactionBody.CryptoDelete = new CryptoDeleteTransactionBody
            {
                DeleteAccountID = Protobuf.ToAccountID(addressToDelete),
                TransferAccountID = Protobuf.ToAccountID(transferToAddress)
            };
            var signatures = Transactions.SignProtoTransactionBody(transactionBody, payer);
            var request = new Proto.Transaction
            {
                Body = transactionBody,
                Sigs = signatures
            };
            var response = await Transactions.ExecuteRequestWithRetryAsync(context, request, getServerMethod, shouldRetry);
            ValidateResult.PreCheck(transactionId, response.NodeTransactionPrecheckCode);
            var record = await GetFastRecordAsync(transactionId, context);
            if (record.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to delete account, status: {record.Receipt.Status}", Protobuf.FromTransactionRecord<TransactionRecord>(record, transactionId));
            }
            var result = Protobuf.FromTransactionRecord<AccountTransactionRecord>(record, transactionId);
            result.Address = Protobuf.FromAccountID(record.Receipt.AccountID);
            return result;

            static Func<Proto.Transaction, Task<TransactionResponse>> getServerMethod(Channel channel)
            {
                var client = new CryptoService.CryptoServiceClient(channel);
                return async (Proto.Transaction transaction) => await client.cryptoDeleteAsync(transaction);
            }

            static bool shouldRetry(TransactionResponse response)
            {
                var code = response.NodeTransactionPrecheckCode;
                return
                    code == ResponseCodeEnum.Busy ||
                    code == ResponseCodeEnum.InvalidTransactionStart;
            }
        }
    }
}
