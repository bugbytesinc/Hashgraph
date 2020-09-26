using System;
using System.Diagnostics.CodeAnalysis;

namespace Hashgraph
{
    /// <summary>
    /// Identifies a Token instance, this can be via the 
    /// <code>Address</code> identifying the token, or it
    /// can be the <code>Symbol</code>.  In general the
    /// <code>Address</code> is preferred over the symbol
    /// since the symbol can change over time for mutable 
    /// token definitions, the Address will not.
    /// </summary>
    /// <remarks>
    /// This class is imutable once created, but can be
    /// cast to an Address or string for interoperablity
    /// convenience.
    /// </remarks>
    public sealed class TokenIdentifier : IEquatable<TokenIdentifier>
    {
        /// <summary>
        /// Address identifier (0.0.x) of the token.  This is
        /// the preferred way to identify the token instance.
        /// </summary>
        public Address Address { get; private set; } = Address.None;
        /// <summary>
        /// Common Symbol name of the token.  This can be used
        /// to identify a token if the <code>Address</code> is not
        /// known.
        /// </summary>
        public string Symbol { get; private set; } = string.Empty;
        /// <summary>
        /// Create a token reference using its known address.
        /// </summary>
        /// <param name="address">
        /// Address of the token instance (0.0.x).
        /// </param>
        public TokenIdentifier(Address address)
        {
            Address = address;
        }
        /// <summary>
        /// Create a token reference using the tokens public
        /// symbol name.
        /// </summary>
        /// <param name="symbol">
        /// Symbolic Name of the token.
        /// </param>
        public TokenIdentifier(string symbol)
        {
            Symbol = symbol;
        }
        /// <summary>
        /// Convenience operator for creating the token
        /// identifier using the network shard, realm and number.
        /// </summary>
        /// <param name="shardNum">
        /// Token's Network Shard Number
        /// </param>
        /// <param name="realmNum">
        /// Token's Network Realm Number
        /// </param>
        /// <param name="accountNum">
        /// Token's Network Account Number
        /// </param>
        public TokenIdentifier(long shardNum, long realmNum, long accountNum)
        {
            Address = new Address(shardNum, realmNum, accountNum);
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>TokenIdentifier</code> object to compare.
        /// </param>
        /// <returns>
        /// <code>True</code> if both the Address and Symbol match.
        /// Typically an identifier has one or the other, so this 
        /// comparison WILL NOT MATCH an identifier that has the 
        /// address of a token with an identifier that has the symbol 
        /// for the token.
        /// </returns>
        public bool Equals([AllowNull] TokenIdentifier other)
        {
            if (other is null)
            {
                return false;
            }
            if (Address != other.Address || Symbol != other.Symbol)
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="obj">
        /// The other <code>TokenIdentifier</code> object to compare.
        /// </param>
        /// <returns>
        /// <code>True</code> if both the Address and Symbol match.
        /// Typically an identifier has one or the other, so this 
        /// comparison WILL NOT MATCH an identifier that has the 
        /// address of a token with an identifier that has the symbol 
        /// for the token.
        /// </returns>
        public override bool Equals([AllowNull] object obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is TokenIdentifier other)
            {
                return Equals(other);
            }
            return false;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <returns>
        /// A unique hash of the contents of this <code>TokenIdentifier</code> 
        /// object.  Only consistent within the current instance of 
        /// the application process.
        /// </returns>
        public override int GetHashCode()
        {
            return $"Address:{Address.GetHashCode()}:{Symbol}".GetHashCode();
        }
        /// <summary>
        /// Equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>TokenIdentifier</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>TokenIdentifier</code> argument.
        /// </param>
        /// <returns>
        /// True if Key requirements are identical 
        /// within each <code>TokenIdentifier</code> objects.
        /// </returns>
        public static bool operator ==(TokenIdentifier left, TokenIdentifier right)
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
        /// Left hand <code>TokenIdentifier</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>TokenIdentifier</code> argument.
        /// </param>
        /// <returns>
        /// <code>False</code> if the Key requirements are identical 
        /// within each <code>TokenIdentifier</code> object.  
        /// <code>True</code> if they are not identical.
        /// </returns>
        public static bool operator !=(TokenIdentifier left, TokenIdentifier right)
        {
            return !(left == right);
        }
        /// <summary>
        /// Convenience implicit cast for createing a <code>TokenIdentifier</code>
        /// directly from an <see cref="Address"/>.  Addresses may be used as
        /// method arguments where a <code>TokenIdentifier</code> is expected.
        /// </summary>
        /// <param name="token">
        /// Address of the token instance (0.0.x).
        /// </param>
        public static implicit operator TokenIdentifier(Address token)
        {
            return new TokenIdentifier(token);
        }
        /// <summary>
        /// Convenience implicit cast for creating a <code>TokenIdentifier</code>
        /// directly from a Token Symbol Name.  String values may be used
        /// as method arguments where a <code>TokenIdentifier</code> is epxected.
        /// </summary>
        /// <param name="symbol">
        /// Symbolic Name of the token.
        /// </param>
        public static implicit operator TokenIdentifier(string symbol)
        {
            return new TokenIdentifier(symbol);
        }
    }
}
