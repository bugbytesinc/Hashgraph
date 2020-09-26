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
                Address = AccountID.ToAddress(),
                Crypto = Balance,
                Tokens = FromTokenBalances(TokenBalances)
            };
        }

        private static ReadOnlyDictionary<Hashgraph.Address, ulong> FromTokenBalances(RepeatedField<TokenBalance> tokenBalances)
        {
            var results = new Dictionary<Hashgraph.Address, ulong>();
            foreach (var entry in tokenBalances)
            {
                var account = entry.TokenId.ToAddress();
                results.TryGetValue(account, out ulong amount);
                results[account] = amount + entry.Balance;
            }
            return new ReadOnlyDictionary<Hashgraph.Address, ulong>(results);
        }
    }
}
