using System;

namespace Hashgraph
{
    /// <summary>
    /// Exchange rate information as known by the 
    /// hedera network.  Values returned in receipts.
    /// denominator.
    /// </summary>
    /// <remarks>
    /// The rate is expressed as parts of a numerator 
    /// and denominator expressing the ratio of hbars
    /// to cents. For example to get the value of 
    /// $cent/hbar one would compute that as 
    /// <code>USDCentEquivalent/HBarEquivalent</code>.
    /// to get hbar/$cent one would compute that as
    /// <code>HbarEquivalent/USDEquivalent</code>
    /// This representation allows for fractions that might
    /// otherwise be lost by floating point representations.
    /// </remarks>
    public class ExchangeRate : IEquatable<ExchangeRate>
    {
        /// <summary>
        /// The HBar portion of the exchange rate, can be
        /// used in the numerator to get hbars per cent or
        /// in the denominator to get cents per hbar.
        /// </summary>
        public int HBarEquivalent { get; internal set; }
        /// <summary>
        /// The USD cent portion of the exchange rate, can be
        /// used in the numerator to get cents per hbar or
        /// in the denominator to get hbars per cent.
        /// </summary>
        public int USDCentEquivalent { get; internal set; }
        /// <summary>
        /// The date and time at which this exchange 
        /// rate value is set to expire.
        /// </summary>
        public DateTime Expiration { get; internal set; }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="other">
        /// The other <code>ExchangeRate</code> object to compare.
        /// </param>
        /// <returns>
        /// True if the equivalents and expiration are identical to the 
        /// other <code>ExchangeRate</code> object.
        /// </returns>
        public bool Equals(ExchangeRate other)
        {
            if (other is null)
            {
                return false;
            }
            return
                USDCentEquivalent == other.USDCentEquivalent &&
                HBarEquivalent == other.HBarEquivalent &&
                Expiration == other.Expiration;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <param name="obj">
        /// The other <code>ExchangeRate</code> object to compare (if it is
        /// an <code>ExchangeRate</code>).
        /// </param>
        /// <returns>
        /// If the other object is an ExchangeRate, then <code>True</code> 
        /// if the equivalents and expiration are identical 
        /// to the other <code>ExchangeRate</code> object, otherwise 
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
            if (obj is ExchangeRate other)
            {
                return Equals(other);
            }
            return false;
        }
        /// <summary>
        /// Equality implementation.
        /// </summary>
        /// <returns>
        /// A unique hash of the contents of this <code>ExchangeRate</code> 
        /// object.  Only consistent within the current instance of 
        /// the application process.
        /// </returns>
        public override int GetHashCode()
        {
            return $"ExchangeRate:{HBarEquivalent}:{USDCentEquivalent}:{Expiration.Ticks}".GetHashCode();
        }
        /// <summary>
        /// Equals implementation.
        /// </summary>
        /// <param name="left">
        /// Left hand <code>ExchangeRate</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>ExchangeRate</code> argument.
        /// </param>
        /// <returns>
        /// True if the equivalents and expiration are identical 
        /// within each <code>ExchangeRate</code> object.
        /// </returns>
        public static bool operator ==(ExchangeRate left, ExchangeRate right)
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
        /// Left hand <code>ExchangeRate</code> argument.
        /// </param>
        /// <param name="right">
        /// Right hand <code>ExchangeRate</code> argument.
        /// </param>
        /// <returns>
        /// <code>False</code> if the equivalents and expiration are the
        /// same for each <code>ExchangeRate</code> object.  
        /// <code>True</code> if they are not identical.
        /// </returns>
        public static bool operator !=(ExchangeRate left, ExchangeRate right)
        {
            return !(left == right);
        }
    }
}
