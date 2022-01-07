using Google.Protobuf.Collections;
using Hashgraph;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Proto;

internal static class AssessedCustomFeeExtensions
{
    private static ReadOnlyCollection<RoyaltyTransfer> EMPTY_RESULT = new List<RoyaltyTransfer>().AsReadOnly();
    internal static ReadOnlyCollection<RoyaltyTransfer> AsRoyaltyTransferList(this RepeatedField<AssessedCustomFee> list)
    {
        if (list != null && list.Count > 0)
        {
            return list.Select(fee => new RoyaltyTransfer(fee)).ToList().AsReadOnly();
        }
        return EMPTY_RESULT;
    }
}