using Google.Protobuf;
using Hashgraph.Implementation;
using NSec.Cryptography;
using System;
using System.Linq;

namespace Hashgraph
{
    /// <summary>
    /// Represents a Hedera Network Address with an associated 
    /// private keys capable of signing transactions.
    /// </summary>
    public sealed class Account : ISigner, IDisposable, IEquatable<Account>
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
        /// Private Key Implementation
        /// </summary>
        private readonly Key[] _keys;
        /// <summary>
        /// Public Constructor, an <code>Account</code> is immutable after creation.
        /// </summary>
        /// <param name="address">
        /// Network Address associated with this account.
        /// </param>
        /// <param name="privateKey">
        /// Bytes representing an Ed25519 private key associated with this account for 
        /// signing transactions.  It is expected to be 48 bytes in length, prefixed 
        /// with <code>0x302e020100300506032b6570</code>.
        /// </param>
        public Account(Address address, params ReadOnlyMemory<byte>[] privateKeys) : this(address.RealmNum, address.ShardNum, address.AccountNum, privateKeys) { }
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
        public Account(long realmNum, long shardNum, long accountNum, params ReadOnlyMemory<byte>[] privateKeys)
        {
            RealmNum = RequireInputParameter.RealmNumber(realmNum);
            ShardNum = RequireInputParameter.ShardNumber(shardNum);
            AccountNum = RequireInputParameter.AcountNumber(accountNum);
            _keys = RequireInputParameter.PrivateKeys(privateKeys);
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
        /// <returns>A list of Ed25519 Signatures, one for each private key held by this object.</returns>
        Proto.SignaturePair[] ISigner.Sign(ReadOnlyMemory<byte> data)
        {
            return _keys.Select(k => new Proto.SignaturePair
            {
                PubKeyPrefix = ByteString.CopyFrom(k.PublicKey.Export(KeyBlobFormat.PkixPublicKey).TakeLast(32).Take(6).ToArray()),
                Ed25519 = ByteString.CopyFrom(SignatureAlgorithm.Ed25519.Sign(k, data.Span))
            }).ToArray();
        }
        /// <summary>
        /// .NET Dispose implementation, releases internal resources holding 
        /// private key information.
        /// </summary>
        public void Dispose()
        {
            for (int i = 0; i < _keys.Length; i++)
            {
                _keys[i].Dispose();
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
            if (RealmNum != other.RealmNum ||
                ShardNum != other.ShardNum ||
                AccountNum != other.AccountNum ||
                _keys.Length != other._keys.Length)
            {
                return false;
            }
            for (int i = 0; i < _keys.Length; i++)
            {
                if (!_keys[i].PublicKey.Equals(other._keys[i].PublicKey))
                {
                    return false;
                }
            }
            return true;
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
            return $"Account:{RealmNum}:{ShardNum}:{AccountNum}:{string.Join(':', _keys.Select(k => k.PublicKey.GetHashCode().ToString()))}".GetHashCode();
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
        /// <summary>
        /// Implicit operator for converting Account to an Address
        /// </summary>
        /// <param name="account">
        /// The Account containing the realm, shard and account number
        /// address information to convert into an address object.
        /// </param>
        public static implicit operator Address(Account account)
        {
            return new Address(account.RealmNum, account.ShardNum, account.AccountNum);
        }
    }
}