using Google.Protobuf;
using Hashgraph;
using System;

namespace Proto;

public sealed partial class AccountID
{
    internal AccountID(AddressOrAlias account) : this()
    {
        if (account is null)
        {
            throw new ArgumentNullException(nameof(account), "Account Address/Alias is missing. Please check that it is not null.");
        }
        ShardNum = account.ShardNum;
        RealmNum = account.RealmNum;
        if (account.Endorsement is null)
        {
            AccountNum = account.AccountNum;
        }
        else
        {
            Alias = new Key(account.Endorsement).ToByteString();
        }
    }
}

internal static class AccountIDExtensions
{
    internal static Address AsAddress(this AccountID? accountId)
    {
        if (accountId is not null)
        {
            return new Address(accountId.ShardNum, accountId.RealmNum, accountId.AccountNum);
        }
        return Address.None;
    }
}