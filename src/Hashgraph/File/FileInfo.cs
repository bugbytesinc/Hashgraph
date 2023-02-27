using Proto;
using System;

namespace Hashgraph;

/// <summary>
/// Detailed description of a network file.
/// </summary>
public sealed record FileInfo
{
    /// <summary>
    /// The network address of the file.
    /// </summary>
    public Address File { get; private init; }
    /// <summary>
    /// A short description of the file.
    /// </summary>
    public string Memo { get; private init; }
    /// <summary>
    /// The size of the file in bytes (plus 30 extra for overhead).
    /// </summary>
    public long Size { get; private init; }
    /// <summary>
    /// The file expiration date at which it will be removed from 
    /// the network.  The date can be extended thru updates.
    /// </summary>
    public ConsensusTimeStamp Expiration { get; private init; }
    /// <summary>
    /// A descriptor of the all the keys required to sign transactions 
    /// editing and otherwise manipulating the contents of this file.
    /// </summary>
    public Endorsement[] Endorsements { get; private init; }
    /// <summary>
    /// Flag indicating the file has been deleted.
    /// </summary>
    public bool Deleted { get; private init; }
    /// <summary>
    /// Identification of the Ledger (Network) this 
    /// account information was retrieved from.
    /// </summary>
    public ReadOnlyMemory<byte> Ledger { get; private init; }
    /// <summary>
    /// If an auto-renew account is in use, the added 
    /// lifetime of each auto-renewal.
    /// </summary>
    public TimeSpan AutoRenewPeriod { get; private init; }
    /// <summary>
    /// If specified, pays the fees for renewing this file.
    /// </summary>
    public Address AutoRenewAccount { get; private init; }
    /// <summary>
    /// Intenral Constructor from Raw Response
    /// </summary>
    internal FileInfo(Response response)
    {
        var info = response.FileGetInfo.FileInfo;
        File = info.FileID.AsAddress();
        Memo = info.Memo;
        Size = info.Size;
        Expiration = info.ExpirationTime.ToConsensusTimeStamp();
        Endorsements = info.Keys?.ToEndorsements() ?? Array.Empty<Endorsement>();
        // v0.34.0 Churn
        //AutoRenewPeriod = info.AutoRenewPeriod is null ? TimeSpan.Zero : info.AutoRenewPeriod.ToTimeSpan();
        //AutoRenewAccount = info.AutoRenewAccount.AsAddress();
        Deleted = info.Deleted;
        Ledger = info.LedgerId.Memory;
    }
}