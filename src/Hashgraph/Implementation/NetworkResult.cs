#pragma warning disable CS8618
using Proto;

namespace Hashgraph.Implementation
{
    internal class NetworkResult
    {
        internal TransactionID TransactionID { get; set; }
        internal Proto.TransactionReceipt Receipt { get; set; }
        internal Proto.TransactionRecord? Record { get; set; }

        internal TransactionReceipt ToReceipt()
        {
            if (Receipt.AccountID != null)
            {
                return new CreateAccountReceipt(this);
            }
            else if (Receipt.FileID != null)
            {
                return new FileReceipt(this);
            }
            else if (Receipt.TopicID != null)
            {
                return new CreateTopicReceipt(this);
            }
            else if (Receipt.ContractID != null)
            {
                return new CreateContractReceipt(this);
            }
            else if (Receipt.TokenID != null)
            {
                return new CreateTokenReceipt(this);
            }
            else if (Receipt.NewTotalSupply != 0)
            {
                return new TokenReceipt(this);
            }
            else if (!Receipt.TopicRunningHash.IsEmpty)
            {
                return new SubmitMessageReceipt(this);
            }
            else if (Receipt.SerialNumbers != null && Receipt.SerialNumbers.Count > 0)
            {
                return new AssetMintReceipt(this);
            }
            else
            {
                return new TransactionReceipt(this);
            }
        }

        internal TransactionRecord ToRecord()
        {
            if (Receipt.AccountID != null)
            {
                return new CreateAccountRecord(this);
            }
            else if (Receipt.FileID != null)
            {
                return new FileRecord(this);
            }
            else if (Receipt.TopicID != null)
            {
                return new CreateTopicRecord(this);
            }
            else if (Receipt.ContractID != null)
            {
                return new CreateContractRecord(this);
            }
            else if (Receipt.TokenID != null)
            {
                return new CreateTokenRecord(this);
            }
            else if (Receipt.NewTotalSupply != 0)
            {
                return new TokenRecord(this);
            }
            else if (!Receipt.TopicRunningHash.IsEmpty)
            {
                return new SubmitMessageRecord(this);
            }
            else if (Receipt.SerialNumbers != null && Receipt.SerialNumbers.Count > 0)
            {
                return new AssetMintRecord(this);
            }
            else if (Record?.ContractCallResult != null)
            {
                return new CallContractRecord(this);
            }
            else
            {
                return new TransactionRecord(this);
            }
        }
    }
}
