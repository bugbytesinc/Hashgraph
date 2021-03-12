#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System.Collections.ObjectModel;

namespace Hashgraph
{
    /// <summary>
    /// Extended Balance information for an Account, including Token Balances.
    /// </summary>
    public sealed record AccountBalances
    {
        /// <summary>
        /// The Hedera address of this account.
        /// </summary>
        public Address Address { get; internal init; }
        /// <summary>
        /// Account Crypto Balance in Tinybars
        /// </summary>
        public ulong Crypto { get; internal init; }
        /// <summary>
        /// Balances of tokens associated with this account.
        /// </summary>
        public ReadOnlyDictionary<Address, CryptoBalance> Tokens { get; internal init; }
    }
}
