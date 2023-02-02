using Google.Protobuf;
using Hashgraph;

namespace Proto;

internal static class EvmAddressExtensions
{
    internal static Moniker AsMoniker(this ByteString? bytes, Address? shardRealmAddress = null)
    {
        if (shardRealmAddress == null)
        {
            shardRealmAddress = Address.None;
        }
        if (bytes?.Length == 20)
        {
            return new Moniker(shardRealmAddress.ShardNum, shardRealmAddress.RealmNum, bytes.Memory);
        }
        return Moniker.None;
    }
}