using System;

namespace Hashgraph
{
    /// <summary>
    /// Consensus Topic Creation Parameters.
    /// </summary>
    public sealed class CreateTopicParams
    {
        /// <summary>
        /// Short description of the topic, not checked for uniqueness.
        /// </summary>
        public string? Memo { get; set; }
        /// <summary>
        /// An optional endorsement, when specified, can be used to 
        /// authorize a modification or deletion of this topic, including
        /// control of topic lifetime extensions. Additionally, if null, 
        /// any account can extend the topic lifetime.
        /// </summary>
        public Endorsement? Administrator { get; set; }
        /// <summary>
        /// Identify any key requirements for submitting messages to this topic.
        /// If left blank, any account may submit messages to this topic, 
        /// otherwise they must meet the specified signing requirements.
        /// </summary>
        public Endorsement? Participant { get; set; }
        /// <summary>
        /// Initial lifetime of the topic and auto-renewal period. If
        /// the associated account does not have sufficient funds to 
        /// renew at the expiration time, it will be renewed for a period 
        /// of time the remaining funds can support.  If no funds remain, the
        /// topic instance will be deleted.
        /// </summary>
        public TimeSpan RenewPeriod { get; set; } = TimeSpan.FromDays(90);
        /// <summary>
        /// Optional address of the account supporting the auto renewal of 
        /// the topic at expiration time.  The topic lifetime will be
        /// extended by the RenewPeriod at expiration time if this account
        /// contains sufficient funds.  The private key associated with
        /// this account must sign the transaction if RenewAccount is
        /// specified.
        /// </summary>
        /// <remarks>
        /// If specified, an Administrator Endorsement must also be specified.
        /// </remarks>
        public AddressOrAlias? RenewAccount { get; set; }
        /// <summary>
        /// Additional private key, keys or signing callback method 
        /// required to create to this topic.  Typically matches the
        /// Administrator, Participant and RenwAccount key(s)
        /// associated with this topic.
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
}
