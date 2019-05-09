using Proto;

namespace Hashgraph.Implementation
{
    internal static class Validate
    {
        internal static void ValidatePreCheckResult(TransactionID transactionId, ResponseCodeEnum code)
        {
            if (code == ResponseCodeEnum.Ok)
            {
                return;
            }
            throw new PrecheckException($"Transaction Failed Pre-Check: {code}", Protobuf.FromTransactionId(transactionId), (ResponseCode)code);
        }
    }
}
