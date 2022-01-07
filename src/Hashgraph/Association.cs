using Proto;

namespace Hashgraph;

/// <summary>
/// Identifies which assets/tokens and accounts were associated 
/// as the result of a transaciton.
/// </summary>
public sealed record Association
{
    /// <summary>
    /// The address of the token or asset that
    /// was associated as a result of the enclosing
    /// transaciton.
    /// </summary>
    public Address Token { get; private init; }
    /// <summary>
    /// The address of the crypto account that
    /// was associated as a result of the enclosing
    /// transaaction.
    /// </summary>
    public Address Account { get; private init; }
    /// <summary>
    /// Internal Helper constructor creating an associaiton
    /// record from raw protobuf.
    /// </summary>
    /// <param name="association"></param>
    internal Association(TokenAssociation association)
    {
        Token = association.TokenId.AsAddress();
        Account = association.AccountId.AsAddress();
    }
}