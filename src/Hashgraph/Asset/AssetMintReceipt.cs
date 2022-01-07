using Hashgraph.Implementation;
using System;
using System.Collections.ObjectModel;

namespace Hashgraph;

/// <summary>
/// A transaction receipt containing information regarding
/// new token coin balance, typically returned from methods
/// that can affect a change on the total circulation supply.
/// </summary>
public sealed record AssetMintReceipt : TransactionReceipt
{
    /// <summary>
    /// The current (new) total number of assets.
    /// </summary>
    public ulong Circulation { get; internal init; }
    /// <summary>
    /// The serial numbers of the newly created
    /// assets, related in order to the list of
    /// metadata sent to the mint method.
    /// </summary>
    /// <remarks>
    /// The value will be empty if the update
    /// was scheduled as a pending transaction.
    /// </remarks>
    public ReadOnlyCollection<long> SerialNumbers { get; internal init; }
    /// <summary>
    /// Internal Constructor of the receipt.
    /// </summary>
    internal AssetMintReceipt(NetworkResult response) : base(response)
    {
        Circulation = response.Receipt.NewTotalSupply;
        SerialNumbers = response.Receipt.SerialNumbers is null ?
            new ReadOnlyCollection<long>(Array.Empty<long>()) :
            new ReadOnlyCollection<long>(response.Receipt.SerialNumbers);
    }
}