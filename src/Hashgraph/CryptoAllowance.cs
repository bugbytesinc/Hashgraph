using Proto;
using System;

namespace Hashgraph;

/// <summary>
/// Represents an allowance allocation permitting a
/// agent account privleges of spending the specified
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
    /// <summary>
    /// Represents an allowance allocation permitting a
    /// delegate account privleges of spending the specified
    /// amount of hBars from the owning account.
    /// </summary>
    /// <param name="owner">
    /// The Account owner holding the hBars that
    /// may be spent by the delegate.
    /// </param>
    /// <param name="agent">
    /// The account that may spend the allocated
    /// allowance of hBars
    /// </param>
    /// <param name="amount">
    /// The increase or decrease of the amount of
    /// hBars that the delegate may spend.
    /// </param>
    /// <exception cref="ArgumentException">
    /// If any of the addresses are null or empty.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// If the amount of allowance is zero.
    /// </exception>
    public CryptoAllowance(Address owner, Address agent, long amount)
    {
        if (owner.IsNullOrNone())
        {
            throw new ArgumentException(nameof(owner), "The allowance owner account cannot be null or empty.");
        }
        if (agent.IsNullOrNone())
        {
            throw new ArgumentException(nameof(agent), "The allowance agent account cannot be null or empty.");
        }
        else if (amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "The allowance amount must be greater than or equal to zero.");
        }
        Owner = owner;
        Agent = agent;
        Amount = amount;
    }
    /// <summary>
    /// Helper constructor creating a crypto 
    /// allowance grant from the protobuf message.
    /// </summary>
    internal CryptoAllowance(Proto.GrantedCryptoAllowance allowance, Address owner)
    {
        if (allowance is not null)
        {
            Owner = owner;
            Agent = allowance.Spender.AsAddress();
            Amount = allowance.Amount;
        }
        else
        {
            Owner = owner;
            Agent = Address.None;
            Amount = 0;
        }
    }
}