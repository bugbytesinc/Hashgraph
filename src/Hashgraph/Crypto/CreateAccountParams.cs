using System;

namespace Hashgraph
{
    /// <summary>
    /// Account creation parameters.
    /// </summary>
    public sealed class CreateAccountParams
    {
        /// <summary>
        /// The public key structure representing the signature or signatures
        /// required to sign on behalf of this new account.  It can represent
        /// a single Ed25519 key, set of n-of-m keys or any other key structure
        /// supported by the network.
        /// </summary>
        public Endorsement? Endorsement { get; set; }
        /// <summary>
        /// The initial balance that will be transferred from the 
        /// <see cref="IContext.Payer"/> account to the new account 
        /// upon creation.
        /// </summary>
        public ulong InitialBalance { get; set; }
        /// <summary>
        /// When creating a new account: the newly created account must 
        /// sign any transaction transferring crypto into the newly 
        /// created account.
        /// </summary>
        public bool RequireReceiveSignature { get; set; } = false;
        /// <summary>
        /// The maximum number of token or assets that this account may
        /// be implicitly assoicated with (by means of being made a treasury
        /// or other related actions).
        /// </summary>
        /// <remarks>
        /// Defaults to zero with a maximum value of 1,000.
        /// </remarks>
        public int AutoAssociationLimit { get; set; } = 0;
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
        /// <summary>
        /// Additional private key, keys or signing callback method 
        /// required to create this account.  Typically matches the
        /// Endorsement assigned to this new account.
        /// </summary>
        /// <remarks>
        /// Keys/callbacks added here will be combined with those already
        /// identified in the client object's context when signing this 
        /// transaction to change the state of this account.  They will 
        /// not be asked to sign transactions to retrieve the record
        /// if the "WithRecord" form of the method call is made.  The
        /// client will rely on the Signatory from the context to sign
        /// the transaction requesting the record.
        /// </remarks>
        public Signatory? Signatory { get; set; }
        /// <summary>
        /// Short description of the account.
        /// </summary>
        public string? Memo { get; set; }
    }
}
