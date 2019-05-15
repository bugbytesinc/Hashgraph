using Google.Protobuf;
using Hashgraph.Implementation;
using NSec.Cryptography;
using System;
using System.Linq;

namespace Hashgraph
{
    /// <summary>
    /// Represents the structure and requirements for signing
    /// a transaction that is attached to an account, file or
    /// contract.
    /// </summary>
    /// <remarks>
    /// Presently only implemented one level deep, in other words
    /// only represents n-of-m key requirements, this will change
    /// in the future.
    /// </remarks>
    public sealed class Endorsements : IEquatable<Endorsements>
    {
        /// <summary>
        /// Number of keys required for a valid signature.
        /// </summary>
        public int RequiredCount { get; }
        /// <summary>
        /// Total number of potential signing Keys for a valid signature.
        /// </summary>
        public int KeyCount => _keys.Length;
        /// <summary>
        /// A list of public keys that may sign the transaction.
        /// </summary>
        internal readonly PublicKey[] _keys;
        /// <summary>
        /// Public constructor, requires at least one valid public key.
        /// </summary>
        /// <param name="publicKeys">
        /// The public key representing the private keys that must sign 
        /// for a transaction to be considered valid.  This form of the 
        /// constructor assumes that all the keys listed are required.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">If missing input or the key values are unrecognizable.</exception>
        public Endorsements(params ReadOnlyMemory<byte>[] publicKeys) : this(publicKeys.Length, publicKeys) { }
        /// <summary>
        /// Public constructor establishing a n of m key signing requirement.
        /// </summary>
        /// <param name="requiredCount">
        /// The number of private keys of this set of keys that must sign the 
        /// transaction for it to be considered valid, must be at least one and 
        /// cannot exceed the number of keys provided in the list.
        /// </param>
        /// <param name="publicKeys">
        /// A list of public keys corresponding to private keys that may sign 
        /// this transaction for it to be considered valid.  The number of keys 
        /// must be equal to or greater than the <code>requiredCount</code> value.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">If missing input or the key values are unrecognizable.</exception>
        public Endorsements(int requiredCount, params ReadOnlyMemory<byte>[] publicKeys)
        {
            _keys = RequireInputParameter.PublicKeys(publicKeys);
            RequiredCount = RequireInputParameter.RequiredCount(requiredCount, publicKeys.Length);
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>Endorsements</code> object to compare.
        /// </param>
        /// <returns>
        /// True if public key layout and requirements are equivalent to the 
        /// other <code>Endorsements</code> object.
        /// </returns>
        public bool Equals(Endorsements other)
        {
            if (other is null)
            {
                return false;
            }
            if (RequiredCount != other.RequiredCount ||
                _keys.Length != other._keys.Length)
            {
                return false;
            }
            for (int i = 0; i < _keys.Length; i++)
            {
                if (!_keys[i].Equals(other._keys[i]))
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
        /// The other <code>Endorsements</code> object to compare (if it is
        /// an <code>Endorsements</code>).
        /// </param>
        /// <returns>
        /// If the other object is an Endorsements, then <code>True</code> 
        /// if key requirements are identical to the other 
        /// <code>Endorsements</code> object, otherwise 
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
            if (obj is Endorsements other)
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
            return $"Endorsements:{RequiredCount}:{string.Join(':', _keys.Select(k => k.GetHashCode().ToString()))}".GetHashCode();
        }
        /// <summary>
        /// Equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>Endorsements</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Endorsements</code> argument.
        /// </param>
        /// <returns>
        /// True if Key requirements are identical 
        /// within each <code>Endorsements</code> objects.
        /// </returns>
        public static bool operator ==(Endorsements left, Endorsements right)
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
        /// Left hand <code>Endorsements</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Endorsements</code> argument.
        /// </param>
        /// <returns>
        /// <code>False</code> if the Key requirements are identical 
        /// within each <code>Endorsements</code> object.  
        /// <code>True</code> if they are not identical.
        /// </returns>
        public static bool operator !=(Endorsements left, Endorsements right)
        {
            return !(left == right);
        }
    }
}