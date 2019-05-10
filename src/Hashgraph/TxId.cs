﻿using Hashgraph.Implementation;
using System;

namespace Hashgraph
{
    /// <summary>
    /// Represents the transaction id associated with a network request.  
    /// This is generated by the library automatically for each request.  
    /// It does not presently expose the detailed fields associated with 
    /// the protobuf equivalent, but does implement the equitable 
    /// interface and can be compared to other transaction ids returned 
    /// from the library.
    /// </summary>
    public class TxId : IData, IEquatable<TxId>
    {
        /// <summary>
        /// Data storing the internal representation of the transaction id.  
        /// Even if the library exposed the timestamp and account info, 
        /// the resolution of the ID is higher than the native time 
        /// primitives in .net so it would still be necessary to use 
        /// this data for equitable comparisons.
        /// </summary>
        private readonly ReadOnlyMemory<byte> _data;
        /// <summary>
        /// Internal interface used to store protobuf encoded data for 
        /// converting library object to protobuf objects and reverse.
        /// </summary>
        ReadOnlyMemory<byte> IData.Data { get { return _data; } }
        /// <summary>
        /// Internal constructor, only the library can create this object.
        /// </summary>
        /// <param name="data">
        /// The protobuf serialized bytes representing the transaction id.
        /// </param>
        internal TxId(ReadOnlyMemory<byte> data)
        {
            _data = data;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>TxId</code> object to compare.
        /// </param>
        /// <returns>
        /// True if the id is identical to the 
        /// other <code>TxId</code> object.
        /// </returns>
        public bool Equals(TxId other)
        {
            if (other is null)
            {
                return false;
            }
            return _data.Equals(other._data);
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="obj">
        /// The other <code>TxId</code> object to compare (if it is
        /// an <code>TxId</code>).
        /// </param>
        /// <returns>
        /// If the other object is an TxId, then <code>True</code> 
        /// if id is identical to the other <code>TxId</code> object, 
        /// otherwise <code>False</code>.
        /// </returns>
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
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <returns>
        /// A unique hash of the contents of this <code>TxId</code> 
        /// object.  Only consistent within the current instance of 
        /// the application process.
        /// </returns>
        public override int GetHashCode()
        {
            return BitConverter.ToString(_data.ToArray()).GetHashCode();
        }
        /// <summary>
        /// Equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>TxId</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>TxId</code> argument.
        /// </param>
        /// <returns>
        /// <code>True</code> id is identical within each <code>TxId</code> objects.
        /// </returns>
        public static bool operator ==(TxId left, TxId right)
        {
            if (left is null)
            {
                return right is null;
            }
            return left.Equals(right);
        }
        /// <summary>
        /// Not equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>TxId</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>TxId</code> argument.
        /// </param>
        /// <returns>
        /// <code>False</code> if the id is identical within 
        /// each <code>TxId</code> object.  <code>True</code> 
        /// if they are not identical.
        /// </returns>
        public static bool operator !=(TxId left, TxId right)
        {
            return !(left == right);
        }
    }
}