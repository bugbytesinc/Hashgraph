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

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await test.Client.GetFileInfoAsync(test.Record.File);
            });
            Assert.Equal(ResponseCode.FileDeleted, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: FileDeleted", pex.Message);

        }
    }
}
