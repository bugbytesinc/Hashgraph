using Hashgraph.Test.Fixtures;
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

            var result = await test.Client.DeleteFileWithRecordAsync(test.Record.File, test.Signatory);
            Assert.NotNull(result);
            Assert.Equal(ResponseCode.Success, result.Status);

            var info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(0, info.Size);
            Assert.Equal(test.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { test.PublicKey }, info.Endorsements);
            Assert.True(info.Deleted);
        }
    }
}
