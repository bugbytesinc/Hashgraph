using System;
using System.Collections.Generic;

namespace Hashgraph.Mirror.Implementation;
/// <summary>
/// Paged list of account information.
/// </summary>
internal class AccountDataPage : Page<AccountData>
{
    /// <summary>
    /// List of account info objects.
    /// </summary>
    public AccountData[]? Accounts { get; set; }
    /// <summary>
    /// Enumerates the list of account info objects.
    /// </summary>
    /// <returns>
    /// Enumerator of account info objects for this paged list.
    /// </returns>
    public override IEnumerable<AccountData> GetItems()
    {
        return Accounts ?? Array.Empty<AccountData>();
    }
}