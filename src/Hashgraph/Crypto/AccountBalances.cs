using Proto;
using System.Collections.Generic;
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
        public Address Address { get; private init; }
        /// <summary>
        /// Account Crypto Balance in Tinybars
        /// </summary>
        public ulong Crypto { get; private init; }
        /// <summary>
        /// Balances of tokens associated with this account.
        /// </summary>
        public ReadOnlyDictionary<Address, CryptoBalance> Tokens { get; private init; }
        /// <summary>
        /// Internal Constructor from Raw Response
        /// </summary>
        internal AccountBalances(Response response)
        {
            var balances = response.CryptogetAccountBalance;
            Address = balances.AccountID.AsAddress();
            Crypto = balances.Balance;
            var tokens = new Dictionary<Address, CryptoBalance>();
            foreach (var entry in balances.TokenBalances)
            {
                var account = entry.TokenId.AsAddress();
                if (tokens.TryGetValue(account, out CryptoBalance? crypto))
                {
                    tokens[account] = crypto with
                    {
                        Balance = crypto.Balance + entry.Balance
                    };
                }
                else
                {
                    tokens[account] = new CryptoBalance
                    {
                        Balance = entry.Balance,
                        Decimals = entry.Decimals
                    };
                }
            }
            Tokens = new ReadOnlyDictionary<Address, CryptoBalance>(tokens);
        }
    }
}
