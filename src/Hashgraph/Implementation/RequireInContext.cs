using System;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal helper class providing validation checks for 
    /// for the current context throwing invalid operation 
    /// exceptions when required information is missing 
    /// from the client context.
    /// </summary>
    internal static class RequireInContext
    {
        internal static Address Payer(ContextStack context)
        {
            var payer = context.Payer;
            if (payer is null)
            {
                throw new InvalidOperationException("The Payer address has not been configured. Please check that 'Payer' is set in the Client context.");
            }
            return payer;
        }
        internal static Signatory Signatory(ContextStack context)
        {
            var signatory = context.Signatory;
            if (signatory is null)
            {
                throw new InvalidOperationException("The Payer's signatory (signing key/callback) has not been configured. This is required for retreiving records and other general network Queries. Please check that 'Signatory' is set in the Client context.");
            }
            return signatory;
        }
        internal static Gateway Gateway(ContextStack context)
        {
            var gateway = context.Gateway;
            if (gateway is null)
            {
                throw new InvalidOperationException("The Network Gateway Node has not been configured. Please check that 'Gateway' is set in the Client context.");
            }
            return gateway;
        }

        internal static long QueryFee(ContextStack context, long requiredFee)
        {
            var feeLimit = context.FeeLimit;
            if (feeLimit < requiredFee)
            {
                throw new InvalidOperationException($"The user specified fee limit is not enough for the anticipated query required fee of {requiredFee:n0} tinybars.");
            }
            return requiredFee;
        }
    }
}
