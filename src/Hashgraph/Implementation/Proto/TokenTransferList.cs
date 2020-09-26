using Google.Protobuf.Collections;
using Hashgraph;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Proto
{
    internal static class TokenTransferListExtensions
    {
        private static ReadOnlyCollection<Hashgraph.TokenAddressTransfer> EMPTY_RESULT = new List<Hashgraph.TokenAddressTransfer>().AsReadOnly();
        internal static ReadOnlyCollection<Hashgraph.TokenAddressTransfer> ToTransfers(this RepeatedField<TokenTransferList> list)
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
                var result = new List<Hashgraph.TokenAddressTransfer>(collector.Count);
                foreach (var entry in collector)
                {
                    result.Add(new Hashgraph.TokenAddressTransfer(entry.Key.Item1, entry.Key.Item2, entry.Value));
                }
                return result.AsReadOnly();
            }
            return EMPTY_RESULT;
        }
    }
}
