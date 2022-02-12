using Proto;

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
    public Address Delegate { get; private init; }
    /// <summary>
    /// The increase or decrease of the amount of
    /// hBars that the delegate may spend.
    /// </summary>
    public long Amount { get; private init; }
    internal CryptoAllowance(Proto.CryptoAllowance allowance)
    {
        if (allowance is not null)
        {
            Owner = allowance.Owner.AsAddress();
            Delegate = allowance.Spender.AsAddress();
            Amount = allowance.Amount;
        }
        else
        {
            Owner = Address.None;
            Delegate = Address.None;
            Amount = 0;
        }
    }

}