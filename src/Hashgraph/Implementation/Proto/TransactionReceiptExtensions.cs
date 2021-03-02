using Google.Protobuf.Collections;
using Hashgraph.Implementation;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Proto
{
    internal static class TransactionReceiptExtensions
    {
        private static ReadOnlyCollection<Hashgraph.TransactionReceipt> EMPTY_RESULT = new List<Hashgraph.TransactionReceipt>().AsReadOnly();
        internal static ReadOnlyCollection<Hashgraph.TransactionReceipt> ToTransactionReceiptList(this RepeatedField<TransactionReceipt> list, TransactionReceipt first, TransactionID transactionId)
        {
            var count = (first != null ? 1 : 0) + (list != null ? list.Count : 0);
            if (count > 0)
            {
                var result = new List<Hashgraph.TransactionReceipt>(count);
                if (first != null)
                {
                    result.Add(new NetworkResult
                    {
                        TransactionID = transactionId,
                        Receipt = first
                    }.ToReceipt());
                }
                if (list != null && list.Count > 0)
                {
                    foreach (var entry in list)
                    {
                        result.Add(new NetworkResult
                        {
                            TransactionID = transactionId,
                            Receipt = entry
                        }.ToReceipt());
                    }
                }
                return result.AsReadOnly();
            }
            return EMPTY_RESULT;
        }
    }
}
