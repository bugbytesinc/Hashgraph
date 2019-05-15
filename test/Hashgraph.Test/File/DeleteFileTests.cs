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

            var info = await test.Client.GetFileInfoAsync(test.CreateRecord.File);
            Assert.NotNull(info);
            Assert.Equal(test.CreateRecord.File, info.File);
            Assert.Equal(0, info.Size);
            Assert.Equal(test.Expiration, info.Expiration);
            Assert.Equal(new Endorsements(test.PublicKey), info.Endorsements);
            Assert.True(info.Deleted);
        }
    }
}
