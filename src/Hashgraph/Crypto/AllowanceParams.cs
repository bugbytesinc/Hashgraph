using System.Collections.Generic;

namespace Hashgraph;

/// <summary>
/// Allowance Creation and Adjustment Parameters
/// </summary>
public sealed class AllowanceParams
{
    /// <summary>
    /// A list of accounts and allocated allowances that 
    /// each account may sign transactions moving crypto
    /// out of this account up to the specified limit.
    /// </summary>
    public IEnumerable<CryptoAllowanceGrant>? CryptoAllowances { get; set; }
    /// <summary>
    /// A list of accounts and allocated allowances that 
    /// each account may sign transactions moving tokens
    /// out of this account up to the specified limit.
    /// </summary>
    public IEnumerable<TokenAllowanceGrant>? TokenAllowances { get; set; }
    /// <summary>
    /// A list of accounts and allocated allowances that 
    /// each account may sign transactions moving assets
    /// out of this account up to the specified limit.
    /// </summary>
    public IEnumerable<AssetAllowanceGrant>? AssetAllowances { get; set; }
    /// <summary>
    /// Additional private key, keys or signing callback method 
    /// required to authorize the transaction.
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