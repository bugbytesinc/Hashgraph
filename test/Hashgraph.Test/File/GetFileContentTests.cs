using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File
{
    [Collection(nameof(NetworkCredentials))]
    public class GetFileContentTests
    {
        private readonly NetworkCredentials _network;
        public GetFileContentTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "File Content: Can Get File Content")]
        public async Task CanGetFileContent()
        {
            await using var test = await TestFile.CreateAsync(_network);

            var retrievedContents = await test.Client.GetFileContentAsync(test.Record.File);
            Assert.Equal(test.Contents, retrievedContents.ToArray());
        }
    }
}
