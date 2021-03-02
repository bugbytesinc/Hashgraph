using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using System;
using Xunit;

namespace Hashgraph.Tests
{
    public class TxIdTests
    {
        [Fact(DisplayName = "TxId: Equivalent TxIds are considered Equal")]
        public void EquivalentTxIdAreConsideredEqual()
        {
            var transactionId = Generator.TransactionID();
            var txId1 = transactionId.ToTxId();
            var txId2 = transactionId.ToTxId();
            Assert.Equal(txId1, txId2);
            Assert.True(txId1 == txId2);
            Assert.False(txId1 != txId2);
            Assert.Equal(txId1.GetHashCode(), txId2.GetHashCode());
        }
        [Fact(DisplayName = "TxId: Disimilar TxIdes are not considered Equal")]
        public void DisimilarTxIdesAreNotConsideredEqual()
        {
            var txId1 = Generator.TransactionID().ToTxId();
            var txId2 = Generator.TransactionID().ToTxId();
            Assert.NotEqual(txId1, txId2);
            Assert.False(txId1 == txId2);
            Assert.True(txId1 != txId2);
            Assert.NotEqual(txId1.GetHashCode(), txId2.GetHashCode());
        }
        [Fact(DisplayName = "TxId: Exposes Account Value")]
        public void ExposesAccountValue()
        {
            var transactionId = Generator.TransactionID();
            var txId = transactionId.ToTxId();
            Assert.Equal(txId.Address, transactionId.AccountID.ToAddress());
        }
        [Fact(DisplayName = "TxId: Exposes Valid Start Seconds")]
        public void ExposesValidStartSeconds()
        {
            var transactionId = Generator.TransactionID();
            var txId = transactionId.ToTxId();
            Assert.Equal(txId.ValidStartSeconds, transactionId.TransactionValidStart.Seconds);
        }
        [Fact(DisplayName = "TxId: Exposes VBalid Start Nanoseconds")]
        public void ExposesValidStartNano()
        {
            var transactionId = Generator.TransactionID();
            var txId = transactionId.ToTxId();
            Assert.Equal(txId.ValidStartNanos, transactionId.TransactionValidStart.Nanos);
        }
        [Fact(DisplayName = "TxId: Empty Transaction exposes all zeros")]
        public void EmptyTransaction()
        {
            var empty = TxId.None;
            Assert.Equal(Address.None, empty.Address);
            Assert.Equal(new Address(0, 0, 0), empty.Address);
            Assert.Equal(0, empty.ValidStartSeconds);
            Assert.Equal(0, empty.ValidStartNanos);
        }
        [Fact(DisplayName = "TxId: Can Create a Transaction Id Mannually with Seconds and Nanos")]
        public void CreateManuallyWithSecondsAndNanos()
        {
            var address = new Address(Generator.Integer(0, 10), Generator.Integer(0, 10), Generator.Integer(10, 20));
            var totalNanos = Epoch.UniqueClockNanosAfterDrift();
            var seconds = totalNanos / 1_000_000_000;
            var nanos = (int)(totalNanos % 1_000_000_000);

            var txId = new TxId(address, seconds, nanos);

            Assert.Equal(address, txId.Address);
            Assert.Equal(seconds, txId.ValidStartSeconds);
            Assert.Equal(nanos, txId.ValidStartNanos);
        }
        [Fact(DisplayName = "TxId: Can Create a Transaction Id Mannually with DateTime")]
        public void CreateManuallyWithDateTime()
        {
            var address = new Address(Generator.Integer(0, 10), Generator.Integer(0, 10), Generator.Integer(10, 20));
            var dateTime = DateTime.UtcNow;
            var (seconds, nanos) = Epoch.FromDate(dateTime);

            var txId = new TxId(address, dateTime);

            Assert.Equal(address, txId.Address);
            Assert.Equal(seconds, txId.ValidStartSeconds);
            Assert.Equal(nanos, txId.ValidStartNanos);
        }
    }
}
