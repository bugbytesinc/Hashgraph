using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;

namespace Hashgraph.Implementation
{
    internal static class DerEncodingUtil
    {
        internal static (KeyType keyType, AsymmetricKeyParameter publicKeyParam) ParsePrivateKeyFromDerBytes(ReadOnlyMemory<byte> privateKey)
        {
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
        internal static (KeyType keyType, AsymmetricKeyParameter publicKeyParam) ParsePublicKeyFromDerBytes(ReadOnlyMemory<byte> publicKey)
        {
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
}