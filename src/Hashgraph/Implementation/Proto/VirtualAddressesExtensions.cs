using Google.Protobuf.Collections;
using Hashgraph;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Proto;

public static class VirtualAddressesExtensions
{
    private static readonly ReadOnlyDictionary<Moniker, bool> EMPTY_RESULT = new(new Dictionary<Moniker, bool>());
    internal static ReadOnlyDictionary<Moniker, bool> ToMonikers(this RepeatedField<VirtualAddress> list, Address shardRealmAddress)
    {
        if (list != null && list.Count > 0)
        {
            return new(list.ToDictionary(record => new Moniker(shardRealmAddress.ShardNum, shardRealmAddress.RealmNum, record.Address.Memory), record => record.IsDefault));
        }
        return EMPTY_RESULT;
    }
}