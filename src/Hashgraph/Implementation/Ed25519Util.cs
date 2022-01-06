using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math.EC.Rfc8032;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal helper class that provides conversion services 
    /// between raw bytes and Ed25519 public and private keys.
    /// </summary>
    internal static class Ed25519Util
    {
        internal static Ed25519PrivateKeyParameters PrivateParamsFromDerOrRaw(ReadOnlyMemory<byte> privateKey)
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
                throw new ArgumentOutOfRangeException("This is not an Ed25519 private key, it appears to be a public key.");
            }
            throw new ArgumentOutOfRangeException("The private key does not appear to be encoded in Ed25519 format.");
        }
        internal static Ed25519PublicKeyParameters PublicParamsFromDerOrRaw(ReadOnlyMemory<byte> publicKey)
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
                throw new ArgumentOutOfRangeException("This is not an Ed25519 public key, it appears to be a private key.");
            }
            throw new ArgumentOutOfRangeException("The public key does not appear to be encoded in a recognizable Ed25519 format.");
        }
        internal static ReadOnlyMemory<byte> ToDerBytes(Ed25519PublicKeyParameters publicKeyParameters)
        {
            return SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKeyParameters).GetDerEncoded();
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
    }
}