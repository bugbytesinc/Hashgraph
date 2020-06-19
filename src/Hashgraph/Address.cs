using Hashgraph.Implementation;
using System;

namespace Hashgraph
{
    /// <summary>
    /// Represents a Hedera Network Account Address.
    /// </summary>
    public sealed class Address : IEquatable<Address>
    {
        /// <summary>
        /// Network Shard Number for Account
        /// </summary>
        public long ShardNum { get; private set; }
        /// <summary>
        /// Network Realm Number for Account
        /// </summary>
        public long RealmNum { get; private set; }
        /// <summary>
        /// Network Account Number for Account
        /// </summary>
        public long AccountNum { get; private set; }
        /// <summary>
        /// A special designation of an account that can't be created.
        /// It represents the absence of a valid account.  The network will
        /// intrepret as "no account" when applied to change parameters.
        /// (typically the value null is intepreted as "make no change").  
        /// In this way, it is possible to remove a auto-renew account
        /// from a topic.
        /// </summary>
        public static Address None { get; private set; } = new Address();
        /// <summary>
        /// Internal Constructor representing the "None" version of an
        /// address.  This is a special construct that is used by the network
        /// to represent "removing" auto-renew address from a topic.
        /// </summary>
        private Address()
        {
            ShardNum = 0;
            RealmNum = 0;
            AccountNum = 0;
        }
        /// <summary>
        /// Public Constructor, an <code>Address</code> is immutable after creation.
        /// </summary>
        /// <param name="shardNum">
        /// Network Shard Number
        /// </param>
        /// <param name="realmNum">
        /// Network Realm Number
        /// </param>
        /// <param name="accountNum">
        /// Network Account Number
        /// </param>
        public Address(long shardNum, long realmNum, long accountNum)
        {
            ShardNum = RequireInputParameter.ShardNumber(shardNum);
            RealmNum = RequireInputParameter.RealmNumber(realmNum);
            AccountNum = RequireInputParameter.AcountNumber(accountNum);
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>Address</code> object to compare.
        /// </param>
        /// <returns>
        /// True if the  Shard, Realm and Account Number are identical to the 
        /// other <code>Address</code> object.
        /// </returns>
        public bool Equals(Address? other)
        {
            if (other is null)
            {
                return false;
            }
            return
                ShardNum == other.ShardNum &&
                RealmNum == other.RealmNum &&
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
        /// if the Shard, Realm and Account Number are identical 
        /// to the other <code>Address</code> object, otherwise 
        /// <code>False</code>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is Address other)
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
        /// True if the Shard, Realm and Account Number are identical 
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
        /// <code>False</code> if the Shard, Realm, Account Number and 
        /// Key are identical within each <code>Address</code> object.  
        /// <code>True</code> if they are not identical.
        /// </returns>
        public static bool operator !=(Address left, Address right)
        {
            return !(left == right);
        }
    }
}
