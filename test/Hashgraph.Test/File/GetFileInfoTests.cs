using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File
{
    [Collection(nameof(NetworkCredentials))]
    public class GetFileInfoTests
    {
        private readonly NetworkCredentials _network;
        public GetFileInfoTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "File Info: Can Get File Info")]
        public async Task CanCreateAndDeleteAFileAsync()
        {
            await using var test = await TestFile.CreateAsync(_network);

            var info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(test.Contents.Length, info.Size);
            Assert.Equal(test.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { test.PublicKey }, info.Endorsements);
            Assert.False(info.Deleted);
        }
    }
}
