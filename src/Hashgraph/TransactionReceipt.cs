using Google.Protobuf.Collections;
using Hashgraph.Implementation;
using Proto;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Hashgraph
{
    /// <summary>
    /// The details returned from the network after consensus 
    /// has been reached for a network request.
    /// </summary>
    public record TransactionReceipt
    {
        /// <summary>
        /// The Transaction ID associated with the request.
        /// </summary>
        public TxId Id { get; internal init; }
        /// <summary>
        /// The response code returned from the server.
        /// </summary>
        public ResponseCode Status { get; internal init; }
        /// <summary>
        /// The current exchange between USD and hBars as
        /// broadcast by the hedera Network.
        /// </summary>
        /// <remarks>
        /// Not all Receipts and Records will have this information
        /// returned from the network.  This value can be <code>null</code>.
        /// </remarks>
        public ExchangeRate? CurrentExchangeRate { get; internal init; }
        /// <summary>
        /// The next/future exchange between USD and 
        /// hBars as broadcast by the hedera Network.
        /// </summary>
        /// <remarks>
        /// Not all Receipts and Records will have this information
        /// returned from the network.  This value can be <code>null</code>.
        /// </remarks>
        public ExchangeRate? NextExchangeRate { get; internal init; }
        /// <summary>
        /// If this transaction resulted in the pending (to be scheduled)
        /// transaction retained by the network, this property will contain
        /// the identifier of the pending transaction record.  This includes 
        /// the identifier of the pending transaction as well as the bytes
        /// representing the transaction which must be signed by the 
        /// remaining parties.
        /// </summary>
        public PendingTransaction? Pending { get; internal init; }
        /// <summary>
        /// Internal Constructor of the record.
        /// </summary>
        /// <param name="receipt">Network Receipt Containing Info</param>
        internal TransactionReceipt(NetworkResult result)
        {
            var receipt = result.Receipt;
            Id = result.TransactionID.AsTxId();
            Status = (ResponseCode)receipt.Status;
            if (receipt.ExchangeRate is not null)
            {
                CurrentExchangeRate = receipt.ExchangeRate.CurrentRate?.ToExchangeRate();
                NextExchangeRate = receipt.ExchangeRate.NextRate?.ToExchangeRate();
            }
            if (receipt.ScheduleID is not null)
            {
                Pending = new PendingTransaction
                {
                    Id = receipt.ScheduleID.ToAddress(),
                    TxId = receipt.ScheduledTransactionID.AsTxId()
                };
            }
        }
    }
    internal static class TransactionReceiptExtensions
    {
        private static ReadOnlyCollection<TransactionReceipt> EMPTY_RESULT = new List<TransactionReceipt>().AsReadOnly();
        internal static ReadOnlyCollection<TransactionReceipt> Create(this RepeatedField<Proto.TransactionReceipt> list, Proto.TransactionReceipt first, TransactionID transactionId)
        {
            var count = (first != null ? 1 : 0) + (list != null ? list.Count : 0);
            if (count > 0)
            {
                var result = new List<TransactionReceipt>(count);
                if (first != null)
                {
                    result.Add(new NetworkResult
                    {
                        TransactionID = transactionId,
                        Receipt = first
                    }.ToReceipt());
                }
                if (list != null && list.Count > 0)
                {
                    foreach (var entry in list)
                    {
                        result.Add(new NetworkResult
                        {
                            TransactionID = transactionId,
                            Receipt = entry
                        }.ToReceipt());
                    }
                }
                return result.AsReadOnly();
            }
            return EMPTY_RESULT;
        }
    }
}
