using Google.Protobuf;
using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class FreezeTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Failed to submit suspend/freeze command, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { Freeze = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { Freeze = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new FreezeService.FreezeServiceClient(channel).freezeAsync;
        }

        internal FreezeTransactionBody(Hashgraph.SuspendNetworkParams suspendParameters) : this()
        {
            if (suspendParameters is null)
            {
                throw new ArgumentNullException("Suspend Network Parameters can not be null.", nameof(suspendParameters));
            }
            if (suspendParameters.Duration.Ticks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(suspendParameters.Duration), "The duration of the suspension must be greater than zero.");
            }
            if (suspendParameters.Duration.TotalHours > 24)
            {
                throw new ArgumentOutOfRangeException(nameof(suspendParameters.Duration), "The duration of suspension must not exceed 24 hours.");
            }
            if (suspendParameters.Starting.TotalHours > 24)
            {
                throw new ArgumentOutOfRangeException(nameof(suspendParameters.Starting), "The starting wait must not exceed 24 hours.");
            }
            var now = DateTime.UtcNow;
            var then = now.Add(suspendParameters.Starting).Add(suspendParameters.Duration);
            if (then < now)
            {
                throw new ArgumentOutOfRangeException(nameof(suspendParameters.Starting), "The combination of Starting wait and Duration has already passed.");
            }
            if (suspendParameters.UpdateFile.IsNullOrNone())
            {
                if (!suspendParameters.UpdateFileHash.IsEmpty)
                {
                    throw new ArgumentOutOfRangeException(nameof(suspendParameters.UpdateFile), "The the the hash of the file contents is specified, an address for the update file must be included.");
                }
            }
            else if (suspendParameters.UpdateFileHash.IsEmpty)
            {
                throw new ArgumentOutOfRangeException(nameof(suspendParameters.UpdateFileHash), "The an update file address is specified, the hash of the file contents must be included.");
            }
            var startDate = DateTime.UtcNow.Add(suspendParameters.Starting);
            var endDate = startDate.Add(suspendParameters.Duration);
            StartHour = startDate.Hour;
            StartMin = startDate.Minute;
            EndHour = endDate.Hour;
            EndMin = endDate.Minute;
            if (!suspendParameters.UpdateFile.IsNullOrNone())
            {
                UpdateFile = new FileID(suspendParameters.UpdateFile);
                FileHash = ByteString.CopyFrom(suspendParameters.UpdateFileHash.Span);
            }
        }
    }
}
