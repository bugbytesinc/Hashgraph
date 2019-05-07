using System;

namespace Hashgraph
{
    public sealed class Gateway : Address, IEquatable<Gateway>
    {
        public string Url { get; private set; }
        public Gateway(string url, long realmNum, long shardNum, long accountNum) :
            base(realmNum, shardNum, accountNum)
        {
            Url = url;
        }
        public bool Equals(Gateway other)
        {
            if (other is null)
            {
                return false;
            }
            return
                RealmNum == other.RealmNum &&
                ShardNum == other.ShardNum &&
                AccountNum == other.AccountNum &&
                string.Equals(Url, other.Url);
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
            if (obj is Gateway other)
            {
                return Equals(other);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return $"Gateway:{RealmNum}:{ShardNum}:{AccountNum}:{Url}".GetHashCode();
        }
        public static bool operator ==(Gateway left, Gateway right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(Gateway left, Gateway right)
        {
            return !(left == right);
        }
    }
}