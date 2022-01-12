namespace Hashgraph.Extensions;

/// <summary>
/// Object containing the current and next exchange rate
/// returned from the network.
/// </summary>
public sealed class ExchangeRates
{
    /// <summary>
    /// Internal constructor, used by the library to create an
    /// initialized exchange rates object.
    /// </summary>
    /// <param name="current">Current Exchange Rate</param>
    /// <param name="next">Next Exchange Rate</param>
    internal ExchangeRates(ExchangeRate current, ExchangeRate next)
    {
        Current = current;
        Next = next;
    }
    /// <summary>
    /// Current Exchange Rate
    /// </summary>
    public ExchangeRate Current { get; }
    /// <summary>
    /// Exchange rate that is in effect after 
    /// the current exchange rate expires.
    /// </summary>
    public ExchangeRate Next { get; }
}