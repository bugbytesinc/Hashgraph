using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File
{
    [Collection(nameof(NetworkCredentials))]
    public class SystemRestoreFileTests
    {
        private readonly NetworkCredentials _network;
        public SystemRestoreFileTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "System Restore File: Can Restore File")]
        public async Task CanRestoreAFile()
        {
            var deleteAddress = await _network.GetSystemDeleteAdminAddress();
            if (deleteAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
                return;
            }
            var restoreAddress = await _network.GetSystemUndeleteAdminAddress();
            if (restoreAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Undelete Administrator Account.");
                return;
            }

            await using var fxFile = await TestFile.CreateAsync(_network);

            await fxFile.Client.SystemDeleteFileAsync(fxFile, ctx => ctx.Payer = deleteAddress);

            var receipt = await fxFile.Client.SystemRestoreFileAsync(fxFile, ctx => ctx.Payer = restoreAddress);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(restoreAddress, receipt.Id.Address);

            var info = await fxFile.Client.GetFileInfoAsync(fxFile.Record.File);
            Assert.NotNull(info);
            Assert.Equal(fxFile.Record.File, info.File);
            Assert.Equal(fxFile.CreateParams.Contents.Length, info.Size);
            Assert.Equal(fxFile.CreateParams.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { fxFile.PublicKey }, info.Endorsements);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "System Restore File: Can Restore File using Signatory")]
        public async Task CanRestoreAFileUsingSignatory()
        {
            var deleteAddress = await _network.GetSystemDeleteAdminAddress();
            if (deleteAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
                return;
            }
            var restoreAddress = await _network.GetSystemUndeleteAdminAddress();
            if (restoreAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Restore Administrator Account.");
                return;
            }

            await using var fxFile = await TestFile.CreateAsync(_network);

            await fxFile.Client.SystemDeleteFileAsync(fxFile, ctx => ctx.Payer = deleteAddress);

            var receipt = await fxFile.Client.SystemRestoreFileAsync(fxFile, _network.Signatory, ctx => ctx.Payer = restoreAddress);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(restoreAddress, receipt.Id.Address);

            var info = await fxFile.Client.GetFileInfoAsync(fxFile.Record.File);
            Assert.NotNull(info);
            Assert.Equal(fxFile.Record.File, info.File);
            Assert.Equal(fxFile.CreateParams.Contents.Length, info.Size);
            Assert.Equal(fxFile.CreateParams.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { fxFile.PublicKey }, info.Endorsements);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "System Restore File: Can Restore File and get Record")]
        public async Task CanRestoreAFileWithRecord()
        {
            var deleteAddress = await _network.GetSystemDeleteAdminAddress();
            if (deleteAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
                return;
            }
            var restoreAddress = await _network.GetSystemUndeleteAdminAddress();
            if (restoreAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Restore Administrator Account.");
                return;
            }

            await using var fxFile = await TestFile.CreateAsync(_network);

            await fxFile.Client.SystemDeleteFileAsync(fxFile, ctx => ctx.Payer = deleteAddress);

            var record = await fxFile.Client.SystemRestoreFileWithRecordAsync(fxFile, ctx => ctx.Payer = restoreAddress);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(restoreAddress, record.Id.Address);

            var info = await fxFile.Client.GetFileInfoAsync(fxFile.Record.File);
            Assert.NotNull(info);
            Assert.Equal(fxFile.Record.File, info.File);
            Assert.Equal(fxFile.CreateParams.Contents.Length, info.Size);
            Assert.Equal(fxFile.CreateParams.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { fxFile.PublicKey }, info.Endorsements);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "System Restore File: Can Restore File and get Record using Signatory")]
        public async Task CanRestoreAFileWithRecordUsingSignatory()
        {
            var deleteAddress = await _network.GetSystemDeleteAdminAddress();
            if (deleteAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
                return;
            }
            var restoreAddress = await _network.GetSystemUndeleteAdminAddress();
            if (restoreAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Restore Administrator Account.");
                return;
            }

            await using var fxFile = await TestFile.CreateAsync(_network);

            await fxFile.Client.SystemDeleteFileAsync(fxFile, ctx => ctx.Payer = deleteAddress);

            var record = await fxFile.Client.SystemRestoreFileWithRecordAsync(fxFile, _network.PrivateKey, ctx => ctx.Payer = restoreAddress);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(restoreAddress, record.Id.Address);

            var info = await fxFile.Client.GetFileInfoAsync(fxFile.Record.File);
            Assert.NotNull(info);
            Assert.Equal(fxFile.Record.File, info.File);
            Assert.Equal(fxFile.CreateParams.Contents.Length, info.Size);
            Assert.Equal(fxFile.CreateParams.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { fxFile.PublicKey }, info.Endorsements);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "System Restore File: Can Not Schedule Restore.")]
        public async Task CanNotScheduleRestore()
        {
            var deleteAddress = await _network.GetSystemDeleteAdminAddress();
            if (deleteAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
                return;
            }
            var restoreAddress = await _network.GetSystemUndeleteAdminAddress();
            if (restoreAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Restore Administrator Account.");
                return;
            }

            await using var fxFile = await TestFile.CreateAsync(_network);
            await fxFile.Client.DeleteFileAsync(fxFile, fxFile.PrivateKey);

            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxFile.Client.SystemRestoreFileAsync(
                    fxFile.Record.File,
                    ctx =>
                    {
                        ctx.Payer = restoreAddress;
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
}
