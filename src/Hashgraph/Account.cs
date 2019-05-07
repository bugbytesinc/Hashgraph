using Hashgraph.Implementation;
using NSec.Cryptography;
using System;

namespace Hashgraph
{
    public sealed class Account : Address, ISigner, IDisposable, IEquatable<Account>
    {
        private Key _key;

        public Account(long realmNum, long shardNum, long accountNum, string privateKeyInHex) :
            this(realmNum, shardNum, accountNum, Signatures.DecodeByteArrayFromHexString(privateKeyInHex))
        {
        }
        public Account(long realmNum, long shardNum, long accountNum, ReadOnlySpan<byte> privateKey) :
            base(realmNum, shardNum, accountNum)
        {
            _key = Signatures.ImportPrivateEd25519KeyFromBytes(privateKey);
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
        public bool Equals(Account other)
        {
            if (other is null)
            {
                return false;
            }
            return
                RealmNum == other.RealmNum &&
                ShardNum == other.ShardNum &&
                AccountNum == other.AccountNum &&
                _key.PublicKey.Equals(other._key.PublicKey);
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is Account other)
            {
                return Equals(other);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return $"Account:{RealmNum}:{ShardNum}:{AccountNum}:{_key.PublicKey.GetHashCode()}".GetHashCode();
        }
        public static bool operator ==(Account left, Account right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(Account left, Account right)
        {
            return !(left == right);
        }
    }
}