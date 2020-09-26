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
        internal static Address Topic(Address topic)
        {
            if (topic is null)
            {
                throw new ArgumentNullException(nameof(topic), "Topic Address is missing. Please check that it is not null.");
            }
            return topic;
        }
        internal static ReadOnlyMemory<byte> Message(ReadOnlyMemory<byte> message)
        {
            if (message.IsEmpty)
            {
                throw new ArgumentOutOfRangeException(nameof(message), "Topic Message can not be empty.");
            }
            return message;
        }
        internal static Address AddressToDelete(Address addressToDelete)
        {
            if (addressToDelete is null)
            {
                throw new ArgumentNullException(nameof(addressToDelete), "Address to Delete is missing. Please check that it is not null.");
            }
            return addressToDelete;
        }
        internal static TokenIdentifier TokenIdentifier(TokenIdentifier token)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token), "Token is missing. Please check that it is not null.");
            }
            if ((token.Address is null || token.Address == Hashgraph.Address.None) && string.IsNullOrWhiteSpace(token.Symbol))
            {
                throw new ArgumentNullException(nameof(token), "Token Address and Symbol are missing. At least one is required.");
            }
            return token;
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
        internal static Address FileToRestore(Address fileToRestore)
        {
            if (fileToRestore is null)
            {
                throw new ArgumentNullException(nameof(fileToRestore), "File to Restore is missing. Please check that it is not null.");
            }
            return fileToRestore;
        }
        internal static Address ContractToDelete(Address contractToDelete)
        {
            if (contractToDelete is null)
            {
                throw new ArgumentNullException(nameof(contractToDelete), "Contract to Delete is missing. Please check that it is not null.");
            }
            return contractToDelete;
        }
        internal static Address ContractToRestore(Address contractToRestore)
        {
            if (contractToRestore is null)
            {
                throw new ArgumentNullException(nameof(contractToRestore), "Contract to Restore is missing. Please check that it is not null.");
            }
            return contractToRestore;
        }
        internal static Address TransferToAddress(Address transferToAddress)
        {
            if (transferToAddress is null)
            {
                throw new ArgumentNullException(nameof(transferToAddress), "Transfer address is missing. Please check that it is not null.");
            }
            return transferToAddress;
        }
        internal static Address FromAddress(Address fromAddress)
        {
            if (fromAddress is null)
            {
                throw new ArgumentNullException(nameof(fromAddress), "Address to transfer from is missing. Please check that it is not null.");
            }
            return fromAddress;
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
        internal static ulong ConfiscateAmount(ulong amount)
        {
            if (amount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "The amount to confiscate must be greater than zero.");
            }
            return amount;
        }
        internal static (Address address, long amount)[] CryptoTransferList(IEnumerable<KeyValuePair<Address, long>> transfers)
        {
            long sum = 0;
            if (transfers is null)
            {
                throw new ArgumentNullException(nameof(transfers), "The dictionary of transfers can not be null.");
            }
            var result = new List<(Address address, long amount)>();
            foreach (var transfer in transfers)
            {
                if (transfer.Value == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(transfers), $"The amount to transfer to/from {transfer.Key.ShardNum}.{transfer.Key.RealmNum}.{transfer.Key.AccountNum} must be a value, negative for transfers out, and positive for transfers in. A value of zero is not allowed.");
                }
                result.Add((transfer.Key, transfer.Value));
                sum = sum + transfer.Value;
            }
            if (result.Count == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transfers), "The dictionary of transfers can not be empty.");
            }
            if (sum != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(transfers), "The sum of sends and receives does not balance.");
            }
            return result.ToArray();
        }
        internal static TxId Transaction(TxId transaction)
        {
            if (transaction is null)
            {
                throw new ArgumentNullException(nameof(transaction), "Transaction is missing. Please check that it is not null.");
            }
            return transaction;
        }
        internal static UpdateAccountParams UpdateParameters(UpdateAccountParams updateParameters)
        {
            if (updateParameters is null)
            {
                throw new ArgumentNullException(nameof(updateParameters), "Account Update Parameters argument is missing. Please check that it is not null.");
            }
            if (updateParameters.Address is null)
            {
                throw new ArgumentNullException(nameof(updateParameters.Address), "Account is missing. Please check that it is not null.");
            }
            if (Hashgraph.Endorsement.None.Equals(updateParameters.Endorsement))
            {
                throw new ArgumentOutOfRangeException(nameof(updateParameters.Endorsement), "Endorsement can not be 'None', it must contain at least one key requirement.");
            }
            if (updateParameters.Endorsement is null &&
                updateParameters.SendThresholdCreateRecord is null &&
                updateParameters.ReceiveThresholdCreateRecord is null &&
                updateParameters.RequireReceiveSignature is null &&
                updateParameters.Expiration is null &&
                updateParameters.AutoRenewPeriod is null &&
                updateParameters.Proxy is null)
            {
                throw new ArgumentException(nameof(updateParameters), "The Account Updates contains no update properties, it is blank.");
            }
            return updateParameters;
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

        internal static UpdateTopicParams UpdateParameters(UpdateTopicParams updateParameters)
        {
            if (updateParameters is null)
            {
                throw new ArgumentNullException(nameof(updateParameters), "Topic Update Parameters argument is missing. Please check that it is not null.");
            }
            if (updateParameters.Topic is null)
            {
                throw new ArgumentNullException(nameof(updateParameters.Topic), "Topic address is missing. Please check that it is not null.");
            }
            if (updateParameters.Memo is null &&
                updateParameters.Administrator is null &&
                updateParameters.Participant is null &&
                updateParameters.RenewPeriod is null &&
                updateParameters.RenewAccount is null)
            {
                throw new ArgumentException("The Topic Updates contain no update properties, it is blank.", nameof(updateParameters));
            }
            return updateParameters;
        }
        internal static UpdateTokenParams UpdateParameters(UpdateTokenParams updateParameters)
        {
            if (updateParameters is null)
            {
                throw new ArgumentNullException(nameof(updateParameters), "Token Update Parameters argument is missing. Please check that it is not null.");
            }
            if (updateParameters.Token is null || ((updateParameters.Token.Address is null || updateParameters.Token.Address == Hashgraph.Address.None) && string.IsNullOrWhiteSpace(updateParameters.Token.Symbol)))
            {
                throw new ArgumentNullException(nameof(updateParameters.Token), "The Token Identifer (Address or Symbol) is missing, at least Address or Symbol is required.");
            }
            if (updateParameters.Treasury is null &&
                updateParameters.Administrator is null &&
                updateParameters.GrantKycEndorsement is null &&
                updateParameters.SuspendEndorsement is null &&
                updateParameters.ConfiscateEndorsement is null &&
                updateParameters.SupplyEndorsement is null &&
                string.IsNullOrWhiteSpace(updateParameters.Symbol) &&
                string.IsNullOrWhiteSpace(updateParameters.Name) &&
                !updateParameters.Expiration.HasValue &&
                !updateParameters.RenewPeriod.HasValue &&
                updateParameters.RenewAccount is null)
            {
                throw new ArgumentException("The Topic Updates contain no update properties, it is blank.", nameof(updateParameters));
            }
            if (!string.IsNullOrWhiteSpace(updateParameters.Symbol))
            {
                if (updateParameters.Symbol.Trim().Length != updateParameters.Symbol.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(updateParameters.Symbol), "The new token symbol cannot contain leading or trailing white space.");
                }
                if (updateParameters.Symbol.Length > 32)
                {
                    throw new ArgumentOutOfRangeException(nameof(updateParameters.Symbol), "The new token symbol cannot exceed 32 characters in length.");
                }
                if (!updateParameters.Symbol.Equals(updateParameters.Symbol.ToUpperInvariant()))
                {
                    throw new ArgumentOutOfRangeException(nameof(updateParameters.Symbol), "The new token symbol must contain upper case characters.");
                }
            }
            if (!string.IsNullOrWhiteSpace(updateParameters.Name))
            {
                if (updateParameters.Name.Trim().Length != updateParameters.Name.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(updateParameters.Name), "The new token name cannot contain leading or trailing white space.");
                }
            }
            if (updateParameters.Expiration.HasValue)
            {
                if (updateParameters.Expiration.Value < DateTime.UtcNow)
                {
                    throw new ArgumentOutOfRangeException(nameof(updateParameters.Expiration), "The new expiration can not be set to the past.");
                }
            }
            if (updateParameters.RenewPeriod.HasValue)
            {
                if (updateParameters.RenewPeriod.Value.TotalSeconds < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(updateParameters.RenewPeriod), "The renew period must be non negative.");
                }
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
        internal static int MajorNumber(int major)
        {
            if (major < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(major), "Major Version Number cannot be negative.");
            }
            return major;
        }
        internal static int MinorNumber(int minor)
        {
            if (minor < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minor), "Minor Version Number cannot be negative.");
            }
            return minor;
        }
        internal static int PatchNumber(int patch)
        {
            if (patch < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(patch), "Patch Version Number cannot be negative.");
            }
            return patch;
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
        internal static Func<IInvoice, Task> SigningCallback(Func<IInvoice, Task> signingCallback)
        {
            if (signingCallback is null)
            {
                throw new ArgumentNullException(nameof(signingCallback), "The signing callback must not be null.");
            }
            return signingCallback;
        }
        internal static Proto.Key KeysFromEndorsements(CreateAccountParams createParameters)
        {
            if (createParameters is null)
            {
                throw new ArgumentNullException(nameof(createParameters), "The create parameters are missing. Please check that the argument is not null.");
            }
            if (createParameters.Endorsement is null)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters), "The Endorsement for the account is missing, it is required.");
            }
            return new Proto.Key(createParameters.Endorsement);
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
        internal static CreateTopicParams CreateParameters(CreateTopicParams createParameters)
        {
            if (createParameters is null)
            {
                throw new ArgumentNullException(nameof(createParameters), "The create parameters are missing. Please check that the argument is not null.");
            }
            if (createParameters.Memo is null)
            {
                throw new ArgumentNullException(nameof(createParameters.Memo), "Memo can not be null.");
            }
            if (!(createParameters.RenewAccount is null) && createParameters.Administrator is null)
            {
                throw new ArgumentNullException(nameof(createParameters.Administrator), "The Administrator endorssement must not be null if RenewAccount is specified.");
            }
            if (createParameters.RenewPeriod.Ticks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.RenewPeriod), "The renew period must be greater than zero, and typically less than or equal to 90 days.");
            }
            return createParameters;
        }
        internal static CreateTokenParams CreateParameters(CreateTokenParams createParameters)
        {
            if (createParameters is null)
            {
                throw new ArgumentNullException(nameof(createParameters), "The create parameters are missing. Please check that the argument is not null.");
            }
            if (string.IsNullOrWhiteSpace(createParameters.Name))
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.Name), "The name cannot be null or empty.");
            }
            if (string.IsNullOrWhiteSpace(createParameters.Symbol))
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.Symbol), "The token symbol must be specified.");
            }
            if (createParameters.Symbol.Trim().Length != createParameters.Symbol.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.Symbol), "The token symbol cannot contain leading or trailing white space.");
            }
            if (createParameters.Symbol.Length > 32)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.Symbol), "The token symbol cannot exceed 32 characters in length.");
            }
            if (!createParameters.Symbol.Equals(createParameters.Symbol.ToUpperInvariant()))
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.Symbol), "The token symbol must contain upper case characters.");
            }
            if (createParameters.Circulation < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.Circulation), "The initial circulation of tokens must be greater than zero.");
            }
            if (createParameters.Decimals < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.Decimals), "The divisibility of tokens cannot be negative.");
            }
            if (createParameters.Treasury is null || createParameters.Treasury == Hashgraph.Address.None)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.Treasury), "The treasury must be specified.");
            }
            if (createParameters.Expiration < DateTime.UtcNow)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.Treasury), "The expiration time must be in the future.");
            }
            if (createParameters.RenewAccount.IsNullOrNone() ^ createParameters.RenewPeriod.Ticks <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.RenewPeriod), "Both the renew account and period must be specified, or not at all.");
            }
            return createParameters;
        }
        internal static SubscribeTopicParams SubscribeParameters(SubscribeTopicParams subscribeParameters)
        {
            if (subscribeParameters is null)
            {
                throw new ArgumentNullException(nameof(subscribeParameters), "Topic Subscribe Parameters argument is missing. Please check that it is not null.");
            }
            if (subscribeParameters.Topic is null)
            {
                throw new ArgumentNullException(nameof(subscribeParameters.Topic), "Topic address is missing. Please check that it is not null.");
            }
            if (subscribeParameters.MessageWriter is null)
            {
                throw new ArgumentNullException(nameof(subscribeParameters.MessageWriter), "The destination channel writer missing. Please check that it is not null.");
            }
            if (subscribeParameters.Starting.HasValue && subscribeParameters.Ending.HasValue)
            {
                if (subscribeParameters.Ending.Value < subscribeParameters.Starting.Value)
                {
                    throw new ArgumentOutOfRangeException(nameof(subscribeParameters.Ending), "The ending filter date is less than the starting filter date, no records can be returned.");
                }
            }
            return subscribeParameters;
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
        internal static ulong TokenAmount(ulong amount)
        {
            if (amount == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "The token amount must be greater than zero.");
            }
            return amount;
        }
        internal static (TimeSpan starting, TimeSpan length) StartingAndEndingTimes(TimeSpan starting, TimeSpan length)
        {
            if (length.Ticks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The length of time must be greater than zero.");
            }
            if (length.TotalHours > 24)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The length of time must not exceed 24 hours.");
            }
            if (starting.TotalHours > 24)
            {
                throw new ArgumentOutOfRangeException(nameof(length), "The starting wait must not exceed 24 hours.");
            }
            var now = DateTime.UtcNow;
            var then = now.Add(starting).Add(length);
            if (then < now)
            {
                throw new ArgumentOutOfRangeException(nameof(starting), "The combination of starting wait and suspension lenght has already passed.");
            }
            return (starting, length);
        }
        internal static Proto.TransactionID IdFromTransactionBytes(ReadOnlyMemory<byte> transaction)
        {
            Proto.Transaction decodedTransaction;
            Proto.TransactionBody decodedTransactionBody;
            if (transaction.IsEmpty)
            {
                throw new ArgumentOutOfRangeException(nameof(transaction), "Missing Transaction Bytes (was empty.)");
            }
            try
            {
                decodedTransaction = Proto.Transaction.Parser.ParseFrom(transaction.ToArray());
            }
            catch (Exception pe)
            {
                throw new ArgumentException(nameof(transaction), "The submitted bytes do not appear to belong to a transaction.", pe);
            }
            try
            {
                decodedTransactionBody = Proto.TransactionBody.Parser.ParseFrom(decodedTransaction.BodyBytes);
            }
            catch (Exception pe)
            {
                throw new ArgumentException(nameof(transaction), "The submitted bytes do not appear to have a transaction body.", pe);
            }
            if (decodedTransactionBody is null)
            {
                throw new ArgumentException(nameof(transaction), "The submitted bytes do not appear to have a transaction body.");
            }
            if (decodedTransactionBody.TransactionID is null)
            {
                throw new ArgumentException(nameof(transaction), "The submitted transaction bytes do not appear to contain a transaction id.");
            }
            return decodedTransactionBody.TransactionID;
        }
    }
}
