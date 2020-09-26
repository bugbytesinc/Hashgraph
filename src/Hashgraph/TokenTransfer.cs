using System;

namespace Hashgraph
{
    /// <summary>
    /// Represents a token transfer (Token (Address/Symbol), Account, Amount)
    /// </summary>
    public sealed class TokenTransfer : IEquatable<TokenTransfer>
    {
        /// <summary>
        /// The Token (Address/Symbol) of the Token who's coins have transferred.
        /// </summary>
        public TokenIdentifier Token { get; private set; }
        /// <summary>
        /// The Address receiving or sending the token's coins.
        /// </summary>
        public Address Address { get; private set; }
        /// <summary>
        /// The (divisible) amount of coins transferred.  Negative values
        /// indicate an outflow of coins to the <code>Account</code> positive
        /// values indicate an inflow of coins from the associated <code>Account</code>.
        /// </summary>
        public long Amount { get; private set; }
        /// <summary>
        /// Internal Constructor representing the "None" version of an
        /// version.  This is a special construct indicating the version
        /// number is not known or is not specified.
        /// </summary>
        private TokenTransfer()
        {
            Token = Address.None;
            Address = Address.None;
            Amount = 0;
        }
        /// <summary>
        /// Public Constructor, an <code>TokenIdentifierTransfer</code> is immutable after creation.
        /// </summary>
        /// <param name="token">
        /// The Token (Address/Symbol) of the Token who's coins have transferred.
        /// </param>
        /// <param name="address">
        /// The Address receiving or sending the token's coins.
        /// </param>
        /// <param name="amount">
        /// The (divisible) amount of coins transferred.  Negative values
        /// indicate an outflow of coins to the <code>Account</code> positive
        /// values indicate an inflow of coins from the associated <code>Account</code>.
        /// </param>
        public TokenTransfer(TokenIdentifier token, Address address, long amount)
        {
            Token = token;
            Address = address;
            Amount = amount;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>TokenIdentifierTransfer</code> object to compare.
        /// </param>
        /// <returns>
        /// True if the Token, Address and Amount are identical to the 
        /// other <code>TokenIdentifierTransfer</code> object.
        /// </returns>
        public bool Equals(TokenTransfer? other)
        {
            if (other is null)
            {
                return false;
            }
            return
                Token == other.Token &&
                Address == other.Address &&
                Amount == other.Amount;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="obj">
        /// The other <code>TokenIdentifierTransfer</code> object to compare (if it is
        /// an <code>TokenIdentifierTransfer</code>).
        /// </param>
        /// <returns>
        /// If the other object is an TokenIdentifierTransfer, then <code>True</code> 
        /// if the Token, Address and Amount are identical 
        /// to the other <code>TokenIdentifierTransfer</code> object, otherwise 
        /// <code>False</code>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj is TokenTransfer other)
            {
                return Equals(other);
            }
            return false;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <returns>
        /// A unique hash of the contents of this <code>TokenIdentifierTransfer</code> 
        /// object.  Only consistent within the current instance of 
        /// the application process.
        /// </returns>
        public override int GetHashCode()
        {
            return $"TokenIdentifierTransfer:{Token.GetHashCode()}:{Address.GetHashCode()}:{Amount}".GetHashCode();
        }
        /// <summary>
        /// Equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>TokenIdentifierTransfer</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>TokenIdentifierTransfer</code> argument.
        /// </param>
        /// <returns>
        /// True if the Token, Address and Amount are identical
        /// within each <code>TokenIdentifierTransfer</code> objects.
        /// </returns>
        public static bool operator ==(TokenTransfer left, TokenTransfer right)
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
        /// Left hand <code>TokenIdentifierTransfer</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>TokenIdentifierTransfer</code> argument.
        /// </param>
        /// <returns>
        /// <code>False</code> if Token, Address and Amount 
        /// are identical within each <code>TokenIdentifierTransfer</code> object.  
        /// <code>True</code> if they are not identical.
        /// </returns>
        public static bool operator !=(TokenTransfer left, TokenTransfer right)
        {
            return !(left == right);
        }
        /// <summary>
        /// Creates a new TokenIdentifierTransfer object representing a transfer
        /// of the sum of the coins identified by the transfer plus the
        /// additional amount.  This creates a new <code>TokenIdentifierTransfer</code>
        /// object.
        /// </summary>
        /// <param name="amount">The amount to add (or subtract if negative)
        /// in the divisible amount of coins.</param>
        /// <returns></returns>
        internal TokenTransfer Add(long amount)
        {
            return new TokenTransfer(Token, Address, Amount + amount);
        }
    }
}
