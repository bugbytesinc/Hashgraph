using System;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal helper class providing validation checks for 
    /// various methods, throwing invalid operation and argument 
    /// exceptions when required information is missing from the 
    /// context or parameter arguments.
    /// </summary>
    internal static class RequireInputParameter
    {
        internal static Address Address(Address address)
        {
            if (address is null)
            {
                throw new ArgumentNullException(nameof(address), "Account Address is is missing. Please check that it is not null.");
            }
            return address;
        }
        internal static Address AddressToDelete(Address addressToDelete)
        {
            if (addressToDelete is null)
            {
                throw new ArgumentNullException(nameof(addressToDelete), "Account to Delete is missing. Please check that it is not null.");
            }
            return addressToDelete;
        }

        internal static Address TransferToAddress(Address transferToAddress)
        {
            if (transferToAddress is null)
            {
                throw new ArgumentNullException(nameof(transferToAddress), "Transfer address is is missing. Please check that it is not null.");
            }
            return transferToAddress;
        }

        internal static Account FromAccount(Account fromAccount)
        {
            if (fromAccount is null)
            {
                throw new ArgumentNullException(nameof(fromAccount), "Account to transfer from is is missing. Please check that it is not null.");
            }
            return fromAccount;
        }

        internal static Address ToAddress(Address toAddress)
        {
            if (toAddress is null)
            {
                throw new ArgumentNullException(nameof(toAddress), "Account to transfer to is is missing. Please check that it is not null.");
            }
            return toAddress;
        }

        internal static long Amount(long amount)
        {
            if (amount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "The amount to transfer must be non-negative.");
            }
            return amount;
        }
        internal static UpdateAccountParams UpdateParameters(UpdateAccountParams updateParameters)
        {
            if (updateParameters is null)
            {
                throw new ArgumentNullException(nameof(updateParameters), "Account Update Parameters argument is missing. Please check that it is not null.");
            }
            if (updateParameters.Account is null)
            {
                throw new ArgumentNullException(nameof(updateParameters.Account), "Account is is missing. Please check that it is not null.");
            }
            if (updateParameters.PrivateKey is null &&
                updateParameters.SendThresholdCreateRecord is null &&
                updateParameters.ReceiveThresholdCreateRecord is null &&
                updateParameters.Expiration is null &&
                updateParameters.AutoRenewPeriod is null)
            {
                throw new ArgumentException(nameof(updateParameters), "The Account Updates contains no update properties, it is blank.");
            }
            return updateParameters;
        }
        internal static long AcountNumber(long accountNum)
        {
            if (accountNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(accountNum), "Account Number cannot be negative.");
            }
            return accountNum;
        }

        internal static long ShardNumber(long shardNum)
        {
            if (shardNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(shardNum), "Shard Number cannot be negative.");
            }
            return shardNum;
        }

        internal static long RealmNumber(long realmNum)
        {
            if (realmNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(realmNum), "Realm Number cannot be negative.");
            }
            return realmNum;
        }
        internal static string Url(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentOutOfRangeException(nameof(url), "URL is required.");
            }
            return url;
        }
        internal static CreateAccountParams CreateParameters(CreateAccountParams createParameters)
        {
            if (createParameters is null)
            {
                throw new ArgumentNullException(nameof(createParameters), "The create parameters are is missing. Please check that the argument is not null.");
            }
            if (createParameters.PublicKey.IsEmpty)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.PublicKey), "The public key is required.");
            }
            try
            {
                Keys.ImportPublicEd25519KeyFromBytes(createParameters.PublicKey);
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.PublicKey), ex.Message);
            }
            return createParameters;
        }
    }
}
