using Google.Protobuf;
using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class FileAppendTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Unable to append to file, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { FileAppend = this };
        }
        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { FileAppend = this };
        }
        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new FileService.FileServiceClient(channel).appendContentAsync;
        }

        internal FileAppendTransactionBody(Hashgraph.AppendFileParams appendParameters) : this()
        {
            if (appendParameters is null)
            {
                throw new ArgumentNullException(nameof(appendParameters), "File Update Parameters argument is missing. Please check that it is not null.");
            }
            if (appendParameters.File is null)
            {
                throw new ArgumentNullException(nameof(appendParameters.File), "File identifier is missing. Please check that it is not null.");
            }
            FileID = new FileID(appendParameters.File);
            Contents = ByteString.CopyFrom(appendParameters.Contents.ToArray());
        }
    }
}
