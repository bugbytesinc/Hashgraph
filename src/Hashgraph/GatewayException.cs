using System;

namespace Hashgraph
{
    public sealed class GatewayException : Exception
    {
        public PrecheckResponse PrecheckResponseCode { get; private set; }
        public GatewayException(string message, PrecheckResponse code) : base(message)
        {
            PrecheckResponseCode = code;
        }
        public GatewayException(string message, PrecheckResponse code, Exception innerException) : base(message, innerException)
        {
            PrecheckResponseCode = code;
        }
    }
}
