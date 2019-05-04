using Hashgraph.Implementation;
using NSec.Cryptography;
using System;

namespace Hashgraph
{
    public sealed class Account : Address, ISigner, IDisposable
    {
        private Key _key;

        public Account(long realmNum, long shardNum, long accountNum, string privateKeyInHex) :
            this(realmNum, shardNum, accountNum, EecodeByteArrayFromHexString(privateKeyInHex))
        {
        }
        public Account(long realmNum, long shardNum, long accountNum, ReadOnlySpan<byte> privateKey) :
            base(realmNum, shardNum, accountNum)
        {
            try
            {
                _key = Key.Import(SignatureAlgorithm.Ed25519, privateKey, KeyBlobFormat.PkixPrivateKey);
            }
            catch (FormatException fe)
            {
                throw new ArgumentOutOfRangeException("The private key was not provided in a recognizable Ed25519 format.", fe);
            }
        }
        byte[] ISigner.Sign(ReadOnlySpan<byte> data)
        {
            return SignatureAlgorithm.Ed25519.Sign(_key, data);
        }
        public void Dispose()
        {
            if (_key != null)
            {
                _key.Dispose();
            }
            GC.SuppressFinalize(this);
        }
        private static byte[] EecodeByteArrayFromHexString(String privateKeyInHex)
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
                throw new ArgumentOutOfRangeException(nameof(privateKeyInHex), "Private Key does not appear to be properly encoded in Hex, found an odd number of characters.");
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
                throw new ArgumentOutOfRangeException("Private Key does not appear to be encoded in Hex.", fe);
            }
        }
    }
}
