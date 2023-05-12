using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;

namespace Hashgraph.Implementation;

internal static class KeyUtils
{
    private static readonly ReadOnlyMemory<byte> _hederaSecp256k1PublicKeyDerPrefix = Hex.ToBytes("302d300706052b8104000a032200");
    private static readonly ReadOnlyMemory<byte> _hederaSecp256k1PrivateKeyDerPrefix = Hex.ToBytes("3030020100300706052b8104000a04220420");
    private static readonly X9ECParameters _ecdsaSecp256k1curve = SecNamedCurves.GetByName("secp256k1");
    internal static readonly ECDomainParameters EcdsaSecp256k1DomainParams = new(_ecdsaSecp256k1curve.Curve, _ecdsaSecp256k1curve.G, _ecdsaSecp256k1curve.N, _ecdsaSecp256k1curve.H);

    internal static (KeyType keyType, AsymmetricKeyParameter publicKeyParam) ParsePrivateKey(ReadOnlyMemory<byte> privateKey)
    {
        if (privateKey.Length == 32)
        {
            throw new ArgumentOutOfRangeException(nameof(privateKey), $"The private key byte length of 32 is ambiguous, unable to determine which type of key this refers to.");
        }
        // This is the "special" hedera encoding.
        if (privateKey.Length == 50 && privateKey.Span.StartsWith(_hederaSecp256k1PrivateKeyDerPrefix.Span))
        {
            try
            {
                return (KeyType.ECDSASecp256K1, new ECPrivateKeyParameters(new BigInteger(1, privateKey.ToArray(), 18, 32), EcdsaSecp256k1DomainParams));
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException($"Expected the 50 byte length key to be an Hedera ECDSA Secp256k1 private key, it is not parsable as such.", ex);
            }
        }
        // Bouncy Castle Recognized DER Encodings
        AsymmetricKeyParameter asymmetricKeyParameter;
        try
        {
            asymmetricKeyParameter = PrivateKeyFactory.CreateKey(privateKey.ToArray());
        }
        catch (Exception ex)
        {
            throw new ArgumentOutOfRangeException("The private key does not appear to be encoded as a recognizable private key format.", ex);
        }
        if (asymmetricKeyParameter is Ed25519PrivateKeyParameters ed25519PrivateKeyParameters)
        {
            if (ed25519PrivateKeyParameters.IsPrivate)
            {
                return (KeyType.Ed25519, ed25519PrivateKeyParameters);
            }
            throw new ArgumentOutOfRangeException(nameof(privateKey), "This is not an Ed25519 private key, it appears to be a public key.");
        }
        if (asymmetricKeyParameter is ECPrivateKeyParameters ecPrivateKeyParameters)
        {
            if (ecPrivateKeyParameters.IsPrivate)
            {
                return (KeyType.ECDSASecp256K1, ecPrivateKeyParameters);
            }
            throw new ArgumentOutOfRangeException(nameof(privateKey), "This is not an ECDSA Secp256K1 private key, it appears to be a public key.");
        }
        throw new ArgumentOutOfRangeException(nameof(privateKey), "The private key does not appear to be encoded in Ed25519 or ECDSA Secp256k1 format.");
    }
    internal static (KeyType keyType, AsymmetricKeyParameter publicKeyParam) ParsePublicKey(ReadOnlyMemory<byte> publicKey)
    {
        if (publicKey.Length == Ed25519.PublicKeySize)
        {
            try
            {
                return (KeyType.Ed25519, new Ed25519PublicKeyParameters(publicKey.ToArray(), 0));
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException($"Expected the {Ed25519.PublicKeySize} byte length key to be an Ed25519 public key, it is not parsable as such.", ex);
            }
        }
        if (publicKey.Length == 33)
        {
            try
            {
                var q = _ecdsaSecp256k1curve.Curve.DecodePoint(publicKey.ToArray());
                return (KeyType.ECDSASecp256K1, new ECPublicKeyParameters(q, EcdsaSecp256k1DomainParams));
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException($"Expected the 33 byte length key to be an ECDSA Secp256k1 public key, it is not parsable as such.", ex);
            }
        }
        // This is the "special" hedera encoding.
        if (publicKey.Length == 47 && publicKey.Span.StartsWith(_hederaSecp256k1PublicKeyDerPrefix.Span))
        {
            try
            {
                var q = _ecdsaSecp256k1curve.Curve.DecodePoint(publicKey[14..].ToArray());
                return (KeyType.ECDSASecp256K1, new ECPublicKeyParameters(q, EcdsaSecp256k1DomainParams));
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException($"Expected the 47 byte length key to be an Hedera ECDSA Secp256k1 public key, it is not parsable as such.", ex);
            }
        }
        // Bouncy Castle Recognized DER Encodings
        AsymmetricKeyParameter asymmetricKeyParameter;
        try
        {
            asymmetricKeyParameter = PublicKeyFactory.CreateKey(publicKey.ToArray());
        }
        catch (Exception ex)
        {
            throw new ArgumentOutOfRangeException("The public key does not appear to be encoded as a recognizable public key format.", ex);
        }
        if (asymmetricKeyParameter is Ed25519PublicKeyParameters ed25519PublicKeyParameters)
        {
            if (!ed25519PublicKeyParameters.IsPrivate)
            {
                return (KeyType.Ed25519, ed25519PublicKeyParameters);
            }
            throw new ArgumentOutOfRangeException(nameof(publicKey), "This is not an Ed25519 public key, it appears to be a private key.");
        }
        if (asymmetricKeyParameter is ECPublicKeyParameters ecPublicKeyParameters)
        {
            if (!ecPublicKeyParameters.IsPrivate)
            {
                return (KeyType.ECDSASecp256K1, ecPublicKeyParameters);
            }
            throw new ArgumentOutOfRangeException(nameof(publicKey), "This is not an ECDSA Secp256K1 public key, it appears to be a private key.");
        }
        throw new ArgumentOutOfRangeException(nameof(publicKey), $"The public key of type {asymmetricKeyParameter.GetType().Name} is not supported.");
    }

