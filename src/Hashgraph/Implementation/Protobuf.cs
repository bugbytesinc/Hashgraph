using Google.Protobuf;
using Proto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal helper class providing methods to convert 
    /// from protobuf messages into public objects exposed 
    /// by the library and from public objects back into 
    /// protobuf messages.
    /// </summary>
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
        internal static FileID ToFileId(Address file)
        {
            return new FileID
            {
                RealmNum = file.RealmNum,
                ShardNum = file.ShardNum,
                FileNum = file.AccountNum
            };
        }

        internal static Address FromFileID(FileID fileID)
        {
            return new Address(fileID.RealmNum, fileID.ShardNum, fileID.FileNum);
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
        internal static Key[] ToPublicKeys(Endorsements endorsements)
        {
            return endorsements._keys.Select(k => new Key { Ed25519 = ByteString.CopyFrom(k.Export(NSec.Cryptography.KeyBlobFormat.PkixPublicKey).TakeLast(32).ToArray()) }).ToArray();
        }
        internal static KeyList ToPublicKeyList(Endorsements endorsements)
        {
            var result = new KeyList();
            result.Keys.AddRange(ToPublicKeys(endorsements));
            return result;
        }
        private static Endorsements FromPublicKeyList(KeyList keys)
        {
            var publicKeys = new List<ReadOnlyMemory<byte>>(keys.Keys.Count);
            foreach(var key in keys.Keys)
            {
                if(key.KeyCase == Key.KeyOneofCase.Ed25519)
                {
                    publicKeys.Add(new ReadOnlyMemory<byte>(Keys.publicKeyPrefix.Concat(key.Ed25519.ToByteArray()).ToArray()));
                }
            }
            return new Endorsements(publicKeys.ToArray());
        }
        private static Endorsements FromPublicKey(Key key)
        {
            switch(key.KeyCase)
            {
                case Key.KeyOneofCase.Ed25519:
                    return new Endorsements(new ReadOnlyMemory<byte>(Keys.publicKeyPrefix.Concat(key.Ed25519.ToByteArray()).ToArray()));
                case Key.KeyOneofCase.KeyList:
                    return FromPublicKeyList(key.KeyList);
            }
            throw new NotSupportedException($"Unrecognized Public Key type: {key.KeyCase}.");
        }
        internal static AccountInfo FromAccountInfo(CryptoGetInfoResponse.Types.AccountInfo accountInfo)
        {
            return new AccountInfo
            {
                Address = FromAccountID(accountInfo.AccountID),
                SmartContractId = accountInfo.ContractAccountID,
                Deleted = accountInfo.Deleted,
                Proxy = FromAccountID(accountInfo.ProxyAccountID), 
                ProxyShareFraction = accountInfo.ProxyFraction,
                ProxiedToAccount = accountInfo.ProxyReceived,
                Endorsements = FromPublicKey(accountInfo.Key),
                Balance = accountInfo.Balance,
                SendThresholdCreateRecord = accountInfo.GenerateSendRecordThreshold,
                ReceiveThresholdCreateRecord = accountInfo.GenerateReceiveRecordThreshold,
                ReceiveSignatureRequired = accountInfo.ReceiverSigRequired,
                Expiration = FromTimestamp(accountInfo.ExpirationTime),
                AutoRenewPeriod = FromDuration(accountInfo.AutoRenewPeriod)
            };
        }
        internal static FileInfo FromFileInfo(FileGetInfoResponse.Types.FileInfo fileInfo)
        {
            return new FileInfo
            {
                File = FromFileID(fileInfo.FileID),
                Size = fileInfo.Size,
                Expiration = FromTimestamp(fileInfo.ExpirationTime),
                Endorsements = FromPublicKeyList(fileInfo.Keys),
                Deleted = fileInfo.Deleted
            };
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
                Hash = record.TransactionHash?.ToByteArray(),
                Concensus = record.ConsensusTimestamp == null ? null : (DateTime?) FromTimestamp(record.ConsensusTimestamp),
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
