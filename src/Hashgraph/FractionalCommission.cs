using Proto;

namespace Hashgraph
{
    /// <summary>
    /// Represents a Fractional Commission associated with transfers of a token or asset.
    /// </summary>
    public sealed record FractionalCommission : ICommission
    {
        /// <summary>
        /// A Porpotional Commission Fee based on the amount of Token Transferred.
        /// </summary>
        public CommissionType CommissionType => CommissionType.Fractional;
        /// <summary>
        /// The account receiving the commision fee.
        /// </summary>
        public Address Account { get; private init; }
        /// <summary>
        /// The minimum fee value, in terms of the 
        /// smallest denomination of the payment token.
        /// </summary>
        public long Minimum { get; private init; }
        /// <summary>
        /// The numerator portion of the fraction of the 
        /// transferred units to assess as a fee.
        /// </summary>
        /// <remarks>
        /// This is not expressed as a floating point number
        /// in order to avoid rounding fees inheret in 
        /// computing platforms.
        /// </remarks>
        public long Numerator { get; private init; }
        /// <summary>
        /// The denominator portion of the fraction of the 
        /// transferred units to assess as a fee.
        /// </summary>
        /// <remarks>
        /// This is not expressed as a floating point number
        /// in order to avoid rounding fees inheret in 
        /// computing platforms.
        /// </remarks>
        public long Denominator { get; private init; }
        /// <summary>
        /// The maximum allowed fee value, in terms of
        /// the smallest denomination of the payment token.
        /// </summary>
        public long Maximum { get; private init; }
        /// <summary>
        /// Internal Constructor representing the "None" version of a commission.
        /// </summary>
        private FractionalCommission()
        {
            Account = Address.None;
            Numerator = 0;
            Denominator = 0;
            Minimum = 0;
            Maximum = 0;
        }
        /// <summary>
        /// Public Constructor, an <code>FractionalCommission</code> is immutable after creation.
        /// </summary>
        /// <param name="account">
        /// The account receiving the commision fee.
        /// </param>
        /// <param name="token">
        /// The address id of the token type used to pay
        /// the commission, if <code>None</code> then
        /// native hBar crypto is assumed.
        /// </param>
        /// <param name="numerator">
        /// The numerator portion of the fraction of the 
        /// transferred units to assess as a fee.
        /// </param>
        /// <param name="denominator">
        /// The denominator portion of the fraction of the 
        /// transferred units to assess as a fee.
        /// </param>
        /// <param name="minimum">
        /// The minimum fee value, in terms of the 
        /// smallest denomination of the payment token.
        /// </param>
        /// <param name="maximum">
        /// The maximum allowed fee value, in terms of
        /// the smallest denomination of the payment token.
        /// </param>
        public FractionalCommission(Address account, long numerator, long denominator, long minimum, long maximum)
        {
            Account = account;
            Numerator = numerator;
            Denominator = denominator;
            Minimum = minimum;
            Maximum = maximum;
        }

        internal FractionalCommission(CustomFee fee)
        {
            Account = fee.FeeCollectorAccountId.AsAddress();
            var fraction = fee.FractionalFee;
            Numerator = fraction.FractionalAmount.Numerator;
            Denominator = fraction.FractionalAmount.Denominator;
            Minimum = fraction.MinimumAmount;
            Maximum = fraction.MaximumAmount;
        }
    }
}
