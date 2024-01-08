namespace Hashgraph;

/// <summary>
/// Represents a token transfer (Token, Account, Amount)
/// </summary>
public sealed record CryptoTransfer
{
    /// <summary>
    /// The Address receiving or sending the crypto.
    /// </summary>
    public Address Address { get; private init; }
    /// <summary>
    /// The amount of crypto transferred in itnybars.  Negative values
    /// indicate an outflow of tinybars to the <code>Account</code> positive
    /// values indicate an inflow of tinybars from the associated <code>Account</code>.
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
    private CryptoTransfer()
    {
        Address = Hashgraph.Address.None;
        Amount = 0;
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
    public CryptoTransfer(Address address, long amount, bool delegated = false)
    {
        Address = address;
        Amount = amount;
        Delegated = delegated;
    }
}