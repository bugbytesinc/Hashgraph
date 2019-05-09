using Google.Protobuf;
using Proto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hashgraph.Implementation
{
    internal static class Protobuf
    {
        private static readonly DateTime EPOCH = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private static readonly long NanosPerTick = 1_000_000_000L / TimeSpan.TicksPerSecond;

        internal static TxId FromTransactionId(TransactionID transactionId)
        {
            return new TxId(transactionId.ToByteArray());
        }

        internal static TransactionID ToTransactionID(TxId transaction)
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

        internal static Timestamp ToTimestamp(DateTime dateTime)
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
                accountInfo.Key.Ed25519.ToByteArray(),
                accountInfo.Balance,
                accountInfo.GenerateSendRecordThreshold,
                accountInfo.GenerateReceiveRecordThreshold,
                accountInfo.ReceiverSigRequired,
                FromTimestamp(accountInfo.ExpirationTime),
                FromDuration(accountInfo.AutoRenewPeriod));
        }

        // Note: sometimes when this is being used to create
        // a context for throwing an exception (because
        // the transaction failed, or the server is too busy
        // for the given retry settings) the transaction ID
        // is not returned in the <code>TransactionRecord</code>.
        // However, the calling context should allways know the
        // transaction so it is passed in as a backup.
        internal static TRecord FromTransactionRecord<TRecord>(Proto.TransactionRecord record, TransactionID originatingID) where TRecord : TransactionRecord, new()
        {
            return new TRecord
            {
                Id = FromTransactionId(record.TransactionID ?? originatingID),
                Status = (ResponseCode)record.Receipt.Status,
                Hash = record.TransactionHash.ToByteArray(),
                Concensus = FromTimestamp(record.ConsensusTimestamp),
                Memo = record.Memo,
                Fee = record.TransactionFee
            };
        }
        internal static ReadOnlyDictionary<Address, long> FromTransferList(TransferList transferList)
        {
            var results = new Dictionary<Address, long>();
            foreach (var xfer in transferList.AccountAmounts)
            {
                var account = FromAccountID(xfer.AccountID);
                results.TryGetValue(account, out long amount);
                results[account] = amount + xfer.Amount;
            }
            return new ReadOnlyDictionary<Address, long>(results);
        }
    }
}
