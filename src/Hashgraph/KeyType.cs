namespace Hashgraph
{
    /// <summary>
    /// Key types supported by the network, this library
    /// natively supports Ed25519, for signing with other
    /// keys, the <see cref="Signatory" /> should use 
    /// the callback form.
    /// </summary>
    public enum KeyType
    {
        /// <summary>
        /// Ed25519 Key Pair
        /// </summary>
        Ed25519 = 1,
        /// <summary>
        /// RSA-3072 Key Pair
        /// </summary>
        RSA3072 = 2,
        /// <summary>
        /// ECDSA with the p-384 curve.
        /// </summary>
        ECDSA384 = 3,
        /// <summary>
        /// Smart Contract ID.
        /// </summary>
        ContractID = 4
    }
}
