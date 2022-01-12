using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using Proto;
using System;
using Xunit;

namespace Hashgraph.Tests;

public class TxIdTests
{
    [Fact(DisplayName = "TxId: Equivalent TxIds are considered Equal")]
    public void EquivalentTxIdAreConsideredEqual()
    {
        var transactionId = Generator.TransactionID();
        var txId1 = transactionId.AsTxId();
        var txId2 = transactionId.AsTxId();
        Assert.Equal(txId1, txId2);
        Assert.True(txId1 == txId2);
        Assert.False(txId1 != txId2);
        Assert.Equal(txId1.GetHashCode(), txId2.GetHashCode());
    }
    [Fact(DisplayName = "TxId: Equivalent TxIds with Nonces are considered Equal")]
    public void EquivalentTxIdWithNoncesAreConsideredEqual()
    {
        var transactionId = Generator.TransactionID();
        transactionId.Nonce = Generator.Integer(10, 20);
        var txId1 = transactionId.AsTxId();
        var txId2 = transactionId.AsTxId();
        Assert.Equal(txId1, txId2);
        Assert.True(txId1 == txId2);
        Assert.False(txId1 != txId2);
        Assert.Equal(txId1.GetHashCode(), txId2.GetHashCode());
    }
    [Fact(DisplayName = "TxId: Disimilar TxIds are not considered Equal")]
    public void DisimilarTxIdsAreNotConsideredEqual()
    {
        var txId1 = Generator.TransactionID().AsTxId();
        var txId2 = Generator.TransactionID().AsTxId();
        Assert.NotEqual(txId1, txId2);
        Assert.False(txId1 == txId2);
        Assert.True(txId1 != txId2);
        Assert.NotEqual(txId1.GetHashCode(), txId2.GetHashCode());
    }
    [Fact(DisplayName = "TxId: TxIds with Disimilar Nonces are not considered Equal")]
    public void TxIdsWithDisimilarNoncesAreNotConsideredEqual()
    {
        var transactionId = Generator.TransactionID();
        transactionId.Nonce = Generator.Integer(10, 20);
        var txId1 = transactionId.AsTxId();
        transactionId.Nonce = Generator.Integer(30, 40);
        var txId2 = transactionId.AsTxId();
        Assert.NotEqual(txId1, txId2);
        Assert.False(txId1 == txId2);
        Assert.True(txId1 != txId2);
        Assert.NotEqual(txId1.GetHashCode(), txId2.GetHashCode());
    }
    [Fact(DisplayName = "TxId: Exposes Account Value")]
    public void ExposesAccountValue()
    {
        var transactionId = Generator.TransactionID();
        var txId = transactionId.AsTxId();
        Assert.Equal(txId.Address, transactionId.AccountID.AsAddress());
    }
    [Fact(DisplayName = "TxId: Exposes Valid Start Seconds")]
    public void ExposesValidStartSeconds()
    {
        var transactionId = Generator.TransactionID();
        var txId = transactionId.AsTxId();
        Assert.Equal(txId.ValidStartSeconds, transactionId.TransactionValidStart.Seconds);
    }
    [Fact(DisplayName = "TxId: Exposes VBalid Start Nanoseconds")]
    public void ExposesValidStartNano()
    {
        var transactionId = Generator.TransactionID();
        var txId = transactionId.AsTxId();
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
        Assert.False(empty.Pending);
        Assert.Equal(0, empty.Nonce);
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
        Assert.False(txId.Pending);
        Assert.Equal(0, txId.Nonce);
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
        Assert.False(txId.Pending);
        Assert.Equal(0, txId.Nonce);
    }
    [Fact(DisplayName = "TxId: Can Create a Transaction Id Mannually with Nonce")]
    public void CreateManuallyWithNonce()
    {
        var address = new Address(Generator.Integer(0, 10), Generator.Integer(0, 10), Generator.Integer(10, 20));
        var dateTime = DateTime.UtcNow;
        var (seconds, nanos) = Epoch.FromDate(dateTime);
        var nonce = Generator.Integer(5, 20);

        var txId = new TxId(address, dateTime, false, nonce);

        Assert.Equal(address, txId.Address);
        Assert.Equal(seconds, txId.ValidStartSeconds);
        Assert.Equal(nanos, txId.ValidStartNanos);
        Assert.False(txId.Pending);
        Assert.Equal(nonce, txId.Nonce);
    }
    [Fact(DisplayName = "TxId: Can Create a Pending Transaction Id Mannually")]
    public void CanCreateAPendingTransactionIdMannually()
    {
        var address = new Address(Generator.Integer(0, 10), Generator.Integer(0, 10), Generator.Integer(10, 20));
        var dateTime = DateTime.UtcNow;
        var (seconds, nanos) = Epoch.FromDate(dateTime);
        var nonce = Generator.Integer(5, 20);

        var txId = new TxId(address, dateTime, true, nonce);

        Assert.Equal(address, txId.Address);
        Assert.Equal(seconds, txId.ValidStartSeconds);
        Assert.Equal(nanos, txId.ValidStartNanos);
        Assert.True(txId.Pending);
        Assert.Equal(nonce, txId.Nonce);
    }
}