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
    /// Helper constructor creating a crypto 
    /// allowance from the protobuf message.
    /// </summary>
    internal CryptoAllowance(Proto.GrantedCryptoAllowance allowance)
    {
        if (allowance is not null)
        {
            Agent = allowance.Spender.AsAddress();
            Amount = allowance.Amount;
        }
        else
        {
            Agent = Address.None;
            Amount = 0;
        }
    }
}