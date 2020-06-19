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
            throw new PrecheckException($"Transaction Failed Pre-Check: {response.NodeTransactionPrecheckCode}", transactionId.ToTxId(), (ResponseCode)response.NodeTransactionPrecheckCode, response.Cost);
        }
        internal static void ResponseHeader(TransactionID transactionId, ResponseHeader? header)
        {
            if (header == null)
            {
                throw new PrecheckException($"Transaction Failed to Produce a Response.", transactionId.ToTxId(), ResponseCode.Unknown, 0);
            }
            if (header.NodeTransactionPrecheckCode == Proto.ResponseCodeEnum.Ok)
            {
                return;
            }
            throw new PrecheckException($"Transaction Failed Pre-Check: {header.NodeTransactionPrecheckCode}", transactionId.ToTxId(), (ResponseCode)header.NodeTransactionPrecheckCode, header.Cost);
        }
    }
}