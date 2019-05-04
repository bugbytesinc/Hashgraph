using Proto;

namespace Hashgraph.Implementation
{
    internal static class Validate
    {
        internal static void validatePreCheckResult(ResponseHeader header)
        {
            if (header.NodeTransactionPrecheckCode == ResponseCodeEnum.Ok)
            {
                return;
            }
            throw new GatewayException($"Failed Pre-Check: {header.NodeTransactionPrecheckCode}", (PrecheckResponse)header.NodeTransactionPrecheckCode);
        }
    }
}
