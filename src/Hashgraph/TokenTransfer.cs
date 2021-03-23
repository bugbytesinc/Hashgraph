using Google.Protobuf.Collections;
using Proto;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hashgraph
{
    /// <summary>
    /// Represents a token transfer (Token, Account, Amount)
    /// </summary>
    public sealed record TokenTransfer
    {
        /// <summary>
        /// The Address of the Token who's coins have transferred.
        /// </summary>
        public Address Token { get; private init; }
        /// <summary>
        /// The Address receiving or sending the token's coins.
        /// </summary>
        public Address Address { get; private init; }
        /// <summary>
        /// The (divisible) amount of coins transferred.  Negative values
        /// indicate an outflow of coins to the <code>Account</code> positive
        /// values indicate an inflow of coins from the associated <code>Account</code>.
        /// </summary>
        public long Amount { get; init; }
        /// <summary>
        /// Internal Constructor representing the "None" version of an
        /// version.  This is a special construct indicating the version
        /// number is not known or is not specified.
        /// </summary>
        private TokenTransfer()
        {
            Token = Address.None;
            Address = Address.None;
            Amount = 0;
        }
        /// <summary>
        /// Public Constructor, an <code>TokenTransfer</code> is immutable after creation.
        /// </summary>
        /// <param name="token">
        /// The Address of the Token who's coins have transferred.
        /// </param>
        /// <param name="address">
        /// The Address receiving or sending the token's coins.
        /// </param>
        /// <param name="amount">
        /// The (divisible) amount of coins transferred.  Negative values
        /// indicate an outflow of coins to the <code>Account</code> positive
        /// values indicate an inflow of coins from the associated <code>Account</code>.
        /// </param>
        public TokenTransfer(Address token, Address address, long amount)
        {
            Token = token;
            Address = address;
            Amount = amount;
        }
    }
    internal static class TokenTransferExtensions
    {
        private static ReadOnlyCollection<TokenTransfer> EMPTY_RESULT = new List<TokenTransfer>().AsReadOnly();
        internal static ReadOnlyCollection<TokenTransfer> Create(RepeatedField<Proto.TokenTransferList> list)
        {
            if (list != null && list.Count > 0)
            {
                var collector = new Dictionary<(Address, Address), long>();
                foreach (var xferList in list)
                {
                    var token = xferList.Token.AsAddress();
                    foreach (var xfer in xferList.Transfers)
                    {
                        var key = (token, xfer.AccountID.AsAddress());
                        collector.TryGetValue(key, out long amount);
                        collector[key] = amount + xfer.Amount;
                    }
                }
                var result = new List<TokenTransfer>(collector.Count);
                foreach (var entry in collector)
                {
                    result.Add(new TokenTransfer(entry.Key.Item1, entry.Key.Item2, entry.Value));
                }
                return result.AsReadOnly();
            }
            return EMPTY_RESULT;
        }
    }
}
