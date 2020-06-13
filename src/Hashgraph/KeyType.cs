namespace Hashgraph
{
    /// <summary>
    /// Identifies the type of <see cref="Signatory"/> or
    /// <see cref="Endorsement"/>.  Typically these objects
    /// a represent a public key or a list of child keys.
    /// For endorsments a list type can represent an n of m
    /// number of child keys that must sign to validat a
    /// transaction.  For signatories, the list 
    /// type represents simply a collection of keys, all of
    /// which sill sign a given transaction.
    /// Presently this library natively supports individual
    /// key types of Ed25519, for signing with other
    /// keys, the <see cref="Signatory" /> should use 
    /// the callback form.
    /// </summary>
    public enum KeyType
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
        /// Correlates to a Smart Contract Instance, sometimes produced by the 
        /// network identifying smart contracts that are immutable.
        /// </summary>
        Contract = 4,
        /// <summary>
        /// A list of keys, for endorsements it may represent an n-to-m list, 
        /// for signatores its simply a bag of keys that will sign the transaction.
        /// </summary>
        List = 5
    }
}