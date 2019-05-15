#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// The information returned from the CreateAccountAsync Client method call.  
    /// It represents the details concerning a Hedera Network Account, including 
    /// the public key value to use in smart contract interaction.
    /// </summary>
    public sealed class AccountInfo
    {
        /// <summary>
        /// The Hedera address of this account.
        /// </summary>
        public Address Address { get; internal set; }
        /// <summary>
        /// The identity of the Hedera Account in a form to be
        /// used with smart contracts.  This can also be the
        /// ID of a smart contract instance if this is the account
        /// associated with a smart contract.
        /// </summary>
        public string SmartContractId { get; internal set; }
        /// <summary>
        /// <code>True</code> if this account has been deleted.
        /// Its existence in the network will cease after the expiration
        /// date for the account lapses.  It cannot participate in
        /// transactions except to extend the expiration/removal date.
        /// </summary>
        public bool Deleted { get; internal set; }
        /// <summary>
        /// The Address of the Account to which this account has staked.
        /// </summary>
        public Address Proxy { get; internal set; }
        /// <summary>
        /// Metric indicating the fraction of payments earned by
        /// the proxy node that are shared with this account,
        /// with <code>proxyFraction / 10000</code> going to this account.
        /// </summary>
        public int ProxyShareFraction { get; internal set; }
        /// <summary>
        /// The total number of tinybars that are proxy staked to this account.
        /// </summary>
        public long ProxiedToAccount { get; internal set; }
        /// <summary>
        /// Account's Ed25519 Public Key.
        /// </summary>
        public Endorsements Endorsements { get; internal set; }
        /// <summary>
        /// Account Balance in Tinybars
        /// </summary>
        public ulong Balance { get; internal set; }
        /// <summary>
        /// Threshold in tinybars at which withdraws larger than
        /// this value will automatically trigger the creation of a record 
        /// for the transaction. This account will be charged for the 
        /// record creation.
        /// </summary>
        public ulong SendThresholdCreateRecord { get; internal set; }
        /// <summary>
        /// Threshold in tinybars at which deposits larger than
        /// this value will automatically trigger the creation of a 
        /// record for the transaction.  This account will be charged
        /// for the record creation.
        /// </summary>
        public ulong ReceiveThresholdCreateRecord { get; internal set; }
        /// <summary>
        /// <code>True</code> if any receipt of funds require
        /// a signature from this account.
        /// </summary>
        public bool ReceiveSignatureRequired { get; internal set; }
        /// <summary>
        /// Date this account expires if not renewed.
        /// </summary>
        public DateTime Expiration { get; internal set; }
        /// <summary>
        /// Incremental period for auto-renewal of the account. If
        /// account does not have sufficient funds to renew at the
        /// expiration time, it will be renewed for a period of time
        /// the remaining funds can support.  If no funds remain, the
        /// account will be deleted.
        /// </summary>
        public TimeSpan AutoRenewPeriod { get; internal set; }
    }
}
