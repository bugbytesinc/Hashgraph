namespace Hashgraph.Mirror.Filters;
/// <summary>
/// Retrieve consensus messages filtered by acocunt id.
/// </summary>
public class AccountIsFilter : IMirrorQueryFilter
{
    /// <summary>
    /// The account id to filter the request by.
    /// </summary>
    private readonly Address _account;
    /// <summary>
    /// Constructor requres the account to filter the request by.
    /// </summary>
    /// <param name="account">
    /// Address of the account to filter the response by.
    /// </param>
    public AccountIsFilter(Address account)
    {
        _account = account;
    }
    /// <summary>
    /// The query parameter name recognized by the remote mirror node.
    /// </summary>
    public string Name => "account.id";

    /// <summary>
    /// The value of the query parameter sent to the mirror node.
    /// </summary>
    public string Value => _account.ToString();
}
