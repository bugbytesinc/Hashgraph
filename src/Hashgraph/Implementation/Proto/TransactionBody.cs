using Google.Protobuf;
using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Proto
{
    public sealed partial class TransactionBody
    {
        internal TransactionBody(GossipContextStack context, TransactionID transactionId)
        {
            OnConstruction();
            TransactionID = transactionId;
            NodeAccountID = new AccountID(RequireInContext.Gateway(context));
            TransactionFee = (ulong)context.FeeLimit;
            TransactionValidDuration = new Proto.Duration(context.TransactionDuration);
            Memo = context.Memo ?? "";
        }

        internal async Task<Transaction> SignAsync(ISignatory signatory, int prefixTrimLimit)
        {
            var invoice = new Invoice(this);
            await signatory.SignAsync(invoice);
            return new Transaction
            {
                SignedTransactionBytes = invoice.GetSignedTransaction(prefixTrimLimit).ToByteString()
            };
        }

        internal Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> InstantiateNetworkRequestMethod(Channel channel)
        {
            return dataCase_ switch
            {
                DataOneofCase.ContractCall => new SmartContractService.SmartContractServiceClient(channel).contractCallMethodAsync,
                DataOneofCase.ContractCreateInstance => new SmartContractService.SmartContractServiceClient(channel).createContractAsync,
                DataOneofCase.ContractUpdateInstance => new SmartContractService.SmartContractServiceClient(channel).updateContractAsync,
                DataOneofCase.ContractDeleteInstance => new SmartContractService.SmartContractServiceClient(channel).deleteContractAsync,
                //QueryOneofCase.CryptoAddLiveHash = 10,
                DataOneofCase.CryptoCreateAccount => new CryptoService.CryptoServiceClient(channel).createAccountAsync,
                DataOneofCase.CryptoDelete => new CryptoService.CryptoServiceClient(channel).cryptoDeleteAsync,
                //QueryOneofCase.CryptoDeleteLiveHash = 13,
                DataOneofCase.CryptoTransfer => new CryptoService.CryptoServiceClient(channel).cryptoTransferAsync,
                DataOneofCase.CryptoUpdateAccount => new CryptoService.CryptoServiceClient(channel).updateAccountAsync,
                DataOneofCase.FileAppend => new FileService.FileServiceClient(channel).appendContentAsync,
                DataOneofCase.FileCreate => new FileService.FileServiceClient(channel).createFileAsync,
                DataOneofCase.FileDelete => new FileService.FileServiceClient(channel).deleteFileAsync,
                DataOneofCase.FileUpdate => new FileService.FileServiceClient(channel).updateFileAsync,
                DataOneofCase.SystemDelete when SystemDelete.FileID is not null => new FileService.FileServiceClient(channel).systemDeleteAsync,
                DataOneofCase.SystemDelete when SystemDelete.ContractID is not null => new SmartContractService.SmartContractServiceClient(channel).systemDeleteAsync,
                DataOneofCase.SystemUndelete when SystemUndelete.FileID is not null => new FileService.FileServiceClient(channel).systemUndeleteAsync,
                DataOneofCase.SystemUndelete when SystemUndelete.ContractID is not null => new SmartContractService.SmartContractServiceClient(channel).systemUndeleteAsync,
                DataOneofCase.Freeze => new FreezeService.FreezeServiceClient(channel).freezeAsync,
                DataOneofCase.ConsensusCreateTopic => new ConsensusService.ConsensusServiceClient(channel).createTopicAsync,
                DataOneofCase.ConsensusUpdateTopic => new ConsensusService.ConsensusServiceClient(channel).updateTopicAsync,
                DataOneofCase.ConsensusDeleteTopic => new ConsensusService.ConsensusServiceClient(channel).deleteTopicAsync,
                DataOneofCase.ConsensusSubmitMessage => new ConsensusService.ConsensusServiceClient(channel).submitMessageAsync,
                DataOneofCase.UncheckedSubmit => new NetworkService.NetworkServiceClient(channel).uncheckedSubmitAsync,
                DataOneofCase.TokenCreation => new TokenService.TokenServiceClient(channel).createTokenAsync,
                DataOneofCase.TokenFreeze => new TokenService.TokenServiceClient(channel).freezeTokenAccountAsync,
                DataOneofCase.TokenUnfreeze => new TokenService.TokenServiceClient(channel).unfreezeTokenAccountAsync,
                DataOneofCase.TokenGrantKyc => new TokenService.TokenServiceClient(channel).grantKycToTokenAccountAsync,
                DataOneofCase.TokenRevokeKyc => new TokenService.TokenServiceClient(channel).revokeKycFromTokenAccountAsync,
                DataOneofCase.TokenDeletion => new TokenService.TokenServiceClient(channel).deleteTokenAsync,
                DataOneofCase.TokenUpdate => new TokenService.TokenServiceClient(channel).updateTokenAsync,
                DataOneofCase.TokenMint => new TokenService.TokenServiceClient(channel).mintTokenAsync,
                DataOneofCase.TokenBurn => new TokenService.TokenServiceClient(channel).burnTokenAsync,
                DataOneofCase.TokenWipe => new TokenService.TokenServiceClient(channel).wipeTokenAccountAsync,
                DataOneofCase.TokenAssociate => new TokenService.TokenServiceClient(channel).associateTokensAsync,
                DataOneofCase.TokenDissociate => new TokenService.TokenServiceClient(channel).dissociateTokensAsync,
                _ => throw new InvalidOperationException("Transaction has No Type Set, unable to find Network Request Method for Unknown Transaction Type.")
            };
        }
        internal async Task<TransactionReceipt> SignAndExecuteWithRetryAsync(ISignatory signatory, GossipContextStack context)
        {
            var precheck = await SignAndSubmitWithRetryAsync(signatory, context);
            ValidateResult.PreCheck(TransactionID, precheck);
            var receipt = await Transactions.GetReceiptAsync(context, TransactionID);
            return receipt;
        }
        internal async Task<TransactionResponse> SignAndSubmitWithRetryAsync(ISignatory signatory, GossipContextStack context)
        {
            var request = await SignAsync(signatory, context.SignaturePrefixTrimLimit);
            return await Transactions.ExecuteSignedRequestWithRetryImplementationAsync(context, request, InstantiateNetworkRequestMethod, getResponseCode);

            static ResponseCodeEnum getResponseCode(TransactionResponse response)
            {
                return response.NodeTransactionPrecheckCode;
            }
        }
    }
}

