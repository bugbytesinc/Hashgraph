using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Proto
{
    public sealed partial class TransferList
    {
        internal ReadOnlyDictionary<Hashgraph.Address, long> ToTransfers()
        {
            var results = new Dictionary<Hashgraph.Address, long>();
            foreach (var xfer in AccountAmounts)
            {
                var account = xfer.AccountID.AsAddress();
                results.TryGetValue(account, out long amount);
                results[account] = amount + xfer.Amount;
            }
            return new ReadOnlyDictionary<Hashgraph.Address, long>(results);
        }
    }
}
