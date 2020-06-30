using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System;
using System.Linq;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal helper class that provides conversion services 
    /// between raw bytes and Ed25519 public and private keys.
    /// </summary>
    internal static class Ed25519Util
    {
        internal static readonly byte[] privateKeyPrefix = Hex.ToBytes("302e020100300506032b6570").ToArray();
        internal static readonly byte[] publicKeyPrefix = Hex.ToBytes("302a300506032b6570032100").ToArray();
        internal static Ed25519PrivateKeyParameters PrivateKeyParamsFromBytes(ReadOnlyMemory<byte> privateKey)
        {
            AsymmetricKeyParameter asymmetricKeyParameter;
            try
            {
                asymmetricKeyParameter = PrivateKeyFactory.CreateKey(privateKey.ToArray());
            }
            catch (Exception ex)
            {
                if (privateKey.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("Private Key cannot be empty.", ex);
                }
                if (privateKey.Length == 36)
                {
                    throw new ArgumentOutOfRangeException("The private key was not provided in a recognizable Ed25519 format. Is it missing the encoding format prefix 0x302e020100300506032b6570?", ex);
                }
                else if (!privateKey.Span.StartsWith(privateKeyPrefix) || privateKey.Length != 48)
                {
                    throw new ArgumentOutOfRangeException("The private key was not provided in a recognizable Ed25519 format. Was expecting 48 bytes starting with the prefix 0x302e020100300506032b6570.", ex);
                }
                throw new ArgumentOutOfRangeException("The private key does not appear to be encoded as a recognizable Ed25519 format.", ex);
            }
            if (asymmetricKeyParameter is Ed25519PrivateKeyParameters ed25519PrivateKeyParameters)
            {
                if (ed25519PrivateKeyParameters.IsPrivate)
                {
                    return ed25519PrivateKeyParameters;
                }
                throw new ArgumentOutOfRangeException("This is not an Ed25519 private key, it appears to be a public key.");
            }
            throw new ArgumentOutOfRangeException("The private key does not appear to be encoded in Ed25519 format.");
        }
        internal static Ed25519PublicKeyParameters PublicKeyParamsFromBytes(ReadOnlyMemory<byte> publicKey)
        {
            AsymmetricKeyParameter asymmetricKeyParameter;
            try
            {
                asymmetricKeyParameter = PublicKeyFactory.CreateKey(publicKey.ToArray());
            }
            catch (Exception ex)
            {
                if (publicKey.Length == 32)
                {
                    throw new ArgumentOutOfRangeException("The public key was not provided in a recognizable Ed25519 format. Is it missing the encoding format prefix 0x302a300506032b6570032100?", ex);
                }
                else if (!publicKey.Span.StartsWith(publicKeyPrefix) || publicKey.Length != 44)
                {
                    throw new ArgumentOutOfRangeException("The public key was not provided in a recognizable Ed25519 format. Was expecting 44 bytes starting with the prefix 0x302a300506032b6570032100.", ex);
                }
                throw new ArgumentOutOfRangeException("The public key does not appear to be encoded as a recognizable Ed25519 format.", ex);
            }
            if (asymmetricKeyParameter is Ed25519PublicKeyParameters ed25519PublicKeyParameters)
            {
                if (!ed25519PublicKeyParameters.IsPrivate)
                {
                    return ed25519PublicKeyParameters;
                }
                throw new ArgumentOutOfRangeException("This is not an Ed25519 public key, it appears to be a private key.");
            }
            throw new ArgumentOutOfRangeException("The public key does not appear to be encoded as a recognizable Ed25519 format.");
        }
        internal static ReadOnlyMemory<byte> ToBytes(Ed25519PublicKeyParameters publicKeyParameters)
        {
            return publicKeyPrefix.Concat(publicKeyParameters.GetEncoded()).ToArray();
        }
    }
}