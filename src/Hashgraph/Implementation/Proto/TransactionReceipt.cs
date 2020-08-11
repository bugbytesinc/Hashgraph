﻿using Hashgraph;
using Hashgraph.Implementation;

namespace Proto
{
    public sealed partial class TransactionReceipt
    {
        internal Hashgraph.TransactionReceipt ToTransactionReceipt(TransactionID transactionId)
        {
            if (AccountID != null)
            {
                return FillProperties(transactionId, new CreateAccountReceipt());
            }
            else if (FileID != null)
            {
                return FillProperties(transactionId, new FileReceipt());
            }
            else if (TopicID != null)
            {
                return FillProperties(transactionId, new CreateTopicReceipt());
            }
            else if (ContractID != null)
            {
                return FillProperties(transactionId, new CreateContractReceipt());
            }
            else if (!TopicRunningHash.IsEmpty)
            {
                return FillProperties(transactionId, new SubmitMessageReceipt());
            }
            else
            {
                return FillProperties(transactionId, new Hashgraph.TransactionReceipt());
            }
        }
        internal CreateTopicReceipt FillProperties(TransactionID transactionId, CreateTopicReceipt receipt)
        {
            FillCommonProperties(transactionId, receipt);
            receipt.Topic = TopicID.ToAddress();
            return receipt;
        }
        internal SubmitMessageReceipt FillProperties(TransactionID transactionId, SubmitMessageReceipt receipt)
        {
            FillCommonProperties(transactionId, receipt);
            receipt.RunningHash = TopicRunningHash?.ToByteArray();
            receipt.RunningHashVersion = TopicRunningHashVersion;
            receipt.SequenceNumber = TopicSequenceNumber;
            return receipt;
        }
        internal CreateContractReceipt FillProperties(TransactionID transactionId, CreateContractReceipt receipt)
        {
            FillCommonProperties(transactionId, receipt);
            receipt.Contract = ContractID.ToAddress();
            return receipt;
        }
        internal CreateAccountReceipt FillProperties(TransactionID transactionId, CreateAccountReceipt receipt)
        {
            FillCommonProperties(transactionId, receipt);
            receipt.Address = AccountID.ToAddress();
            return receipt;
        }
        internal FileReceipt FillProperties(TransactionID transactionId, FileReceipt receipt)
        {
            FillCommonProperties(transactionId, receipt);
            receipt.File = FileID.ToAddress();
            return receipt;
        }
        internal Hashgraph.TransactionReceipt FillProperties(TransactionID transactionId, Hashgraph.TransactionReceipt result)
        {
            FillCommonProperties(transactionId, result);
            return result;
        }

        private void FillCommonProperties(TransactionID transactionId, Hashgraph.TransactionReceipt result)
        {
            result.Id = transactionId.ToTxId();
            result.Status = (ResponseCode)Status;
            if (ExchangeRate != null)
            {
                result.CurrentExchangeRate = ExchangeRate.CurrentRate?.ToExchangeRate();
                result.NextExchangeRate = ExchangeRate.NextRate?.ToExchangeRate();
            }
        }

    }
}