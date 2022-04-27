using Hashgraph;
using System;

namespace Proto;

public sealed partial class FileID
{
    internal FileID(Address file) : this()
    {
        if (file is null)
        {
            throw new ArgumentNullException(nameof(file), "File is missing. Please check that it is not null.");
        }
        else if (file.AddressType == AddressType.ShardRealmNum)
        {
            ShardNum = file.ShardNum;
            RealmNum = file.RealmNum;
            FileNum = file.AccountNum;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(file), "File Address does not appear to be in a valid <shard>.<realm>.<num> form.");
        }
    }
}

internal static class FileIDExtensions
{
    internal static Address AsAddress(this FileID? id)
    {
        if (id is not null)
        {
            return new Address(id.ShardNum, id.RealmNum, id.FileNum);
        }
        return Address.None;
    }
}