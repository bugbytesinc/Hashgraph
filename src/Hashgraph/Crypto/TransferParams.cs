using System.Collections.Generic;

namespace Hashgraph;

/// <summary>
/// Supports complex atomic multi-party multi-token and crypto transfers requests.
/// Can support multi-account crypto transfers and/or multi-account token transfers
/// in the same transaction.  The crypto transfer list or token transfer list may
/// be null if not used, however at least one transfer of some type must be defined 
/// to be valid.  
/// </summary>
public sealed class TransferParams
{
    /// <summary>
    /// Transfer tinybars from an arbitray set of accounts to
    /// another arbitrary set of accounts.
    /// </summary>
    public IEnumerable<CryptoTransfer>? CryptoTransfers { get; set; }
    /// <summary>
    /// A list of tokens transfered from an arbitray set of accounts to
    /// another arbitrary set of accounts.
    /// </summary>
    public IEnumerable<TokenTransfer>? TokenTransfers { get; set; }
    /// <summary>
    /// A list of assets transfered from an arbitray set of accounts to
    /// another arbitrary set of accounts.
    /// </summary>
    public IEnumerable<AssetTransfer>? AssetTransfers { get; set; }
    /// <summary>
    /// Additional private key, keys or signing callback method 
    /// required to authorize the transfers.  Typically matches the
    /// Endorsement assigned to sending accounts.
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