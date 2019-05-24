#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// Record produced from creating a new contract.
    /// </summary>
    public sealed class CallContractRecord : TransactionRecord
    {
        /// <summary>
        /// The address of the contract.
        /// </summary>
        public Address Contract { get; internal set; }
        /// <summary>
        /// The values returned from the contract call.
        /// </summary>
        public FunctionResult Result { get; internal set; }
        /// <summary>
        /// An error string returned from the system if there was a problem.
        /// </summary>
        public string Error { get; internal set; }
        /// <summary>
        /// Bloom filter for record
        /// </summary>
        public ReadOnlyMemory<byte> Bloom { get; internal set; }
        /// <summary>
        /// The amount of gas that was used.
        /// </summary>
        public ulong Gas { get; set; }
        /// <summary>
        /// Log events returned by the function.
        /// </summary>
        public ContractEvent[] Events { get; internal set; }
    }
    /// <summary>
    /// Represents the log events returned by a contract function call.
    /// </summary>
    public sealed class ContractEvent
    {
        /// <summary>
        /// Address of a contract that emitted the event.
        /// </summary>
        public Address Contract { get; set; }
        /// <summary>
        /// Bloom filter for this log record.
        /// </summary>
        public ReadOnlyMemory<byte> Bloom { get; internal set; }
        /// <summary>
        /// Associated Event Topics
        /// </summary>
        public ReadOnlyMemory<byte>[] Topic { get; internal set; }
        /// <summary>
        /// The event data returned.
        /// </summary>
        public FunctionResult Data { get; internal set; }
    }
}