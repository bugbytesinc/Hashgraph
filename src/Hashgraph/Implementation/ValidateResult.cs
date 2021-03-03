using Proto;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal helper class validating server responses.
    /// </summary>
    internal static class ValidateResult
    {
        internal static void PreCheck(TransactionID transactionId, TransactionResponse response)
        {
            if (response.NodeTransactionPrecheckCode == Proto.ResponseCodeEnum.Ok)
            {
                return;
            }
            var responseCode = (ResponseCode)response.NodeTransactionPrecheckCode;
            throw new PrecheckException($"Transaction Failed Pre-Check: {responseCode}", transactionId.AsTxId(), responseCode, response.Cost);
        }
    }
}