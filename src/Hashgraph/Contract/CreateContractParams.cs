#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// Smart Contract creation properties.
    /// </summary>
    public sealed class CreateContractParams
    {
        /// <summary>
        /// The address of the file containing the bytecode for the contract. 
        /// The bytecode is encoded as a hexadecimal string in the file 
        /// (not directly as the bytes of the bytescode).
        /// </summary>
        public Address File { get; set; }
        /// <summary>
        /// An optional endorsement that can be used to modify the contract details.  
        /// If left null, the contract is immutable once created.
        /// </summary>
        public Endorsement? Administrator { get; set; }
        /// <summary>
        /// Maximum gas to pay for the constructor, unused gas will be 
        /// refunded to the paying account.
        /// </summary>
        public long Gas { get; set; }
        /// <summary>
        /// The renewal period for maintaining the contract bytecode and state.  
        /// The contract instance will be charged at this interval as appropriate.
        /// </summary>
        public TimeSpan RenewPeriod { get; set; }
        /// <summary>
        /// The initial value in tinybars to send to this contract instance.  
        /// If the contract is not payable, providing a non-zero value will result 
        /// in a contract create failure.
        /// </summary>
        public long InitialBalance { get; set; }
        /// <summary>
        /// The arguments to pass to the smart contract constructor method.
        /// </summary>
        public object[] Arguments { get; set; }
    }
}
