#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// The information returned from the GetContractInfoAsync Client method call.  
    /// It represents the details concerning a Hedera Network Contract instance, including 
    /// the public key value to use in smart contract interaction.
    /// </summary>
    public sealed class ContractInfo
    {
        /// <summary>
        /// ID of the contract instance.
        /// </summary>
        public Address Contract { get; internal set; }
        /// <summary>
        /// Address of the Crypto Currency Account owned by this
        /// contract instance.
        /// </summary>
        public Address Address { get; internal set; }
        /// <summary>
        /// The identity of both the contract ID and the associated
        /// crypto currency Hedera Account in a form to be
        /// used with smart contracts.  
        /// </summary>
        public string SmartContractId { get; internal set; }
        /// <summary>
        /// An optional endorsement that can be used to modify the contract details.  
        /// If null, the contract is immutable.
        /// </summary>
        public Endorsement? Administrator { get; internal set; }
        /// <summary>
        /// The time at which this instance of the contract is
        /// (and associated account) is set to expire.
        /// </summary>
        public DateTime Expiration { get; internal set; }
        /// <summary>
        /// Incremental period for auto-renewal of the contract and account. If
        /// account does not have sufficient funds to renew at the
        /// expiration time, it will be renewed for a period of time
        /// the remaining funds can support.  If no funds remain, the
        /// contract instance and associated account will be deleted.
        /// </summary>
        public TimeSpan RenewPeriod { get; internal set; }
        /// <summary>
        /// The number of bytes of required to store this contract instance.
        /// This value impacts the cost of extending the expiration time.
        /// </summary>
        public long Size { get; internal set; }
        /// <summary>
        /// The memo associated with the contract instance.
        /// </summary>
        public string Memo { get; internal set; }
        /// <summary>
        /// Contract's Account Balance in Tinybars
        /// </summary>
        public ulong Balance { get; internal set; }

    }
}
