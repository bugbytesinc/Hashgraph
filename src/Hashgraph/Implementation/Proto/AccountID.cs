using Google.Protobuf;
using Hashgraph;
using System;

namespace Proto;

public sealed partial class AccountID
{
    internal AccountID(Address account) : this()
    {
        if (account is null)
        {
            throw new ArgumentNullException(nameof(account), "Account Address/Alias is missing. Please check that it is not null.");
        }
        if (account.TryGetAlias(out var alias))
        {
            ShardNum = alias.ShardNum;
            RealmNum = alias.RealmNum;
            Alias = new Key(alias.Endorsement).ToByteString();
        }
        else if(account.AddressType == AddressType.ShardRealmNum)
        {
            ShardNum = account.ShardNum;
            RealmNum = account.RealmNum;
            AccountNum = account.AccountNum;
        } 
        else
        {
            throw new ArgumentOutOfRangeException(nameof(account), "Crypto Account Address does not appear to be a valid <shard>.<realm>.<num> or alias.");
        }
    }
}

internal static class AccountIDExtensions
{
    internal static Address AsAddress(this AccountID? accountId)
    {
        if (accountId is null)
        {
            return Address.None;
        }
        if (accountId.AccountCase == AccountID.AccountOneofCase.Alias)
        {
            var publicKey = Key.Parser.ParseFrom(accountId.Alias.Memory.Span);            
            return new Address(new Alias(accountId.ShardNum, accountId.RealmNum, publicKey.ToEndorsement()));
        }
        return new Address(accountId.ShardNum, accountId.RealmNum, accountId.AccountNum);
    }
}