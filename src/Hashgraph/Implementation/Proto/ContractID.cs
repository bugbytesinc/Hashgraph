using Google.Protobuf;
using Hashgraph;
using System;

namespace Proto;

public sealed partial class ContractID
{
    internal ContractID(Address contract) : this()
    {
        if (contract is null)
        {
            throw new ArgumentNullException(nameof(contract), "Contract Address is missing. Please check that it is not null.");
        }
        if (contract.TryGetMoniker(out var moniker))
        {
            ShardNum = moniker.ShardNum;
            RealmNum = moniker.RealmNum;
            EvmAddress = ByteString.CopyFrom(moniker.Bytes.Span);
        }
        else
        {
            ShardNum = contract.ShardNum;
            RealmNum = contract.RealmNum;
            ContractNum = contract.AccountNum;
        }
    }
}

internal static class ContractIDExtensions
{
    internal static Address AsAddress(this ContractID? id)
    {
        if (id is null)
        {
            return Address.None;
        }
        if (id.ContractCase == ContractID.ContractOneofCase.EvmAddress)
        {
            return new Address(new Moniker(id.ShardNum, id.RealmNum, id.EvmAddress.Memory));
        }
        return new Address(id.ShardNum, id.RealmNum, id.ContractNum);
    }
}