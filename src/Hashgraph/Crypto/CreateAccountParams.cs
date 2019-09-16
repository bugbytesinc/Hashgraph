using System;

namespace Hashgraph
{
    /// <summary>
    /// Account creation parameters.
    /// </summary>
    public sealed class CreateAccountParams
    {
        /// <summary>
        /// The public Ed25519 key corresponding to the private key authorized 
        /// to sign transactions on behalf of this new account.  The key 
        /// length is expected to be 44 bytes long and start with the prefix 
        /// of 0x302a300506032b6570032100.
        /// </summary>
        public ReadOnlyMemory<byte> PublicKey { get; set; }
        /// <summary>
        /// The initial balance that will be transferred from the 
        /// <see cref="IContext.Payer"/> account to the new account 
        /// upon creation.
        /// </summary>
        public ulong InitialBalance { get; set; }
        /// <summary>
        /// Threshold in tinybars at which withdraws larger than
        /// this value will automatically trigger the creation of a record 
        /// for the transaction. This new account will be charged for the 
        /// record creation.
        /// </summary>
        /// <remarks>
        /// On might think that since the protocol allows the
        /// maximum value for an unsigned 64 bit integer, however
        /// the network will accept values not larger than what
        /// a signed 64 bit integer can carry (9,223,372,036,854,775,807)
        /// </remarks>
        public ulong SendThresholdCreateRecord { get; set; } = ulong.MaxValue / 2;
        /// <summary>
        /// Threshold in tinybars at which deposits larger than
        /// this value will automatically trigger the creation of a 
        /// record for the transaction.  This new account will be charged
        /// for the record creation.
        /// </summary>
        /// <remarks>
        /// On might think that since the protocol allows the
        /// maximum value for an unsigned 64 bit integer, however
        /// the network will accept values not larger than what
        /// a signed 64 bit integer can carry (9,223,372,036,854,775,807)
        /// </remarks>
        public ulong ReceiveThresholdCreateRecord { get; set; } = ulong.MaxValue / 2;
        /// <summary>
        /// When creating a new account: the newly created account must 
        /// sign any transaction transferring crypto into the newly 
        /// created account.
        /// </summary>
        public bool RequireReceiveSignature { get; set; } = false;
        /// <summary>
        /// The auto-renew period for the newly created account, it will continue 
        /// to be renewed at the given interval for as long as the account contains 
        /// hbars sufficient to cover the renewal charge.
        /// </summary>
        public TimeSpan AutoRenewPeriod { get; set; } = TimeSpan.FromSeconds(7890000);
        /// <summary>
        /// The account to which the created account will proxy its stake to.
        /// If null or is an invalid, or is the account that is not a node, or the
        /// node does not accept proxy staking; then this account is automatically 
        /// proxy staked to a node chosen by the network without earning payments.
        /// </summary>
        public Address? Proxy { get; set; }
    }
}
