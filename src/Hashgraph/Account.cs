using Hashgraph.Implementation;
using NSec.Cryptography;
using System;

namespace Hashgraph
{
    /// <summary>
    /// Represents a Hedera Network Address with an associated 
    /// private key capable of signing transactions.
    /// </summary>
    public sealed class Account : Address, ISigner, IDisposable, IEquatable<Account>
    {
        /// <summary>
        /// Private Key Implementation
        /// </summary>
        private Key _key;
        /// <summary>
        /// Public Constructor, an <code>Account</code> is immutable after creation.
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
        /// <param name="privateKey">
        /// Bytes representing an Ed25519 private key associated with this account for 
        /// signing transactions.  It is expected to be 48 bytes in length, prefixed 
        /// with <code>0x302e020100300506032b6570</code>.
        /// </param>
        public Account(long realmNum, long shardNum, long accountNum, ReadOnlyMemory<byte> privateKey) :
            base(realmNum, shardNum, accountNum)
        {
            try
            {
                _key = Keys.ImportPrivateEd25519KeyFromBytes(privateKey);
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException(nameof(privateKey), ex.Message);
            }
        }
        /// <summary>
        /// Signs the given bytes with the private key.  Typically, this is a transaction.  
        /// This method is used internally by the library at the point when it needs this 
        /// account to sign a transaction.  It is not intended to be used by client code.
        /// </summary>
        /// <param name="data">
        /// The bytes to sign, typically this is a transaction message body serialized in 
        /// the protobuf format.
        /// </param>
        /// <returns>The Ed25519 Signature</returns>
        byte[] ISigner.Sign(ReadOnlyMemory<byte> data)
        {
            return SignatureAlgorithm.Ed25519.Sign(_key, data.Span);
        }
        /// <summary>
        /// .NET Dispose implementation, releases internal resources holding 
        /// private key information.
        /// </summary>
        public void Dispose()
        {
            if (_key != null)
            {
                _key.Dispose();
            }
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>Account</code> object to compare.
        /// </param>
        /// <returns>
        /// True if the Realm, Shard, Account Number and Key are identical to the 
        /// other <code>Account</code> object.
        /// </returns>
        public bool Equals(Account other)
        {
            if (other is null)
            {
                return false;
            }
            return
                RealmNum == other.RealmNum &&
                ShardNum == other.ShardNum &&
                AccountNum == other.AccountNum &&
                _key.PublicKey.Equals(other._key.PublicKey);
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="obj">
        /// The other <code>Account</code> object to compare (if it is
        /// an <code>Account</code>).
        /// </param>
        /// <returns>
        /// If the other object is an Account, then <code>True</code> 
        /// if the Realm, Shard, Account Number and Key are identical 
        /// to the other <code>Account</code> object, otherwise 
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
        /// A unique hash of the contents of this <code>Account</code> 
        /// object.  Only consistent within the current instance of 
        /// the application process.
        /// </returns>
        public override int GetHashCode()
        {
            return $"Account:{RealmNum}:{ShardNum}:{AccountNum}:{_key.PublicKey.GetHashCode()}".GetHashCode();
        }
        /// <summary>
        /// Equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>Account</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Account</code> argument.
        /// </param>
        /// <returns>
        /// True if the Realm, Shard, Account Number and Key are identical 
        /// within each <code>Account</code> objects.
        /// </returns>
        public static bool operator ==(Account left, Account right)
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
        /// Left hand <code>Account</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Account</code> argument.
        /// </param>
        /// <returns>
        /// <code>False</code> if the Realm, Shard, Account Number and 
        /// Key are identical within each <code>Account</code> object.  
        /// <code>True</code> if they are not identical.
        /// </returns>
        public static bool operator !=(Account left, Account right)
        {
            return !(left == right);
        }
    }
}