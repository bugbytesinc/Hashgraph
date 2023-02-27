using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class FileUpdateTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { FileUpdate = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { FileUpdate = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(GrpcChannel channel)
    {
        return new FileService.FileServiceClient(channel).updateFileAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Unable to update file, status: {0}", result.Receipt.Status), result);
        }
    }

    public FileUpdateTransactionBody(Hashgraph.UpdateFileParams updateParameters) : this()
    {
        if (updateParameters is null)
        {
            throw new ArgumentNullException(nameof(updateParameters), "File Update Parameters argument is missing. Please check that it is not null.");
        }
        if (updateParameters.File is null)
        {
            throw new ArgumentNullException(nameof(updateParameters.File), "File identifier is missing. Please check that it is not null.");
        }
        if (updateParameters.Endorsements is null &&
            updateParameters.Contents is null &&
            updateParameters.Memo is null &&
            updateParameters.Expiration is null)// &&
                                                // v0.34.0 Churn
                                                //updateParameters.AutoRenewPeriod is null &&
                                                //updateParameters.AutoRenewAccount is null)
        {
            throw new ArgumentException("The File Update parameters contain no update properties, it is blank.", nameof(updateParameters));
        }
        FileID = new FileID(updateParameters.File);
        if (updateParameters.Endorsements is not null)
        {
            Keys = new KeyList(updateParameters.Endorsements);
        }
        if (updateParameters.Contents.HasValue)
        {
            Contents = ByteString.CopyFrom(updateParameters.Contents.Value.ToArray());
        }
        if (updateParameters.Memo is not null)
        {
            Memo = updateParameters.Memo;
        }
        if (updateParameters.Expiration.HasValue)
        {
            ExpirationTime = new Timestamp(updateParameters.Expiration.Value);
        }
        // v0.34.0 Churn
        //if (updateParameters.AutoRenewPeriod.HasValue)
        //{
        //    AutoRenewPeriod = new Duration(updateParameters.AutoRenewPeriod.Value);
        //}
        //if (updateParameters.AutoRenewAccount is not null)
        //{
        //    AutoRenewAccount = new AccountID(updateParameters.AutoRenewAccount);
        //}
    }
}