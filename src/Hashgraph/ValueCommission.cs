using Proto;

namespace Hashgraph
{
    /// <summary>
    /// Represents a commission on the toltal value of
    /// fungible token exchange or hBar exhange in payment
    /// for a Asset or Token.
    /// </summary>
    public sealed record ValueCommission : ICommission
    {
        /// <summary>
        /// A Royalty Commission Fee based on overall Value Transferred
        /// </summary>
        public CommissionType CommissionType => CommissionType.Value;
        /// <summary>
        /// The account receiving the commision fee.
        /// </summary>
        public Address Account { get; private init; }
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
        /// The fixed amount of token or cryptocurrency
        /// that will be assessed from the account
        /// receiving the associated token(s) if the 
        /// transaction transfering the token provides
        /// no other discernable exchange of value in
        /// payment.
        /// </summary>
        /// <remarks>Set to <code>0</code> if no
        /// fallback amount is required.</remarks>
        public long FallbackAmount { get; private init; }
        /// <summary>
        /// The address id of the token type used to pay
        /// the commission if no other transfer value exists
        /// in payment for the tranfer of the associated token
        /// or asset, if set to <code>None</code> then
        /// native hBar crypto is assumed.
        /// </summary>
        public Address FallbackToken { get; private init; }
        /// <summary>
        /// Internal Constructor representing the "None" version of a commission.
        /// </summary>
        private ValueCommission()
        {
            Account = Address.None;
            Numerator = 0;
            Denominator = 0;
            FallbackToken = Address.None;
            FallbackAmount = 0;
        }
        /// <summary>
        /// Public Constructor, an <code>ValueCommission</code> is immutable after creation.
        /// </summary>
        /// <param name="account">
        /// The account receiving the commision fee.
        /// </param>
        /// <param name="numerator">
        /// The numerator portion of the fraction of the 
        /// transferred units to assess as a fee.
        /// </param>
        /// <param name="denominator">
        /// The denominator portion of the fraction of the 
        /// transferred units to assess as a fee.
        /// </param>
        /// <param name="fallbackAmount">
        /// The fixed amount of token or cryptocurrency
        /// that will be assessed from the account
        /// receiving the associated token(s) if the 
        /// transaction transfering the token provides
        /// no other discernable exchange of value in
        /// payment.
        /// </param>
        /// <param name="fallbackToken">
        /// The address id of the token type used to pay
        /// the commission if no other transfer value exists
        /// in payment for the tranfer of the associated token
        /// or asset, if set to <code>None</code> then
        /// native hBar crypto is assumed.
        /// </param>
        public ValueCommission(Address account, long numerator, long denominator, long fallbackAmount, Address fallbackToken)
        {
            Account = account;
            Numerator = numerator;
            Denominator = denominator;
            FallbackAmount = fallbackAmount;
            FallbackToken = fallbackToken;
        }

        internal ValueCommission(CustomFee fee)
        {
            Account = fee.FeeCollectorAccountId.AsAddress();
            var royalty = fee.RoyaltyFee;
            Numerator = royalty.ExchangeValueFraction.Numerator;
            Denominator = royalty.ExchangeValueFraction.Denominator;
            var fallback = royalty.FallbackFee;
            if (fallback is null)
            {
                FallbackToken = Address.None;
                FallbackAmount = 0;
            }
            else
            {
                FallbackAmount = fallback.Amount;
                FallbackToken = fallback.DenominatingTokenId.AsAddress();
            }
        }
    }
}
