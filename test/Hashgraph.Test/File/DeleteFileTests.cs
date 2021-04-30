using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File
{
    [Collection(nameof(NetworkCredentials))]
    public class DeleteFileTests
    {
        private readonly NetworkCredentials _network;
        public DeleteFileTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Delete File: Can Delete")]
        public async Task CanDeleteAFileAsync()
        {
            await using var test = await TestFile.CreateAsync(_network);

            var result = await test.Client.DeleteFileWithRecordAsync(test.Record.File, test.CreateParams.Signatory);
            Assert.NotNull(result);
            Assert.Equal(ResponseCode.Success, result.Status);

            var info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(0, info.Size);
            Assert.Equal(test.CreateParams.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { test.PublicKey }, info.Endorsements);
            Assert.True(info.Deleted);
        }
        [Fact(DisplayName = "Delete File: Can Delete with Record (no extra Signatory)")]
        public async Task CanDeleteAFileWithRecordNoSignatoryAsync()
        {
            await using var test = await TestFile.CreateAsync(_network);

            var result = await test.Client.DeleteFileWithRecordAsync(test.Record.File, ctx => ctx.Signatory = new Signatory(_network.PrivateKey, test.CreateParams.Signatory));
            Assert.NotNull(result);
            Assert.Equal(ResponseCode.Success, result.Status);

            var info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(0, info.Size);
            Assert.Equal(test.CreateParams.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { test.PublicKey }, info.Endorsements);
            Assert.True(info.Deleted);
        }
        [Fact(DisplayName = "Delete File: Cannot Delete and Imutable File")]
        public async Task CanNotDeleteAnImutableFileAsync()
        {
            await using var test = await TestFile.CreateAsync(_network, fx =>
            {
                fx.CreateParams.Endorsements = Array.Empty<Endorsement>();
            });

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await test.Client.DeleteFileWithRecordAsync(test.Record.File, test.CreateParams.Signatory);
            });
            Assert.Equal(ResponseCode.Unauthorized, tex.Status);
            Assert.StartsWith("Unable to delete file, status: Unauthorized", tex.Message);

            var info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(test.CreateParams.Contents.Length, info.Size);
            Assert.Equal(test.CreateParams.Expiration, info.Expiration);
            Assert.Empty(info.Endorsements);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "Delete File: Can Not Schedule a Delete File")]
        public async Task CanNotScheduleADeleteFile()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxFile = await TestFile.CreateAsync(_network);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxFile.Client.DeleteFileAsync(
                    fxFile.Record.File,
                    new Signatory(
                        fxFile.PrivateKey,
                        new PendingParams
                        {
                            PendingPayer = fxPayer
                        }));
            });
            Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
            Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
        }
    }
}
