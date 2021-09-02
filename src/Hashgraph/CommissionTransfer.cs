using Proto;
using System.Collections.Generic;
using System.Linq;

namespace Hashgraph
{
    /// <summary>
    /// Represents a token transfer (Token, Payer, Amount, Receiver)
    /// fufilling a royalty or commission payment for the transfer
    /// of a token or asset.
    /// </summary>
    public sealed record CommissionTransfer
    {
        /// <summary>
        /// The Address of the token who's coins have transferred 
        /// to pay the commisison.
        /// </summary>
        public Address Token { get; private init; }
        /// <summary>
        /// The effective paying Address receiving the commission 
        /// transfer value.
        /// </summary>
        public IReadOnlyCollection<Address> Payers { get; private init; }
        /// <summary>
        /// The Address receiving the commission transfer value.
        /// </summary>
        public Address Receiver { get; private init; }
        /// <summary>
        /// The (divisible) amount of coins transferred.
        /// </summary>
        public long Amount { get; init; }
        /// <summary>
        /// Internal Constructor representing the "None" 
        /// version of an commission transfer.
        /// </summary>
        private CommissionTransfer()
        {
            Token = Address.None;
            Payers = new List<Address>().AsReadOnly();
            Receiver = Address.None;
            Amount = 0;
        }
        /// <summary>
        /// Intenral Helper Class to Create Commission Transfer
        /// from raw protobuf.
        /// </summary>        
        internal CommissionTransfer(AssessedCustomFee fee)
        {
            Token = fee.TokenId.AsAddress();
            Receiver = fee.FeeCollectorAccountId.AsAddress();
            Amount = fee.Amount;
            Payers = fee.EffectivePayerAccountId.Select(payerID => payerID.AsAddress()).ToList().AsReadOnly();
        }
    }
}
