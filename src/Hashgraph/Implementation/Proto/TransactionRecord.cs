using Hashgraph;
using Hashgraph.Implementation;

namespace Proto
{
    public sealed partial class TransactionRecord
    {
        internal Hashgraph.TransactionRecord ToTransactionRecord()
        {
            if (Receipt.AccountID != null)
            {
                return FillProperties(new CreateAccountRecord());
            }
            else if (Receipt.FileID != null)
            {
                return FillProperties(new FileRecord());
            }
            else if (Receipt.TopicID != null)
            {
                return FillProperties(new CreateTopicRecord());
            }
            else if (Receipt.ContractID != null)
            {
                return FillProperties(new CreateContractRecord());
            }
            else if (!Receipt.TopicRunningHash.IsEmpty)
            {
                return FillProperties(new SubmitMessageRecord());
            }
            else if (ContractCallResult != null)
            {
                return FillProperties(new CallContractRecord());
            }
            else
            {
                return FillProperties(new Hashgraph.TransactionRecord());
            }
        }
        internal CreateTopicRecord FillProperties(CreateTopicRecord record)
        {
            FillCommonProperties(record);
            record.Topic = Receipt.TopicID.ToAddress();
            return record;
        }
        internal SubmitMessageRecord FillProperties(SubmitMessageRecord record)
        {
            FillCommonProperties(record);
            record.RunningHash = Receipt.TopicRunningHash?.ToByteArray();
            record.RunningHashVersion = Receipt.TopicRunningHashVersion;
            record.SequenceNumber = Receipt.TopicSequenceNumber;
            return record;
        }
        internal CallContractRecord FillProperties(CallContractRecord record)
        {
            FillCommonProperties(record);
            record.CallResult = ContractCallResult.ToContractCallResult();
            return record;
        }
        internal CreateContractRecord FillProperties(CreateContractRecord record)
        {
            FillCommonProperties(record);
            record.Contract = Receipt.ContractID.ToAddress();
            record.CallResult = ContractCreateResult.ToContractCallResult();
            return record;
        }
        internal CreateAccountRecord FillProperties(CreateAccountRecord record)
        {
            FillCommonProperties(record);
            record.Address = Receipt.AccountID.ToAddress()
                ;
            return record;
        }
        internal FileRecord FillProperties(FileRecord record)
        {
            FillCommonProperties(record);
            record.File = Receipt.FileID.ToAddress();
            return record;
        }
        internal Hashgraph.TransactionRecord FillProperties(Hashgraph.TransactionRecord record)
        {
            FillCommonProperties(record);
            return record;
        }
        private void FillCommonProperties(Hashgraph.TransactionRecord record)
        {
            record.Id = TransactionID.ToTxId();
            record.Status = (ResponseCode)Receipt.Status;
            record.Hash = TransactionHash?.ToByteArray();
            record.Concensus = ConsensusTimestamp?.ToDateTime();
            record.Memo = Memo;
            record.Fee = TransactionFee;
            record.Transfers = TransferList.ToTransfers();
            if (Receipt.ExchangeRate != null)
            {
                record.CurrentExchangeRate = Receipt.ExchangeRate.CurrentRate?.ToExchangeRate();
                record.NextExchangeRate = Receipt.ExchangeRate.NextRate?.ToExchangeRate();
            }
        }
    }
}
