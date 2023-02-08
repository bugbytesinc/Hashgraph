using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Security;
using System;
using System.Security.Cryptography.X509Certificates;

namespace Hashgraph.Implementation;

internal static class MultiKeyEncodingUtil
{
    private static ReadOnlyMemory<byte> secp256k1PublicKeyDerPrefix = Hex.ToBytes("302d300706052b8104000a032200");
    private static ReadOnlyMemory<byte> secp256k1PrivateKeyDerPrefix = Hex.ToBytes("3030020100300706052b8104000a04220420");
    internal static (KeyType keyType, AsymmetricKeyParameter publicKeyParam) ParsePrivateKeyFromDerOrRawBytes(ReadOnlyMemory<byte> privateKey)
    {
        if (privateKey.Length == 32)
        {
            throw new ArgumentOutOfRangeException(nameof(privateKey), $"The private key byte length of 32 is ambiguous, unable to determine which type of key this refers to.");
        }
        if (privateKey.Length == 50 && privateKey.Span.StartsWith(secp256k1PrivateKeyDerPrefix.Span))
        {
            try
            {
                return (KeyType.ECDSASecp256K1, new ECPrivateKeyParameters(new BigInteger(privateKey.ToArray(), 18, 32), EcdsaSecp256k1Util.domain));
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException($"Expected the 50 byte length key to be an ECDSA Secp256k1 private key, it is not parsable as such.", ex);
            }
        }
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
            throw new ArgumentOutOfRangeException("This is not an Ed25519 private key, it appears to be a public key.");
        }
        if (asymmetricKeyParameter is ECPrivateKeyParameters ecPrivateKeyParameters)
        {
            if (ecPrivateKeyParameters.IsPrivate)
            {
                return (KeyType.ECDSASecp256K1, ecPrivateKeyParameters);
            }
            throw new ArgumentOutOfRangeException("This is not an ECDSA Secp256K1 private key, it appears to be a public key.");
        }
        throw new ArgumentOutOfRangeException("The private key does not appear to be encoded in Ed25519 format.");
    }
    internal static (KeyType keyType, AsymmetricKeyParameter publicKeyParam) ParsePublicKeyFromDerOrRawBytes(ReadOnlyMemory<byte> publicKey)
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
                var q = EcdsaSecp256k1Util.curve.Curve.DecodePoint(publicKey.ToArray());
                return (KeyType.ECDSASecp256K1, new ECPublicKeyParameters(q, EcdsaSecp256k1Util.domain));
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException($"Expected the 33 byte length key to be an ECDSA Secp256k1 public key, it is not parsable as such.", ex);
            }
        }
        if (publicKey.Length == 47 && publicKey.Span.StartsWith(secp256k1PublicKeyDerPrefix.Span))
        {
            try
            {
                var q = EcdsaSecp256k1Util.curve.Curve.DecodePoint(publicKey.Slice(14).ToArray());
                return (KeyType.ECDSASecp256K1, new ECPublicKeyParameters(q, EcdsaSecp256k1Util.domain));
            }
            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException($"Expected the 47 byte length key to be an ECDSA Secp256k1 public key, it is not parsable as such.", ex);
            }
        }
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
            throw new ArgumentOutOfRangeException("This is not an Ed25519 public key, it appears to be a private key.");
        }
        if (asymmetricKeyParameter is ECPublicKeyParameters ecPublicKeyParameters)
        {
            if (!ecPublicKeyParameters.IsPrivate)
            {
                return (KeyType.ECDSASecp256K1, ecPublicKeyParameters);
            }
            throw new ArgumentOutOfRangeException("This is not an ECDSA Secp256K1 public key, it appears to be a private key.");
        }
        throw new ArgumentOutOfRangeException($"The public key of type {asymmetricKeyParameter.GetType().Name} is not supported.");
    }
}