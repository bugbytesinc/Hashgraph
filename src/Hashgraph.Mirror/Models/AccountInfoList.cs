namespace Hashgraph.Mirror;
/// <summary>
/// Paged list of account information.
/// </summary>
public class AccountInfoList : PagedList<AccountInfo>
{
    /// <summary>
    /// List of account info objects.
    /// </summary>
    public AccountInfo[]? Accounts { get; set; }
    /// <summary>
    /// Enumerates the list of account info objects.
    /// </summary>
    /// <returns>
    /// Enumerator of account info objects for this paged list.
    /// </returns>
    public override IEnumerable<AccountInfo> GetItems()
    {
        return Accounts ?? Array.Empty<AccountInfo>();
    }
}