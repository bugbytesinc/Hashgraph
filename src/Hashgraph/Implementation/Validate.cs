using Proto;

namespace Hashgraph.Implementation
{
    internal static class Validate
    {
        internal static void ValidatePreCheckResult(ResponseCodeEnum code)
        {
            if (code == ResponseCodeEnum.Ok)
            {
                return;
            }
            throw new PrecheckException($"Failed Pre-Check: {code}", (PrecheckResponse)code);
        }
    }
}
