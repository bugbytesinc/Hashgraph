using NSec.Cryptography;
using System;

namespace Hashgraph.Implementation
{
    internal class Signer : ISigner
    {
        private Key _key;

        internal Signer(Key key)
        {
            _key = key;
        }
        public byte[] Sign(ReadOnlyMemory<byte> data)
        {
            return SignatureAlgorithm.Ed25519.Sign(_key, data.Span);
        }
    }
}
