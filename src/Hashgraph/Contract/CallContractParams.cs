#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph
{
    /// <summary>
    /// Provides the details of the request to the client when invoking a contract function.
    /// </summary>
    public class CallContractParams
    {
        /// <summary>
        /// The address of the contract to call.
        /// </summary>
        public Address Contract { get; set; }
        /// <summary>
        /// The amount of gas that is allowed for the call.
        /// </summary>
        public long Gas { get; set; }
        /// <summary>
        /// For payable function calls, the amount of tinybars to send to the contract.
        /// </summary>
        public long PayableAmount { get; set; }
        /// <summary>
        /// Name of the contract function to call.
        /// </summary>
        public string FunctionName { get; set; }
        /// <summary>
        /// The function arguments to send with the method call.
        /// </summary>
        public object[] FunctionArgs { get; set; }
    }
}
