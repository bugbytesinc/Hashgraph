namespace Hashgraph;

/// <summary>
/// Represents a token transfer (Token, Account, Amount)
/// </summary>
public sealed record TokenTransfer
{
    /// <summary>
    /// The Address of the Token who's coins have transferred.
    /// </summary>
    public Address Token { get; private init; }
    /// <summary>
    /// The Address receiving or sending the token's coins.
    /// </summary>
    public Address Address { get; private init; }
    /// <summary>
    /// The (divisible) amount of coins transferred.  Negative values
    /// indicate an outflow of coins to the <code>Account</code> positive
    /// values indicate an inflow of coins from the associated <code>Account</code>.
    /// </summary>
    public long Amount { get; init; }
    /// <summary>
    /// Indicates the parties involved in the transaction
    /// are acting as delegates thru a granted allowance.
    /// </summary>
    public bool Delegated { get; private init; }
    /// <summary>
    /// Internal Constructor representing the "None" 
    /// version of an transfer.
    /// </summary>
    private TokenTransfer()
    {
        Token = Hashgraph.Address.None;
        Address = Hashgraph.Address.None;
        Amount = 0;
        Delegated = false;
    }
    /// <summary>
    /// Public Constructor, an <code>TokenTransfer</code> is immutable after creation.
    /// </summary>
    /// <param name="token">
    /// The Address of the Token who's coins have transferred.
    /// </param>
    /// <param name="address">
    /// The Address receiving or sending the token's coins.
    /// </param>
    /// <param name="amount">
    /// The (divisible) amount of coins transferred.  Negative values
    /// indicate an outflow of coins to the <code>Account</code> positive
    /// values indicate an inflow of coins from the associated <code>Account</code>.
    /// </param>
    /// <param name="delegated">
    /// Indicates the parties involved in the transaction
    /// are acting as delegates thru a granted allowance.
    /// </param>
    public TokenTransfer(Address token, Address address, long amount, bool delegated = false)
    {
        Token = token;
        Address = address;
        Amount = amount;
        Delegated = delegated;
    }
}