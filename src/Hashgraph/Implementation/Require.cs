using System;

namespace Hashgraph.Implementation
{
    internal static class Require
    {
        internal static void PayerInContext(ContextStack context)
        {
            if (context.Payer == null)
            {
                throw new InvalidOperationException("The Payer account has not been configured. Please check that 'Payer' is set in the Client context.");
            }
        }

        internal static void GatewayInContext(ContextStack context)
        {
            if (context.Gateway == null)
            {
                throw new InvalidOperationException("Network Gateway Node has not been configured. Please check that 'Gateway' is set in the Client context.");
            }
        }

        internal static void AddressArgument(Address address)
        {
            if (address == null)
            {
                throw new ArgumentNullException(nameof(address), "Account Address is is missing. Please check that it is not null.");
            }
        }
    }
}
