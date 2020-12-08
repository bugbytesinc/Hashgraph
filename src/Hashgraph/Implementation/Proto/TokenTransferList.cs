using Google.Protobuf.Collections;
using Hashgraph;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Proto
{
    internal static class TokenTransferListExtensions
    {
        private static ReadOnlyCollection<TokenTransfer> EMPTY_RESULT = new List<TokenTransfer>().AsReadOnly();
        internal static ReadOnlyCollection<TokenTransfer> ToTransfers(this RepeatedField<TokenTransferList> list)
        {
            if (list != null && list.Count > 0)
            {
                var collector = new Dictionary<(Address, Address), long>();
                foreach (var xferList in list)
                {
                    var token = xferList.Token.ToAddress();
                    foreach (var xfer in xferList.Transfers)
                    {
                        var key = (token, xfer.AccountID.ToAddress());
                        collector.TryGetValue(key, out long amount);
                        collector[key] = amount + xfer.Amount;
                    }
                }
                var result = new List<TokenTransfer>(collector.Count);
                foreach (var entry in collector)
                {
                    result.Add(new TokenTransfer(entry.Key.Item1, entry.Key.Item2, entry.Value));
                }
                return result.AsReadOnly();
            }
            return EMPTY_RESULT;
        }
    }
}
