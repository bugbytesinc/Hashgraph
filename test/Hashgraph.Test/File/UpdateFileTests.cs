using Hashgraph.Test.Fixtures;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.File
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class UpdateFileTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public UpdateFileTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "File Update: Can Update File Expiration")]
        public async Task CanUpdateFileExpiration()
        {
            await using var test = await TestFileInstance.CreateAsync(_networkCredentials);

            var newExpiration = test.Expiration.AddDays(2);

            var updateRecord = await test.Client.UpdateFileAsync(new UpdateFileParams {
                File = test.CreateRecord.File,
                Expiration = newExpiration
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);

            var info = await test.Client.GetFileInfoAsync(test.CreateRecord.File);
            Assert.NotNull(info);
            Assert.Equal(test.CreateRecord.File, info.File);
            Assert.Equal(test.Contents.Length, info.Size);
            Assert.Equal(newExpiration, info.Expiration);
            Assert.Equal(new Endorsements(test.PublicKey), info.Endorsements);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "File Update: Can Update File Key")]
        public async Task CanUpdateFileKey()
        {
            await using var test = await TestFileInstance.CreateAsync(_networkCredentials);

            var (newPublicKey, newPrivateKey) = Generator.KeyPair();
            test.Client.Configure(ctx => {
                ctx.Payer = new Account(
                    _networkCredentials.AccountRealm,
                    _networkCredentials.AccountShard,
                    _networkCredentials.AccountNumber,
                    _networkCredentials.AccountPrivateKey,
                    test.PrivateKey,
                    newPrivateKey);
            });

            var updateRecord = await test.Client.UpdateFileAsync(new UpdateFileParams
            {
                File = test.CreateRecord.File,
                Endorsements = new Endorsements(newPublicKey)
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);

            var info = await test.Client.GetFileInfoAsync(test.CreateRecord.File);
            Assert.NotNull(info);
            Assert.Equal(test.CreateRecord.File, info.File);
            Assert.Equal(test.Contents.Length, info.Size);
            Assert.Equal(test.Expiration, info.Expiration);
            Assert.Equal(new Endorsements(newPublicKey), info.Endorsements);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "File Update: Can Replace Contents")]
        public async Task CanUpdateFileContents()
        {
            await using var test = await TestFileInstance.CreateAsync(_networkCredentials);

            var newContents = Encoding.Unicode.GetBytes("Hello Again Hashgraph " + Generator.Code(50));

            var updateRecord = await test.Client.UpdateFileAsync(new UpdateFileParams
            {
                File = test.CreateRecord.File,
                Contents = newContents
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);

            var retrievedContents = await test.Client.GetFileContentAsync(test.CreateRecord.File);
            Assert.Equal(newContents, retrievedContents.ToArray());
        }
        [Fact(DisplayName = "File Update: Can Replace Contents of deleted file?")]
        public async Task CanUpdateFileContentsOfDeletedFile()
        {
            await using var test = await TestFileInstance.CreateAsync(_networkCredentials);

            var deleteResult = await test.Client.DeleteFileAsync(test.CreateRecord.File);
            Assert.Equal(ResponseCode.Success, deleteResult.Status);

            var newContents = Encoding.Unicode.GetBytes("Hello Again Hashgraph " + Generator.Code(50));

            var updateRecord = await test.Client.UpdateFileAsync(new UpdateFileParams
            {
                File = test.CreateRecord.File,
                Contents = newContents
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);

            var info = await test.Client.GetFileInfoAsync(test.CreateRecord.File);
            Assert.NotNull(info);
            Assert.Equal(test.CreateRecord.File, info.File);
            Assert.Equal(newContents.Length, info.Size);
            Assert.Equal(test.Expiration, info.Expiration);
            Assert.Equal(new Endorsements(test.PublicKey), info.Endorsements);
            Assert.True(info.Deleted);

            var retrievedContents = await test.Client.GetFileContentAsync(test.CreateRecord.File);
            Assert.Equal(newContents, retrievedContents.ToArray());
        }
    }
}
