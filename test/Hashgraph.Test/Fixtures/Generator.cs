using NSec.Cryptography;
using System;

namespace Hashgraph.Test.Fixtures
{
    public static class Generator
    {
        private static Random _random = new Random();
        private static char[] _sample = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-*&%$#@!".ToCharArray();

        public static Int32 Integer(Int32 minValueInclusive, Int32 maxValueInclusive)
        {
            return _random.Next(minValueInclusive, maxValueInclusive + 1);
        }

        public static Double Double(Double minValueInclusive, Double maxValueInclusive)
        {
            return (_random.NextDouble() * (maxValueInclusive - minValueInclusive)) + minValueInclusive;
        }

        public static String String(Int32 minLengthInclusive, Int32 maxLengthInclusive)
        {
            return Code(Integer(minLengthInclusive, maxLengthInclusive));
        }

        public static String Code(Int32 length)
        {
            var buffer = new char[length];
            for (int i = 0; i < length; i++)
            {
                buffer[i] = _sample[_random.Next(0, _sample.Length)];
            }
            return new string(buffer);
        }

        public static String[] ArrayOfStrings(Int32 minCount, Int32 maxCount, Int32 minLength, Int32 maxLength)
        {
            String[] result = new String[Integer(minCount, maxCount)];
            for (Int32 index = 0; index < result.Length; index++)
            {
                result[index] = String(minLength, maxLength);
            }
            return result;
        }

        public static (ReadOnlyMemory<byte> publicKey, ReadOnlyMemory<byte> privateKey) KeyPair()
        {
            // public prefix:  302a300506032b6570032100 
            // private prefix: 302e020100300506032b6570

            /*
             * From Discord: Greg Scullard (Hedera Hashgraph)05/01/2019
             * 
             Check the public key and private key are the right way round (I've done it wrong myself a few times). The private key is the longest of the two.
             If that isn't the issue, try with the encoded keys (they start with 302), there may be a bug with non encoded keys
             If you don't have them to hand, you can prefix the public key with 302a300506032b6570032100 and the private key with 302e020100300506032b6570
             this is common to all keys, it's ASN.1 Encoding that specifies the format of the data in the string.
             so 20aa4780f.... becomes 302a300506032b657003210020aa4780f... for a public key
             * */
            using var key = Key.Create(SignatureAlgorithm.Ed25519, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
            var publicKey = key.Export(KeyBlobFormat.PkixPublicKey);
            var privateKey = key.Export(KeyBlobFormat.PkixPrivateKey);
            return (publicKey, privateKey);
        }
    }
}
