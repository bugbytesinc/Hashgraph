using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class CryptoDeleteTransactionBody : INetworkTransaction
    {
        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { CryptoDelete = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { CryptoDelete = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new CryptoService.CryptoServiceClient(channel).cryptoDeleteAsync;
        }

        void INetworkTransaction.CheckReceipt(NetworkResult result)
        {
            if (result.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException(string.Format("Unable to delete account, status: {0}", result.Receipt.Status), result);
            }
        }

        internal CryptoDeleteTransactionBody(AddressOrAlias addressToDelete, AddressOrAlias transferToAddress) : this()
        {
            if (addressToDelete is null)
            {
                throw new ArgumentNullException(nameof(addressToDelete), "Address to Delete is missing. Please check that it is not null.");
            }
            if (transferToAddress is null)
            {
                throw new ArgumentNullException(nameof(transferToAddress), "Transfer address is missing. Please check that it is not null.");
            }
            DeleteAccountID = new AccountID(addressToDelete);
            TransferAccountID = new AccountID(transferToAddress);
        }
    }
}
