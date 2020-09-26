#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph
{
    /// <summary>
    /// Record produced from creating a new token.
    /// </summary>
    public sealed class CreateTokenRecord : TransactionRecord
    {
        /// <summary>
        /// The newly created token address.
        /// </summary>
        public Address Token { get; internal set; }
    }
}