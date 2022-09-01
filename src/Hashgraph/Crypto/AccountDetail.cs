using Proto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hashgraph;

/// <summary>
/// The information returned from the CreateAccountAsync Client method call.  
/// It represents the details concerning a Hedera Network Account, including 
/// the public key value to use in smart contract interaction.
/// </summary>
public sealed record AccountDetail
{
    /// <summary>
    /// The Hedera address of this account.
    /// </summary>
    public Address Address { get; private init; }
    /// <summary>
    /// The identity of the Hedera Account in a form to be
    /// used with smart contracts.  This can also be the
    /// ID of a smart contract instance if this is the account
    /// associated with a smart contract.
    /// </summary>
    public string SmartContractId { get; private init; }
    /// <summary>
    /// <code>True</code> if this account has been deleted.
    /// Its existence in the network will cease after the expiration
    /// date for the account lapses.  It cannot participate in
    /// transactions except to extend the expiration/removal date.
    /// </summary>
    public bool Deleted { get; private init; }
    /// <summary>
    /// The total number of tinybars that are proxy staked to this account.
    /// </summary>
    public long ProxiedToAccount { get; private init; }
    /// <summary>
    /// Account's Public Key (typically a single Ed25519 key).
    /// </summary>
    public Endorsement Endorsement { get; private init; }
    /// <summary>
    /// Account Balance in Tinybars
    /// </summary>
    public ulong Balance { get; private init; }
    /// <summary>
    /// Balances of tokens and assets associated with this account.
    /// </summary>
    public ReadOnlyCollection<TokenBalance> Tokens { get; private init; }
    /// <summary>
    /// <code>True</code> if any receipt of funds require
    /// a signature from this account.
    /// </summary>
    public bool ReceiveSignatureRequired { get; private init; }
    /// <summary>
    /// Incremental period for auto-renewal of the account. If
    /// account does not have sufficient funds to renew at the
    /// expiration time, it will be renewed for a period of time
    /// the remaining funds can support.  If no funds remain, the
    /// account will be deleted.
    /// </summary>
    public TimeSpan AutoRenewPeriod { get; private init; }
    /// <summary>
    /// The account expiration time, at which it will attempt
    /// to renew if sufficient funds remain in the account.
    /// </summary>
    public DateTime Expiration { get; private init; }
    /// <summary>
    /// A short description associated with the account.
    /// </summary>
    public string Memo { get; private init; }
    /// <summary>
    /// The number of assets (non fungible tokens) held
    /// by this account.
    /// </summary>
    public long AssetCount { get; private init; }
    /// <summary>
    /// The maximum number of token or assets that this account may
    /// be implicitly assoicated with (by means of being made a treasury
    /// or other related actions).
    /// </summary>
    public int AutoAssociationLimit { get; private init; }
    /// <summary>
    /// The alternate identifier associated with this account that is
    /// in the form of a public key.  If an alternate identifer for this
    /// account does not exist, this value will be <code>None</code>.
    /// </summary>
    public Alias Alias { get; private init; }
    /// <summary>
    /// Identification of the Ledger (Network) this 
    /// account information was retrieved from.
    /// </summary>
    public ReadOnlyMemory<byte> Ledger { get; private init; }
    /// <summary>
    /// List of crypto delegate allowances 
    /// allocated  by this account.
    /// </summary>
    public IReadOnlyList<CryptoAllowance> CryptoAllowances { get; private init; }
    /// <summary>
    /// List of token delegate allowances 
    /// allocated  by this account.
    /// </summary>
    public IReadOnlyList<TokenAllowance> TokenAllowances { get; private init; }
    /// <summary>
    /// List of asset delegate allowances 
    /// allocated  by this account.
    /// </summary>
    public IReadOnlyList<AssetAllowance> AssetAllowances { get; private init; }
    /// <summary>
    /// Internal Constructor from Raw Response
    /// </summary>
    internal AccountDetail(Response response)
    {
        var info = response.AccountDetails.AccountDetails;
        var address = Address = info.AccountId.AsAddress();
        SmartContractId = info.ContractAccountId;
        Deleted = info.Deleted;
        ProxiedToAccount = info.ProxyReceived;
        Endorsement = info.Key.ToEndorsement();
        Balance = info.Balance;
        Tokens = info.TokenRelationships.ToBalances();
        ReceiveSignatureRequired = info.ReceiverSigRequired;
        AutoRenewPeriod = info.AutoRenewPeriod.ToTimeSpan();
        Expiration = info.ExpirationTime.ToDateTime();
        Memo = info.Memo;
        AssetCount = info.OwnedNfts;
        AutoAssociationLimit = info.MaxAutomaticTokenAssociations;
        Alias = info.Alias.ToAlias(info.AccountId.ShardNum, info.AccountId.RealmNum);
        Ledger = info.LedgerId.Memory;
        CryptoAllowances = info.GrantedCryptoAllowances?.Select(a => new CryptoAllowance(a, address)).ToList().AsReadOnly() ?? new ReadOnlyCollection<CryptoAllowance>(Array.Empty<CryptoAllowance>());
        TokenAllowances = info.GrantedTokenAllowances?.Select(a => new TokenAllowance(a, address)).ToList().AsReadOnly() ?? new ReadOnlyCollection<TokenAllowance>(Array.Empty<TokenAllowance>());
        AssetAllowances = info.GrantedNftAllowances?.Select(a => new AssetAllowance(a, address)).ToList().AsReadOnly() ?? new ReadOnlyCollection<AssetAllowance>(Array.Empty<AssetAllowance>());
    }
}