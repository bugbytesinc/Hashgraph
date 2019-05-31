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

            var result = await test.Client.DeleteFileAsync(test.CreateRecord.File);
            Assert.NotNull(result);
            Assert.Equal(ResponseCode.Success, result.Status);

            var exception = await Assert.ThrowsAnyAsync<PrecheckException>(async () =>
            {
                await test.Client.GetFileInfoAsync(test.CreateRecord.File);
            });
        }
    }
}
