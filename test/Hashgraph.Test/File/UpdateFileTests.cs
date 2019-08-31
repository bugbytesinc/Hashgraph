using Hashgraph.Test.Fixtures;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File
{
    [Collection(nameof(NetworkCredentials))]
    public class UpdateFileTests
    {
        private readonly NetworkCredentials _network;
        public UpdateFileTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "File Update: Can Update File Key")]
        public async Task CanUpdateFileKey()
        {
            await using var test = await TestFile.CreateAsync(_network);

            var (newPublicKey, newPrivateKey) = Generator.KeyPair();
            test.Client.Configure(ctx =>
            {
                ctx.Payer = new Account( _network.Payer, _network.PrivateKey, test.PrivateKey, newPrivateKey);
            });

            var updateRecord = await test.Client.UpdateFileWithRecordAsync(new UpdateFileParams
            {
                File = test.Record.File,
                Endorsements = new Endorsement[] { newPublicKey }
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);

            var info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(test.Contents.Length, info.Size);
            Assert.Equal(test.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { newPublicKey }, info.Endorsements);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "File Update: Can Replace Contents")]
        public async Task CanUpdateFileContents()
        {
            await using var test = await TestFile.CreateAsync(_network);

            var newContents = Encoding.Unicode.GetBytes("Hello Again Hashgraph " + Generator.Code(50));

            var updateRecord = await test.Client.UpdateFileAsync(new UpdateFileParams
            {
                File = test.Record.File,
                Contents = newContents
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);

            var retrievedContents = await test.Client.GetFileContentAsync(test.Record.File);
            Assert.Equal(newContents, retrievedContents.ToArray());
        }
        [Fact(DisplayName = "File Update: Cannot Replace Contents of deleted file")]
        public async Task CanUpdateFileContentsOfDeletedFile()
        {
            await using var test = await TestFile.CreateAsync(_network);

            var deleteResult = await test.Client.DeleteFileAsync(test.Record.File);
            Assert.Equal(ResponseCode.Success, deleteResult.Status);

            var exception = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await test.Client.UpdateFileAsync(new UpdateFileParams
                {
                    File = test.Record.File,
                    Contents = Encoding.Unicode.GetBytes("Hello Again Hashgraph " + Generator.Code(50))
                });
            });
            Assert.StartsWith("Unable to update file, status: FileDeleted", exception.Message);
        }
    }
}
