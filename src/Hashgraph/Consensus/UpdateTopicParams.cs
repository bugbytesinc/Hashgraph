#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph;

/// <summary>
/// Represents the properties on a topic that can be changed.
/// Any property set to <code>null</code> on this object when submitted to the 
/// <see cref="Client.UpdateTopicAsync(UpdateTopicParams, Action{IContext})"/>
/// method will be left unchanged by the system.  The transaction must be
/// appropriately signed as described by the original
/// <see cref="CreateTopicParams.Administrator"/> endorsement in order
/// to make changes.  If there is no administrator endorsement specified,
/// the topic is imutable and cannot be changed.
/// </summary>
public sealed class UpdateTopicParams
{
    /// <summary>
    /// The network address of the topic to update.
    /// </summary>
    public Address Topic { get; set; }
    /// <summary>
    /// The publicly visible memo to be associated with the topic.
    /// </summary>
    public string? Memo { get; set; }
    /// <summary>
    /// Replace this Topics's current administrative key signing rquirements 
    /// with new signing requirements.  To completely remove the administrator
    /// key and make the Topic imutable, use the <see cref="Endorsement.None"/>
    /// endorsement value.
    /// </summary>
    /// <remarks>
    /// For this request to be accepted by the network, both the current private
    /// key(s) for this account and the new private key(s) must sign the transaction.  
    /// The existing key must sign for security and the new key must sign as a 
    /// safeguard to avoid accidentally changing the key to an invalid value.  
    /// </remarks>
    public Endorsement? Administrator { get; set; }
    /// <summary>
    /// Identify any key requirements for submitting messages to this topic.
    /// If left blank, no changes will be made. To completely remove the 
    /// key requirements and make the Topic open for all to submit, use
    /// the <see cref="Endorsement.None"/> endorsement value.
    /// </summary>
    public Endorsement? Participant { get; set; }
    /// <summary>
    /// The new expiration date for this topic, it will be ignored
    /// if it is equal to or before the current expiration date value
    /// for this topic.  This allows non-administrator accounts to
    /// extend the lifetime of this topic when no auto renew 
    /// account has been specified.
    /// </summary>
    public ConsensusTimeStamp? Expiration { get; set; }
    /// <summary>
    /// Incremental period for auto-renewal of the topic account. If
    /// the associated account does not have sufficient funds to 
    /// renew at the expiration time, it will be renewed for a period 
    /// of time the remaining funds can support.  If no funds remain, the
    /// topic instance will be deleted.
    /// </summary>
    public TimeSpan? RenewPeriod { get; set; }
    /// <summary>
    /// Optional address of the account supporting the auto renewal of 
    /// the topic at expiration time.  The topic lifetime will be
    /// extended by the RenewPeriod at expiration time if this account
    /// contains sufficient funds.  The private key associated with
    /// this account must sign the transaction if RenewAccount is
    /// specified.
    /// </summary>
    public Address? RenewAccount { get; set; }
    /// <summary>
    /// Additional private key, keys or signing callback method 
    /// required to update this topic.  Typically matches the
    /// Administrator endorsement associated with this contract.
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
}