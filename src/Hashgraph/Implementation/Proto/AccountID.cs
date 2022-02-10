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
        else
        {
            ShardNum = account.ShardNum;
            RealmNum = account.RealmNum;
            AccountNum = account.AccountNum;
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
            return new Address(new Alias(accountId.ShardNum, accountId.RealmNum, accountId.Alias.Memory));
        }
        return new Address(accountId.ShardNum, accountId.RealmNum, accountId.AccountNum);
    }
}