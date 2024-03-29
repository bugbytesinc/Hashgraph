﻿namespace Hashgraph.Test.Token;

[Collection(nameof(NetworkCredentials))]
public class GetPendingTransactionInfoTests
{
    private readonly NetworkCredentials _network;
    public GetPendingTransactionInfoTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Pending Transaction Info: Can Get Scheduled Transaction Info")]
    public async Task CanGetTokenInfo()
    {
        await using var fx = await TestPendingTransfer.CreateAsync(_network);
        Assert.Equal(ResponseCode.Success, fx.Record.Status);

        var info = await fx.PayingAccount.Client.GetPendingTransactionInfoAsync(fx.Record.Pending.Id);
        Assert.Equal(fx.Record.Pending.Id, info.Id);
        Assert.Equal(fx.Record.Pending.TxId, info.TxId);
        Assert.Equal(_network.Payer, info.Creator);
        Assert.Equal(fx.PayingAccount, info.Payer);
        Assert.Single(info.Endorsements);
        Assert.Equal(new Endorsement(fx.PayingAccount.PublicKey), info.Endorsements[0]);
        Assert.Equal(new Endorsement(fx.PublicKey), info.Administrator);
        Assert.Equal(fx.Memo, info.Memo);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Null(info.Executed);
        Assert.Null(info.Deleted);
        Assert.False(info.PendingTransactionBody.IsEmpty);
        AssertHg.NotEmpty(info.Ledger);
        Assert.False(info.DelayExecution);
    }
}