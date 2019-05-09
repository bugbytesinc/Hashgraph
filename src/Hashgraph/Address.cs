using System;

namespace Hashgraph
{
    /// <summary>
    /// Represents a Hedera Network Account Address.
    /// </summary>
    public class Address : IEquatable<Address>
    {
        /// <summary>
        /// Network Realm Number for Account
        /// </summary>
        public long RealmNum { get; private set; }
        /// <summary>
        /// Network Shard Number for Account
        /// </summary>
        public long ShardNum { get; private set; }
        /// <summary>
        /// Network Account Number for Account
        /// </summary>
        public long AccountNum { get; private set; }
        /// <summary>
        /// Public Constructor, an <code>Address</code> is immutable after creation.
        /// </summary>
        /// <param name="realmNum">
        /// Network Realm Number
        /// </param>
        /// <param name="shardNum">
        /// Network Shard Number
        /// </param>
        /// <param name="accountNum">
        /// Network Account Number
        /// </param>
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
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>Address</code> object to compare.
        /// </param>
        /// <returns>
        /// True if the Realm, Shard and Account Number are identical to the 
        /// other <code>Address</code> object.
        /// </returns>
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
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="obj">
        /// The other <code>Address</code> object to compare (if it is
        /// an <code>Address</code>).
        /// </param>
        /// <returns>
        /// If the other object is an Address, then <code>True</code> 
        /// if the Realm, Shard and Account Number are identical 
        /// to the other <code>Address</code> object, otherwise 
        /// <code>False</code>.
        /// </returns>
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
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <returns>
        /// A unique hash of the contents of this <code>Address</code> 
        /// object.  Only consistent within the current instance of 
        /// the application process.
        /// </returns>
        public override int GetHashCode()
        {
            return $"Address:{RealmNum}:{ShardNum}:{AccountNum}".GetHashCode();
        }
        /// <summary>
        /// Equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>Address</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Address</code> argument.
        /// </param>
        /// <returns>
        /// True if the Realm, Shard and Account Number are identical 
        /// within each <code>Address</code> objects.
        /// </returns>
        public static bool operator ==(Address left, Address right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        /// <summary>
        /// Not equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>Address</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Address</code> argument.
        /// </param>
        /// <returns>
        /// <code>False</code> if the Realm, Shard, Account Number and 
        /// Key are identical within each <code>Address</code> object.  
        /// <code>True</code> if they are not identical.
        /// </returns>
        public static bool operator !=(Address left, Address right)
        {
            return !(left == right);
        }
    }
}
