using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class CreateFileTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public CreateFileTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "Create File: Can Create")]
        public async Task CanCreateAFileAsync()
        {
            await using var test = await TestFileInstance.CreateAsync(_networkCredentials);
            Assert.NotNull(test.CreateRecord);
            Assert.NotNull(test.CreateRecord.File);
            Assert.Equal(ResponseCode.Success, test.CreateRecord.Status);
        }
    }
}
