using Google.Protobuf;
using Hashgraph.Extensions;
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
        internal static TxId FromTransactionId(TransactionID transactionId)
        {
            return new TxId
            {
                Address = FromAccountID(transactionId.AccountID),
                ValidStartSeconds = transactionId.TransactionValidStart.Seconds,
                ValidStartNanos = transactionId.TransactionValidStart.Nanos
            };
        }
        internal static TransactionID ToTransactionID(TxId transaction)
        {
            return new TransactionID
            {
                AccountID = ToAccountID(transaction.Address),
                TransactionValidStart = new Timestamp
                {
                    Seconds = transaction.ValidStartSeconds,
                    Nanos = transaction.ValidStartNanos
                }
            };
        }
        internal static AccountID ToAccountID(Address address)
        {
            return new AccountID
            {
                ShardNum = address.ShardNum,
                RealmNum = address.RealmNum,
                AccountNum = address.AccountNum
            };
        }
        internal static Address FromAccountID(AccountID accountId)
        {
            return new Address(accountId.ShardNum, accountId.RealmNum, accountId.AccountNum);
        }
        internal static FileID ToFileId(Address file)
        {
            return new FileID
            {
                ShardNum = file.ShardNum,
                RealmNum = file.RealmNum,
                FileNum = file.AccountNum
            };
        }

        internal static Address FromFileID(FileID fileID)
        {
            return new Address(fileID.ShardNum, fileID.RealmNum, fileID.FileNum);
        }

        internal static ContractID ToContractID(Address address)
        {
            return new ContractID
            {
                ShardNum = address.ShardNum,
                RealmNum = address.RealmNum,
                ContractNum = address.AccountNum
            };
        }

        internal static Address FromContractID(ContractID contract)
        {
            return new Address(contract.ShardNum, contract.RealmNum, contract.ContractNum);
        }

        internal static TopicID ToTopicID(Address address)
        {
            return new TopicID
            {
                ShardNum = address.ShardNum,
                RealmNum = address.RealmNum,
                TopicNum = address.AccountNum
            };
        }

        internal static Address FromTopicID(TopicID Topic)
        {
            return new Address(Topic.ShardNum, Topic.RealmNum, Topic.TopicNum);
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
            var (seconds, nanos) = Epoch.FromDate(dateTime);
            return new Timestamp
            {
                Seconds = seconds,
                Nanos = nanos
            };
        }
        internal static DateTime FromTimestamp(Timestamp timestamp)
        {
            return Epoch.ToDate(timestamp.Seconds, timestamp.Nanos);
        }
        internal static DateTime FromTimestamp(TimestampSeconds timestamp)
        {
            return Epoch.ToDate(timestamp.Seconds, 0);
        }
        internal static Key ToPublicKey(Endorsement endorsement)
        {
            switch (endorsement.Type)
            {
                case KeyType.Ed25519: return new Key { Ed25519 = ByteString.CopyFrom(((NSec.Cryptography.PublicKey)endorsement._data).Export(NSec.Cryptography.KeyBlobFormat.PkixPublicKey).TakeLast(32).ToArray()) };
                case KeyType.RSA3072: return new Key { RSA3072 = ByteString.CopyFrom(((ReadOnlyMemory<byte>)endorsement._data).ToArray()) };
                case KeyType.ECDSA384: return new Key { ECDSA384 = ByteString.CopyFrom(((ReadOnlyMemory<byte>)endorsement._data).ToArray()) };
                case KeyType.List:
                    return new Key
                    {
                        ThresholdKey = new ThresholdKey
                        {
                            Threshold = endorsement.RequiredCount,
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
                case Key.KeyOneofCase.Ed25519: return new Endorsement(KeyType.Ed25519, new ReadOnlyMemory<byte>(Keys.publicKeyPrefix.Concat(key.Ed25519.ToByteArray()).ToArray()));
                case Key.KeyOneofCase.RSA3072: return new Endorsement(KeyType.RSA3072, key.RSA3072.ToByteArray());
                case Key.KeyOneofCase.ECDSA384: return new Endorsement(KeyType.ECDSA384, key.ECDSA384.ToByteArray());
                case Key.KeyOneofCase.ThresholdKey:
                    return key.ThresholdKey.Keys.Keys.Count == 0 ?
                        Endorsement.None :
                        new Endorsement(key.ThresholdKey.Threshold, FromPublicKeyList(key.ThresholdKey.Keys));
                case Key.KeyOneofCase.KeyList:
                    return
                        key.KeyList.Keys.Count == 0 ?
                        Endorsement.None :
                        new Endorsement(FromPublicKeyList(key.KeyList));
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
                AutoRenewPeriod = FromDuration(accountInfo.AutoRenewPeriod),
                Expiration = FromTimestamp(accountInfo.ExpirationTime)
            };
        }
        internal static ContractInfo FromContractInfo(ContractGetInfoResponse.Types.ContractInfo contractInfo)
        {
            return new ContractInfo
            {
                Contract = FromContractID(contractInfo.ContractID),
                Address = FromAccountID(contractInfo.AccountID),
                SmartContractId = contractInfo.ContractAccountID,
                Administrator = contractInfo.AdminKey is null ? null : FromPublicKey(contractInfo.AdminKey),
                Expiration = FromTimestamp(contractInfo.ExpirationTime),
                RenewPeriod = FromDuration(contractInfo.AutoRenewPeriod),
                Size = contractInfo.Storage,
                Memo = contractInfo.Memo,
                Balance = contractInfo.Balance
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

        internal static TopicInfo FromTopicInfo(ConsensusTopicInfo topicInfo)
        {
            return new TopicInfo
            {
                Memo = topicInfo.Memo,
                RunningHash = topicInfo.RunningHash.ToArray(),
                SequenceNumber = topicInfo.SequenceNumber,
                Expiration = FromTimestamp(topicInfo.ExpirationTime),
                Administrator = topicInfo.AdminKey == null ? null : FromPublicKey(topicInfo.AdminKey),
                Participant = topicInfo.SubmitKey == null ? null : FromPublicKey(topicInfo.SubmitKey),
                AutoRenewPeriod = FromDuration(topicInfo.AutoRenewPeriod),
                RenewAccount = topicInfo.AutoRenewAccount == null ? null : FromAccountID(topicInfo.AutoRenewAccount)
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
            result.Id = FromTransactionId(transactionId);
            result.Status = (ResponseCode)receipt.Status;
            if (receipt.ExchangeRate != null)
            {
                result.CurrentExchangeRate = FromExchangeRate(receipt.ExchangeRate.CurrentRate);
                result.NextExchangeRate = FromExchangeRate(receipt.ExchangeRate.NextRate);
            }
        }

        internal static void FillRecordProperties(Proto.TransactionRecord record, TransactionRecord result)
        {
            result.Id = FromTransactionId(record.TransactionID);
            result.Status = (ResponseCode)record.Receipt.Status;
            result.Hash = record.TransactionHash?.ToByteArray();
            result.Concensus = record.ConsensusTimestamp == null ? null : (DateTime?)Protobuf.FromTimestamp(record.ConsensusTimestamp);
            result.Memo = record.Memo;
            result.Fee = record.TransactionFee;
            result.Transfers = FromTransferList(record.TransferList);
            if (record.Receipt.ExchangeRate != null)
            {
                result.CurrentExchangeRate = FromExchangeRate(record.Receipt.ExchangeRate.CurrentRate);
                result.NextExchangeRate = FromExchangeRate(record.Receipt.ExchangeRate.NextRate);
            }
        }
        internal static ContractCallResult FromContractCallResult(ContractFunctionResult contractFunctionResult)
        {
            return new ContractCallResult
            {
                Result = new ContractCallResultData(contractFunctionResult.ContractCallResult.ToArray()),
                Error = contractFunctionResult.ErrorMessage,
                Bloom = contractFunctionResult.Bloom.ToArray(),
                Gas = contractFunctionResult.GasUsed,
                Events = contractFunctionResult.LogInfo?.Select(log => new ContractEvent
                {
                    Contract = FromContractID(log.ContractID),
                    Bloom = log.Bloom.ToArray(),
                    Topic = log.Topic.Select(bs => new ReadOnlyMemory<byte>(bs.ToArray())).ToArray(),
                    Data = new ContractCallResultData(log.Data.ToArray())
                }).ToArray() ?? new ContractEvent[0]
            };
        }
        internal static ExchangeRate? FromExchangeRate(Proto.ExchangeRate rate)
        {
            return rate == null
                ? null :
            new ExchangeRate
            {
                HBarEquivalent = rate.HbarEquiv,
                USDCentEquivalent = rate.CentEquiv,
                Expiration = FromTimestamp(rate.ExpirationTime)
            };
        }
        internal static Hashgraph.Extensions.FeeSchedule? FromFeeSchedule(Proto.FeeSchedule schedule)
        {
            if (schedule == null)
            {
                return null;
            }
            return new Hashgraph.Extensions.FeeSchedule()
            {
                Expires = FromTimestamp(schedule.ExpiryTime),
                Data = schedule.TransactionFeeSchedule.ToDictionary(fee => fee.HederaFunctionality.ToString(), fee => JsonFormatter.Default.Format(fee.FeeData))
            };
        }
        internal static NodeInfo[] FromNodeAddressBook(NodeAddressBook book)
        {
            return book.NodeAddress.Select(a => new NodeInfo
            {
                IpAddress = a.IpAddress.ToStringUtf8(),
                Port = a.Portno,
                Memo = a.Memo.ToStringUtf8(),
                RsaPublicKey = a.RSAPubKey
            }).ToArray();
        }
    }
}
