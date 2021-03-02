using Google.Protobuf.Collections;
using Hashgraph.Implementation;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Proto
{
    internal static class TransactionRecordExtensions
    {
        private static ReadOnlyCollection<Hashgraph.TransactionRecord> EMPTY_RESULT = new List<Hashgraph.TransactionRecord>().AsReadOnly();
        internal static ReadOnlyCollection<Hashgraph.TransactionRecord> ToTransactionRecordList(this RepeatedField<TransactionRecord> list, TransactionRecord? first)
        {
            var count = (first != null ? 1 : 0) + (list != null ? list.Count : 0);
            if (count > 0)
            {
                var result = new List<Hashgraph.TransactionRecord>(count);
                if (first != null)
                {
                    result.Add(new NetworkResult
                    {
                        TransactionID = first.TransactionID,
                        Receipt = first.Receipt,
                        Record = first
                    }.ToRecord());
                }
                if (list != null && list.Count > 0)
                {
                    foreach (var entry in list)
                    {
                        result.Add(new NetworkResult
                        {
                            TransactionID = entry.TransactionID,
                            Receipt = entry.Receipt,
                            Record = entry
                        }.ToRecord());
                    }
                }
                return result.AsReadOnly();
            }
            return EMPTY_RESULT;
        }
    }
}
