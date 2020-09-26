using Google.Protobuf.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Proto
{
    public static class TokenRelationshipExtensions
    {
        internal static ReadOnlyCollection<Hashgraph.TokenBalance> ToBalances(this RepeatedField<TokenRelationship> list)
        {
            var result = new List<Hashgraph.TokenBalance>(list.Count);
            if (list != null && list.Count > 0)
            {
                foreach (var entry in list)
                {
                    result.Add(new Hashgraph.TokenBalance
                    {
                        Token = entry.TokenId.ToAddress(),
                        Symbol = entry.Symbol,
                        Balance = entry.Balance,
                        KycStatus = (Hashgraph.TokenKycStatus)entry.KycStatus,
                        TradableStatus = (Hashgraph.TokenTradableStatus)entry.FreezeStatus
                    });
                }
            }
            return result.AsReadOnly();
        }
    }
}
