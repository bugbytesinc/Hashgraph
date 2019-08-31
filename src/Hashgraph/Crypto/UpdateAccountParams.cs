#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// Represents the properties on an account that can be changed.
    /// Any property set to <code>null</code> on this object when submitted to the 
    /// <see cref="Client.UpdateAccountAsync(UpdateAccountParams, Action{IContext})"/>
    /// method will be left unchanged by the system.  Certain additional condidions
    /// apply to certain propertites such as the signing key are described below.
    /// </summary>
    public sealed class UpdateAccountParams
    {
        /// <summary>
        /// The network address and private key for the account to update.
        /// </summary>
        public Account Account { get; set; }
        /// <summary>
        /// Replace this Account's current key signing rquirements with new signing
        /// requirements.</summary>
        /// <remarks>
        /// For this request to be accepted by the network, both the current private
        /// key(s) for this account and the new private key(s) must sign the transaction.  
        /// The existing key must sign for security and the new key must sign as a 
        /// safeguard to avoid accidentally changing the key to an invalid value.  
        /// Either the <see cref="IContext.Payer"/> account or 
        /// <see cref="UpdateAccountParams.Account"/> may carry the new private key 
        /// for signing to meet this requirement.
        /// </remarks>
        public Endorsement? Endorsement { get; set; }
        /// <summary>
        /// Threshold in tinybars at which withdraws larger than
        /// this value will automatically trigger the creation of a record 
        /// for the transaction. This account will be charged for the 
        /// record creation.
        /// </summary>
        public ulong? SendThresholdCreateRecord { get; set; }
        /// <summary>
        /// Threshold in tinybars at which deposits larger than
        /// this value will automatically trigger the creation of a 
        /// record for the transaction.  This account will be charged
        /// for the record creation.
        /// </summary>
        public ulong? ReceiveThresholdCreateRecord { get; set; }
        /// <summary>
        /// The new expiration date for this account, it will be ignored
        /// if it is equal to or before the current expiration date value
        /// for this account.
        /// </summary>
        public DateTime? Expiration { get; set; }
        /// <summary>
        /// Incremental period for auto-renewal of the account. If
        /// account does not have sufficient funds to renew at the
        /// expiration time, it will be renewed for a period of time
        /// the remaining funds can support.  If no funds remain, the
        /// account will be deleted.
        /// </summary>
        public TimeSpan? AutoRenewPeriod { get; set; }
        /// <summary>
        /// The account to which the created account will proxy its stake to.
        /// If invalid, or is the account that is not a node, or the
        /// node does not accept proxy staking; then this account is automatically 
        /// proxy staked to a node chosen by the network without earning payments.
        /// </summary>
        public Address? Proxy { get; set; }
    }
}
