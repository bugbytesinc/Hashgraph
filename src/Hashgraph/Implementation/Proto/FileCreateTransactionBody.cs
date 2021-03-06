﻿using Google.Protobuf;
using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class FileCreateTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Unable to create file, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { FileCreate = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { FileCreate = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new FileService.FileServiceClient(channel).createFileAsync;
        }

        internal FileCreateTransactionBody(Hashgraph.CreateFileParams createParameters) : this()
        {
            if (createParameters is null)
            {
                throw new ArgumentNullException(nameof(createParameters), "The create parameters are missing. Please check that the argument is not null.");
            }
            if (createParameters.Endorsements is null)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.Endorsements), "Endorsements are required.");
            }
            ExpirationTime = new Timestamp(createParameters.Expiration);
            Keys = new KeyList(createParameters.Endorsements);
            Contents = ByteString.CopyFrom(createParameters.Contents.ToArray());
            Memo = createParameters.Memo ?? "";
        }
    }
}
