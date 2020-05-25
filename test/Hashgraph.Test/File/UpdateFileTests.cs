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
            var updateRecord = await test.Client.UpdateFileWithRecordAsync(new UpdateFileParams
            {
                File = test.Record.File,
                Endorsements = new Endorsement[] { newPublicKey },
                Signatory = new Signatory(_network.PrivateKey, test.PrivateKey, newPrivateKey)
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);

            var info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(test.CreateParams.Contents.Length, info.Size);
            Assert.Equal(test.CreateParams.Expiration, info.Expiration);
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
                Contents = newContents,
                Signatory = test.CreateParams.Signatory
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);

            var retrievedContents = await test.Client.GetFileContentAsync(test.Record.File);
            Assert.Equal(newContents, retrievedContents.ToArray());
        }
        [Fact(DisplayName = "File Update: Cannot Replace Contents of deleted file")]
        public async Task CanUpdateFileContentsOfDeletedFile()
        {
            await using var test = await TestFile.CreateAsync(_network);

            var deleteResult = await test.Client.DeleteFileAsync(test.Record.File, test.CreateParams.Signatory);
            Assert.Equal(ResponseCode.Success, deleteResult.Status);

            var exception = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await test.Client.UpdateFileAsync(new UpdateFileParams
                {
                    File = test.Record.File,
                    Contents = Encoding.Unicode.GetBytes("Hello Again Hashgraph " + Generator.Code(50)),
                    Signatory = test.CreateParams.Signatory
                });
            });
            Assert.StartsWith("Unable to update file, status: FileDeleted", exception.Message);
        }
        [Fact(DisplayName = "File Update: Can Update File after Rotating Keys")]
        public async Task CanUpdateFileAfterKeyRotation()
        {
            await using var test = await TestFile.CreateAsync(_network);

            var (newPublicKey1, newPrivateKey1) = Generator.KeyPair();
            var (newPublicKey2, newPrivateKey2) = Generator.KeyPair();
            var updateRecord = await test.Client.UpdateFileWithRecordAsync(new UpdateFileParams
            {
                File = test.Record.File,
                Endorsements = new Endorsement[] { newPublicKey1, newPublicKey2 },
                Signatory = new Signatory(_network.PrivateKey, test.PrivateKey, newPrivateKey1, newPrivateKey2)
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);

            var info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(test.CreateParams.Contents.Length, info.Size);
            Assert.Equal(test.CreateParams.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { newPublicKey1, newPublicKey2 }, info.Endorsements);
            Assert.False(info.Deleted);

            var newContents = Encoding.Unicode.GetBytes("Hello Again Hashgraph " + Generator.Code(50));

            // Should Fail if try to Update with old File Key (note: payer signatory part of context already)
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await test.Client.UpdateFileAsync(new UpdateFileParams
                {
                    File = test.Record.File,
                    Contents = newContents,
                    Signatory = test.CreateParams.Signatory
                });
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to update file, status: InvalidSignature", tex.Message);

            // Should Fail if try to Update with new Private Key One
            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await test.Client.UpdateFileAsync(new UpdateFileParams
                {
                    File = test.Record.File,
                    Contents = newContents,
                    Signatory = newPrivateKey1
                });
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to update file, status: InvalidSignature", tex.Message);

            // Should Fail if try to Update with new Private Key One
            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await test.Client.UpdateFileAsync(new UpdateFileParams
                {
                    File = test.Record.File,
                    Contents = newContents,
                    Signatory = newPrivateKey2
                });
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to update file, status: InvalidSignature", tex.Message);

            // Both of the new Keys are Required to update
            var updateContentRecord = await test.Client.UpdateFileAsync(new UpdateFileParams
            {
                File = test.Record.File,
                Contents = newContents,
                Signatory = new Signatory(newPrivateKey1, newPrivateKey2)
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);

            var retrievedContents = await test.Client.GetFileContentAsync(test.Record.File);
            Assert.Equal(newContents, retrievedContents.ToArray());

            // Only one key in the list is required to delete the file.
            var deleteResult = await test.Client.DeleteFileWithRecordAsync(test.Record.File, new Signatory(newPrivateKey1));
            Assert.NotNull(deleteResult);
            Assert.Equal(ResponseCode.Success, deleteResult.Status);

            // Confirm File is Deleted
            info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(0, info.Size);
            Assert.Equal(test.CreateParams.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { newPublicKey1, newPublicKey2 }, info.Endorsements);
            Assert.True(info.Deleted);

        }

        [Fact(DisplayName = "File Update: Can Update File after Rotating Keys using One of Many List")]
        public async Task CanUpdateFileAfterKeyRotationOneOfMany()
        {
            await using var test = await TestFile.CreateAsync(_network);

            var (newPublicKey1, newPrivateKey1) = Generator.KeyPair();
            var (newPublicKey2, newPrivateKey2) = Generator.KeyPair();
            var (newPublicKey3, newPrivateKey3) = Generator.KeyPair();
            var updateRecord = await test.Client.UpdateFileWithRecordAsync(new UpdateFileParams
            {
                File = test.Record.File,
                Endorsements = new Endorsement[] { new Endorsement(1, newPublicKey1, newPublicKey2, newPublicKey3) },
                Signatory = new Signatory(_network.PrivateKey, test.PrivateKey, newPrivateKey1, newPrivateKey2, newPrivateKey3)
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);

            var info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(test.CreateParams.Contents.Length, info.Size);
            Assert.Equal(test.CreateParams.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { new Endorsement(1, newPublicKey1, newPublicKey2, newPublicKey3) }, info.Endorsements);
            Assert.False(info.Deleted);

            // First Key can change contents.
            var newContents = Encoding.Unicode.GetBytes("Hello Again Hashgraph " + Generator.Code(50));
            var updateContentRecord = await test.Client.UpdateFileAsync(new UpdateFileParams
            {
                File = test.Record.File,
                Contents = newContents,
                Signatory = newPrivateKey1
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);
            var retrievedContents = await test.Client.GetFileContentAsync(test.Record.File);
            Assert.Equal(newContents, retrievedContents.ToArray());

            // Try Second Key
            newContents = Encoding.Unicode.GetBytes("Hello Again Hashgraph " + Generator.Code(50));
            updateContentRecord = await test.Client.UpdateFileAsync(new UpdateFileParams
            {
                File = test.Record.File,
                Contents = newContents,
                Signatory = newPrivateKey2
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);
            retrievedContents = await test.Client.GetFileContentAsync(test.Record.File);
            Assert.Equal(newContents, retrievedContents.ToArray());

            // Try Third Key
            newContents = Encoding.Unicode.GetBytes("Hello Again Hashgraph " + Generator.Code(50));
            updateContentRecord = await test.Client.UpdateFileAsync(new UpdateFileParams
            {
                File = test.Record.File,
                Contents = newContents,
                Signatory = newPrivateKey3
            });
            Assert.Equal(ResponseCode.Success, updateRecord.Status);
            retrievedContents = await test.Client.GetFileContentAsync(test.Record.File);
            Assert.Equal(newContents, retrievedContents.ToArray());

            // Only One Signature needed to Delete
            var deleteResult = await test.Client.DeleteFileWithRecordAsync(test.Record.File, newPrivateKey1);
            Assert.NotNull(deleteResult);
            Assert.Equal(ResponseCode.Success, deleteResult.Status);

            // Confirm File is Deleted
            info = await test.Client.GetFileInfoAsync(test.Record.File);
            Assert.NotNull(info);
            Assert.Equal(test.Record.File, info.File);
            Assert.Equal(0, info.Size);
            Assert.Equal(test.CreateParams.Expiration, info.Expiration);
            Assert.Equal(new Endorsement[] { new Endorsement(1, newPublicKey1, newPublicKey2, newPublicKey3) }, info.Endorsements);
            Assert.True(info.Deleted);
        }
    }
}
