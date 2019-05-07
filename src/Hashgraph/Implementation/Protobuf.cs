using Google.Protobuf;
using Proto;
using System;

namespace Hashgraph.Implementation
{
    internal static class Protobuf
    {
        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly long NanosPerTick = 1_000_000_000L / TimeSpan.TicksPerSecond;

        internal static Transaction FromTransactionId(TransactionID transactionId)
        {
            return new Transaction(toProtoBytes(transactionId));
        }

        internal static TransactionID ToTransactionID(Transaction transaction)
        {
            return TransactionID.Parser.ParseFrom((transaction as IData).Data.ToArray());
        }

        internal static ShardID ToShardID(long shardNum)
        {
            return new ShardID { ShardNum = shardNum };
        }
        internal static RealmID ToRealmID(long realmNum, long shardNum)
        {
            return new RealmID
            {
                RealmNum = realmNum,
                ShardNum = shardNum
            };
        }

        internal static AccountID ToAccountID(Address address)
        {
            return new AccountID
            {
                RealmNum = address.RealmNum,
                ShardNum = address.ShardNum,
                AccountNum = address.AccountNum
            };
        }
        internal static Address FromAccountID(AccountID accountId)
        {
            return new Address(accountId.RealmNum, accountId.ShardNum, accountId.AccountNum);
        }
        internal static Duration ToDuration(TimeSpan timespan)
        {
            long seconds = (long)timespan.TotalSeconds;
            int nanos = (int)((timespan.Ticks - (seconds * TimeSpan.TicksPerSecond)) * NanosPerTick);
            return new Duration
            {
                Seconds = seconds,
                Nanos = nanos
            };
        }
        internal static TimeSpan FromDuration(Duration duration)
        {
            return TimeSpan.FromTicks(duration.Seconds * TimeSpan.TicksPerSecond + duration.Nanos / NanosPerTick);
        }
        internal static Timestamp toProtoTimestamp(DateTime dateTime)
        {
            TimeSpan timespan = dateTime - EPOCH;
            long seconds = (long)timespan.TotalSeconds;
            int nanos = (int)((timespan.Ticks - (seconds * TimeSpan.TicksPerSecond)) * NanosPerTick);
            return new Timestamp
            {
                Seconds = seconds,
                Nanos = nanos
            };
        }
        internal static DateTime FromTimestamp(Timestamp timestamp)
        {
            return EPOCH.AddTicks(timestamp.Seconds * TimeSpan.TicksPerSecond + timestamp.Nanos / NanosPerTick);
        }
        internal static ReadOnlySpan<byte> toProtoBytes(IMessage message)
        {
            var size = message.CalculateSize();
            var buffer = new byte[size];
            using (var stream = new CodedOutputStream(buffer))
            {
                message.WriteTo(stream);
            }
            return buffer;
        }
        internal static AccountInfo FromAccountInfo(CryptoGetInfoResponse.Types.AccountInfo accountInfo)
        {
            var accountId = accountInfo.AccountID;
            var proxyId = accountInfo.ProxyAccountID;
            return new AccountInfo(
                new Address(accountId.RealmNum, accountId.ShardNum, accountId.AccountNum),
                accountInfo.ContractAccountID,
                accountInfo.Deleted,
                new Address(proxyId.RealmNum, proxyId.ShardNum, proxyId.AccountNum),
                accountInfo.ProxyFraction,
                accountInfo.ProxyReceived,
                Signatures.EncodeByteArrayToHexString(accountInfo.Key.Ed25519.ToByteArray()),
                accountInfo.Balance,
                accountInfo.GenerateSendRecordThreshold,
                accountInfo.GenerateReceiveRecordThreshold,
                accountInfo.ReceiverSigRequired,
                FromTimestamp(accountInfo.ExpirationTime),
                FromDuration(accountInfo.AutoRenewPeriod));
        }
    }
}
