using Hashgraph;
using System;

namespace Proto;

public sealed partial class ScheduleID
{
    internal ScheduleID(Address pending) : this()
    {
        if (pending is null)
        {
            throw new ArgumentNullException(nameof(pending), "Pending Transaction Schedule Address is missing. Please check that it is not null.");
        }
        if (Address.None.Equals(pending))
        {
            throw new ArgumentOutOfRangeException(nameof(pending), "Pending Transaction Schedule Addresss can not be empty or None.  Please provide a valid value.");
        }
        else if (pending.AddressType == AddressType.ShardRealmNum)
        {
            ShardNum = pending.ShardNum;
            RealmNum = pending.RealmNum;
            ScheduleNum = pending.AccountNum;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(pending), "Pending Address does not appear to be in a valid <shard>.<realm>.<num> form.");
        }
    }
    internal Address ToAddress()
    {
        return new Address(ShardNum, RealmNum, ScheduleNum);
    }
}