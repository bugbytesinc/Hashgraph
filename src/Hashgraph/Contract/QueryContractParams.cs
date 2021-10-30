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
        /// The amount of tinybars required to pay for 
        /// returning the output data from the contract.
        /// </summary>
        /// <remarks>
        /// At the time of writing it is 186 tinybars for
        /// every 32byte ABI encoded block returned.
        /// </remarks>
        public long ReturnValueCharge { get; set; }
        /// <summary>
        /// Name of the contract function to call.
        /// </summary>
        public string FunctionName { get; set; }
        /// <summary>
        /// The function arguments to send with the method call.
        /// </summary>
        public object[] FunctionArgs { get; set; }
        /// <summary>
        /// Throw a <see cref="ContractException"/> exception if the query
        /// call returns a code other than success.  Default is true to maintain
        /// backwards compatibility.  If set to false, the 
        /// <see cref="ContractCallResult"/> will be returned without an exception.
        /// The exception returned also includes the contract call result.
        /// Default is <code>true</code>.
        /// </summary>
        public bool ThrowOnFail { get; set; } = true;
    }
}
