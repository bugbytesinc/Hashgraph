#pragma warning disable CS8604 // Possible null reference argument.
using System;
using System.Threading.Tasks;

namespace Hashgraph.Extensions;

/// <summary>
/// Extends the client functionality to include retrieving the
/// Exchange Rate directly from the network.
/// </summary>
public static class ExchangeRateExtension
{
    /// <summary>
    /// Well known address of the exchange rate file.
    /// </summary>
    public static readonly Address EXCHANGE_RATE_FILE_ADDRESS = new Address(0, 0, 112);
    /// <summary>
    /// Retrieves the current USD to hBar exchange rate information from the
    /// network.
    /// </summary>
    /// <remarks>
    /// NOTE: this method incours a charge to retrieve the file from the network.
    /// </remarks>
    /// <param name="client">Client Object</param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// An Exchange Rates object providing the current and next 
    /// exchange rates.
    /// </returns>
    public static async Task<ExchangeRates> GetExchangeRatesAsync(this Client client, Action<IContext>? configure = null)
    {
        var file = await client.GetFileContentAsync(EXCHANGE_RATE_FILE_ADDRESS, configure).ConfigureAwait(false);
        var set = Proto.ExchangeRateSet.Parser.ParseFrom(file.ToArray());
        return new ExchangeRates(set.CurrentRate?.ToExchangeRate(), set.NextRate?.ToExchangeRate());
    }
}