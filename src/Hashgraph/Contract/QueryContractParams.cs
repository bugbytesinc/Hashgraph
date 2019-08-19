#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph
{
    /// <summary>
    /// Provides the details of the request to the client when invoking a contract local query function.
    /// </summary>
    public class QueryContractParams
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
        /// Name of the contract function to call.
        /// </summary>
        public string FunctionName { get; set; }
        /// <summary>
        /// The function arguments to send with the method call.
        /// </summary>
        public object[] FunctionArgs { get; set; }
        /// <summary>
        /// The maximum number of bytes that are allowed to be returned, if the
        /// contract attemps to return more information than the max allowed size
        /// it will fail.
        /// </summary>
        public long MaxAllowedReturnSize { get; set; } = 256;
    }
}
