using Hashgraph.Implementation;
using System;

namespace Hashgraph
{
    public class TxId : IData, IEquatable<TxId>
    {
        private readonly ReadOnlyMemory<byte> _data;

        ReadOnlyMemory<byte> IData.Data { get { return _data; } }

        internal TxId(ReadOnlyMemory<byte> data)
        {
            _data = data;
        }
        public bool Equals(TxId other)
        {
            if (other is null)
            {
                return false;
            }
            return _data.Equals(other._data);
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
            if (obj is TxId other)
            {
                return Equals(other);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return BitConverter.ToString(_data.ToArray()).GetHashCode();
        }
        public static bool operator ==(TxId left, TxId right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        public static bool operator !=(TxId left, TxId right)
        {
            return !(left == right);
        }
    }
}
