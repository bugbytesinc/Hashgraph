using Google.Protobuf;
using System;
using System.Linq;

namespace Proto;

public sealed partial class FeeSchedule
{
    internal Hashgraph.Extensions.FeeSchedule ToFeeSchedule()
    {
        return new Hashgraph.Extensions.FeeSchedule
        {
            Expires = ExpiryTime.ToDateTime(),
            Data = TransactionFeeSchedule.ToDictionary(
                fee => fee.HederaFunctionality.ToString(),
                fee => fee.Fees?.Select(item => JsonFormatter.Default.Format(item)).ToArray() ?? Array.Empty<string>())
        };
    }
}