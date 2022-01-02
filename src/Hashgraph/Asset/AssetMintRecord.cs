using Hashgraph.Implementation;
using System;
using System.Collections.ObjectModel;

namespace Hashgraph
{
    /// <summary>
    /// A transaction record containing information regarding
    /// new token coin balance, typically returned from methods
    /// that can affect a change on the total circulation supply.
    /// </summary>
    public sealed record AssetMintRecord : TransactionRecord
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
        public ReadOnlyCollection<long> SerialNumbers { get; internal init; }
        /// <summary>
        /// Internal Constructor of the record.
        /// </summary>
        internal AssetMintRecord(NetworkResult result) : base(result)
        {
            Circulation = result.Receipt.NewTotalSupply;
            SerialNumbers = result.Receipt.SerialNumbers is null ?
                new ReadOnlyCollection<long>(Array.Empty<long>()) :
                new ReadOnlyCollection<long>(result.Receipt.SerialNumbers);
        }
    }
}