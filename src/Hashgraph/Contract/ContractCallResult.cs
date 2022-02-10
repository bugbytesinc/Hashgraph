using Proto;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hashgraph;

/// <summary>
/// The results returned from a contract call.
/// </summary>
public sealed record ContractCallResult
{
    /// <summary>
    /// The values returned from the contract call.
    /// </summary>
    public ContractCallResultData Result { get; private init; }
    /// <summary>
    /// An error string returned from the system if there was a problem.
    /// </summary>
    public string Error { get; private init; }
    /// <summary>
    /// Bloom filter for record
    /// </summary>
    public ReadOnlyMemory<byte> Bloom { get; private init; }
    /// <summary>
    /// The amount of gas that was used.
    /// </summary>
    public ulong Gas { get; private init; }
    /// <summary>
    /// Log events returned by the function.
    /// </summary>
    public ReadOnlyCollection<ContractEvent> Events { get; private init; }
    /// <summary>
    /// The list of storage slots having been read or
    /// written to during the processing of the transaction
    /// for each contract processed by this transaction.
    /// </summary>
    public ReadOnlyCollection<ContractStateChange> StateChanges { get; internal init; }
    /// <summary>
    /// The contract's 20-byte EVM address, may or may 
    /// correspond to the shard.realm.num encoded, the
    /// EIP-1014 or <code>None</code> if not returned 
    /// from the network.
    /// </summary>
    public Moniker EncodedAddress { get; private init; }
    /// <summary>
    /// Internal Constructor from Raw Results
    /// </summary>
    internal ContractCallResult(Response response) : this(response.ContractCallLocal.FunctionResult)
    {
    }
    /// <summary>
    /// Internal Constructor from Raw Results
    /// </summary>
    internal ContractCallResult(ContractFunctionResult result)
    {
        Result = new ContractCallResultData(result.ContractCallResult.ToArray());
        Error = result.ErrorMessage;
        Bloom = result.Bloom.ToArray();
        Gas = result.GasUsed;
        Events = result.LogInfo.Select(log => new ContractEvent(log)).ToList().AsReadOnly();
        StateChanges = result.StateChanges.Select(s => new ContractStateChange(s)).ToList().AsReadOnly();
        EncodedAddress = result.EvmAddress is null || result.EvmAddress.IsEmpty ? Moniker.None : new Moniker(result.EvmAddress.Memory);
    }
}
/// <summary>
/// Represents the log events returned by a contract function call.
/// </summary>
public sealed class ContractEvent
{
    /// <summary>
    /// Address of a contract that emitted the event.
    /// </summary>
    public Address Contract { get; private init; }
    /// <summary>
    /// Bloom filter for this log record.
    /// </summary>
    public ReadOnlyMemory<byte> Bloom { get; private init; }
    /// <summary>
    /// Associated Event Topics
    /// </summary>
    public ReadOnlyMemory<byte>[] Topic { get; private init; }
    /// <summary>
    /// The event data returned.
    /// </summary>
    public ContractCallResultData Data { get; private init; }
    /// <summary>
    /// Internal Constructor from Raw Results
    /// </summary>
    internal ContractEvent(ContractLoginfo log)
    {
        Contract = log.ContractID.AsAddress();
        Bloom = log.Bloom.ToArray();
        Topic = log.Topic.Select(bs => new ReadOnlyMemory<byte>(bs.ToArray())).ToArray();
        Data = new ContractCallResultData(log.Data.ToArray());
    }
}