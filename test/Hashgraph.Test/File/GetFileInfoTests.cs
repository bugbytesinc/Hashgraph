using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File
{
    [Collection(nameof(NetworkCredentials))]
    public class GetFileInfoTests
    {
        private readonly NetworkCredentials _network;
        public GetFileInfoTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "File Info: Can Get File Info")]
        public async Task CanGetFileInfo()
        {
            await using var test = await TestFile.CreateAsync(_network);

            var info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(test.CreateParams.Memo, info.Memo);
            Assert.Equal(test.CreateParams.Contents.Length, info.Size);
            Assert.Equal(test.CreateParams.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { test.PublicKey }, info.Endorsements);
            Assert.False(info.Deleted);
            // NETWORK V0.21.0 DEFECT vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(info.Ledger.ToArray());
            // NETWORK V0.21.0 DEFECT: ^^^^
        }
        [Fact(DisplayName = "File Info: Can Get Imutable File Info")]
        public async Task CanGetImutableFileInfo()
        {
            await using var test = await TestFile.CreateAsync(_network, fx => {
                fx.CreateParams.Endorsements = Array.Empty<Endorsement>();
            });

            var info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(test.CreateParams.Memo, info.Memo);
            Assert.Equal(test.CreateParams.Contents.Length, info.Size);
            Assert.Equal(test.CreateParams.Expiration, info.Expiration);
            Assert.Empty(info.Endorsements);
            Assert.False(info.Deleted);
            // NETWORK V0.21.0 DEFECT vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(info.Ledger.ToArray());
            // NETWORK V0.21.0 DEFECT: ^^^^
        }
        [Fact(DisplayName = "File Info: Can Get Empty File Info")]
        public async Task CanGetEmptyFileInfo()
        {
            await using var test = await TestFile.CreateAsync(_network, fx => {
                fx.CreateParams.Contents = ReadOnlyMemory<byte>.Empty;
            });

            var info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(test.CreateParams.Memo, info.Memo);
            Assert.Equal(0, info.Size);
            Assert.Equal(test.CreateParams.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { test.PublicKey }, info.Endorsements);
            Assert.False(info.Deleted);
            // NETWORK V0.21.0 DEFECT vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(info.Ledger.ToArray());
            // NETWORK V0.21.0 DEFECT: ^^^^
        }
        [Fact(DisplayName = "File Info: Can Get Imutable Empty File Info")]
        public async Task CanGetImutableEmptyFileInfo()
        {
            await using var test = await TestFile.CreateAsync(_network, fx => {
                fx.CreateParams.Endorsements = Array.Empty<Endorsement>();
                fx.CreateParams.Contents = ReadOnlyMemory<byte>.Empty;
            });

            var info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(test.CreateParams.Memo, info.Memo);
            Assert.Equal(0, info.Size);
            Assert.Equal(test.CreateParams.Expiration, info.Expiration);
            Assert.Empty(info.Endorsements);
            Assert.False(info.Deleted);
            // NETWORK V0.21.0 DEFECT vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(info.Ledger.ToArray());
            // NETWORK V0.21.0 DEFECT: ^^^^
        }
    }
}
