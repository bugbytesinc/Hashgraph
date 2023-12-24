namespace Hashgraph.Test.File;

[Collection(nameof(NetworkCredentials))]
public class CreateFileTests
{
    private readonly NetworkCredentials _network;
    public CreateFileTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Create File: Can Create")]
    public async Task CanCreateAFileAsync()
    {
        await using var test = await TestFile.CreateAsync(_network);
        Assert.NotNull(test.Record);
        Assert.NotNull(test.Record.File);
        Assert.Equal(ResponseCode.Success, test.Record.Status);
    }
    [Fact(DisplayName = "Create File: Can Not Schedule a Create File")]
    public async Task CanNotScheduleACreateFile()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await TestFile.CreateAsync(_network, fx =>
            {
                fx.CreateParams.Signatory = new Signatory(
                    fx.CreateParams.Signatory,
                    new PendingParams
                    {
                        PendingPayer = fxPayer,
                    });
            });
        });
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }
}