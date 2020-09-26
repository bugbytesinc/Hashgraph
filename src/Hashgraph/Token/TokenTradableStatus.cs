namespace Hashgraph
{
    /// <summary>
    /// The Frozen Status of a Token
    /// </summary>
    public enum TokenTradableStatus
    {
        /// <summary>
        /// Frozen status does not apply to this token.
        /// </summary>
        NotApplicable = 0,
        /// <summary>
        /// Token is a suspended/frozen state.
        /// </summary>
        Suspended = 1,
        /// <summary>
        /// Token is tradable/not in a frozen state.
        /// </summary>
        Tradable = 2
    }
}
