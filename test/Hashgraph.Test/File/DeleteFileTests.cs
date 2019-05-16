using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class DeleteFileTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public DeleteFileTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "Delete File: Can Delete")]
        public async Task CanDeleteAFileAsync()
        {
            await using var test = await TestFileInstance.CreateAsync(_networkCredentials);

            var result = await test.Client.DeleteFileAsync(test.CreateRecord.File);
            Assert.NotNull(result);            
            Assert.Equal(ResponseCode.Success, result.Status);

            var exception = await Assert.ThrowsAnyAsync<PrecheckException>(async () => {
                await test.Client.GetFileInfoAsync(test.CreateRecord.File);
            });
        }
    }
}
