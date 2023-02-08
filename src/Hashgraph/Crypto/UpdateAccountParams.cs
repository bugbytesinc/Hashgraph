#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph;

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
    /// The network address of account to update.
    /// </summary>
    public Address Address { get; set; }
    /// <summary>
    /// Any additional signing keys required to validate the transaction
    /// that are not already specified in the client object's context.
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
    /// Replace this Account's current key signing rquirements with new signing
    /// requirements.</summary>
    /// <remarks>
    /// For this request to be accepted by the network, both the current private
    /// key(s) for this account and the new private key(s) must sign the transaction.  
    /// The existing key must sign for security and the new key must sign as a 
    /// safeguard to avoid accidentally changing the key to an invalid value.  
    /// Either the <see cref="IContext.Payer"/> account or 
    /// <see cref="UpdateAccountParams.Address"/> may carry the new private key 
    /// for signing to meet this requirement.
    /// </remarks>
    public Endorsement? Endorsement { get; set; }
    /// <summary>
    /// If set to True, the account must sign any transaction 
    /// transferring crypto into account.
    /// </summary>
    public bool? RequireReceiveSignature { get; set; }
    /// <summary>
    /// The new expiration date for this account, it will be ignored
    /// if it is equal to or before the current expiration date value
    /// for this account.
    /// </summary>
    public ConsensusTimeStamp? Expiration { get; set; }
    /// <summary>
    /// Incremental period for auto-renewal of the account. If
    /// account does not have sufficient funds to renew at the
    /// expiration time, it will be renewed for a period of time
    /// the remaining funds can support.  If no funds remain, the
    /// account will be deleted.
    /// </summary>
    public TimeSpan? AutoRenewPeriod { get; set; }
    /// <summary>
    /// If not null, updates the auto-renew account for this
    /// account to a new payer.  Setting value to <code>None</code> 
    /// will remove the existing auto renew account value from
    /// this account.
    /// </summary>
    public Address? AutoRenewAccount { get; set; }
    /// <summary>
    /// If not null, a new description of the account.
    /// </summary>
    public string? Memo { get; set; }
    /// <summary>
    /// If set, updates the maximum number of token or assets that this account may
    /// be implicitly assoicated with (by means of being made a treasury
    /// or other related actions).
    /// </summary>
    public int? AutoAssociationLimit { get; set; }
    /// <summary>
    /// If set, updates the account's alias.  The Alias can only be updated
    /// on an account if it has not yet been set.  Once set, the Alias is
    /// imutable for the life of the account.
    /// </summary>
    /// <remarks>
    /// Additionally: The private key corresponding to the Alias' public key
    /// must sign the update transaction.
    /// 
    /// NOTE: MARKED INTERNAL UNTIL IMPLEMENTED BY THE NETWORK
    /// While this feature exists in the HAPI protobuf, it appears
    /// to not be implemented by the network yet, when it is, the
    /// marker tests will indicate it and this will made public.
    /// </remarks>
    internal Alias? Alias { get; set; }
    /// <summary>
    /// If set, updates this account's staking proxy
    /// account.  If set The funds of this account will 
    /// be staked to the node that this account is staked 
    /// to and the specified proxy account will receive 
    /// the earned reward.
    /// </summary>
    public Address? ProxyAccount { get; set; }
    /// <summary>
    /// If set, updates this accounts's staked node.
    /// The funds of this account will be staked to
    /// the gossip node with the given ID.
    /// </summary>
    /// <remarks>
    /// Node IDs are used instead of node account
    /// IDs because a node has the ability to change
    /// its wallet account ID.
    /// </remarks>
    public long? StakedNode { get; set; }
    /// <summary>
    /// If set, updates the flag indicating to the network 
    /// that this account does not wish to receive any 
    /// earned staking rewards.
    /// </summary>
    public bool? DeclineStakeReward { get; set; }
    /// <summary>
    /// If set, applies the desired action on the list
    /// of moniker virtual address for this account.
    /// Address can be added or removed, one at a time
    /// using the crypto account update transaction.
    /// </summary>
    public UpdateMonikerParams? UpdateMoniker { get; set; }
}