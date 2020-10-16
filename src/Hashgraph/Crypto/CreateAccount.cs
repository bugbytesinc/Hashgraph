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
        public Task<CreateAccountReceipt> CreateAccountAsync(CreateAccountParams createParameters, Action<IContext>? configure = null)
        {
            return CreateAccountImplementationAsync<CreateAccountReceipt>(createParameters, configure);
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
        public Task<CreateAccountRecord> CreateAccountWithRecordAsync(CreateAccountParams createParameters, Action<IContext>? configure = null)
        {
            return CreateAccountImplementationAsync<CreateAccountRecord>(createParameters, configure);
        }
        /// <summary>
        /// Internal implementation for Create Account
        /// Returns either a receipt or record or throws
        /// an exception.
        /// </summary>
        private async Task<TResult> CreateAccountImplementationAsync<TResult>(CreateAccountParams createParameters, Action<IContext>? configure) where TResult : new()
        {
            var publicKey = RequireInputParameter.KeysFromEndorsements(createParameters);
            await using var context = CreateChildContext(configure);
            RequireInContext.Gateway(context);
            var payer = RequireInContext.Payer(context);
            var signatories = Transactions.GatherSignatories(context, createParameters.Signatory);
            var transactionId = Transactions.GetOrCreateTransactionID(context);
            var transactionBody = Transactions.CreateTransactionBody(context, transactionId);
            transactionBody.CryptoCreateAccount = new CryptoCreateTransactionBody
            {
                Key = publicKey,
                InitialBalance = createParameters.InitialBalance,
                ReceiverSigRequired = createParameters.RequireReceiveSignature,
                AutoRenewPeriod = new Duration(createParameters.AutoRenewPeriod),
                ProxyAccountID = createParameters.Proxy is null ? null : new AccountID(createParameters.Proxy),
            };
            var request = await Transactions.SignTransactionAsync(transactionBody, signatories);
            var precheck = await Transactions.ExecuteSignedRequestWithRetryAsync(context, request, getRequestMethod, getResponseCode);
            ValidateResult.PreCheck(transactionId, precheck);
            var receipt = await GetReceiptAsync(context, transactionId);
            if (receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException($"Unable to create account, status: {receipt.Status}", transactionId.ToTxId(), (ResponseCode)receipt.Status);
            }
            var result = new TResult();
            if (result is CreateAccountRecord rec)
            {
                var record = await GetTransactionRecordAsync(context, transactionId);
                record.FillProperties(rec);
            }
            else if (result is CreateAccountReceipt rcpt)
            {
                receipt.FillProperties(transactionId, rcpt);
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
