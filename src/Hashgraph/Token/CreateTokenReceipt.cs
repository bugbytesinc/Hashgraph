#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph
{
    /// <summary>
    /// Receipt produced from creating a new token.
    /// </summary>
    public sealed class CreateTokenReceipt : TransactionReceipt
    {
        /// <summary>
        /// The newly created token address.
        /// </summary>
        public Address Token { get; internal set; }
    }
}