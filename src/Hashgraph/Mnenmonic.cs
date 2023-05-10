using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.X509;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Hashgraph;
/// <summary>
/// Helper class to produce public and private key values
/// from mnemonic word phrases.
/// </summary>
/// <remarks>
/// THIS IS A PRELIMINARY implementation and does very little
/// error checking.   USE AT YOUR OWN RISK.  The architecture 
/// and surface API is subject to drastic changes in followup 
/// versions of this library.
/// </remarks>
public class Mnenmonic
{
    /// <summary>
    /// The Ed25519 DER Encoding prefix, for just the PRIVATE key.
    /// </summary>
    private readonly ReadOnlyMemory<byte> _edd25519PrivateKeyDerPrefix = Hex.ToBytes("302e020100300506032b657004220420");
    /// <summary>
    /// The root seed key phrase for generating Ed25519 keys
    /// from a mnenmonic seed phrase.
    /// </summary>
    private readonly ReadOnlyMemory<byte> _ed25519SeedKey = Encoding.UTF8.GetBytes("ed25519 seed");
    /// <summary>
    /// The master seed value in bytes generated from the
    /// mnemonic words given to the constructor.  (words
    /// are not saved internally)
    /// </summary>
    private readonly ReadOnlyMemory<byte> _seed;
    /// <summary>
    /// Constructor taking an array of mnemonic words
    /// and a passphrase.
    /// </summary>
    /// <param name="words">
    /// An array of words that make up the mnemonic.
    /// </param>
    /// <param name="passphrase">
    /// Optional password (empty string or null is allowed 
    /// for no password).
    /// </param>
    public Mnenmonic(string[] words, string passphrase)
    {
        var mnemonicBytes = Encoding.UTF8.GetBytes(string.Join(' ', words));
        var saltBytes = Encoding.UTF8.GetBytes("mnemonic" + (passphrase ?? ""));
        using var derive = new Rfc2898DeriveBytes(mnemonicBytes, saltBytes, 2048, HashAlgorithmName.SHA512);
        _seed = derive.GetBytes(64);
    }
    /// <summary>
    /// Computes the HD Ed25519 key pair for this mnenmonic.
    /// </summary>
    /// <param name="path">
    /// The key derivitation path that should be used to
    /// generate the private and public key values.
    /// </param>
    /// <returns>
    /// DER Encoded Ed25519 public and private key values.
    /// </returns>
    public (ReadOnlyMemory<byte> publicKey, ReadOnlyMemory<byte> privateKey) GenerateEd25519KeyPair(KeyDerivitationPath path)
    {
        var keyDataAndChainCode = new HMACSHA512(_ed25519SeedKey.ToArray()).ComputeHash(_seed.ToArray());
        foreach (uint index in path.Path.ToArray())
        {
            keyDataAndChainCode = CKDpriv(keyDataAndChainCode, index);
        }
        var keyParams = new Ed25519PrivateKeyParameters(keyDataAndChainCode[..32], 0);
        var publicKey = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyParams.GeneratePublicKey()).GetDerEncoded();
        var privateKey = new byte[_edd25519PrivateKeyDerPrefix.Length + Ed25519.SecretKeySize];
        Array.Copy(_edd25519PrivateKeyDerPrefix.ToArray(), privateKey, _edd25519PrivateKeyDerPrefix.Length);
        Array.Copy(keyParams.GetEncoded(), 0, privateKey, _edd25519PrivateKeyDerPrefix.Length, Ed25519.SecretKeySize);
        return (publicKey, privateKey);
    }
    /// <summary>
    /// The Private Child Key Derivitation Function used to 
    /// navigate the Key Derivitation path.
    /// </summary>
    /// <param name="parentKeyDataAndChainCode">
    /// The parent key (first 32 bytes) and the parent
    /// chain code (last 32 bytes) representing the current
    /// step in the derivitation path.
    /// </param>
    /// <param name="index">
    /// The index value of child key and code to generate.
    /// </param>
    /// <returns>
    /// A child key and chain code derived from the parent
    /// values for the specified index.
    /// </returns>
    private static byte[] CKDpriv(byte[] parentKeyDataAndChainCode, uint index)
    {
        var indexBytes = BitConverter.GetBytes(index);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(indexBytes);
        }
        byte[] data = new byte[37];
        Array.Copy(parentKeyDataAndChainCode, 0, data, 1, 32);
        Array.Copy(indexBytes, 0, data, 33, 4);
        return new HMACSHA512(parentKeyDataAndChainCode[32..]).ComputeHash(data);
    }
}