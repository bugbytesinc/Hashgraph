using NSec.Cryptography;
using System;

namespace Hashgraph.Implementation
{
    internal static class Keys
    {
        private static readonly byte[] privateKeyPrefix = Hex.ToBytes("302e020100300506032b6570").ToArray();
        private static readonly byte[] publicKeyPrefix = Hex.ToBytes("302a300506032b6570032100").ToArray();
        internal static Key ImportPrivateEd25519KeyFromBytes(ReadOnlyMemory<byte> privateKey)
        {
            try
            {
                return Key.Import(SignatureAlgorithm.Ed25519, privateKey.Span, KeyBlobFormat.PkixPrivateKey);
            }
            catch (FormatException fe)
            {
                if (privateKey.Length == 0)
                {
                    throw new ArgumentOutOfRangeException("Private Key cannot be empty.", fe);
                }
                if (privateKey.Length == 36)
                {
                    throw new ArgumentOutOfRangeException("The private key was not provided in a recognizable Ed25519 format. Is it missing the encoding format prefix 0x302e020100300506032b6570?", fe);
                }
                else if (!privateKey.Span.StartsWith(privateKeyPrefix) || privateKey.Length != 48)
                {
                    throw new ArgumentOutOfRangeException("The private key was not provided in a recognizable Ed25519 format. Was expecting 48 bytes starting with the prefix 0x302e020100300506032b6570.", fe);
                }
                throw new ArgumentOutOfRangeException("The private key does not appear to be encoded as a recognizable Ed25519 format.", fe);
            }
        }
        internal static PublicKey ImportPublicEd25519KeyFromBytes(ReadOnlyMemory<byte> publicKey)
        {
            try
            {
                return PublicKey.Import(SignatureAlgorithm.Ed25519, publicKey.Span, KeyBlobFormat.PkixPublicKey);
            }
            catch (FormatException fe)
            {
                if (publicKey.Length == 32)
                {
                    throw new ArgumentOutOfRangeException("The public key was not provided in a recognizable Ed25519 format. Is it missing the encoding format prefix 0x302a300506032b6570032100?", fe);
                }
                else if (!publicKey.Span.StartsWith(publicKeyPrefix) || publicKey.Length != 44)
                {
                    throw new ArgumentOutOfRangeException("The public key was not provided in a recognizable Ed25519 format. Was expecting 44 bytes starting with the prefix 0x302a300506032b6570032100.", fe);
                }
                throw new ArgumentOutOfRangeException("The public key does not appear to be encoded as a recognizable Ed25519 format.", fe);
            }
        }
    }
}