using System;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal helper class providing validation checks for 
    /// various methods, throwing invalid operation and argument 
    /// exceptions when required information is missing from the 
    /// context or parameter arguments.
    /// </summary>
    internal static class Require
    {
        internal static Account PayerInContext(ContextStack context)
        {
            var payer = context.Payer;
            if (payer is null)
            {
                throw new InvalidOperationException("The Payer account has not been configured. Please check that 'Payer' is set in the Client context.");
            }
            return payer;
        }

        internal static Gateway GatewayInContext(ContextStack context)
        {
            var gateway = context.Gateway;
            if (gateway is null)
            {
                throw new InvalidOperationException("Network Gateway Node has not been configured. Please check that 'Gateway' is set in the Client context.");
            }
            return gateway;
        }
        internal static void AddressArgument(Address address)
        {
            if (address is null)
            {
                throw new ArgumentNullException(nameof(address), "Account Address is is missing. Please check that it is not null.");
            }
        }
        internal static void InitialBalanceArgument(ulong initialBalance)
        {
            if (initialBalance < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(initialBalance), "Initial Balance must be greater than zero.");
            }
        }

        internal static void AccountToDeleteArgument(Address accountToDelete)
        {
            if (accountToDelete is null)
            {
                throw new ArgumentNullException(nameof(accountToDelete), "Account to Delete is missing. Please check that it is not null.");
            }
        }

        internal static void TransferAccountArgument(Address transferAccount)
        {
            if (transferAccount is null)
            {
                throw new ArgumentNullException(nameof(transferAccount), "Transfer account is is missing. Please check that it is not null.");
            }
        }
        internal static void FromAccountArgument(Account fromAccount)
        {
            if (fromAccount is null)
            {
                throw new ArgumentNullException(nameof(fromAccount), "Account to transfer from is is missing. Please check that it is not null.");
            }
        }

        internal static void ToAddressArgument(Address toAddress)
        {
            if (toAddress is null)
            {
                throw new ArgumentNullException(nameof(toAddress), "Account to transfer to is is missing. Please check that it is not null.");
            }
        }

        internal static void AmountArgument(long amount)
        {
            if (amount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "The amount to transfer must be non-negative.");
            }
        }
        internal static void PublicKeyArgument(ReadOnlyMemory<byte> publicKey)
        {
            if (publicKey.IsEmpty)
            {
                throw new ArgumentOutOfRangeException(nameof(publicKey), "The public key is required.");
            }
            try
            {
                Keys.ImportPublicEd25519KeyFromBytes(publicKey);
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException(nameof(publicKey), ex.Message);
            }
        }
    }
}
