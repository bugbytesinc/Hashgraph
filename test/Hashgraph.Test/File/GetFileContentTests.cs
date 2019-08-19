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
        [Fact(DisplayName = "File Content: Get File Content Requires a Fee")]
        public async Task RequiresAFee()
        {
            await using var test = await TestFile.CreateAsync(_network);

            var txId = Generator.GenerateTxId(_network.Payer);
            var contents = await test.Client.GetFileContentAsync(test.Record.File, ctx => ctx.Transaction = txId);
            var record = await test.Client.GetTransactionRecordAsync(txId);
            Assert.True(record.Transfers[_network.Payer] < 0);
        }
    }
}
