using Hashgraph;

namespace Proto;

public sealed partial class NftAllowance
{
    internal NftAllowance(Hashgraph.AssetAllowance allowance) : this()
    {
        TokenId = new TokenID(allowance.Token);
        Owner = new AccountID(allowance.Owner);
        Spender = new AccountID(allowance.Agent);
        if (allowance.SerialNumbers == null)
        {
            ApprovedForAll = true;
        }
        else
        {
            SerialNumbers.AddRange(allowance.SerialNumbers);
        }
        if (!allowance.OwnersAgent.IsNullOrNone())
        {
            DelegatingSpender = new AccountID(allowance.OwnersAgent);
        }
    }
}