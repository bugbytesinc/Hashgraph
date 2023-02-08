using System;

namespace Hashgraph;

/// <summary>
/// Pending (Scheduled) Transaction Parameters.  Used for creating
/// a <see cref="Signatory"/> signaling that the transaction should 
/// be accepted but not immediately executed.  It includes optional 
/// details descrbing the details of how the transaction is to be 
/// scheduled for execution.
/// </summary>
public sealed record PendingParams
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
    /// The date/time at which the network will discard this
    /// pending (scheduled) transaction if it has not already 
    /// been signed by a sufficient number of signatures or
    /// deleted by the holder of the admin key.
    /// </summary>
    /// <remarks>
    /// If not specified, the network will set the expiration
    /// time to 30 minutes after the consensus creation time
    /// of the pending transaction.
    /// </remarks>
    public ConsensusTimeStamp? Expiration { get; init; }
    /// <summary>
    /// If set to <code>true</code>, the network will delay the
    /// attempt to execute the pending transaction until the
    /// expiration time, even if it receives sufficient signatures
    /// satisfying the signing requirements prior to the deadline.
    /// </summary>
    public bool DelayExecution { get; init; }

}