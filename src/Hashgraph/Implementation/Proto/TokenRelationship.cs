using Google.Protobuf.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Proto;

public static class TokenRelationshipExtensions
{
    private static ReadOnlyCollection<Hashgraph.TokenBalance> EMPTY_RESULT = new List<Hashgraph.TokenBalance>().AsReadOnly();
    internal static ReadOnlyCollection<Hashgraph.TokenBalance> ToBalances(this RepeatedField<TokenRelationship> list)
    {
        if (list != null && list.Count > 0)
        {
            return list.Select(record => new Hashgraph.TokenBalance(record)).ToList().AsReadOnly();
        }
        return EMPTY_RESULT;
    }
}