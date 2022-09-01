using Hashgraph.Implementation;
using System;

namespace Hashgraph;

/// <summary>
/// Record produced from an Ethereum Transaction call.
/// </summary>
public sealed record EthereumTransactionRecord : TransactionRecord
{
    /// <summary>
    /// The keccak256 hash of the ethereumData.
    /// </summary>
    public ReadOnlyMemory<byte> EthereumHash { get; internal init; }
    /// <summary>
    /// Internal Constructor of the record.
    /// </summary>
    internal EthereumTransactionRecord(NetworkResult result) : base(result)
    {
        var record = result.Record!;
        EthereumHash = record.EthereumHash.Memory;
    }
}