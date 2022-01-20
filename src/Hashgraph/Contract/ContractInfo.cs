using Proto;
using System;
using System.Collections.ObjectModel;

namespace Hashgraph;

/// <summary>
/// The information returned from the GetContractInfoAsync Client method call.  
/// It represents the details concerning a Hedera Network Contract instance, including 
/// the public key value to use in smart contract interaction.
/// </summary>
public sealed record ContractInfo
{
    /// <summary>
    /// ID of the contract instance.
    /// </summary>
    public Address Contract { get; private init; }
    /// <summary>
    /// Address of the Crypto Currency Account owned by this
    /// contract instance.
    /// </summary>
    public Address Address { get; private init; }
    /// <summary>
    /// The identity of both the contract ID and the associated
    /// crypto currency Hedera Account in a form to be
    /// used with smart contracts.  
    /// </summary>
    public string SmartContractId { get; private init; }
    /// <summary>
    /// An optional endorsement that can be used to modify the contract details.  
    /// If null, the contract is immutable.
    /// </summary>
    public Endorsement? Administrator { get; private init; }
    /// <summary>
    /// The time at which this instance of the contract is
    /// (and associated account) is set to expire.
    /// </summary>
    public DateTime Expiration { get; private init; }
    /// <summary>
    /// Incremental period for auto-renewal of the contract and account. If
    /// account does not have sufficient funds to renew at the
    /// expiration time, it will be renewed for a period of time
    /// the remaining funds can support.  If no funds remain, the
    /// contract instance and associated account will be deleted.
    /// </summary>
    public TimeSpan RenewPeriod { get; private init; }
    /// <summary>
    /// The number of bytes of required to store this contract instance.
    /// This value impacts the cost of extending the expiration time.
    /// </summary>
    public long Size { get; private init; }
    /// <summary>
    /// The memo associated with the contract instance.
    /// </summary>
    public string Memo { get; private init; }
    /// <summary>
    /// Contract's Account's Crypto Balance in Tinybars
    /// </summary>
    public ulong Balance { get; private init; }
    /// <summary>
    /// Balances of tokens associated with this account.
    /// </summary>
    public ReadOnlyCollection<TokenBalance> Tokens { get; private init; }
    /// <summary>
    /// <code>True</code> if this contract has been deleted.
    /// </summary>
    public bool Deleted { get; private init; }
    /// <summary>
    /// Identification of the Ledger (Network) this 
    /// contract information was retrieved from.
    /// </summary>
    public ReadOnlyMemory<byte> Ledger { get; private init; }
    /// <summary>
    /// Internal Constructor from Raw Results
    /// </summary>
    internal ContractInfo(Response response)
    {
        var info = response.ContractGetInfo.ContractInfo;
        Contract = info.ContractID.AsAddress();
        Address = info.AccountID.AsAddress();
        SmartContractId = info.ContractAccountID;
        Administrator = info.AdminKey?.ToEndorsement();
        Expiration = info.ExpirationTime.ToDateTime();
        RenewPeriod = info.AutoRenewPeriod.ToTimeSpan();
        Size = info.Storage;
        Memo = info.Memo;
        Balance = info.Balance;
        Tokens = info.TokenRelationships.ToBalances();
        Deleted = info.Deleted;
        Ledger = info.LedgerId.Memory;
    }
}