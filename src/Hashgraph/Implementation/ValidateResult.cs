using Proto;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal helper class validating server responses.
    /// </summary>
    internal static class ValidateResult
    {
        internal static void PreCheck(TransactionID transactionId, ResponseCodeEnum code)
        {
            if (code == Proto.ResponseCodeEnum.Ok)
            {
                return;
            }
            throw new PrecheckException($"Transaction Failed Pre-Check: {code}", Protobuf.FromTransactionId(transactionId), (ResponseCode)code);
        }
    }
}