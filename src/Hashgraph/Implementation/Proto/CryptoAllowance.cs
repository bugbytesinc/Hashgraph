using System;

namespace Proto;

public sealed partial class CryptoAllowance
{
    internal CryptoAllowance(Hashgraph.CryptoAllowance allowance) : this()
    {
        if (allowance.Amount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(allowance.Amount), "The allowance amount must be greater than or equal to zero.");
        }
        Owner = new AccountID(allowance.Owner);
        Spender = new AccountID(allowance.Agent);
        Amount = allowance.Amount;
    }
}