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
        ShardNum = contract.ShardNum;
        RealmNum = contract.RealmNum;
        ContractNum = contract.AccountNum;
    }
}

internal static class ContractIDExtensions
{
    internal static Address AsAddress(this ContractID? id)
    {
        if (id is not null)
        {
            return new Address(id.ShardNum, id.RealmNum, id.ContractNum);
        }
        return Address.None;
    }
}