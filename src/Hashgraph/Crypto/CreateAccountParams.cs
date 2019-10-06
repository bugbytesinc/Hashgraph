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
        /// <remarks>
        /// If this value is <code>null</code>, then the <code>Endorsement</code>
        /// value must be set.  Setting this value is the equivalent of creating
        /// a single Ed25519 key endorsement.  If this value is set, then the
        /// <code>Endorsement</code> value must be null.
        /// At some point in the future, this property will be depricated.
        /// </remarks>
        public ReadOnlyMemory<byte>? PublicKey { get; set; }
        /// <summary>
        /// The public key structure representing the signature or signatures
        /// required to sign on behalf of this new account.  It can represent
        /// a single Ed25519 key, set of n-of-m keys or any other key structure
        /// supported by the network.
        /// </summary>
        /// <remarks>
        /// If this value is not <code>null</code> then the <code>PublicKey</code>
        /// value must be set instead.  Setting that value is the equivalent of 
        /// setting this value with a single Ed25519 key.  Alternatively, if this 
        /// value is <code>null</code>, the <code>PublicKey</code> must 
        /// not be set to an Ed25519 public key.
        /// </remarks>
        public Endorsement? Endorsement { get; set; }
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
