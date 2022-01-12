#pragma warning disable CS8604 // Possible null reference argument.
using System;
using System.Threading.Tasks;

namespace Hashgraph.Extensions;

/// <summary>
/// Extends the client functionality to include retrieving the
/// Exchange Rate directly from the network.
/// </summary>
public static class AddressBookExtension
{
    /// <summary>
    /// Well known address of the exchange rate file.
    /// </summary>
    public static readonly Address ADDRESS_BOOK_FILE_ADDRESS = new Address(0, 0, 102);
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
    public static async Task<NodeInfo[]> GetAddressBookAsync(this Client client, Action<IContext>? configure = null)
    {
        var file = await client.GetFileContentAsync(ADDRESS_BOOK_FILE_ADDRESS, configure).ConfigureAwait(false);
        var book = Proto.NodeAddressBook.Parser.ParseFrom(file.ToArray());
        return book.ToNodeInfoArray();
    }
}