#pragma warning disable CS8618 // Non-nullable field is uninitialized.

using System.Collections.ObjectModel;

namespace Hashgraph
{
    /// <summary>
    /// A transaction record containing information concerning 
    /// the crypto transfers associated with the request.
    /// </summary>
    public sealed class TransferRecord : TransactionRecord
    {
        /// <summary>
        /// A map of tinybar transfers to and from accounts.  
        /// Does not include the transaction fee payed by the
        /// <see cref="IContext.Payer"/>.
        /// </summary>
        public ReadOnlyDictionary<Address, long> Transfers { get; internal set; }
    }
}