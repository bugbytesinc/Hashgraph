using System;

namespace Hashgraph
{
    public class Address : IEquatable<Address>
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

        public bool Equals(Address other)
        {
            if (other is null)
            {
                return false;
            }
            return
                RealmNum == other.RealmNum &&
                ShardNum == other.ShardNum &&
                AccountNum == other.AccountNum;
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is Account other)
            {
                return Equals(other);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return $"Address:{RealmNum}:{ShardNum}:{AccountNum}".GetHashCode();
        }
        public static bool operator ==(Address left, Address right)
        {
            if(left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(Address left, Address right)
        {
            return !(left == right);
        }
    }
}
