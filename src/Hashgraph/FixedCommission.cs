using Proto;

namespace Hashgraph
{
    /// <summary>
    /// Represents a fixed commission associated with transfers of a token or asset.
    /// </summary>
    public sealed record FixedCommission : ICommission
    {
        /// <summary>
        /// A Fixed Commission Fee
        /// </summary>
        public CommissionType CommissionType => CommissionType.Fixed;
        /// <summary>
        /// The account receiving the commision fee.
        /// </summary>
        public Address Account { get; private init; }
        /// <summary>
        /// The address id of the token type used to pay
        /// the commission, if <code>None</code> then
        /// native hBar crypto is assumed.
        /// </summary>
        public Address Token { get; private init; }
        /// <summary>
        /// The amount of token or cryptocurrency
        /// that will be assessed from the sending
        /// account.
        /// </summary>
        public long Amount { get; private init; }
        /// <summary>
        /// Internal Constructor representing the "None" version of a commission.
        /// </summary>
        private FixedCommission()
        {
            Account = Address.None;
            Token = Address.None;
            Amount = 0;
        }
        /// <summary>
        /// Public Constructor, an <code>FixedCommission</code> is immutable after creation.
        /// </summary>
        /// <param name="account">
        /// The account receiving the commision fee.
        /// </param>
        /// <param name="token">
        /// The address id of the token type used to pay
        /// the commission, if <code>None</code> then
        /// native hBar crypto is assumed.
        /// </param>
        /// <param name="amount">
        /// The amount of token or cryptocurrency
        /// that will be assessed from the sending
        /// account.
        /// </param>
        public FixedCommission(Address account, Address token, long amount)
        {
            Account = account;
            Token = token;
            Amount = amount;
        }
        internal FixedCommission(CustomFee fee)
        {
            Account = fee.FeeCollectorAccountId.AsAddress();
            Token = fee.FixedFee.DenominatingTokenId.AsAddress();
            Amount = fee.FixedFee.Amount;
        }
    }
}
