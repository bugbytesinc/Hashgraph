namespace Hashgraph
{
    /// <summary>
    /// Pending (Scheduled) Transaction Parameters.  Used for creating
    /// a <see cref="Signatory"/> signaling that the transaction should 
    /// be scheduled and not immediately executed.  It includes optional 
    /// details descrbing the details of how the transaction is to be 
    /// scheduled.
    /// </summary>
    public sealed record ScheduleParams
    {
        /// <summary>
        /// An optional endorsement that can be used to cancel or delete the 
        /// scheduling of the pending transaction if it has not already been 
        /// executed or expired and removed by the network. If left 
        /// <code>null</code>, the scheduling of the pending transaction is 
        /// immutable.   It will only be removed by the network by execution 
        /// or expiration.
        /// </summary>
        public Endorsement? Administrator { get; init; }
        /// <summary>
        /// Short memo/description that will be attached to network record holding 
        /// the pending transaction (not the memo of pending transaction itself), 
        /// limited to 100 bytes.
        /// </summary>
        public string? Memo { get; init; }
        /// <summary>
        /// Optional address of the operator account that explicitly 
        /// pays for the execution of the pending transaction when it
        /// executes.  If not specified (left null), the payer of the 
        /// transaction scheduling this pending transaction will pay
        /// for the pending transaction.  (Which is the current account 
        /// identified as the payer in the Context).
        /// </summary>
        public Address? PendingPayer { get; init; }
        /// <summary>
        /// Additional private key, keys or signing callback method 
        /// that will sign the scheduling transaction (NOT the pending
        /// scheduled transaction, that will be signed by the signatories
        /// found in the originating methods extra signatories parameter).
        /// Typically matches the Administrator endorsment if specified.
        /// </summary>
        public Signatory? Signatory { get; init; }
    }
}
