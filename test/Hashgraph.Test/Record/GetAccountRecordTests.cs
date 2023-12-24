namespace Hashgraph.Test.Record;

[Collection(nameof(NetworkCredentials))]
public class GetAccountRecordTests
{
    private readonly NetworkCredentials _network;
    public GetAccountRecordTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Account Records: Transaction Records are stored for a limited time.")]
    public async Task TransactionRecordsAreStoredForALimitedTime()
    {
        await using var fx = await TestAccount.CreateAsync(_network);
        var childFeeLImit = 1_000_000;
        var transferAmount = Generator.Integer(200, 500);
        var transactionCount = Generator.Integer(3, 6);
        var childAccount = fx.Record.Address;
        var parentAccount = _network.Payer;
        await fx.Client.TransferAsync(parentAccount, childAccount, transactionCount * (childFeeLImit + transferAmount));
        await using (var client = fx.Client.Clone(ctx => { ctx.Payer = childAccount; ctx.Signatory = fx.PrivateKey; ctx.FeeLimit = childFeeLImit; }))
        {
            for (int i = 0; i < transactionCount; i++)
            {
                await client.TransferAsync(childAccount, parentAccount, transferAmount);
            }
        }
        var records = await fx.Client.GetAccountRecordsAsync(childAccount);
        Assert.NotNull(records);
        Assert.Equal(transactionCount, records.Length);
        foreach (var record in records)
        {
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.Equal(5, record.Transfers.Count);
            Assert.Equal(-transferAmount - (long)record.Fee, record.Transfers[childAccount]);
            Assert.Equal(transferAmount, record.Transfers[parentAccount]);
            Assert.Empty(record.TokenTransfers);
            Assert.Empty(record.AssetTransfers);
            Assert.Empty(record.Royalties);
            Assert.Empty(record.Associations);
        }
    }
    [Fact(DisplayName = "Account Records: Get with Empty Account raises Error.")]
    public async Task EmptyAccountRaisesError()
    {
        await using var client = _network.NewClient();
        var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await client.GetAccountRecordsAsync(null);
        });
        Assert.Equal("account", ane.ParamName);
        Assert.StartsWith("Account Address/Alias is missing. Please check that it is not null.", ane.Message);
    }
    [Fact(DisplayName = "Account Records: Get with Deleted Account raises Error.")]
    public async Task DeletedAccountRaisesError()
    {
        await using var fx = await TestAccount.CreateAsync(_network);
        await fx.Client.DeleteAccountAsync(fx.Record.Address, _network.Payer, fx.PrivateKey);
        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fx.Client.GetAccountRecordsAsync(fx.Record.Address);
        });
        Assert.Equal(ResponseCode.AccountDeleted, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: AccountDeleted", pex.Message);
    }
    [Fact(DisplayName = "Account Records: Get with Deleted Account raises Error (other Delete)")]
    public async Task DeletedAccountRaisesErrorOtherDelete()
    {
        await using var fx = await TestAccount.CreateAsync(_network);
        await fx.Client.DeleteAccountAsync(fx.Record.Address, _network.Payer, ctx => ctx.Signatory = new Signatory(_network.PrivateKey, fx.PrivateKey));
        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fx.Client.GetAccountRecordsAsync(fx.Record.Address);
        });
        Assert.Equal(ResponseCode.AccountDeleted, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: AccountDeleted", pex.Message);
    }
}