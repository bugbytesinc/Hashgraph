using System;

namespace Hashgraph
{
    public class Address
    {
        public long RealmNum { get; private set; }
        public long ShardNum { get; private set; }
        public long AccountNum { get; private set; }

        public Address(long realmNum, long shardNum, long accountNum)
        {
            if (realmNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(realmNum), "Realm Number cannot be negative.");
            }
            if (shardNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(shardNum), "Shard Number cannot be negative.");
            }
            if (accountNum < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(accountNum), "Account Number cannot be negative.");
            }
            RealmNum = realmNum;
            ShardNum = shardNum;
            AccountNum = accountNum;
        }
    }
}
