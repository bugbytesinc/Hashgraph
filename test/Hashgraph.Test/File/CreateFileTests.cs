using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File
{
    [Collection(nameof(NetworkCredentials))]
    public class CreateFileTests
    {
        private readonly NetworkCredentials _network;
        public CreateFileTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Create File: Can Create")]
        public async Task CanCreateAFileAsync()
        {
            await using var test = await TestFile.CreateAsync(_network);
            Assert.NotNull(test.CreateRecord);
            Assert.NotNull(test.CreateRecord.File);
            Assert.Equal(ResponseCode.Success, test.CreateRecord.Status);
        }
    }
}
