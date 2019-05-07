using NSec.Cryptography;
using System;
using System.Text;

namespace Hashgraph.Implementation
{
    internal static class Signatures
    {
        internal static Key ImportPrivateEd25519KeyFromBytes(ReadOnlySpan<byte> privateKey)
        {
            try
            {
                return Key.Import(SignatureAlgorithm.Ed25519, privateKey, KeyBlobFormat.PkixPrivateKey);
            }
            catch (FormatException fe)
            {
                throw new ArgumentOutOfRangeException("The private key was not provided in a recognizable Ed25519 format.", fe);
            }
        }
        internal static PublicKey ImportPublicEd25519KeyFromBytes(ReadOnlySpan<byte> publicKey)
        {
            try
            {
                return PublicKey.Import(SignatureAlgorithm.Ed25519, publicKey, KeyBlobFormat.PkixPublicKey);
            }
            catch (FormatException fe)
            {
                throw new ArgumentOutOfRangeException("The public key was not provided in a recognizable Ed25519 format.", fe);
            }
        }
        internal static byte[] DecodeByteArrayFromHexString(String privateKeyInHex)
        {
            if (privateKeyInHex == null)
            {
                throw new ArgumentNullException(nameof(privateKeyInHex), "Private Key cannot be null.");
            }
            else if (String.IsNullOrWhiteSpace(privateKeyInHex))
            {
                throw new ArgumentOutOfRangeException(nameof(privateKeyInHex), "Private Key cannot be empty.");
            }
            else if (privateKeyInHex.Length % 2 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(privateKeyInHex), "Key does not appear to be properly encoded in Hex, found an odd number of characters.");
            }
            try
            {
                byte[] result = new byte[privateKeyInHex.Length / 2];
                for (int i = 0; i < privateKeyInHex.Length; i += 2)
                {
                    result[i / 2] = Convert.ToByte(privateKeyInHex.Substring(i, 2), 16);
                }
                return result;
            }
            catch (FormatException fe)
            {
                throw new ArgumentOutOfRangeException("Key does not appear to be encoded in Hex.", fe);
            }
        }
        public static string EncodeByteArrayToHexString(ReadOnlySpan<byte> bytes)
        {
            var size = bytes.Length * 2;
            if (size == 0)
            {
                return string.Empty;
            }
            var buff = new StringBuilder(size, size);
            for (int i = 0; i < bytes.Length; i++)
            {
                buff.AppendFormat("{0:x2}", bytes[i]);
            }
            return buff.ToString();
        }

    }
}
