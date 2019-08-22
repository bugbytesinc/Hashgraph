#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// Claim Details
    /// </summary>
    public sealed class Claim : IEquatable<Claim>
    {
        /// <summary>
        /// The account to which the claim is attached
        /// </summary>
        public Address Address { get; set; }
        /// <summary>
        /// A 48 byte SHA-384 hash, typically of the claim
        /// content itself, such as a credential or certificate
        /// </summary>
        public ReadOnlyMemory<byte> Hash { get; set; }
        /// <summary>
        /// The list of endorsments that must sign the transaction
        /// adding the claim to the address. Only one of the
        /// endorsements is required to remove the claim.
        /// </summary>
        public Endorsement[] Endorsements { get; set; }
        /// <summary>
        /// The duration of time which the claim will remain
        /// attached to the account.
        /// </summary>
        public TimeSpan ClaimDuration { get; set; }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>Claim</code> object to compare.
        /// </param>
        /// <returns>
        /// True if public key layout and requirements are equivalent to the 
        /// other <code>Claim</code> object.
        /// </returns>
        public bool Equals(Claim other)
        {
            if (other is null)
            {
                return false;
            }
            return
                Address == other.Address &&
                Hash.Equals(other.Hash) &&
                Endorsements == other.Endorsements &&
                ClaimDuration.Equals(other.ClaimDuration);
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="obj">
        /// The other <code>Claim</code> object to compare (if it is
        /// an <code>Claim</code>).
        /// </param>
        /// <returns>
        /// If the other object is a Claim, then <code>True</code> 
        /// if key requirements are identical to the other 
        /// <code>Claim</code> object, otherwise 
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
            if (obj is Claim other)
            {
                return Equals(other);
            }
            return false;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <returns>
        /// A unique hash of the contents of this <code>Claim</code> 
        /// object.  Only consistent within the current instance of 
        /// the application process.
        /// </returns>
        public override int GetHashCode()
        {
            return $"Claim:{Address?.GetHashCode()}:{Hex.FromBytes(Hash)}:{Endorsements.GetHashCode()}:{ClaimDuration}".GetHashCode();
        }
        /// <summary>
        /// Equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>Claim</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Claim</code> argument.
        /// </param>
        /// <returns>
        /// True if claims are identical 
        /// within each <code>Claim</code> objects.
        /// </returns>
        public static bool operator ==(Claim left, Claim right)
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
        /// Left hand <code>Claim</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>Claim</code> argument.
        /// </param>
        /// <returns>
        /// <code>False</code> if the claims are identical 
        /// within each <code>Claim</code> object.  
        /// <code>True</code> if they are not identical.
        /// </returns>
        public static bool operator !=(Claim left, Claim right)
        {
            return !(left == right);
        }
    }
}