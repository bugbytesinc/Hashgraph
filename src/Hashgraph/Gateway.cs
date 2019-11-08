using Hashgraph.Implementation;
using System;

namespace Hashgraph
{
    /// <summary>
    /// Class representing the Network Address and Node 
    /// Account address for gaining access to the Hedera Network.
    /// </summary>
    /// <remarks>
    /// This class consists of both an <see cref="Address"/> representing 
    /// the main node within the network and the host name and port of the 
    /// network address where the public network endpoint is located.
    /// This class is immutable once created.
    /// </remarks>
    public sealed class Gateway : IEquatable<Gateway>
    {
        /// <summary>
        /// The URL and port of the public Hedera Network access point.
        /// </summary>
        public string Url { get; private set; }
        /// <summary>
        /// Network Shard Number for Gateway Account
        /// </summary>
        public long ShardNum { get; private set; }
        /// <summary>
        /// Network Realm Number for Gateway Account
        /// </summary>
        public long RealmNum { get; private set; }
        /// <summary>
        /// Network Account Number for Gateway Account
        /// </summary>
        public long AccountNum { get; private set; }
        /// <summary>
        /// Public Constructor, a <code>Gateway</code> is immutable after creation.
        /// </summary>
        /// <param name="url">
        /// The URL and port of the public Hedera Network access point.
        /// </param>
        /// <param name="address">
        /// Main Network Node Address
        /// </param>
        public Gateway(string url, Address address) : this(url, address.ShardNum, address.RealmNum, address.AccountNum) { }
        /// <summary>
        /// Public Constructor, a <code>Gateway</code> is immutable after creation.
        /// </summary>
        /// <param name="url">
        /// The URL and port of the public Hedera Network access point.
        /// </param>
        /// <param name="shardNum">
        /// Main Network Node Shard Number
        /// </param>
        /// <param name="realmNum">
        /// Main Network Node Realm Number
        /// </param>
        /// <param name="accountNum">
        /// Main Network Node Account Number
        /// </param>
        public Gateway(string url, long shardNum, long realmNum, long accountNum)
        {
            Url = RequireInputParameter.Url(url);
            ShardNum = RequireInputParameter.ShardNumber(shardNum);
            RealmNum = RequireInputParameter.RealmNumber(realmNum);
            AccountNum = RequireInputParameter.AcountNumber(accountNum);
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>Gateway</code> object to compare.
        /// </param>
        /// <returns>
        /// True if the Shard, Realm, Account Number and Url are identical to the 
        /// other <code>Gateway</code> object.
        /// </returns>
        public bool Equals(Gateway other)
        {
            if (other is null)
            {
                return false;
            }
            return
                ShardNum == other.ShardNum &&
                RealmNum == other.RealmNum &&
                AccountNum == other.AccountNum &&
                string.Equals(Url, other.Url);
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="obj">
        /// The other <code>Gateway</code> object to compare (if it is
        /// an <code>Gateway</code>).
        /// </param>
        /// <returns>
        /// If the other object is an Account, then <code>True</code> 
        /// if the Shard, Realm, Account Number and Url are identical 
        /// to the other <code>Gateway</code> object, otherwise 
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
            if (obj is Gateway other)
            {
                return Equals(other);
            }
            return false;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <returns>
        /// A unique hash of the contents of this <code>Gateway</code> 
        /// object.  Only consistent within the current instance of 
        /// the application process.
        /// </returns>
        public override int GetHashCode()
        {
            return $"Gateway:{ShardNum}:{RealmNum}:{AccountNum}:{Url}".GetHashCode();
        }
        /// <summary>
        /// Equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>Gateway</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Gateway</code> argument.
        /// </param>
        /// <returns>
        /// True if the Shard, Realm, Account Number and Url are identical 
        /// within each <code>Gateway</code> objects.
        /// </returns>
        public static bool operator ==(Gateway left, Gateway right)
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
        /// Left hand <code>Gateway</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Gateway</code> argument.
        /// </param>
        /// <returns>
        /// <code>False</code> if the Shard, Realm, Account Number and 
        /// Url are identical within each <code>Account</code> object.  
        /// <code>True</code> if they are not identical.
        /// </returns>
        public static bool operator !=(Gateway left, Gateway right)
        {
            return !(left == right);
        }
        /// <summary>
        /// Implicit operator for converting a Gateway to an Address
        /// </summary>
        /// <param name="gateway">
        /// The Gateway object containing the realm, shard and account 
        /// number address information to convert into an address object.
        /// </param>
        public static implicit operator Address(Gateway gateway)
        {
            return new Address(gateway.ShardNum, gateway.RealmNum, gateway.AccountNum);
        }
    }
}