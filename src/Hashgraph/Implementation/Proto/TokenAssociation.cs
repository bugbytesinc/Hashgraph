using Google.Protobuf.Collections;
using Hashgraph;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Proto;

internal static class TokenAssociationExtensions
{
    private static ReadOnlyCollection<Association> EMPTY_RESULT = new List<Association>().AsReadOnly();
    internal static ReadOnlyCollection<Association> AsAssociationList(this RepeatedField<TokenAssociation> list)
    {
        if (list != null && list.Count > 0)
        {
            return list.Select(record => new Association(record)).ToList().AsReadOnly();
        }
        return EMPTY_RESULT;
    }
}