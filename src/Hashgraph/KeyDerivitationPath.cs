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
    public static readonly KeyDerivitationPath HashPack = new KeyDerivitationPath(44 | 0x80000000, 3030 | 0x80000000, 0 | 0x80000000, 0 | 0x80000000, 0 | 0x80000000);
    /// <summary>
    /// The individual steps in the key derivitation path 
    /// requried to recreate keys from a mnemonic seed.
    /// </summary>
    public ReadOnlyMemory<uint> Path { get; private init; }
    /// <summary>
    /// Constructor taking the various expeced key deriviation
    /// path components as input.
    /// </summary>
    /// <param name="purpose">
    /// The purpose of the of the structure, typically 44 indicating
    /// this is a HD Wallet key (BIP44)
    /// </param>
    /// <param name="coinType">
    /// Identifies the cryptocurrency or blockchain type, typicall
    /// for Hedera this value is 3030.
    /// </param>
    /// <param name="account">
    /// Allows for the organization of multiple accounts or 
    /// sub-wallets within a single HD wallet, typically 0
    /// </param>
    /// <param name="change">
    /// Indicates whether the address is meant for change 
    /// (value 1) or for receiving transactions (value 0).
    /// Typically is 0
    /// </param>
    /// <param name="index">
    /// Specifies the index of the individual address within 
    /// the account and change levels, this is typically 0.
    /// </param>
    private KeyDerivitationPath(uint purpose, uint coinType, uint account, uint change, uint index)
    {
        Path = new[] { purpose, coinType, account, change, index };
    }
}
