using Proto;
using System;
using System.Collections.ObjectModel;

namespace Hashgraph;

/// <summary>
/// The information returned from the CreateAccountAsync Client method call.  
/// It represents the details concerning a Hedera Network Account, including 
/// the public key value to use in smart contract interaction.
/// </summary>
public sealed record AccountInfo
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
    public string ContractId { get; private init; }
    /// <summary>
    /// The smart contract (EVM) transaction counter nonce
    /// associated with this account.
    /// </summary>
    public long ContractNonce { get; private init; }
    /// <summary>
    /// <code>True</code> if this account has been deleted.
    /// Its existence in the network will cease after the expiration
    /// date for the account lapses.  It cannot participate in
    /// transactions except to extend the expiration/removal date.
    /// </summary>
    public bool Deleted { get; private init; }
    /// <summary>
    /// Account's Public Key (typically a single Ed25519 key).
    /// </summary>
    public Endorsement Endorsement { get; private init; }
    /// <summary>
    /// Account Balance in Tinybars
    /// </summary>
    public ulong Balance { get; private init; }
    /// <summary>
    /// [DEPRECATED] Balances of tokens and assets associated with this account.
    /// </summary>
    [Obsolete("This field is deprecated by HIP-367")]
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
    /// If specified, pays the fees for renewing this account.
    /// If not specified, this account pays renew fees.
    /// </summary>
    public Address AutoRenewAccount { get; private init; }
    /// <summary>
    /// The account expiration time, at which it will attempt
    /// to renew if sufficient funds remain in the account.
    /// </summary>
    public ConsensusTimeStamp Expiration { get; private init; }
    // HIP-583 Churn
    ///// <summary>
    ///// List of virtual addresss (keys) associated with this 
    ///// account as seen by the hedera virtual machine (ECDSA types),
    ///// The value of the dictionary is a flag indicating the
    ///// address should be considered the 'default'.
    ///// </summary>
    //public ReadOnlyDictionary<Moniker, bool> Monikers { get; private init; }
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
    /// Staking Metadata Information for the account.
    /// </summary>
    public StakingInfo StakingInfo { get; private init; }
    /// <summary>
    /// Internal Constructor from Raw Response
    /// </summary>
    internal AccountInfo(Response response)
    {
        var info = response.CryptoGetInfo.AccountInfo;
        Address = info.AccountID.AsAddress();
        ContractId = info.ContractAccountID;
        ContractNonce = info.EthereumNonce;
        Deleted = info.Deleted;
        Endorsement = info.Key.ToEndorsement();
        Balance = info.Balance;
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CS0612 // Type or member is obsolete
        Tokens = info.TokenRelationships.ToBalances();
#pragma warning restore CS0612 // Type or member is obsolete
#pragma warning restore CS0618 // Type or member is obsolete
        ReceiveSignatureRequired = info.ReceiverSigRequired;
        AutoRenewPeriod = info.AutoRenewPeriod.ToTimeSpan();
        AutoRenewAccount = info.AutoRenewAccount.AsAddress();
        Expiration = info.ExpirationTime.ToConsensusTimeStamp();
        Memo = info.Memo;
        AssetCount = info.OwnedNfts;
        AutoAssociationLimit = info.MaxAutomaticTokenAssociations;
        Alias = info.Alias.ToAlias(info.AccountID.ShardNum, info.AccountID.RealmNum);
        // HIP-583 Churn
        //Monikers = info.VirtualAddresses.ToMonikers(Address);
        Ledger = info.LedgerId.Memory;
        StakingInfo = new StakingInfo(info.StakingInfo);
    }
}