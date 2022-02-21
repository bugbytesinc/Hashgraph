using Hashgraph.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Hashgraph;

/// <summary>
/// A transaction record containing information concerning the newly adjusted allowances.
/// </summary>
/// <remarks>
/// MARKED INTERNAL because this feature is not 
/// implemented in full by the network and should
/// not be made publicly available.
/// </remarks>
internal sealed record AllowanceRecord : TransactionRecord
{
    /// <summary>
    /// List of crypto delegate allowances 
    /// allocated  by this account.
    /// </summary>
    public IReadOnlyList<CryptoAllowance> CryptoAllowances { get; private init; }
    /// <summary>
    /// List of token delegate allowances 
    /// allocated  by this account.
    /// </summary>
    public IReadOnlyList<TokenAllowance> TokenAllowances { get; private init; }
    /// <summary>
    /// List of asset delegate allowances 
    /// allocated  by this account.
    /// </summary>
    public IReadOnlyList<AssetAllowance> AssetAllowances { get; private init; }
    /// <summary>
    /// Internal Constructor of the record.
    /// </summary>
    internal AllowanceRecord(NetworkResult result) : base(result)
    {
        CryptoAllowances = result.Record!.CryptoAdjustments?.Select(a => new CryptoAllowance(a)).ToArray() ?? Array.Empty<CryptoAllowance>();
        TokenAllowances = result.Record!.TokenAdjustments?.Select(a => new TokenAllowance(a)).ToArray() ?? Array.Empty<TokenAllowance>();
        AssetAllowances = result.Record!.NftAdjustments?.Select(a => new AssetAllowance(a)).ToArray() ?? Array.Empty<AssetAllowance>();
    }
}