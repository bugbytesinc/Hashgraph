﻿namespace Hashgraph.Test.Record;

[Collection(nameof(NetworkCredentials))]
public class CreateNewTxIdTests
{
    private readonly NetworkCredentials _network;
    public CreateNewTxIdTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Create TxId: Can get Transaction Record")]
    public async Task CanGetCreateTxId()
    {
        await using var client = _network.NewClient();

        var txId = client.CreateNewTxId();

        Assert.NotNull(txId);
    }
    [Fact(DisplayName = "Create TxId: Transaction matches Paying Account")]
    public async Task TransactionNodeMatchesPayingAccount()
    {
        await using var client = _network.NewClient();

        var txId = client.CreateNewTxId();

        Assert.Equal(_network.Payer, txId.Address);
    }
    [Fact(DisplayName = "Create TxId: Transaction Has Valid Increasing Timestamp")]
    public async Task TransactionTimeStampIsReasonable()
    {
        await using var client = _network.NewClient();

        var (seconds, nanos) = Epoch.UniqueSecondsAndNanos(true);
        var txId = client.CreateNewTxId();

        Assert.True(txId.ValidStartSeconds > seconds || (txId.ValidStartSeconds == seconds && txId.ValidStartNanos > nanos));
    }

    [Fact(DisplayName = "Create TxId: Requires Payer Account")]
    public async Task RequiesPayerAccount()
    {
        await using var client = _network.NewClient();
        client.Configure(ctx => ctx.Payer = null);

        var ioe = Assert.Throws<InvalidOperationException>(() =>
        {
            var txId = client.CreateNewTxId();
        });
        Assert.StartsWith("The Payer address has not been configured. Please check that 'Payer' is set in the Client context.", ioe.Message);
    }
    [Fact(DisplayName = "Create TxId: Creating Two in a row increases timestamp.")]
    public async Task CreatingTwoInARowIncreasesTimestamp()
    {
        await using var client = _network.NewClient();

        var txId1 = client.CreateNewTxId();
        var txId2 = client.CreateNewTxId();

        Assert.NotEqual(txId1, txId2);
        Assert.True(txId2.ValidStartSeconds > txId1.ValidStartSeconds || (txId2.ValidStartSeconds == txId1.ValidStartSeconds && txId2.ValidStartNanos > txId1.ValidStartNanos));
    }
    [Fact(DisplayName = "Create TxId: Creating Multiple in parallel produces unique transactions.")]
    public async Task CreatingMultipleInParallelProducesUniqueTxIds()
    {
        await using var client = _network.NewClient();

        async Task<TxId> asyncMethod()
        {
            await Task.Delay(1);
            return client.CreateNewTxId();
        }

        var tasks = Enumerable.Range(0, 20).Select(_ => Task.Run(asyncMethod)).ToArray();

        await Task.WhenAll(tasks);
        for (int i = 0; i < tasks.Length; i++)
        {
            for (int j = i + 1; j < tasks.Length; j++)
            {
#pragma warning disable xUnit1031 // Do not use blocking task operations in test method
                Assert.NotEqual(tasks[i].Result, tasks[j].Result);
#pragma warning restore xUnit1031 // Do not use blocking task operations in test method
            }
        }
    }
    [Fact(DisplayName = "Create TxId: Can Change Address in Method Options")]
    public async Task CanChangeAddressInMethodOptions()
    {
        await using var client = _network.NewClient();

        var address = new Address(Generator.Integer(20, 100), 0, Generator.Integer(20, 100));
        var txId = client.CreateNewTxId(ctx => ctx.Payer = address);

        Assert.Equal(address, txId.Address);
    }
    [Fact(DisplayName = "Create TxId: Can Pin Transaction ID in options.")]
    public async Task CanPinTransactionIdInOptions()
    {
        await using var client = _network.NewClient();

        var txExpected = new TxId(new Address(Generator.Integer(20, 100), 0, Generator.Integer(20, 100)), DateTime.UtcNow);

        var txReturned = client.CreateNewTxId(ctx => ctx.Transaction = txExpected);

        Assert.Equal(txExpected, txReturned);
    }
}