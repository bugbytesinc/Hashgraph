#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph;

/// <summary>
/// Represents the properties on a contract that can be changed.
/// Any property set to <code>null</code> on this object when submitted to the 
/// <see cref="Client.UpdateContractAsync(UpdateContractParams, Action{IContext})"/>
/// method will be left unchanged by the system.  The transaction must be
/// appropriately signed as described by the original
/// <see cref="CreateContractParams.Administrator"/> endorsement in order
/// to make changes.  If there is no administrator endorsement specified,
/// the contract is imutable and cannot be changed.
/// </summary>
public sealed class UpdateContractParams
{
    /// <summary>
    /// The network address of the contract to update.
    /// </summary>
    public Address Contract { get; set; }
    /// <summary>
    /// The new expiration date for this contract instance, it will be 
    /// ignored if it is equal to or before the current expiration date 
    /// value for this contract.
    /// </summary>
    /// <remarks>
    /// NOTE: Presently this functionality is not correctly implemented
    /// by the network.  Therefore this property is makred internal so
    /// it can not be mistakenly used.  When properly implemented by
    /// the network, this property will be made public again.
    /// </remarks>
    internal DateTime? Expiration { get; set; }
    /// <summary>
    /// Replace this Contract's current administrative key signing rquirements 
    /// with new signing requirements.</summary>
    /// <remarks>
    /// For this request to be accepted by the network, both the current private
    /// key(s) for this account and the new private key(s) must sign the transaction.  
    /// The existing key must sign for security and the new key must sign as a 
    /// safeguard to avoid accidentally changing the key to an invalid value.  
    /// The <see cref="IContext.Payer"/> account must carry the old and new 
    /// private keys for signing to meet this requirement.
    /// </remarks>
    public Endorsement? Administrator { get; set; }
    /// <summary>
    /// Incremental period for auto-renewal of the contract account. If
    /// account does not have sufficient funds to renew at the
    /// expiration time, it will be renewed for a period of time
    /// the remaining funds can support.  If no funds remain, the
    /// account will be deleted.
    /// </summary>
    public TimeSpan? RenewPeriod { get; set; }
    /// <summary>
    /// If specified updates the address of the account supporting the auto 
    /// renewal of the contract at expiration time.  The topic lifetime will be
    /// extended by the RenewPeriod at expiration time if this account
    /// contains sufficient funds.  The private key associated with
    /// this account must sign the transaction if RenewAccount is
    /// specified.  Setting the value to <code>Address.None</code> clears the
    /// renewal account.
    /// </summary>
    public Address? RenewAccount { get; set; }
    /// <summary>
    /// The memo to be associated with the contract.  Maximum
    /// of 100 bytes.
    /// </summary>
    public string? Memo { get; set; }
    /// <summary>
    /// If set, updates the maximum number of token or assets that this contract may
    /// be implicitly assoicated with (by means of being made a treasury
    /// or other related actions).
    /// </summary>
    public int? AutoAssociationLimit { get; set; }
    /// <summary>
    /// If set, updates this contract's staking proxy
    /// account.  If set The funds of this contract will 
    /// be staked to the node that this account is staked 
    /// to and the specified account will receive 
    /// the earned reward.
    /// </summary>
    public Address? ProxyAccount { get; set; }
    /// <summary>
    /// If set, updates this contract's staked node.
    /// The funds of this contract will be staked to
    /// the gossip node with the given ID.
    /// </summary>
    public long? StakedNode { get; set; }
    /// <summary>
    /// If set, updates the flag indicating to the network 
    /// that this contract does not wish to receive any 
    /// earned staking rewards.
    /// </summary>
    public bool? DeclineStakeReward { get; set; }
    /// <summary>
    /// Additional private key, keys or signing callback method 
    /// required to update this contract.  Typically matches the
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