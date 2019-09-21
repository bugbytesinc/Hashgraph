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
    public class ExchangeRate
    {
        /// <summary>
        /// The HBar portion of the exchange rate, can be
        /// used in the numerator to get hbars per cent or
        /// in the denominator to get cents per hbar.
        /// </summary>
        public int HBarEquivalent { get; set; }
        /// <summary>
        /// The USD cent portion of the exchange rate, can be
        /// used in the numerator to get cents per hbar or
        /// in the denominator to get hbars per cent.
        /// </summary>
        public int USDCentEquivalent { get; set; }
        /// <summary>
        /// The date and time at which this exchange 
        /// rate value is set to expire.
        /// </summary>
        public DateTime Expiration { get; set; }
    }
}
