using Hashgraph.Implementation;
using NSec.Cryptography;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph
{
    /// <summary>
    /// Represents a keyholder or group of keyholders that
    /// can sign a transaction for crypto transfer to support
    /// file creation, contract creation and execution or pay
    /// for consensus services among other network tasks.
    /// </summary>
    /// <remarks>
    /// A <code>Signatory</code> is presently created with a pre-existing
    /// Ed25519 private key or a callback action having the
    /// information necessary to sucessfully sign the transaction
    /// as described by its matching <see cref="Endorsement" />
    /// requrements.  RSA-3072, ECDSA and <code>Contract</code> signatures
    /// are not natively supported thru the <code>Signatory</code> at this 
    /// time but can be achieved thru the callback functionality.
    /// </remarks>
    public sealed class Signatory : ISignatory, IEquatable<Signatory>
    {
        /// <summary>
        /// Private helper type tracking the type of signing
        /// information held by this instance.
        /// </summary>
        private enum Type
        {
            /// <summary>
            /// Ed25519 Public Key (Stored as a <see cref="NSec.Cryptography.PublicKey"/>).
            /// </summary>
            Ed25519 = 1,
            /// <summary>
            /// RSA-3072 Public Key (Stored as a <see cref="ReadOnlyMemory{Byte}"/>).
            /// </summary>
            /// <remarks>
            /// Presently directly not supported, trying to create this type should 
            /// result in thrown exception.
            /// </remarks>
            RSA3072 = 2,
            /// <summary>
            /// ECDSA with the p-384 curve (Stored as a <see cref="ReadOnlyMemory{Byte}"/>).
            /// </summary>
            /// <remarks>
            /// Presently directly not supported, trying to create this type should 
            /// result in a thrown exception.
            /// </remarks>
            ECDSA384 = 3,
            /// <summary>
            /// A <code>Func<IInvoice, Task> signingCallback</code> callback function 
            /// having the knowledge to properly sign the binary representation of the 
            /// transaction as serialized using the grpc protocol.
            /// </summary>
            Callback = 4,
            /// <summary>
            /// This signatory holds a list of a number of other signatories that can
            /// in turn sign transactions.  This supports the sceneario where multiple
            /// keys must sign a transaction.
            /// </summary>
            List = 5,
            /// <summary>
            /// This represnts legacy signing features in the library that are slated
            /// for removal over time, such as the Ed25519 key(s) embedded in the
            /// <see cref="Account"/> object.  At some point in the future 
            /// <code>Account</code> will be replaced with <see cref="Address"/> 
            /// and <code>Signatory</code> will be the sole means for communicating 
            /// how to sign transactions.
            /// </summary>
            OtherSigner = 999
        }
        /// <summary>
        /// Internal type of this Signatory.
        /// </summary>
        private readonly Type _type;
        /// <summary>
        /// Internal union of the types of data this Signatory may hold.
        /// The contents are a function of the <code>Type</code>.  It can be a 
        /// list of other signatories, a reference to a callback method, or an 
        /// Ed25519 private key.
        /// </summary>
        private readonly object _data;
        /// <summary>
        /// Create a signatory with a private Ed25519 key.  When transactions
        /// are signed, this signatory will automatically sign the transaction
        /// with this private key.
        /// </summary>
        /// <param name="privateKey">
        /// Bytes representing an Ed25519 private key signing transactions.  
        /// It is expected to be 48 bytes in length, prefixed with 
        /// <code>0x302e020100300506032b6570</code>.
        /// </param>
        public Signatory(ReadOnlyMemory<byte> privateKey) : this(KeyType.Ed25519, privateKey) { }
        /// <summary>
        /// Create a signatory that is a combination of a number of other
        /// signatories.  When this signatory is called to sign a transaction
        /// it will in turn ask all the child signatories in turn to sign the 
        /// given transaction.
        /// </summary>
        /// <param name="Signatories">
        /// One or more signatories that when combined can form a
        /// multi key signature for the transaction.
        /// </param>
        public Signatory(params Signatory[] Signatories)
        {
            _type = Type.List;
            _data = RequireInputParameter.Signatories(Signatories);
        }
        /// <summary>
        /// Create a signatory having a private key of the specified type.
        /// </summary>
        /// <param name="type">
        /// The type of private key this <code>Signatory</code> should use to 
        /// sign transactions.
        /// </param>
        /// <param name="privateKey">
        /// The bytes of a private key corresponding to the specified type.
        /// </param>
        /// <remarks>
        /// At this time, the library only supports Ed25519 keys, any other
        /// key type will result in an exception.  This is why this method
        /// is marked as <code>internal</code> at the moment.  When the library 
        /// can support other key types, it will make sense to make this public.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">
        /// If any key type other than Ed25519 is used.
        /// </exception>
        internal Signatory(KeyType type, ReadOnlyMemory<byte> privateKey)
        {
            switch (type)
            {
                case KeyType.Ed25519:
                    _type = Type.Ed25519;
                    _data = Keys.ImportPrivateEd25519KeyFromBytes(privateKey);
                    break;
                case KeyType.List:
                    throw new ArgumentOutOfRangeException(nameof(type), "Only signatories representing a single key are supported with this constructor, please use the list constructor instead.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Not a presently supported Signatory key type, please consider the callback signatory as an alternative.");
            }
        }
        /// <summary>
        /// Create a Signatory invoking the given async callback function
        /// when asked to sign a transaction.  The <code>Signatory</code> 
        /// will pass an instance of an <see cref="IInvoice"/> containing 
        /// details of the transaction to sign when needed.  The callback 
        /// function may add as many signatures as necessary to properly 
        /// sign the  transaction.
        /// </summary>
        /// <param name="signingCallback">
        /// An async callback method that is invoked when the library 
        /// asks this Signatory to sign a transaction.
        /// </param>
        /// <remarks>
        /// Note:  For a single transaction this method MAY BE CALLED TWICE
        /// in the event the library is being asked to retrieve a record as
        /// a part of the request.  This is because retrieving a record of 
        /// a transaction requires a separate payment.  So, if this Signatory
        /// is directly attached to the root <see cref="IContext"/> it will
        /// be used to sign the request to retrieve the record (since this 
        /// will typically represent the <see cref="IContext.Payer"/>'s 
        /// signature for the transaction).
        /// </remarks>
        public Signatory(Func<IInvoice, Task> signingCallback)
        {
            _type = Type.Callback;
            _data = RequireInputParameter.SigningCallback(signingCallback);
        }
        /// <summary>
        /// Convenience implict cast for creating a <code>Signatory</code> 
        /// directly from an Ed25519 private key.
        /// </summary>
        /// <param name="privateKey">
        /// Bytes representing an Ed25519 private key signing transactions.  
        /// It is expected to be 48 bytes in length, prefixed with 
        /// <code>0x302e020100300506032b6570</code>.
        /// </param>
        public static implicit operator Signatory(ReadOnlyMemory<byte> privateKey)
        {
            return new Signatory(privateKey);
        }
        /// <summary>
        /// Convenience implicit cast for creating a <code>Signatory</code> 
        /// directly from a <code>Func<IInvoice, Task> signingCallback</code> 
        /// callback
        /// method.
        /// </summary>
        /// <param name="signingCallback">
        /// An async callback method that is invoked when the library 
        /// asks this Signatory to sign a transaction.
        /// </param>
        public static implicit operator Signatory(Func<IInvoice, Task> signingCallback)
        {
            return new Signatory(signingCallback);
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>Signatory</code> object to compare.
        /// </param>
        /// <returns>
        /// True if public key layout and requirements are equivalent to the 
        /// other <code>Signatory</code> object.
        /// </returns>
        public bool Equals(Signatory? other)
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
                    return Equals(((Key)_data).PublicKey, ((Key)other._data).PublicKey);
                case Type.RSA3072:  // Will need more work
                case Type.ECDSA384: // Will need more work
                    return Equals(_data, other._data);
                case Type.List:
                    var thisList = (Signatory[])_data;
                    var otherList = (Signatory[])other._data;
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
                    break;
                case Type.Callback:
                    return ReferenceEquals(_data, other._data);
            }
            return false;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="obj">
        /// The other <code>Signatory</code> object to compare (if it is
        /// an <code>Signatory</code>).
        /// </param>
        /// <returns>
        /// If the other object is an Signatory, then <code>True</code> 
        /// if key requirements are identical to the other 
        /// <code>Signatories</code> object, otherwise 
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
            if (obj is Signatory other)
            {
                return Equals(other);
            }
            return false;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <returns>
        /// A unique hash of the contents of this <code>Signatory</code> 
        /// object.  Only consistent within the current instance of 
        /// the application process.
        /// </returns>
        public override int GetHashCode()
        {
            switch (_type)
            {
                case Type.Ed25519:
                    return $"Signatory:{_type}:{((PublicKey)_data).GetHashCode()}".GetHashCode();
                case Type.RSA3072:  // Will need more work
                case Type.ECDSA384: // Will need more work
                    return $"Signatory:{_type}:{_data}".GetHashCode();
                case Type.Callback:
                    return $"Signatory:{_type}:{_data.GetHashCode()}".GetHashCode();
                case Type.List:
                    return $"Signatory:{_type}:{string.Join(':', ((Signatory[])_data).Select(e => e.GetHashCode().ToString()))}".GetHashCode();
            }
            return "Endorsment:Empty".GetHashCode();
        }
        /// <summary>
        /// Equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>Signatory</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Signatory</code> argument.
        /// </param>
        /// <returns>
        /// True if Key requirements are identical 
        /// within each <code>Signatory</code> objects.
        /// </returns>
        public static bool operator ==(Signatory left, Signatory right)
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
        /// Left hand <code>Signatory</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Signatory</code> argument.
        /// </param>
        /// <returns>
        /// <code>False</code> if the Key requirements are identical 
        /// within each <code>Signatory</code> object.  
        /// <code>True</code> if they are not identical.
        /// </returns>
        public static bool operator !=(Signatory left, Signatory right)
        {
            return !(left == right);
        }
        /// <summary>
        /// Legacy support for components that also have
        /// the ability to sign transactions.  Used only internally
        /// by the library.
        /// </summary>
        /// <param name="signer">
        /// The legacy signer instance (such as a <see cref="Account"/>).
        /// </param>
        internal Signatory(ISignatory signer)
        {
            _type = Type.OtherSigner;
            _data = signer;
        }
        /// <summary>
        /// Implement the signing algorithm.  In the case of an Ed25519
        /// it will use the private key to sign the transaction and 
        /// return immediately.  In the case of the callback method, it 
        /// will pass the invoice to the async method and async await
        /// for the method to return.
        /// </summary>
        /// <param name="invoice">
        /// The information for the transaction, including the Transaction 
        /// ID, Memo and serialized bytes of the crypto transfers and other
        /// embedded information making up the transaction.
        /// </param>
        /// <returns></returns>
        async Task ISignatory.SignAsync(IInvoice invoice)
        {
            switch (_type)
            {
                case Type.Ed25519:
                    var ed25519Key = (Key)_data;
                    invoice.AddSignature(
                        KeyType.Ed25519,
                        ed25519Key.PublicKey.Export(KeyBlobFormat.PkixPublicKey).TakeLast(32).Take(6).ToArray(),
                        SignatureAlgorithm.Ed25519.Sign(ed25519Key, invoice.TxBytes.Span));
                    break;
                case Type.List:
                    foreach (ISignatory signer in (Signatory[])_data)
                    {
                        await signer.SignAsync(invoice);
                    }
                    break;
                case Type.Callback:
                    await ((Func<IInvoice, Task>)_data)(invoice);
                    break;
                case Type.OtherSigner:
                    await ((ISignatory)_data).SignAsync(invoice);
                    break;
                default:
                    throw new InvalidOperationException("Not a presently supported Signatory key type, please consider the callback signatory as an alternative.");
            }
        }
    }
}