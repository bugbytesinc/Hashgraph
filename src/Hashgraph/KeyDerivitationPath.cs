using System;

namespace Hashgraph;
/// <summary>
/// Enumerates known HD Key Derivitation paths for
/// various Key formats and Wallets in the Hedera eccosystem.
/// </summary>
public class KeyDerivitationPath
{
    /// <summary>
    /// The Ed25519 Key Derivitation matching the 24 word seed
    /// phrase as implemented by HashPack.
    /// </summary>
    public static readonly KeyDerivitationPath HashPack = new KeyDerivitationPath(KeyType.Ed25519, 44 | 0x80000000, 3030 | 0x80000000, 0 | 0x80000000, 0 | 0x80000000, 0 | 0x80000000);
    /// <summary>
    /// The Ed25519 Key Derivitation matching the 12 word seed
    /// phrase as implemented by HashPack.
    /// </summary>
    public static readonly KeyDerivitationPath Calaxy = new KeyDerivitationPath(KeyType.Ed25519, 44 | 0x80000000, 3030 | 0x80000000, 0 | 0x80000000, 0 | 0x80000000, 0 | 0x80000000);
    /// <summary>
    /// The Ed25519 Key Derivitation matching the 12 or 24 word seed
    /// phrase as implemented by Walla Wallet.
    /// </summary>
    public static readonly KeyDerivitationPath WallaWallet = new KeyDerivitationPath(KeyType.Ed25519, 44 | 0x80000000, 3030 | 0x80000000, 0 | 0x80000000, 0 | 0x80000000, 0 | 0x80000000);
    /// <summary>
    /// The ECDSA Secp256k1 Key Derivitation matching the 12 word seed
    /// phrase as implemented by the Blade Wallet as of May 2023.
    /// </summary>
    public static readonly KeyDerivitationPath Blade = new KeyDerivitationPath(KeyType.ECDSASecp256K1, 44, 3030, 0, 0);
    /// <summary>
    /// The individual steps in the key derivitation path 
    /// requried to recreate keys from a mnemonic seed.
    /// </summary>
    public ReadOnlyMemory<uint> Path { get; private init; }
    /// <summary>
    /// The key type that should be produced when following
    /// this specific path.
    /// </summary>
    public KeyType KeyType { get; private init; }
    /// <summary>
    /// Constructor taking the various expeced key deriviation
    /// path components as input.
    /// </summary>
    /// <param name="keyType">
    /// The type of key that this deriviation path is designed
    /// to generate.
    /// </param>
    /// <param name="path">
    /// The unsigned int values comprising of the derivitation path, 
    /// for example m/44'/3030'/0'/0'/0'
    /// </param>
    private KeyDerivitationPath(KeyType keyType, params uint[] path)
    {
        if (keyType != KeyType.Ed25519 && keyType != KeyType.ECDSASecp256K1)
        {
            throw new ArgumentOutOfRangeException(nameof(keyType), $"Key type of {keyType} is not supported.");
        }
        if (path == null || path.Length == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(path), $"Path must include at least one value.");
        }
        KeyType = keyType;
        Path = path;
    }
}
