using Proto;
using System;

namespace Hashgraph;

/// <summary>
/// Represents an allowance allocation permitting a
/// delegate account privleges of spending the specified
/// amount of hBars from the owning account.
/// </summary>
public sealed record CryptoAllowance
{
    /// <summary>
    /// The Account owner holding the hBars that
    /// may be spent by the delegate.
    /// </summary>
    public Address Owner { get; private init; }
    /// <summary>
    /// The account that may spend the allocated
    /// allowance of hBars
    /// </summary>
    public Address Agent { get; private init; }
    /// <summary>
    /// The increase or decrease of the amount of
    /// hBars that the delegate may spend.
    /// </summary>
    public long Amount { get; private init; }
    /// <remarks>
    /// MARKED INTERNAL because this feature is not 
    /// implemented in full by the network and should
    /// not be made publicly available.
    /// </remarks>
    internal CryptoAllowance(Address owner, Address agent, long amount)
    {
        if (owner.IsNullOrNone())
        {
            throw new ArgumentException(nameof(owner), "The allowance owner account cannot be null or empty.");
        }
        if (agent.IsNullOrNone())
        {
            throw new ArgumentException(nameof(agent), "The allowance agent account cannot be null or empty.");
        }
        else if (amount == 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "The allowance amount must be non zero.");
        }
        Owner = owner;
        Agent = agent;
        Amount = amount;
    }
    /// <summary>
    /// Helper constructor creating a crypto 
    /// allowance from the protobuf message.
    /// </summary>
    /// <param name="allowance"></param>
    internal CryptoAllowance(Proto.CryptoAllowance allowance)
    {
        if (allowance is not null)
        {
            Owner = allowance.Owner.AsAddress();
            Agent = allowance.Spender.AsAddress();
            Amount = allowance.Amount;
        }
        else
        {
            Owner = Address.None;
            Agent = Address.None;
            Amount = 0;
        }
    }
}