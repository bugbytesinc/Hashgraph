#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph;

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
    /// <summary>
    /// Additional private key, keys or signing callback method 
    /// required to invoke this contract.  Typically not used
    /// however there are some edge cases where it may send
    /// crypto to accounts that require a signature to receive
    /// funds.
    /// </summary>
    /// <remarks>
    /// Keys/callbacks added here will be combined with those already
    /// identified in the client object's context when signing this 
    /// transaction to change the state of this account.  They will 
    /// not be asked to sign transactions to retrieve the record
    /// if the "WithRecord" form of the method call is made.  The
    /// client will rely on the Signatory from the context to sign
    /// the transaction requesting the record.
    /// </remarks>
    public Signatory? Signatory { get; set; }
}