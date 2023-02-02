#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph;

/// <summary>
/// File creation parameters.
/// </summary>
public sealed class CreateFileParams
{
    /// <summary>
    /// Original expiration date for the file, fees will be charged as appropriate.
    /// </summary>
    public DateTime Expiration { get; set; }
    /// <summary>
    /// A descriptor of the keys required to sign transactions editing and 
    /// otherwise manipulating the contents of this file.  Only one key
    /// is required to sign the transaction to delete the file.
    /// </summary>
    public Endorsement[] Endorsements { get; set; }
    /// <summary>
    /// The initial contents of the file.
    /// </summary>
    public ReadOnlyMemory<byte> Contents { get; set; }
    /// <summary>
    /// If an auto-renew account is in use, the lifetime to be added by each
    /// auto-renewal.When both auto-renew account and auto-renew period are
    /// set in the create transaction, the initial expiry of the file will be 
    /// the valid start of the create transaction plus the auto-renew period
    /// (the Expiration Time will be ignored). The auto-renew period for the 
    /// newly created file, it will continue to be renewed at the given interval 
    /// for as long as the account contains hbars sufficient to cover the renewal charge.
    /// </summary>
    public TimeSpan AutoRenewPeriod { get; set; }
    /// <summary>
    /// The account to charge for auto-renewal of this file. If not set, or set to an
    /// account with zero hbar balance, the file's expiration must be manually extended
    /// using a FileUpdate transaction, since the network will not have authorization
    /// for any kind of auto-renewal fee collection.
    /// </summary>
    public Address? AutoRenewAccount { get; set; } = null;
    /// <summary>
    /// A short description of the file.
    /// </summary>
    public string Memo { get; set; }
    /// <summary>
    /// Additional private key, keys or signing callback method 
    /// required to create to this file.  Typically matches the
    /// Endorsements associated with this file.
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