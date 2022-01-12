using Proto;
using System.Collections.Generic;
using System.Linq;

namespace Hashgraph;

/// <summary>
/// Represents a token or hBar transfer 
/// (Token, Payer, Amount, Receiver) fufilling a royalty 
/// payment for the transfer of a token or asset.
/// </summary>
public sealed record RoyaltyTransfer
{
    /// <summary>
    /// The Address of the token who's coins (or crypto)
    /// have been transferred to pay the royalty.
    /// </summary>
    public Address Token { get; private init; }
    /// <summary>
    /// The Address(s) that were charged the assessed fee.
    /// </summary>
    public IReadOnlyCollection<Address> Payers { get; private init; }
    /// <summary>
    /// The Address receiving the transferred token or crypto.
    /// </summary>
    public AddressOrAlias Receiver { get; private init; }
    /// <summary>
    /// The (divisible) amount of tokens or crypto transferred.
    /// </summary>
    public long Amount { get; init; }
    /// <summary>
    /// Internal Constructor representing the "None" 
    /// version of an royalty transfer.
    /// </summary>
    private RoyaltyTransfer()
    {
        Token = Address.None;
        Payers = new List<Address>().AsReadOnly();
        Receiver = Address.None;
        Amount = 0;
    }
    /// <summary>
    /// Intenral Helper Class to Create Royalty Transfer
    /// from raw protobuf.
    /// </summary>        
    internal RoyaltyTransfer(AssessedCustomFee fee)
    {
        Token = fee.TokenId.AsAddress();
        Receiver = fee.FeeCollectorAccountId.AsAddress();
        Amount = fee.Amount;
        Payers = fee.EffectivePayerAccountId.Select(payerID => payerID.AsAddress()).ToList().AsReadOnly();
    }
}