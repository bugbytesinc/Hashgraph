#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference or unconstrained type parameter.

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
        internal static void InitialBalanceArgument(ulong initialBalance)
        {
            if (initialBalance < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(initialBalance), "Initial Balance must be greater than zero.");
            }
        }

        internal static void AccountToDeleteArgument(Address accountToDelete)
        {
            if (accountToDelete == null)
            {
                throw new ArgumentNullException(nameof(accountToDelete), "Account to Delete is missing. Please check that it is not null.");
            }
        }

        internal static void TransferAccountArgument(Address transferAccount)
        {
            if (transferAccount == null)
            {
                throw new ArgumentNullException(nameof(transferAccount), "Transfer account is is missing. Please check that it is not null.");
            }
        }
        internal static void FromAccountArgument(Account fromAccount)
        {
            if (fromAccount == null)
            {
                throw new ArgumentNullException(nameof(fromAccount), "Account to transfer from is is missing. Please check that it is not null.");
            }
        }

        internal static void ToAddressArgument(Address toAddress)
        {
            if (toAddress == null)
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
