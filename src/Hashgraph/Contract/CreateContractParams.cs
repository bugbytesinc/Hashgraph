#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph;

/// <summary>
/// Smart Contract creation properties.
/// </summary>
public sealed class CreateContractParams
{
    /// <summary>
    /// The address of the file containing the bytecode for the contract. 
    /// The bytecode in the file must be encoded as a hexadecimal string 
    /// in the file (not directly as the bytes of the bytescode).
    /// 
    /// Typically this field is used for contracts that are so large
    /// that they can not be represented in the hapi transaction size
    /// limit, otherwise the <code>ByteCode</code> property can be set
    /// instead, avoiding the extra fees of uploading a file.
    /// 
    /// This field must be set to <code>None</code> or <code>null</code>
    /// if the <code>ByteCode</code> property is set.
    /// </summary>
    public Address File { get; set; }
    /// <summary>
    /// The binary byte code representing the contract.  This field must
    /// be left <code>Empty</code> if the <code>File</code> property
    /// is specified.  The byte code must fit within the size of
    /// a hapi transaction, or the <code>File</code> option of uploading
    /// a larger contract to a file first must be utilized.
    /// </summary>
    public ReadOnlyMemory<byte> ByteCode { get; set; }
    /// <summary>
    /// An optional endorsement that can be used to modify the contract details.  
    /// If left null, the contract is immutable once created.
    /// </summary>
    public Endorsement? Administrator { get; set; }
    /// <summary>
    /// Maximum gas to pay for the constructor, unused gas will be 
    /// refunded to the paying account.
    /// </summary>
    public long Gas { get; set; }
    /// <summary>
    /// The renewal period for maintaining the contract bytecode and state.  
    /// The contract instance will be charged at this interval as appropriate.
    /// </summary>
    public TimeSpan RenewPeriod { get; set; }
    /// <summary>
    /// Optional address of the account supporting the auto renewal of 
    /// the contract at expiration time.  The contract lifetime will be
    /// extended by the RenewPeriod at expiration time if this account
    /// contains sufficient funds.  The private key associated with
    /// this account must sign the transaction if RenewAccount is
    /// specified.
    /// </summary>
    /// <remarks>
    /// If specified, an Administrator Endorsement must also be specified.
    /// </remarks>
    public Address? RenewAccount { get; set; }
    /// <summary>
    /// The initial value in tinybars to send to this contract instance.  
    /// If the contract is not payable, providing a non-zero value will result 
    /// in a contract create failure.
    /// </summary>
    public long InitialBalance { get; set; }
    /// <summary>
    /// The arguments to pass to the smart contract constructor method.
    /// </summary>
    public object[] Arguments { get; set; }
    /// <summary>
    /// The maximum number of token or assets that this contract may
    /// be implicitly assoicated with (by means of being made a treasury
    /// or other related actions).
    /// </summary>
    /// <remarks>
    /// Defaults to zero.
    /// </remarks>
    public int AutoAssociationLimit { get; set; } = 0;
    /// <summary>
    /// The funds of this contract will be staked to
    /// the node that this account is staked to and the
    /// specified account will receive the earned reward.
    /// </summary>
    /// <remarks>
    /// This value must be set to <code>null</code> or
    /// <code>None</code> if the <code>StakedNode</code>
    /// property is set.
    /// </remarks>
    public Address? ProxyAccount { get; set; } = null;
    /// <summary>
    /// The funds of this contract will be staked to
    /// the gossip node with the given ID.
    /// </summary>
    /// <remarks>
    /// Can not be greater than zero if the 
    /// <code>ProxyAccount</code> property is set.
    /// </remarks>
    public long StakedNode { get; set; } = 0;
    /// <summary>
    /// Indicate to the network that this contract
    /// does not wish to receive any earned staking
    /// rewards.
    /// </summary>
    public bool DeclineStakeReward { get; set; } = false;
    /// <summary>
    /// Additional private key, keys or signing callback method 
    /// required to create this contract.  Typically matches the
    /// Administrator endorsement assigned to this new contract.
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
    /// Short description of the contract, limit to 100 bytes.
    /// </summary>
    public string? Memo { get; set; }
}