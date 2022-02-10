using Proto;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hashgraph;
/// <summary>
/// Holds the list of storage slots having been read or
/// written to during the processing of the transaction
/// for a given contract instance.
/// </summary>
public sealed record ContractStateChange
{
    /// <summary>
    /// The contract's address having been modified.
    /// </summary>
    public Address Contract { get; private init; }
    /// <summary>
    /// The list of storage slots having been read or
    /// written to during the processing of the transaction.
    /// </summary>
    public ReadOnlyCollection<ContractStorageChange> StorageChanges { get; internal init; }
    internal ContractStateChange(Proto.ContractStateChange change)
    {
        Contract = change.ContractID.AsAddress();
        StorageChanges = change.StorageChanges is null ?
            new ReadOnlyCollection<ContractStorageChange>(Array.Empty<ContractStorageChange>()) :
            change.StorageChanges.Select(s => new ContractStorageChange(s)).ToList().AsReadOnly();
    }
}