using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File;

[Collection(nameof(NetworkCredentials))]
public class SystemDeleteFileTests
{
    private readonly NetworkCredentials _network;
    public SystemDeleteFileTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "System Delete File: Can Delete File")]
    public async Task CanDeleteAFile()
    {
        var systemAddress = await _network.GetSystemDeleteAdminAddress();
        if (systemAddress is null)
        {
            _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
            return;
        }

        await using var fxFile = await TestFile.CreateAsync(_network);

        var receipt = await fxFile.Client.SystemDeleteFileAsync(fxFile, ctx => ctx.Payer = systemAddress);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(systemAddress, receipt.Id.Address);

        var info = await fxFile.Client.GetFileInfoAsync(fxFile.Record.File);
        Assert.NotNull(info);
        Assert.Equal(fxFile.Record.File, info.File);
        Assert.Equal(fxFile.CreateParams.Contents.Length, info.Size);
        Assert.Equal(fxFile.CreateParams.Expiration, info.Expiration);
        Assert.Equal(new Endorsement[] { fxFile.PublicKey }, info.Endorsements);
        Assert.True(info.Deleted);
        AssertHg.Equal(_network.Ledger, info.Ledger);
        // v0.34.0 Churn
        //Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        //Assert.Equal(Address.None, info.AutoRenewAccount);
    }
    [Fact(DisplayName = "System Delete File: Can Delete File using Signatory")]
    public async Task CanDeleteAFileUsingSignatory()
    {
        var systemAddress = await _network.GetSystemDeleteAdminAddress();
        if (systemAddress is null)
        {
            _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
            return;
        }

        await using var fxFile = await TestFile.CreateAsync(_network);

        var receipt = await fxFile.Client.SystemDeleteFileAsync(fxFile, _network.Signatory, ctx => ctx.Payer = systemAddress);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(systemAddress, receipt.Id.Address);

        var info = await fxFile.Client.GetFileInfoAsync(fxFile.Record.File);
        Assert.NotNull(info);
        Assert.Equal(fxFile.Record.File, info.File);
        Assert.Equal(fxFile.CreateParams.Contents.Length, info.Size);
        Assert.Equal(fxFile.CreateParams.Expiration, info.Expiration);
        Assert.Equal(new Endorsement[] { fxFile.PublicKey }, info.Endorsements);
        Assert.True(info.Deleted);
        AssertHg.Equal(_network.Ledger, info.Ledger);
        // v0.34.0 Churn
        //Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        //Assert.Equal(Address.None, info.AutoRenewAccount);
    }
    [Fact(DisplayName = "System Delete File: Can Delete File and get Record")]
    public async Task CanDeleteAFileWithRecord()
    {
        var systemAddress = await _network.GetSystemDeleteAdminAddress();
        if (systemAddress is null)
        {
            _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
            return;
        }

        await using var fxFile = await TestFile.CreateAsync(_network);

        var record = await fxFile.Client.SystemDeleteFileWithRecordAsync(fxFile, ctx => ctx.Payer = systemAddress);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(systemAddress, record.Id.Address);

        var info = await fxFile.Client.GetFileInfoAsync(fxFile.Record.File);
        Assert.NotNull(info);
        Assert.Equal(fxFile.Record.File, info.File);
        Assert.Equal(fxFile.CreateParams.Contents.Length, info.Size);
        Assert.Equal(fxFile.CreateParams.Expiration, info.Expiration);
        Assert.Equal(new Endorsement[] { fxFile.PublicKey }, info.Endorsements);
        Assert.True(info.Deleted);
        AssertHg.Equal(_network.Ledger, info.Ledger);
        // v0.34.0 Churn
        //Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        //Assert.Equal(Address.None, info.AutoRenewAccount);
    }
    [Fact(DisplayName = "System Delete File: Can Delete File and get Record using Signatory")]
    public async Task CanDeleteAFileWithRecordUsingSignatory()
    {
        var systemAddress = await _network.GetSystemDeleteAdminAddress();
        if (systemAddress is null)
        {
            _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
            return;
        }

        await using var fxFile = await TestFile.CreateAsync(_network);

        var record = await fxFile.Client.SystemDeleteFileWithRecordAsync(fxFile, _network.PrivateKey, ctx => ctx.Payer = systemAddress);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(systemAddress, record.Id.Address);

        var info = await fxFile.Client.GetFileInfoAsync(fxFile.Record.File);
        Assert.NotNull(info);
        Assert.Equal(fxFile.Record.File, info.File);
        Assert.Equal(fxFile.CreateParams.Contents.Length, info.Size);
        Assert.Equal(fxFile.CreateParams.Expiration, info.Expiration);
        Assert.Equal(new Endorsement[] { fxFile.PublicKey }, info.Endorsements);
        Assert.True(info.Deleted);
        AssertHg.Equal(_network.Ledger, info.Ledger);
        // v0.34.0 Churn
        //Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        //Assert.Equal(Address.None, info.AutoRenewAccount);
    }
    [Fact(DisplayName = "System Delete File: Can Not Schedule Delete.")]
    public async Task CanNotScheduleDelete()
    {
        var systemAddress = await _network.GetSystemDeleteAdminAddress();
        if (systemAddress is null)
        {
            _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
            return;
        }

        await using var fxFile = await TestFile.CreateAsync(_network);
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxFile.Client.SystemDeleteFileAsync(
                fxFile.Record.File,
                ctx =>
                {
                    ctx.Payer = systemAddress;
                    ctx.Signatory = new Signatory(
                        _network.PrivateKey,
                        new PendingParams { PendingPayer = fxPayer }
                    );
                });
        });
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }
}