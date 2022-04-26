using System;

namespace Proto;

public sealed partial class NftAllowance
{
    internal NftAllowance(Hashgraph.AssetAllowanceGrant allowance) : this()
    {
        TokenId = new TokenID(allowance.Token);
        Owner = new AccountID(allowance.Owner);
        Spender = new AccountID(allowance.Agent);
        if(allowance.SerialNumbers == null)
        {
            ApprovedForAll = true;
        }
        else
        {
            SerialNumbers.AddRange(allowance.SerialNumbers);
        }
    }
}