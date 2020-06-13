using Hashgraph.Implementation;
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
        /// Holds the data for this endorsement, may be a Key or 
        /// a list of child endorsements.
        /// </summary>
        internal readonly object _data;
        /// <summary>
        /// Returns a list of child endorsements identified by
        /// this endorsment (of list type).  If the endorsement
        /// is not of a list type, the list will be empty.
        /// </summary>
        public Endorsement[] List
        {
            get
            {
                switch (Type)
                {
                    case KeyType.List: return (Endorsement[])((Endorsement[])_data).Clone();
                    default: return new Endorsement[0];
                }
            }
        }
        /// <summary>
        /// When this endorsment contains a list of child endorsements, 
        /// this represents the number of child endorsements that must
        /// be fulfilled in order to consider this endorsment fulfilled.
        /// </summary>
        public uint RequiredCount { get; internal set; }
        /// <summary>
        /// The type of endorsment this object is.  It either contains
        /// a representation of a public key or a list of child endorsements
        /// with a not of how many are requrired to be fullfilled for this
        /// endorsment to be fulfilled.
        /// </summary>
        public KeyType Type { get; internal set; }
        /// <summary>
        /// The value of the public key held by this endorsement if it is
        /// of a key type.  If it is a list type, the value returned will
        /// be <code>Empty</code>.
        /// </summary>
        public ReadOnlyMemory<byte> PublicKey
        {
            get
            {
                switch (Type)
                {
                    case KeyType.Ed25519:
                        return ((PublicKey)_data).Export(KeyBlobFormat.PkixPublicKey);
                    case KeyType.RSA3072:
                    case KeyType.ECDSA384:
                    case KeyType.Contract:
                        return (ReadOnlyMemory<byte>)_data;
                    default:
                        return new byte[0];
                }
            }
        }
        /// <summary>
        /// A special designation of an endorsement key that can't be created.
        /// It represents an "empty" list of keys, which the network will 
        /// intrepret as "clear all keys" from this setting (typically the value
        /// null is intepreted as "make no change").  In this way, it is possible
        /// to change a topic from mutable (which has an Administrator endorsment)
        /// to imutable (having no Administrator endorsment).
        /// </summary>
        public static Endorsement None { get; private set; } = new Endorsement();
        /// <summary>
        /// Internal Constructor representing the "Empty List" version of an
        /// endorsment.  This is a special construct that is used by the network
        /// to represent "removing" keys from an "Administrator" key list.  For
        /// example, turning a mutable contract into an imutable contract.  One
        /// should never create an empty key list so this is why the constructor
        /// for this type is set to private and exposed on the Imutable property.
        /// </summary>
        private Endorsement()
        {
            Type = KeyType.List;
            _data = new Endorsement[0];
            RequiredCount = 0;
        }
        /// <summary>
        /// Convenience constructor creating an Ed25519 public key
        /// endorsement.  This is the most common key format for the network.
        /// </summary>
        /// <param name="publicKey">
        /// Bytes representing a public Ed25519 key.
        /// </param>
        public Endorsement(ReadOnlyMemory<byte> publicKey) : this(KeyType.Ed25519, publicKey) { }
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
            Type = KeyType.List;
            _data = RequireInputParameter.Endorsements(endorsements);
            RequiredCount = RequireInputParameter.RequiredCount(requiredCount, endorsements.Length);
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
        public Endorsement(KeyType type, ReadOnlyMemory<byte> publicKey)
        {
            Type = type;
            switch (type)
            {
                case KeyType.Ed25519:
                    _data = Keys.ImportPublicEd25519KeyFromBytes(publicKey);
                    break;
                case KeyType.RSA3072:
                case KeyType.ECDSA384:
                case KeyType.Contract:
                    _data = publicKey;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Only endorsements representing a single key are supported with this constructor, please use the list constructor instead.");
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
            return new Endorsement(KeyType.Ed25519, publicKey);
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
            if (Type != other.Type)
            {
                return false;
            }
            switch (Type)
            {
                case KeyType.Ed25519:
                case KeyType.RSA3072:
                case KeyType.ECDSA384:
                case KeyType.Contract:
                    return Equals(_data, other._data);
                case KeyType.List:
                    if (RequiredCount == other.RequiredCount)
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
            switch (Type)
            {
                case KeyType.Ed25519:
                    return $"Endorsement:{Type}:{((PublicKey)_data).GetHashCode().ToString()}".GetHashCode();
                case KeyType.RSA3072:
                case KeyType.ECDSA384:
                case KeyType.Contract:
                    return $"Endorsement:{Type}:{BitConverter.ToString(((ReadOnlyMemory<byte>)_data).ToArray())}".GetHashCode();
                case KeyType.List:
                    return $"Endorsement:{Type}:{string.Join(':', ((Endorsement[])_data).Select(e => e.GetHashCode().ToString()))}".GetHashCode();
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