using Google.Protobuf.Collections;
using Hashgraph;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Proto
{
    internal static class AssessedCustomFeeExtensions
    {
        private static ReadOnlyCollection<TokenTransfer> EMPTY_RESULT = new List<TokenTransfer>().AsReadOnly();
        internal static ReadOnlyCollection<TokenTransfer> AsTokenTransferList(this RepeatedField<AssessedCustomFee> list)
        {
            if (list != null && list.Count > 0)
            {
                return list.Select(fee => new TokenTransfer(fee.TokenId.AsAddress(), fee.FeeCollectorAccountId.AsAddress(), fee.Amount)).ToList().AsReadOnly();
            }
            return EMPTY_RESULT;
        }
    }
}
