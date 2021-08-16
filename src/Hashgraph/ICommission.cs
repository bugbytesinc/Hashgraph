namespace Hashgraph
{
    /// <summary>
    /// Represents a commission (fee or royalty) associated 
    /// with transfers of a token or asset.
    /// </summary>
    public interface ICommission
    {
        /// <summary>
        /// The account receiving the commision fee.
        /// </summary>
        public Address Account { get; }
        /// <summary>
        /// The type of commission this object represents,
        /// will match the concrete implementation type.
        /// </summary>
        public CommissionType CommissionType { get; }
    }
}
