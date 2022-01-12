using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;

namespace Hashgraph.Implementation;

/// <summary>
/// Internal helper class that provides conversion services 
/// between raw bytes and EcdsaSecp256k1Util public and private keys.
/// </summary>
internal static class EcdsaSecp256k1Util
{
    private static X9ECParameters curve = SecNamedCurves.GetByName("secp256k1");
    private static ECDomainParameters domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);

    internal static ECPrivateKeyParameters PrivateParamsFromDerOrRaw(ReadOnlyMemory<byte> privateKey)
    {
        AsymmetricKeyParameter asymmetricKeyParameter;
        try
        {
            if (privateKey.Length == 32)
            {
                return new ECPrivateKeyParameters(new BigInteger(privateKey.ToArray()), domain);
            }
            asymmetricKeyParameter = PrivateKeyFactory.CreateKey(privateKey.ToArray());
        }
        catch (Exception ex)
        {
            if (privateKey.Length == 0)
            {
                throw new ArgumentOutOfRangeException("Private Key cannot be empty.", ex);
            }
            throw new ArgumentOutOfRangeException("The private key was not provided in a recognizable ECDSA Secp256K1 format.", ex);
        }
        if (asymmetricKeyParameter is ECPrivateKeyParameters ecPrivateKeyParameters)
        {
            if (ecPrivateKeyParameters.IsPrivate)
            {
                return ecPrivateKeyParameters;
            }
            throw new ArgumentOutOfRangeException("This is not an ECDSA Secp256K1 private key, it appears to be a public key.");
        }
        throw new ArgumentOutOfRangeException("The private key does not appear to be encoded in ECDSA Secp256K1 format.");
    }
    internal static ECPublicKeyParameters PublicParamsFromDerOrRaw(ReadOnlyMemory<byte> publicKey)
    {
        AsymmetricKeyParameter asymmetricKeyParameter;
        try
        {
            // First, check to see if we have a raw compressed key
            if (publicKey.Length == 33)
            {
                var q = curve.Curve.DecodePoint(publicKey.ToArray());
                return new ECPublicKeyParameters(q, domain);
            }
            // If not, assume it is DER encoded.
            asymmetricKeyParameter = PublicKeyFactory.CreateKey(publicKey.ToArray());
        }
        catch (Exception ex)
        {
            throw new ArgumentOutOfRangeException("The public key was not provided in a recognizable ECDSA Secp256K1 format.", ex);
        }
        if (asymmetricKeyParameter is ECPublicKeyParameters ecPublicKeyParameters)
        {
            if (!ecPublicKeyParameters.IsPrivate)
            {
                return ecPublicKeyParameters;
            }
            throw new ArgumentOutOfRangeException("This is not an ECDSA Secp256K1 public key, it appears to be a private key.");
        }
        throw new ArgumentOutOfRangeException("The public key was not provided in a recognizable ECDSA Secp256K1 format.");
    }
    internal static ReadOnlyMemory<byte> ToDerBytes(ECPublicKeyParameters publicKeyParameters)
    {
        return SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKeyParameters).GetDerEncoded();
    }
    internal static void Sign(IInvoice invoice, ECPrivateKeyParameters privateKey)
    {
        var digest = new KeccakDigest(256);
        digest.BlockUpdate(invoice.TxBytes.ToArray(), 0, invoice.TxBytes.Length);
        var hash = new byte[digest.GetByteLength()];
        digest.DoFinal(hash, 0);
        var signer = new ECDsaSigner(new HMacDsaKCalculator(new Sha256Digest()));
        signer.Init(true, privateKey);
        var components = signer.GenerateSignature(hash);
        var encoded = new byte[64];
        insert256Int(components[0], 0, encoded);
        insert256Int(components[1], 32, encoded);
        var publicKey = domain.G.Multiply(privateKey.D).GetEncoded(true);
        var prefix = new ReadOnlyMemory<byte>(publicKey, 0, Math.Min(Math.Max(6, invoice.MinimumDesiredPrefixSize), publicKey.Length));
        invoice.AddSignature(KeyType.ECDSASecp256K1, prefix, encoded);
    }
    private static void insert256Int(BigInteger component, int offset, byte[] array)
    {
        byte[] bytes = component.ToByteArrayUnsigned();
        var length = bytes.Length;
        if (length >= 32)
        {
            Array.Copy(bytes, length - 32, array, offset, 32);
        }
        else
        {
            Array.Copy(bytes, 0, array, offset + 32 - length, length);
        }
    }
}