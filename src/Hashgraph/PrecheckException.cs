using System;

namespace Hashgraph
{
    public sealed class PrecheckException : Exception
    {
        public PrecheckResponse PrecheckResponseCode { get; private set; }
        public PrecheckException(string message, PrecheckResponse code) : base(message)
        {
            PrecheckResponseCode = code;
        }
        public PrecheckException(string message, PrecheckResponse code, Exception innerException) : base(message, innerException)
        {
            PrecheckResponseCode = code;
        }
    }
}
