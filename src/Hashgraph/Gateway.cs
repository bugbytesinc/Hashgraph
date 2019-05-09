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
    public sealed class Gateway : Address, IEquatable<Gateway>
    {
        /// <summary>
        /// The URL and port of the public Hedera Network access point.
        /// </summary>
        public string Url { get; private set; }
        /// <summary>
        /// Public Constructor, a <code>Gateway</code> is immutable after creation.
        /// </summary>
        /// <param name="url">
        /// The URL and port of the public Hedera Network access point.
        /// </param>
        /// <param name="realmNum">
        /// Main Network Node Realm Number
        /// </param>
        /// <param name="shardNum">
        /// Main Network Node Shard Number
        /// </param>
        /// <param name="accountNum">
        /// Main Network Node Account Number
        /// </param>
        public Gateway(string url, long realmNum, long shardNum, long accountNum) :
            base(realmNum, shardNum, accountNum)
        {
            Url = url;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>Gateway</code> object to compare.
        /// </param>
        /// <returns>
        /// True if the Realm, Shard, Account Number and Url are identical to the 
        /// other <code>Gateway</code> object.
        /// </returns>
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
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="obj">
        /// The other <code>Gateway</code> object to compare (if it is
        /// an <code>Gateway</code>).
        /// </param>
        /// <returns>
        /// If the other object is an Account, then <code>True</code> 
        /// if the Realm, Shard, Account Number and Url are identical 
        /// to the other <code>Gateway</code> object, otherwise 
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
            return $"Gateway:{RealmNum}:{ShardNum}:{AccountNum}:{Url}".GetHashCode();
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
        /// True if the Realm, Shard, Account Number and Url are identical 
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
        /// <code>False</code> if the Realm, Shard, Account Number and 
        /// Url are identical within each <code>Account</code> object.  
        /// <code>True</code> if they are not identical.
        /// </returns>
        public static bool operator !=(Gateway left, Gateway right)
        {
            return !(left == right);
        }
    }
}