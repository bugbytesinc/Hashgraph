using Hashgraph;
using System;

namespace Proto;

public sealed partial class TokenID
{
    internal TokenID(Address token) : this()
    {
        if (token.IsNullOrNone())
        {
            throw new ArgumentNullException(nameof(token), "Token is missing. Please check that it is not null or empty.");
        }
        else if (token.AddressType == AddressType.ShardRealmNum)
        {
            ShardNum = token.ShardNum;
            RealmNum = token.RealmNum;
            TokenNum = token.AccountNum;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(token), "Token Address does not appear to be in a valid <shard>.<realm>.<num> form.");
        }
    }
}
internal static class TokenIDExtensions
{
    internal static Address AsAddress(this TokenID? id)
    {
        if (id is not null)
        {
            return new Address(id.ShardNum, id.RealmNum, id.TokenNum);
        }
        return Address.None;
    }
}