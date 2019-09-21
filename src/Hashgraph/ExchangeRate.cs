using System;

namespace Hashgraph
{
    /// <summary>
    /// Exchange rate information as known by the 
    /// hedera network.  Values returned in receipts.
    /// </summary>
    public class ExchangeRate
    {
        /// <summary>
        /// Value which denote habar equivalent to USD Cent
        /// </summary>
        public int HBarPerUSDCent { get; set; }
        /// <summary>
        /// Value which denote USD Cents equivalent to Hbar
        /// </summary>
        public int USDCentPerHbar { get; set; }
        /// <summary>
        /// The date and time at which this exchange 
        /// rate value is set to expire.
        /// </summary>
        public DateTime Expiration { get; set; }
    }
}
