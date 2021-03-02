using Hashgraph;

namespace Proto
{
    public sealed partial class ScheduleID
    {
        internal ScheduleID(Address address) : this()
        {
            ShardNum = address.ShardNum;
            RealmNum = address.RealmNum;
            ScheduleNum = address.AccountNum;
        }
        internal Address ToAddress()
        {
            return new Address(ShardNum, RealmNum, ScheduleNum);
        }
    }
}
