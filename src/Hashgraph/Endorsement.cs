﻿using Hashgraph.Implementation;
using NSec.Cryptography;
using System;
using System.Linq;

namespace Hashgraph
{
    /// <summary>
    /// Represents the key signing requirements for various
    /// transactions available within the network.
    /// </summary>
    public sealed class Endorsement : IEquatable<Endorsement>
    {
        /// <summary>
        /// The type of endorsment this object is.  It either contains
        /// a representation of a public key or a list of child endorsements
        /// with a not of how many are requrired to be fullfilled for this
        /// endorsment to be fulfilled.
        /// </summary>
        public enum Type
        {
            /// <summary>
            /// Ed25519 Public Key (Stored as a <see cref="NSec.Cryptography.PublicKey"/>).
            /// </summary>
            Ed25519 = 1,
            /// <summary>
            /// RSA-3072 Public Key (Stored as a <see cref="ReadOnlyMemory{Byte}"/>).
            /// </summary>
            RSA3072 = 2,
            /// <summary>
            /// ECDSA with the p-384 curve (Stored as a <see cref="ReadOnlyMemory{Byte}"/>).
            /// </summary>
            ECDSA384 = 3,
            /// <summary>
            /// Smart Contract ID (Stored as a <see cref="ReadOnlyMemory{Byte}"/>).
            /// </summary>
            ContractID = 4,
            /// <summary>
            /// This endorsement represents a n-to-m required list of child endorsements.
            /// </summary>
            List = 5
        }
        /// <summary>
        /// Convenience constructor creating an Ed25519 public key
        /// endorsement.  This is the most common key format for the network.
        /// </summary>
        /// <param name="publicKey">
        /// Bytes representing a public Ed25519 key.
        /// </param>
        public Endorsement(ReadOnlyMemory<byte> publicKey) : this(Type.Ed25519, publicKey) { }
        /// <summary>
        /// When this endorsment contains a list of child endorsements, 
        /// this represents the number of child endorsements that must
        /// be fulfilled in order to consider this endorsment fulfilled.
        /// </summary>
        internal readonly uint _requiredCount;
        /// <summary>
        /// The type of endorsment this object is.  It either contains
        /// a representation of a public key or a list of child endorsements
        /// with a not of how many are requrired to be fullfilled for this
        /// endorsment to be fulfilled.
        /// </summary>
        internal readonly Type _type;
        /// <summary>
        /// Holds the data for this endorsement, may be a Key or 
        /// a list of child endorsements.
        /// </summary>
        internal readonly object _data;
        /// <summary>
        /// Create a M of M requied list of endorsements.  All listed endorsements
        /// must be fulfilled to fulfill this endorsement.
        /// </summary>
        /// <param name="endorsements">
        /// A list of endorsements that must be fullfilled, may be a mix of individual
        /// public keys or additional sub-lists of individual keys.
        /// </param>
        /// <exception cref="ArgumentNullException">if endorsements is null</exception>
        public Endorsement(params Endorsement[] endorsements) : this((uint)endorsements.Length, endorsements) { }
        /// <summary>
        /// Create a N of M required list of endorsements.  Only
        /// <code>requiredCount</code> number of listed endorsements must
        /// be fulfilled to fulfill this endorsement.
        /// </summary>
        /// <param name="requiredCount">
        /// The number of child endorsements that must be fulfilled
        /// in order to fulfill this endorsement.
        /// </param>
        /// <param name="endorsements">
        /// A list of candidate endorsements, may be a mix of individual
        /// public keys or additional sub-lists of individual keys.
        /// </param>
        /// <exception cref="ArgumentNullException">if endorsements is null</exception>
        /// <exception cref="ArgumentOutOfRangeException">if the required amount is negative
        /// greater than tne number of endorsements</exception>
        public Endorsement(uint requiredCount, params Endorsement[] endorsements)
        {
            _type = Type.List;
            _data = RequireInputParameter.Endorsements(endorsements);
            _requiredCount = RequireInputParameter.RequiredCount(requiredCount, endorsements.Length);
        }
        /// <summary>
        /// Creates an endorsement representing a single key of a
        /// valid type.  Will accept Ed25519, RSA3072, ECDSA381
        /// and Smart Contract ID.  Presently the only key 
        /// implemented by the network Ed25519 and validity of 
        /// this key is checked upon creation of this object.        
        /// </summary>
        /// <param name="type">
        /// The type of key the bytes represent.
        /// </param>
        /// <param name="publicKey">
        /// The bytes for the public key.
        /// </param>        
        /// <exception cref="ArgumentOutOfRangeException">
        /// If type passed into the constructor was not a valid single key type. 
        /// Or, for an Ed25519 key that is not recognizable from supplied bytes.
        /// </exception>
        public Endorsement(Type type, ReadOnlyMemory<byte> publicKey)
        {
            _type = type;
            switch (type)
            {
                case Type.Ed25519:
                    _data = Keys.ImportPublicEd25519KeyFromBytes(publicKey);
                    break;
                case Type.RSA3072:
                case Type.ECDSA384:
                case Type.ContractID:
                    _data = publicKey;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Not a valid endorsement type.");
            }
        }
        /// <summary>
        /// Implicit constructor converting an Ed25519 public key
        /// represented in bytes into an <code>Endorsement</code>
        /// representing an Ed25519 key.  This convenience operator
        /// exists becuase Ed25519 keys are the most prominent in 
        /// the system at the moment.
        /// </summary>
        /// <param name="publicKey">
        /// Bytes representing a public Ed25519 key.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If <code>publicKey</code> is not recognizable as an Ed25519 public key.
        /// </exception>
        public static implicit operator Endorsement(ReadOnlyMemory<byte> publicKey)
        {
            return new Endorsement(Type.Ed25519, publicKey);
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>Endorsement</code> object to compare.
        /// </param>
        /// <returns>
        /// True if public key layout and requirements are equivalent to the 
        /// other <code>Endorsement</code> object.
        /// </returns>
        public bool Equals(Endorsement other)
        {
            if (other is null)
            {
                return false;
            }
            if (_type != other._type)
            {
                return false;
            }
            switch (_type)
            {
                case Type.Ed25519:
                case Type.RSA3072:
                case Type.ECDSA384:
                case Type.ContractID:
                    return Equals(_data, other._data);
                case Type.List:
                    if (_requiredCount == other._requiredCount)
                    {
                        var thisList = (Endorsement[])_data;
                        var otherList = (Endorsement[])other._data;
                        if (thisList.Length == otherList.Length)
                        {
                            for (int i = 0; i < thisList.Length; i++)
                            {
                                if (!thisList[i].Equals(otherList[i]))
                                {
                                    return false;
                                }
                            }
                            return true;
                        }
                    }
                    break;
            }
            return false;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="obj">
        /// The other <code>Endorsement</code> object to compare (if it is
        /// an <code>Endorsement</code>).
        /// </param>
        /// <returns>
        /// If the other object is an Endorsement, then <code>True</code> 
        /// if key requirements are identical to the other 
        /// <code>Endorsements</code> object, otherwise 
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
            if (obj is Endorsement other)
            {
                return Equals(other);
            }
            return false;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <returns>
        /// A unique hash of the contents of this <code>Endorsement</code> 
        /// object.  Only consistent within the current instance of 
        /// the application process.
        /// </returns>
        public override int GetHashCode()
        {
            switch (_type)
            {
                case Type.Ed25519:
                    return $"Endorsement:{_type}:{((PublicKey)_data).GetHashCode().ToString()}".GetHashCode();
                case Type.RSA3072:
                case Type.ECDSA384:
                case Type.ContractID:
                    return $"Endorsement:{_type}:{BitConverter.ToString(((ReadOnlyMemory<byte>)_data).ToArray())}".GetHashCode();
                case Type.List:
                    return $"Endorsement:{_type}:{string.Join(':', ((Endorsement[])_data).Select(e => e.GetHashCode().ToString()))}".GetHashCode();
            }
            return "Endorsment:Empty".GetHashCode();
        }
        /// <summary>
        /// Equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>Endorsement</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Endorsement</code> argument.
        /// </param>
        /// <returns>
        /// True if Key requirements are identical 
        /// within each <code>Endorsement</code> objects.
        /// </returns>
        public static bool operator ==(Endorsement left, Endorsement right)
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
        /// Left hand <code>Endorsement</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Endorsement</code> argument.
        /// </param>
        /// <returns>
        /// <code>False</code> if the Key requirements are identical 
        /// within each <code>Endorsement</code> object.  
        /// <code>True</code> if they are not identical.
        /// </returns>
        public static bool operator !=(Endorsement left, Endorsement right)
        {
            return !(left == right);
        }
    }
}