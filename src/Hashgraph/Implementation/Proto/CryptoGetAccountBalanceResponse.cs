using Google.Protobuf.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Proto
{
    public sealed partial class CryptoGetAccountBalanceResponse
    {
        internal Hashgraph.AccountBalances ToAccountBalances()
        {
            return new Hashgraph.AccountBalances
            {
                Address = AccountID.AsAddress(),
                Crypto = Balance,
                Tokens = FromTokenBalances(TokenBalances)
            };
        }

        private static ReadOnlyDictionary<Hashgraph.Address, Hashgraph.CryptoBalance> FromTokenBalances(RepeatedField<TokenBalance> tokenBalances)
        {
            var results = new Dictionary<Hashgraph.Address, Hashgraph.CryptoBalance>();
            foreach (var entry in tokenBalances)
            {
                var account = entry.TokenId.AsAddress();
                if (results.TryGetValue(account, out Hashgraph.CryptoBalance? crypto))
                {
                    results[account] = crypto with
                    {
                        Balance = crypto.Balance + entry.Balance
                    };
                }
                else
                {
                    results[account] = new Hashgraph.CryptoBalance
                    {
                        Balance = entry.Balance,
                        Decimals = entry.Decimals
                    };
                }
            }
            return new ReadOnlyDictionary<Hashgraph.Address, Hashgraph.CryptoBalance>(results);
        }
    }
}
