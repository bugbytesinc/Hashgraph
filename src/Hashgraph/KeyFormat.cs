namespace Hashgraph;
/// <summary>
/// Identifies the desired public key output
/// format.
/// </summary>
public enum KeyFormat
{
    /// <summary>
    /// The default format deemed to be the most
    /// common for the specific public key type.
    /// </summary>
    Default = 0,
    /// <summary>
    /// The raw public key value, without any der
    /// encoding.  Only applies to single type keys.
    /// </summary>
    Raw = 1,
    /// <summary>
    /// The public ANS.1 DER encoded value. Only applies
    /// to single key types.
    /// </summary>
    Der = 2,
    /// <summary>
    /// Encoded as the native Hedera HAPI protobuf value
    /// (this is the most specific and fully supported across
    /// all key types)
    /// </summary>
    Protobuf = 3,
    /// <summary>
    /// Legacy Hedera DER'ish encoding that is compatible
    /// with other Hedera Supported SDKs (for single keys)
    /// or the Protobuf encoded value for complex keys.
    /// </summary>
    Hedera = 4,
    /// <summary>
    /// Format compatible for looking up an account via
    /// the public key using a rest Mirror node.  It is the
    /// raw key for Ed25519 and ECDSA keys, and the serialized
    /// protobuf representation for complex keys.
    /// </summary>
    Mirror = 5
}
