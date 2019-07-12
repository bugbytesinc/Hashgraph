using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using Proto;
using Xunit;

namespace Hashgraph.Tests
{
    public class TxIdTests
    {        
        [Fact(DisplayName = "TxId: Equivalent TxIds are considered Equal")]
        public void EquivalentTxIdAreConsideredEqual()
        {
            var transactionId = Generator.TransactionID();
            var txId1 = Protobuf.FromTransactionId(transactionId);
            var txId2 = Protobuf.FromTransactionId(transactionId);
            Assert.Equal(txId1, txId2);
            Assert.True(txId1 == txId2);
            Assert.False(txId1 != txId2);
            Assert.Equal(txId1.GetHashCode(), txId2.GetHashCode());
        }
        [Fact(DisplayName = "TxId: Disimilar TxIdes are not considered Equal")]
        public void DisimilarTxIdesAreNotConsideredEqual()
        {
            var txId1 = Protobuf.FromTransactionId(Generator.TransactionID());
            var txId2 = Protobuf.FromTransactionId(Generator.TransactionID());
            Assert.NotEqual(txId1,txId2);
            Assert.False(txId1 == txId2);
            Assert.True(txId1 != txId2);
            Assert.NotEqual(txId1.GetHashCode(), txId2.GetHashCode());
        }
        [Fact(DisplayName = "TxId: Exposes Account Value")]
        public void ExposesAccountValue()
        {
            var transactionId = Generator.TransactionID();
            var txId = Protobuf.FromTransactionId(transactionId);
            Assert.Equal(txId.Address, Protobuf.FromAccountID(transactionId.AccountID));
        }
        [Fact(DisplayName = "TxId: Exposes Valid Start Seconds")]
        public void ExposesValidStartSeconds()
        {
            var transactionId = Generator.TransactionID();
            var txId = Protobuf.FromTransactionId(transactionId);
            Assert.Equal(txId.ValidStartSeconds, transactionId.TransactionValidStart.Seconds);
        }
        [Fact(DisplayName = "TxId: Exposes VBalid Start Nanoseconds")]
        public void ExposesValidStartNano()
        {
            var transactionId = Generator.TransactionID();
            var txId = Protobuf.FromTransactionId(transactionId);
            Assert.Equal(txId.ValidStartNanos, transactionId.TransactionValidStart.Nanos);
        }
        [Fact(DisplayName = "TxId: Empty Transaction exposes all zeros")]
        public void EmptyTransaction()
        {
            var empty = new TxId();
            Assert.Equal(new Address(0, 0, 0), empty.Address);
            Assert.Equal(0, empty.ValidStartSeconds);
            Assert.Equal(0, empty.ValidStartNanos);
        }
    }
}
