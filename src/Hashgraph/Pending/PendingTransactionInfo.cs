using Google.Protobuf;
using Proto;
using System;

namespace Hashgraph;

/// <summary>
/// The information returned from the GetPendingTransactionInfo
/// Client  method call.  It represents the details concerning a 
/// pending (scheduled, not yet executed) transaction held by the
/// network awaiting signatures.
/// </summary>
public sealed record PendingTransactionInfo : PendingTransaction
{
    /// <summary>
    /// The Account that paid for the scheduling of the 
    /// pending transaction.
    /// </summary>
    public AddressOrAlias Creator { get; private init; }
    /// <summary>
    /// The account paying for the execution of the
    /// pending transaction.
    /// </summary>
    public AddressOrAlias Payer { get; private init; }
    /// <summary>
    /// A list of keys having signed the pending transaction, when
    /// all necessary keyholders have signed, the network will attempt
    /// to execute the transaction.
    /// </summary>
    public Endorsement[] Endorsements { get; private init; }
    /// <summary>
    /// The endorsement key that can cancel this pending transaction.
    /// It may be null, in wich case it can not be canceled and will
    /// exit until signed or expired by the network.
    /// </summary>
    public Endorsement? Administrator { get; private init; }
    /// <summary>
    /// Optional memo attached to the scheduling of 
    /// the pending transaction.
    /// </summary>
    public string? Memo { get; private init; }
    /// <summary>
    /// The time at which the pending transaction will expire
    /// and be removed from the network if not signed by 
    /// all necessary parties and executed.
    /// </summary>
    public DateTime Expiration { get; private init; }
    /// <summary>
    /// If not null, the consensus time at which this pending
    /// transaction was completed and executed by the network.
    /// </summary>
    public DateTime? Executed { get; private init; }
    /// <summary>
    /// If not null, the consensus time when this pending
    /// transaction was canceled using the administrative key.
    /// </summary>
    public DateTime? Deleted { get; private init; }
    /// <summary>
    /// The body bytes of the pending transaction, serialized
    /// into the binary protobuf message format 
    /// of the SchedulableTransactionBody message.
    /// </summary>
    public ReadOnlyMemory<byte> PendingTransactionBody { get; private init; }
    /// <summary>
    /// Identification of the Ledger (Network) this 
    /// pending transaction information was retrieved from.
    /// </summary>
    /// <remarks>
    /// Marked Internal for Now since this is not yet
    /// implemented on previewnet or testnet.
    /// </remarks>
    internal ReadOnlyMemory<byte> Ledger { get; private init; }
    /// <summary>
    /// Internal Constructor from Raw Results
    /// </summary>
    internal PendingTransactionInfo(Response response)
    {
        var info = response.ScheduleGetInfo.ScheduleInfo;
        Id = info.ScheduleID.ToAddress();
        TxId = info.ScheduledTransactionID.AsTxId();
        Creator = info.CreatorAccountID.AsAddress();
        Payer = info.PayerAccountID.AsAddress();
        Endorsements = info.Signers.ToEndorsements();
        Administrator = info.AdminKey?.ToEndorsement();
        Memo = info.Memo;
        Expiration = info.ExpirationTime.ToDateTime();
        Executed = info.ExecutionTime?.ToDateTime();
        Deleted = info.DeletionTime?.ToDateTime();
        PendingTransactionBody = info.ScheduledTransactionBody.ToByteArray();
        Ledger = info.LedgerId.Memory;
    }
}