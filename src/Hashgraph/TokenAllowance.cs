using Proto;

namespace Hashgraph;

/// <summary>
/// Represents an allowance allocation permitting a
/// delegate account privleges of spending the specified
/// amount of tokens from the owning account.
/// </summary>
public sealed record TokenAllowance
{
    /// <summary>
    /// The address of the token that having
    /// the allocated allowance.
    /// </summary>
    public Address Token { get; private init; }
    /// <summary>
    /// The Account owner holding the tokens that
    /// may be spent by the delegate.
    /// </summary>
    public Address Owner { get; private init; }
    /// <summary>
    /// The account that may spend the allocated
    /// allowance of tokens.
    /// </summary>
    public Address Delegate { get; private init; }
    /// <summary>
    /// The increase or decrease of the amount of
    /// tokens that the delegate may spend.
    /// </summary>
    public long Amount { get; private init; }
    internal TokenAllowance(Proto.TokenAllowance allowance)
    {
        if (allowance is not null)
        {
            Token = allowance.TokenId.AsAddress();
            Owner = allowance.Owner.AsAddress();
            Delegate = allowance.Spender.AsAddress();
            Amount = allowance.Amount;
        }
        else
        {
            Token = Address.None;
            Owner = Address.None;
            Delegate = Address.None;
            Amount = 0;
        }
    }

}