    internal static Ed25519PrivateKeyParameters ParsePrivateEd25519Key(ReadOnlyMemory<byte> privateKey)
    {
        AsymmetricKeyParameter asymmetricKeyParameter;
        try
        {
            // Check to see if we have a raw key.
            if (privateKey.Length == Ed25519.SecretKeySize)
            {
                return new Ed25519PrivateKeyParameters(privateKey.ToArray(), 0);
            }
            asymmetricKeyParameter = PrivateKeyFactory.CreateKey(privateKey.ToArray());
        }
        catch (Exception ex)
        {
            if (privateKey.Length == 0)
            {
                throw new ArgumentOutOfRangeException("Private Key cannot be empty.", ex);
            }
            throw new ArgumentOutOfRangeException("The private key does not appear to be encoded as a recognizable Ed25519 format.", ex);
        }
        if (asymmetricKeyParameter is Ed25519PrivateKeyParameters ed25519PrivateKeyParameters)
        {
            if (ed25519PrivateKeyParameters.IsPrivate)
            {
                return ed25519PrivateKeyParameters;
            }
            throw new ArgumentOutOfRangeException(nameof(privateKey), "This is not an Ed25519 private key, it appears to be a public key.");
        }
        throw new ArgumentOutOfRangeException(nameof(privateKey), "The private key does not appear to be encoded in Ed25519 format.");
    }
    internal static Ed25519PublicKeyParameters ParsePublicEd25519Key(ReadOnlyMemory<byte> publicKey)
    {
        AsymmetricKeyParameter asymmetricKeyParameter;
        try
        {
            // Check to see if we have a raw key.   
            if (publicKey.Length == Ed25519.PublicKeySize)
            {
                return new Ed25519PublicKeyParameters(publicKey.ToArray(), 0);
            }
            // If not, assume it is DER encoded.
            asymmetricKeyParameter = PublicKeyFactory.CreateKey(publicKey.ToArray());
        }
        catch (Exception ex)
        {
            throw new ArgumentOutOfRangeException("The public key does not appear to be encoded in a recognizable Ed25519 format.", ex);
        }
        if (asymmetricKeyParameter is Ed25519PublicKeyParameters ed25519PublicKeyParameters)
        {
            if (!ed25519PublicKeyParameters.IsPrivate)
            {
                return ed25519PublicKeyParameters;
            }
            throw new ArgumentOutOfRangeException(nameof(publicKey), "This is not an Ed25519 public key, it appears to be a private key.");
        }
        throw new ArgumentOutOfRangeException(nameof(publicKey), "The public key does not appear to be encoded in a recognizable Ed25519 format.");
    }
    internal static ReadOnlyMemory<byte> EncodeAsDer(Ed25519PublicKeyParameters publicKeyParameters)
    {
        return SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKeyParameters).GetDerEncoded();
    }
    internal static ReadOnlyMemory<byte> EncodeAsRaw(Ed25519PublicKeyParameters publicKeyParameters)
    {
        return publicKeyParameters.GetEncoded();
    }
    internal static void Sign(IInvoice invoice, Ed25519PrivateKeyParameters privateKey)
    {
        var ed25519Signer = new Ed25519Signer();
        ed25519Signer.Init(true, privateKey);
        ed25519Signer.BlockUpdate(invoice.TxBytes.ToArray(), 0, invoice.TxBytes.Length);
        var signature = ed25519Signer.GenerateSignature();
        ed25519Signer.Reset();
        var publicKey = privateKey.GeneratePublicKey().GetEncoded();
        var prefix = new ReadOnlyMemory<byte>(publicKey, 0, Math.Min(Math.Max(6, invoice.MinimumDesiredPrefixSize), publicKey.Length));
        invoice.AddSignature(KeyType.Ed25519, prefix, signature);
    }

    internal static ECPrivateKeyParameters ParsePrivateEcdsaSecp256k1Key(ReadOnlyMemory<byte> privateKey)
    {
        AsymmetricKeyParameter asymmetricKeyParameter;
        try
        {
            if (privateKey.Length > 30 && privateKey.Length < 34)
            {
                return new ECPrivateKeyParameters(new BigInteger(1, privateKey.ToArray()), EcdsaSecp256k1DomainParams);
            }
            // This is the "special" hedera encoding.
            if (privateKey.Length == 50 && privateKey.Span.StartsWith(_hederaSecp256k1PrivateKeyDerPrefix.Span))
            {
                return new ECPrivateKeyParameters(new BigInteger(1, privateKey.ToArray(), 18, 32), EcdsaSecp256k1DomainParams);
            }
            // Bouncy Castle Recognized DER Encodings
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
            throw new ArgumentOutOfRangeException(nameof(privateKey), "This is not an ECDSA Secp256K1 private key, it appears to be a public key.");
        }
        throw new ArgumentOutOfRangeException(nameof(privateKey), "The private key does not appear to be encoded in ECDSA Secp256K1 format.");
    }
    internal static ECPublicKeyParameters ParsePublicEcdsaSecp256k1Key(ReadOnlyMemory<byte> publicKey)
    {
        AsymmetricKeyParameter asymmetricKeyParameter;
        try
        {
            // First, check to see if we have a raw compressed key
            if (publicKey.Length == 33)
            {
                var q = _ecdsaSecp256k1curve.Curve.DecodePoint(publicKey.ToArray());
                return new ECPublicKeyParameters(q, EcdsaSecp256k1DomainParams);
            }
            // Or is this the "special" hedera encoding.
            if (publicKey.Length == 47 && publicKey.Span.StartsWith(_hederaSecp256k1PublicKeyDerPrefix.Span))
            {
                var q = _ecdsaSecp256k1curve.Curve.DecodePoint(publicKey[14..].ToArray());
                return new ECPublicKeyParameters(q, EcdsaSecp256k1DomainParams);
            }
            // Bouncy Castle Recognized DER Encodings
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
            throw new ArgumentOutOfRangeException(nameof(publicKey), "This is not an ECDSA Secp256K1 public key, it appears to be a private key.");
        }
        throw new ArgumentOutOfRangeException(nameof(publicKey), "The public key was not provided in a recognizable ECDSA Secp256K1 format.");
    }
    internal static ReadOnlyMemory<byte> EncodeAsDer(ECPublicKeyParameters publicKeyParameters)
    {
        return SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKeyParameters).GetDerEncoded();
    }
    internal static ReadOnlyMemory<byte> EncodeAsHedera(ECPublicKeyParameters publicKeyParameters)
    {
        var publicKey = publicKeyParameters.Q.GetEncoded(true);
        var result = new byte[publicKey.Length + _hederaSecp256k1PublicKeyDerPrefix.Length];
        Array.Copy(_hederaSecp256k1PublicKeyDerPrefix.ToArray(), result, _hederaSecp256k1PublicKeyDerPrefix.Length);
        Array.Copy(publicKey, 0, result, _hederaSecp256k1PublicKeyDerPrefix.Length, publicKey.Length);
        return result;
    }
    internal static ReadOnlyMemory<byte> EncodeAsRaw(ECPublicKeyParameters publicKeyParameters)
    {
        return publicKeyParameters.Q.GetEncoded(true);
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
        Insert256Int(components[0], 0, encoded);
        Insert256Int(components[1], 32, encoded);
        var publicKey = EcdsaSecp256k1DomainParams.G.Multiply(privateKey.D).GetEncoded(true);
        var prefix = new ReadOnlyMemory<byte>(publicKey, 0, Math.Min(Math.Max(6, invoice.MinimumDesiredPrefixSize), publicKey.Length));
        invoice.AddSignature(KeyType.ECDSASecp256K1, prefix, encoded);
    }
    private static void Insert256Int(BigInteger component, int offset, byte[] array)
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