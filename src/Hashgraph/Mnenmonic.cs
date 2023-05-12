using Hashgraph.Implementation;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Pkcs;
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
    /// The root seed key phrase for generating ECDSA Secp256k1 keys
    /// from a mnenmonic seed phrase.
    /// </summary>
    private readonly ReadOnlyMemory<byte> _ecdsaSecp256k1 = Encoding.UTF8.GetBytes("Bitcoin seed");
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
    /// Computes the HD key pair for this mnenmonic.
    /// </summary>
    /// <param name="path">
    /// The key derivitation path that should be used to
    /// generate the private and public key values.
    /// </param>
    /// <returns>
    /// DER Encoded public and private key values.
    /// </returns>
    public (ReadOnlyMemory<byte> publicKey, ReadOnlyMemory<byte> privateKey) GenerateKeyPair(KeyDerivitationPath path)
    {
        if (path.KeyType == KeyType.Ed25519)
        {
            var keyDataAndChainCode = new HMACSHA512(_ed25519SeedKey.ToArray()).ComputeHash(_seed.ToArray());
            foreach (uint index in path.Path.ToArray())
            {
                byte[] data = new byte[37];
                var indexBytes = BitConverter.GetBytes(index);
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(indexBytes);
                }
                // TODO - Review the spec for any path that is
                // not fully "hardened" to make sure this is the
                // correct key value to put here. Presently all
                // Ed25519 paths in the eccosystem are comletely
                // "hardened".
                Array.Copy(keyDataAndChainCode, 0, data, 1, 32);
                Array.Copy(indexBytes, 0, data, 33, 4);
                keyDataAndChainCode = new HMACSHA512(keyDataAndChainCode[32..]).ComputeHash(data);
            }
            var keyParams = new Ed25519PrivateKeyParameters(keyDataAndChainCode[..32], 0);
            var publicKey = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(keyParams.GeneratePublicKey()).GetDerEncoded();
            var privateKey = new byte[_edd25519PrivateKeyDerPrefix.Length + Ed25519.SecretKeySize];
            Array.Copy(_edd25519PrivateKeyDerPrefix.ToArray(), privateKey, _edd25519PrivateKeyDerPrefix.Length);
            Array.Copy(keyParams.GetEncoded(), 0, privateKey, _edd25519PrivateKeyDerPrefix.Length, Ed25519.SecretKeySize);
            return (publicKey, privateKey);
        }
        else if (path.KeyType == KeyType.ECDSASecp256K1)
        {
            var keyDataAndChainCode = new HMACSHA512(_ecdsaSecp256k1.ToArray()).ComputeHash(_seed.ToArray());
            foreach (uint index in path.Path.ToArray())
            {
                keyDataAndChainCode = CKDprivEcdsaSecp256k1(keyDataAndChainCode, index);
            }
            var privateKeyParams = new ECPrivateKeyParameters(new BigInteger(1, keyDataAndChainCode[..32]), KeyUtils.EcdsaSecp256k1DomainParams);
            var publicKeyParams = new ECPublicKeyParameters(privateKeyParams.Parameters.G.Multiply(privateKeyParams.D), privateKeyParams.Parameters);
            var privateKey = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKeyParams).GetDerEncoded();
            var publicKey = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKeyParams).GetDerEncoded();
            return (publicKey, privateKey);
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(path), $"Key type of {path.KeyType} is not supported.");
        }
    }
    private static byte[] CKDprivEcdsaSecp256k1(byte[] parentKeyDataAndChainCode, uint index)
    {
        byte[] data = new byte[37];
        var indexBytes = BitConverter.GetBytes(index);
        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(indexBytes);
        }
        if ((index & 0x80000000) == 0x80000000)
        {
            Array.Copy(parentKeyDataAndChainCode, 0, data, 1, 32);
        }
        else
        {
            var parentPrivateKeyParams = new ECPrivateKeyParameters(new BigInteger(1, parentKeyDataAndChainCode[..32]), KeyUtils.EcdsaSecp256k1DomainParams);
            var parentPublicKeyParams = new ECPublicKeyParameters(parentPrivateKeyParams.Parameters.G.Multiply(parentPrivateKeyParams.D), parentPrivateKeyParams.Parameters);
            Array.Copy(parentPublicKeyParams.Q.GetEncoded(true), 0, data, 0, 33);
        }
        Array.Copy(indexBytes, 0, data, 33, 4);
        var digest = new HMACSHA512(parentKeyDataAndChainCode[32..]).ComputeHash(data);
        var digestLeft = new BigInteger(1, digest[..32]);
        var keyParam = new BigInteger(1, parentKeyDataAndChainCode[..32]);
        var keyBytes = digestLeft.Add(keyParam).Mod(KeyUtils.EcdsaSecp256k1DomainParams.N).ToByteArrayUnsigned();
        var childKeyDataAndChainCode = new byte[64];
        Array.Copy(keyBytes, 0, childKeyDataAndChainCode, 32 - keyBytes.Length, keyBytes.Length);
        Array.Copy(digest, 32, childKeyDataAndChainCode, 32, 32);
        return childKeyDataAndChainCode;
    }
}