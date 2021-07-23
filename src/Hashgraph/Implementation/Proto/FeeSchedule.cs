using Google.Protobuf;
using System.Linq;

namespace Proto
{
    public sealed partial class FeeSchedule
    {
        internal Hashgraph.Extensions.FeeSchedule ToFeeSchedule()
        {
            return new Hashgraph.Extensions.FeeSchedule
            {
                Expires = ExpiryTime.ToDateTime(),
                Data = TransactionFeeSchedule.ToDictionary(fee => fee.HederaFunctionality.ToString(), fee => fee.FeeData is null ? "{}" : JsonFormatter.Default.Format(fee.FeeData))
            };
        }
    }
}
