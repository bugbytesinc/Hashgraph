using Hashgraph;
using System;

namespace Proto
{
    public sealed partial class TokenTransferList
    {
        internal TokenTransferList(Address token, Address fromAddress, Address toAddress, long amount) : this()
        {
            if (fromAddress is null)
            {
                throw new ArgumentNullException(nameof(toAddress), "Account to transfer from is missing. Please check that it is not null.");
            }
            if (toAddress is null)
            {
                throw new ArgumentNullException(nameof(toAddress), "Account to transfer to is missing. Please check that it is not null.");
            }
            if (amount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "The amount to transfer must be non-negative.");
            }
            Token = new TokenID(token);
            Transfers.Add(new AccountAmount(fromAddress, -amount));
            Transfers.Add(new AccountAmount(toAddress, amount));
        }
    }
}