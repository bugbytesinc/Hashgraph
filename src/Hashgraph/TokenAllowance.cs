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
    /// The account that may spend the allocated
    /// allowance of tokens.
    /// </summary>
    public Address Agent { get; private init; }
    /// <summary>
    /// The increase or decrease of the amount of
    /// tokens that the delegate may spend.
    /// </summary>
    public long Amount { get; private init; }
    /// <summary>
    /// Internal helper function creating an allowance
    /// representation from protobuf object.
    /// </summary>
    internal TokenAllowance(Proto.GrantedTokenAllowance allowance)
    {
        if (allowance is not null)
        {
            Token = allowance.TokenId.AsAddress();
            Agent = allowance.Spender.AsAddress();
            Amount = allowance.Amount;
        }
        else
        {
            Token = Address.None;
            Agent = Address.None;
            Amount = 0;
        }
    }
}