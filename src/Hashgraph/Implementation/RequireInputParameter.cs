using NSec.Cryptography;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                throw new ArgumentNullException(nameof(address), "Account Address is missing. Please check that it is not null.");
            }
            return address;
        }
        internal static Address Contract(Address contract)
        {
            if (contract is null)
            {
                throw new ArgumentNullException(nameof(contract), "Contract Address is missing. Please check that it is not null.");
            }
            return contract;
        }
        internal static string SmartContractId(string smartContractId)
        {
            if (string.IsNullOrWhiteSpace(smartContractId))
            {
                throw new ArgumentNullException(nameof(smartContractId), "Smart Contract ID is missing. Please check that it is not null.");
            }
            return smartContractId;
        }

        internal static Account AccountToDelete(Account accountToDelete)
        {
            if (accountToDelete is null)
            {
                throw new ArgumentNullException(nameof(accountToDelete), "Account to Delete is missing. Please check that it is not null.");
            }
            return accountToDelete;
        }
        internal static Address File(Address file)
        {
            if (file is null)
            {
                throw new ArgumentNullException(nameof(file), "File is missing. Please check that it is not null.");
            }
            return file;
        }
        internal static Address FileToDelete(Address fileToDelete)
        {
            if (fileToDelete is null)
            {
                throw new ArgumentNullException(nameof(fileToDelete), "File to Delete is missing. Please check that it is not null.");
            }
            return fileToDelete;
        }

        internal static Address ContractToDelete(Address contractToDelete)
        {
            if (contractToDelete is null)
            {
                throw new ArgumentNullException(nameof(contractToDelete), "Contract to Delete is missing. Please check that it is not null.");
            }
            return contractToDelete;
        }

        internal static Address TransferToAddress(Address transferToAddress)
        {
            if (transferToAddress is null)
            {
                throw new ArgumentNullException(nameof(transferToAddress), "Transfer address is missing. Please check that it is not null.");
            }
            return transferToAddress;
        }
        internal static Account FromAccount(Account fromAccount)
        {
            if (fromAccount is null)
            {
                throw new ArgumentNullException(nameof(fromAccount), "Account to transfer from is missing. Please check that it is not null.");
            }
            return fromAccount;
        }

        internal static Address ToAddress(Address toAddress)
        {
            if (toAddress is null)
            {
                throw new ArgumentNullException(nameof(toAddress), "Account to transfer to is missing. Please check that it is not null.");
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
        internal static TxId Transaction(TxId transaction)
        {
            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction), "Transaction is missing. Please check that it is not null.");
            }
            return transaction;
        }

        internal static ReadOnlyMemory<byte> Hash(ReadOnlyMemory<byte> hash)
        {
            if (hash.IsEmpty)
            {
                throw new ArgumentNullException(nameof(hash), "The claim hash is missing. Please check that it is not null.");
            }
            if (hash.Length != 48)
            {
                throw new ArgumentOutOfRangeException(nameof(hash), "The claim hash is expected to be 48 bytes in length.");
            }
            return hash;
        }

        internal static UpdateAccountParams UpdateParameters(UpdateAccountParams updateParameters)
        {
            if (updateParameters is null)
            {
                throw new ArgumentNullException(nameof(updateParameters), "Account Update Parameters argument is missing. Please check that it is not null.");
            }
            if (updateParameters.Account is null)
            {
                throw new ArgumentNullException(nameof(updateParameters.Account), "Account is missing. Please check that it is not null.");
            }
            if (updateParameters.Endorsement is null &&
                updateParameters.SendThresholdCreateRecord is null &&
                updateParameters.ReceiveThresholdCreateRecord is null &&
                updateParameters.Expiration is null &&
                updateParameters.AutoRenewPeriod is null &&
                updateParameters.Proxy is null)
            {
                throw new ArgumentException(nameof(updateParameters), "The Account Updates contains no update properties, it is blank.");
            }
            return updateParameters;
        }

        internal static (Address address, long amount)[] MultiTransfers(Dictionary<Account, long> sendAccounts, Dictionary<Address, long> receiveAddresses)
        {
            if (sendAccounts is null)
            {
                throw new ArgumentNullException(nameof(sendAccounts), "The send accounts parameter cannot be null.");
            }
            if (sendAccounts.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sendAccounts), "There must be at least one send account to transfer money from.");
            }
            if (receiveAddresses is null)
            {
                throw new ArgumentNullException(nameof(receiveAddresses), "The receive address parameter cannot be null.");
            }
            if (receiveAddresses.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(receiveAddresses), "There must be at least one receive address to transfer money to.");
            }
            long total = 0;
            var list = new List<(Address address, long amount)>();
            foreach (var pair in sendAccounts)
            {
                if (pair.Key is null)
                {
                    throw new ArgumentNullException(nameof(sendAccounts), "Found a null entry in the send accounts list.");
                }
                if (pair.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(sendAccounts), "All amount entries must be positive values");
                }
                total -= pair.Value;
                list.Add((pair.Key, -pair.Value));
            }
            foreach (var pair in receiveAddresses)
            {
                if (pair.Key is null)
                {
                    throw new ArgumentNullException(nameof(receiveAddresses), "Found a null entry in the receive addresses list.");
                }
                if (pair.Value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(receiveAddresses), "All amount entries must be positive values");
                }
                total += pair.Value;
                list.Add((pair.Key, pair.Value));
            }
            if (total != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sendAccounts), "The sum of sends and receives does not balance.");
            }
            return list.ToArray();
        }

        internal static UpdateContractParams UpdateParameters(UpdateContractParams updateParameters)
        {
            {
                if (updateParameters is null)
                {
                    throw new ArgumentNullException(nameof(updateParameters), "Contract Update Parameters argument is missing. Please check that it is not null.");
                }
                if (updateParameters.Contract is null)
                {
                    throw new ArgumentNullException(nameof(updateParameters.Contract), "Contract address is missing. Please check that it is not null.");
                }
                if (updateParameters.Expiration is null &&
                    updateParameters.Administrator is null &&
                    updateParameters.RenewPeriod is null &&
                    updateParameters.File is null &&
                    updateParameters.Memo is null)
                {
                    throw new ArgumentException("The Contract Updates contains no update properties, it is blank.", nameof(updateParameters));
                }
                return updateParameters;
            }
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
        internal static PublicKey[] PublicKeys(ReadOnlyMemory<byte>[] publicKeys)
        {
            if (publicKeys.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(publicKeys), "At least one public key is required.");
            }
            var result = new PublicKey[publicKeys.Length];
            for (int i = 0; i < publicKeys.Length; i++)
            {
                try
                {
                    result[i] = Keys.ImportPublicEd25519KeyFromBytes(publicKeys[i]);
                }
                catch (Exception ex)
                {
                    throw new ArgumentOutOfRangeException(nameof(publicKeys), ex.Message);
                }
            }
            return result;
        }
        internal static Key[] PrivateKeys(ReadOnlyMemory<byte>[] privateKeys)
        {
            if(privateKeys is null)
            {
                return new Key[0];
            }
            var result = new Key[privateKeys.Length];
            for (int i = 0; i < result.Length; i++)
            {
                try
                {
                    result[i] = Keys.ImportPrivateEd25519KeyFromBytes(privateKeys[i]);
                }
                catch (Exception ex)
                {
                    throw new ArgumentOutOfRangeException(nameof(privateKeys), $"Unable to create Account object, {ex.Message}");
                }
            }
            return result;
        }
        internal static Endorsement[] Endorsements(Endorsement[] endorsements)
        {
            if (endorsements is null)
            {
                throw new ArgumentNullException(nameof(endorsements), "The list of endorsements may not be null.");
            }
            else if (endorsements.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(endorsements), "At least one endorsement in a list is required.");
            }
            for (int i = 0; i < endorsements.Length; i++)
            {
                if (endorsements[i] is null)
                {
                    throw new ArgumentNullException(nameof(endorsements), "No endorsement within the list may be null.");
                }
            }
            return endorsements;
        }
        internal static uint RequiredCount(uint requiredCount, int maxCount)
        {
            if (requiredCount > maxCount)
            {
                throw new ArgumentOutOfRangeException(nameof(requiredCount), "The required number of keys for a valid signature cannot exceed the number of public keys provided.");
            }
            return requiredCount;
        }
        internal static Signatory[] Signatories(Signatory[] signatories)
        {
            if (signatories is null)
            {
                throw new ArgumentNullException(nameof(signatories), "The list of signatories may not be null.");
            }
            else if (signatories.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(signatories), "At least one Signatory in a list is required.");
            }
            for (int i = 0; i < signatories.Length; i++)
            {
                if (signatories[i] is null)
                {
                    throw new ArgumentNullException(nameof(signatories), "No signatory within the list may be null.");
                }
            }
            return signatories;
        }
        internal static Func<IInvoice, Task> SigningCallback(Func<IInvoice,Task> signingCallback)
        {
            if (signingCallback is null)
            {
                throw new ArgumentNullException(nameof(signingCallback), "The signing callback must not be null.");
            }
            return signingCallback;
        }
        internal static Proto.Key KeysFromCreateParameters(CreateAccountParams createParameters)
        {
            if (createParameters is null)
            {
                throw new ArgumentNullException(nameof(createParameters), "The create parameters are missing. Please check that the argument is not null.");
            }
            if (createParameters.PublicKey is null)
            {
                if (createParameters.Endorsement is null)
                {
                    throw new ArgumentOutOfRangeException(nameof(createParameters), "Both 'PublicKey' and 'Endorsement' are null, one must be specified.");
                }
                return Protobuf.ToPublicKey(createParameters.Endorsement);
            }
            else if (createParameters.Endorsement is null)
            {
                if (createParameters.PublicKey.Value.IsEmpty)
                {
                    throw new ArgumentOutOfRangeException(nameof(createParameters.PublicKey), "The 'PublicKey' must not be empty if specified.");
                }
                try
                {
                    return Protobuf.ToPublicKey(new Endorsement(Endorsement.Type.Ed25519, createParameters.PublicKey.Value));
                }
                catch (Exception ex)
                {
                    throw new ArgumentOutOfRangeException(nameof(createParameters.PublicKey), ex.Message);
                }
            }
            throw new ArgumentOutOfRangeException(nameof(createParameters), "Both 'PublicKey' and 'Endorsement' are specified, only one without the other may be specified.");
        }
        internal static CreateFileParams CreateParameters(CreateFileParams createParameters)
        {
            if (createParameters is null)
            {
                throw new ArgumentNullException(nameof(createParameters), "The create parameters are missing. Please check that the argument is not null.");
            }
            if (createParameters.Endorsements is null)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.Endorsements), "Endorsements are required.");
            }
            return createParameters;
        }
        internal static CreateContractParams CreateParameters(CreateContractParams createParameters)
        {
            if (createParameters is null)
            {
                throw new ArgumentNullException(nameof(createParameters), "The create parameters are missing. Please check that the argument is not null.");
            }
            if (createParameters.File is null)
            {
                throw new ArgumentNullException(nameof(createParameters.File), "The File Address containing the contract is missing, it cannot be null.");
            }
            return createParameters;
        }
        internal static CallContractParams CallContractParameters(CallContractParams callParameters)
        {
            if (callParameters is null)
            {
                throw new ArgumentNullException(nameof(callParameters), "The call parameters are missing. Please check that the argument is not null.");
            }
            return callParameters;
        }
        internal static QueryContractParams QueryParameters(QueryContractParams queryParameters)
        {
            if (queryParameters is null)
            {
                throw new ArgumentNullException(nameof(queryParameters), "The query parameters are missing. Please check that the argument is not null.");
            }
            return queryParameters;
        }
        internal static UpdateFileParams UpdateParameters(UpdateFileParams updateParameters)
        {
            if (updateParameters is null)
            {
                throw new ArgumentNullException(nameof(updateParameters), "File Update Parameters argument is missing. Please check that it is not null.");
            }
            if (updateParameters.File is null)
            {
                throw new ArgumentNullException(nameof(updateParameters.File), "File identifier is missing. Please check that it is not null.");
            }
            if (updateParameters.Endorsements is null &&
                updateParameters.Contents is null)
            {
                throw new ArgumentException(nameof(updateParameters), "The File Update parameters contain no update properties, it is blank.");
            }
            return updateParameters;
        }
        internal static AppendFileParams AppendParameters(AppendFileParams appendParameters)
        {
            if (appendParameters is null)
            {
                throw new ArgumentNullException(nameof(appendParameters), "File Update Parameters argument is missing. Please check that it is not null.");
            }
            if (appendParameters.File is null)
            {
                throw new ArgumentNullException(nameof(appendParameters.File), "File identifier is missing. Please check that it is not null.");
            }
            return appendParameters;
        }
        internal static Claim AddParameters(Claim addParameters)
        {
            if (addParameters is null)
            {
                throw new ArgumentNullException(nameof(addParameters), "Add Claim Parameters argument is missing. Please check that it is not null.");
            }
            if (addParameters.Address is null)
            {
                throw new ArgumentNullException(nameof(addParameters.Address), "The address to attach the claim to is missing. Please check that it is not null.");
            }
            if (addParameters.Hash.IsEmpty)
            {
                throw new ArgumentNullException(nameof(addParameters.Hash), "The claim hash is missing. Please check that it is not null.");
            }
            if (addParameters.Hash.Length != 48)
            {
                throw new ArgumentOutOfRangeException(nameof(addParameters.Hash), "The claim hash is expected to be 48 bytes in length.");
            }
            if (addParameters.Endorsements is null)
            {
                throw new ArgumentNullException(nameof(addParameters.Endorsements), "The endorsements property is missing. Please check that it is not null.");
            }
            if (addParameters.Endorsements.Length == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(addParameters.Endorsements), "The endorsements array is empty. Please must include at least one endorsement.");
            }
            if (addParameters.ClaimDuration.Ticks == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(addParameters.ClaimDuration), "Claim Duration must have some length.");
            }
            return addParameters;
        }
    }
}
