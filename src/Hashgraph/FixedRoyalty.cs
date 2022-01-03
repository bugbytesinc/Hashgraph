using Proto;

namespace Hashgraph
{
    /// <summary>
    /// The definition of a single Fixed Royalty applied to 
    /// the transaction as a whole when transferring an asset or token.
    /// </summary>
    public sealed record FixedRoyalty : IRoyalty
    {
        /// <summary>
        /// Identifies this royalty as a Fixed Royalty type.
        /// </summary>
        public RoyaltyType RoyaltyType => RoyaltyType.Fixed;
        /// <summary>
        /// Account receiving the royalty assessment.
        /// </summary>
        public AddressOrAlias Account { get; private init; }
        /// <summary>
        /// The address id of the token type used to pay
        /// the royalty, if <code>None</code> then
        /// native hBar crypto is assumed.
        /// </summary>
        public Address Token { get; private init; }
        /// <summary>
        /// The amount of token or cryptocurrency
        /// that will be assessed and deducted from
        /// the account sending the associated token
        /// or asset.
        /// </summary>
        public long Amount { get; private init; }
        /// <summary>
        /// Internal Constructor representing the "None" version of a 
        /// fixed royalty.
        /// </summary>
        private FixedRoyalty()
        {
            Account = Address.None;
            Token = Address.None;
            Amount = 0;
        }
        /// <summary>
        /// Public Constructor, an <code>FixedRoyalty</code> is immutable after creation.
        /// </summary>
        /// <param name="account">
        /// Account receiving the royalty assessment.
        /// </param>
        /// <param name="token">
        /// The address id of the token type used to pay
        /// the royalty, if <code>None</code> then
        /// native hBar crypto is assumed.
        /// </param>
        /// <param name="amount">
        /// The amount of token or cryptocurrency
        /// that will be assessed and deducted from
        /// the account sending the associated token
        /// or asset.
        /// </param>
        public FixedRoyalty(AddressOrAlias account, Address token, long amount)
        {
            Account = account;
            Token = token;
            Amount = amount;
        }
        /// <summary>
        /// Internal Helper Constructor converting raw protobuf 
        /// into this royalty definition.
        /// </summary>
        internal FixedRoyalty(CustomFee fee)
        {
            Account = fee.FeeCollectorAccountId.AsAddress();
            Token = fee.FixedFee.DenominatingTokenId.AsAddress();
            Amount = fee.FixedFee.Amount;
        }
    }
}
