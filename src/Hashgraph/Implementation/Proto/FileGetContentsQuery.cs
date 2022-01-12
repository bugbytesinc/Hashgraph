using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class FileGetContentsQuery : INetworkQuery
{
    Query INetworkQuery.CreateEnvelope()
    {
        return new Query { FileGetContents = this };
    }

    Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(Channel channel)
    {
        return new FileService.FileServiceClient(channel).getFileContentAsync;
    }

    void INetworkQuery.SetHeader(QueryHeader header)
    {
        Header = header;
    }

    internal FileGetContentsQuery(Address file) : this()
    {
        FileID = new FileID(file);
    }
}