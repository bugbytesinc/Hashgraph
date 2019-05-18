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
            return new Duration
            {
                Seconds = (long)timespan.TotalSeconds
            };
        }
        internal static TimeSpan FromDuration(Duration duration)
        {
            return TimeSpan.FromSeconds(duration.Seconds);
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

        internal static Key ToPublicKey(Endorsement endorsement)
        {
            switch(endorsement._type)
            {
                case Endorsement.Type.Ed25519: return new Key { Ed25519 = ByteString.CopyFrom(((NSec.Cryptography.PublicKey)endorsement._data).Export(NSec.Cryptography.KeyBlobFormat.PkixPublicKey).TakeLast(32).ToArray()) };
                case Endorsement.Type.RSA3072: return new Key { RSA3072 = ByteString.CopyFrom(((ReadOnlyMemory<byte>)endorsement._data).ToArray()) };
                case Endorsement.Type.ECDSA384: return new Key { ECDSA384 = ByteString.CopyFrom(((ReadOnlyMemory<byte>)endorsement._data).ToArray()) };
                case Endorsement.Type.ContractID: return new Key { ContractID = ContractID.Parser.ParseFrom((byte[])endorsement._data) };
                case Endorsement.Type.List: return new Key
                {
                    ThresholdKey = new ThresholdKey
                    {
                        Threshold = endorsement._requiredCount,
                        Keys = ToPublicKeyList((Endorsement[])endorsement._data)
                    }
                };
            }
            throw new InvalidOperationException("Endorsement is Empty.");
        }
        internal static Endorsement FromPublicKey(Key key)
        {            
            switch (key.KeyCase)
            {
                case Key.KeyOneofCase.ContractID: return new Endorsement(Endorsement.Type.ContractID, key.ContractID.ToByteArray());
                case Key.KeyOneofCase.Ed25519: return new Endorsement(Endorsement.Type.Ed25519, new ReadOnlyMemory<byte>(Keys.publicKeyPrefix.Concat(key.Ed25519.ToByteArray()).ToArray()));
                case Key.KeyOneofCase.RSA3072: return new Endorsement(Endorsement.Type.RSA3072, key.RSA3072.ToByteArray());
                case Key.KeyOneofCase.ECDSA384: return new Endorsement(Endorsement.Type.ECDSA384, key.ECDSA384.ToByteArray());
                case Key.KeyOneofCase.ThresholdKey: return new Endorsement(key.ThresholdKey.Threshold, FromPublicKeyList(key.ThresholdKey.Keys));
                case Key.KeyOneofCase.KeyList: return new Endorsement(FromPublicKeyList(key.KeyList));
            }
            throw new InvalidOperationException($"Unknown Key Type {key.KeyCase}.  Do we have a network/library version mismatch?");
        }
        internal static KeyList ToPublicKeyList(Endorsement[] endorsements)
        {
            var keyList = new KeyList();
            keyList.Keys.AddRange(endorsements.Select(endorsement => ToPublicKey(endorsement)));
            return keyList;
        }
        internal static Endorsement[] FromPublicKeyList(KeyList keyList)
        {
            return keyList.Keys.Select(key => FromPublicKey(key)).ToArray();
        }
        internal static AccountInfo FromAccountInfo(CryptoGetInfoResponse.Types.AccountInfo accountInfo)
        {
            return new AccountInfo
            {
                Address = FromAccountID(accountInfo.AccountID),
                SmartContractId = accountInfo.ContractAccountID,
                Deleted = accountInfo.Deleted,
                Proxy = FromAccountID(accountInfo.ProxyAccountID),
                ProxiedToAccount = accountInfo.ProxyReceived,
                Endorsement = FromPublicKey(accountInfo.Key),
                Balance = accountInfo.Balance,
                SendThresholdCreateRecord = accountInfo.GenerateSendRecordThreshold,
                ReceiveThresholdCreateRecord = accountInfo.GenerateReceiveRecordThreshold,
                ReceiveSignatureRequired = accountInfo.ReceiverSigRequired,
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
        internal static void FillReceiptProperties(TransactionID transactionId, Proto.TransactionReceipt receipt, TransactionReceipt result)
        {
            result.Id = Protobuf.FromTransactionId(transactionId);
            result.Status = (ResponseCode)receipt.Status;
        }
        internal static void FillRecordProperties(TransactionID transactionId, Proto.TransactionReceipt receipt, Proto.TransactionRecord record, TransactionRecord result)
        {
            result.Id = Protobuf.FromTransactionId(transactionId);
            result.Status = (ResponseCode)receipt.Status;
            result.Hash = record.TransactionHash?.ToByteArray();
            result.Concensus = record.ConsensusTimestamp == null ? null : (DateTime?)Protobuf.FromTimestamp(record.ConsensusTimestamp);
            result.Memo = record.Memo;
            result.Fee = record.TransactionFee;
        }
    }
}
