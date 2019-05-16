using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class GetFileContentTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public GetFileContentTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "File Content: Can Get File Content")]
        public async Task CanGetFileContent()
        {
            await using var test = await TestFileInstance.CreateAsync(_networkCredentials);

            var retrievedContents = await test.Client.GetFileContentAsync(test.CreateRecord.File);
            Assert.Equal(test.Contents, retrievedContents.ToArray());
        }
    }
}
