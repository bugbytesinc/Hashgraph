using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class GetFileInfoTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public GetFileInfoTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "File Info: Can Get File Info")]
        public async Task CanCreateAndDeleteAFileAsync()
        {
            await using var test = await TestFileInstance.CreateAsync(_networkCredentials);

            var info = await test.Client.GetFileInfoAsync(test.CreateRecord.File);
            Assert.NotNull(info);
            Assert.Equal(test.CreateRecord.File, info.File);
            Assert.Equal(test.Contents.Length, info.Size);
            Assert.Equal(test.Expiration, info.Expiration);
            Assert.Equal(new Endorsements(test.PublicKey), info.Endorsements);
            Assert.False(info.Deleted);
        }
    }
}
