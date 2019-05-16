using Hashgraph.Test.Fixtures;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.File
{
    public class TestFileInstance : IAsyncDisposable
    {
        public byte[] Contents;
        public ReadOnlyMemory<byte> PublicKey;
        public ReadOnlyMemory<byte> PrivateKey;
        public DateTime Expiration;
        public Account Payer;
        public Client Client;
        public FileRecord CreateRecord;
        public NetworkCredentialsFixture NetworkCredentials;

        public static async Task<TestFileInstance> CreateAsync(NetworkCredentialsFixture networkCredentials)
        {
            var test = new TestFileInstance();
            test.NetworkCredentials = networkCredentials;
            test.NetworkCredentials.TestOutput?.WriteLine("TestFileInstance: STARTING SETUP");
            test.Contents = Encoding.Unicode.GetBytes("Hello Hashgraph " + Generator.Code(50));
            (test.PublicKey, test.PrivateKey) = Generator.KeyPair();
            test.Expiration = Generator.TruncatedFutureDate(2, 4);
            test.Payer = new Account(
                    networkCredentials.AccountRealm,
                    networkCredentials.AccountShard,
                    networkCredentials.AccountNumber,
                    networkCredentials.AccountPrivateKey,
                    test.PrivateKey);
            test.Client = networkCredentials.CreateClientWithDefaultConfiguration();
            test.Client.Configure(ctx => { ctx.Payer = test.Payer; });
            test.CreateRecord = await test.Client.CreateFileWithRecordAsync(new CreateFileParams
            {
                Expiration = test.Expiration,
                Endorsements = new Endorsements(test.PublicKey),
                Contents = test.Contents
            }, ctx =>
            {
                ctx.Memo = "TestFileInstance Setup: Creating Test File on Network";
            });
            Assert.Equal(ResponseCode.Success, test.CreateRecord.Status);
            networkCredentials.TestOutput?.WriteLine("TestFileInstance: SETUP COMPLETED");
            return test;
        }

        public async ValueTask DisposeAsync()
        {
            NetworkCredentials.TestOutput?.WriteLine("TestFileInstance: STARTING TEARDOWN");
            try
            {
                await Client.DeleteFileAsync(CreateRecord.File, ctx =>
                {
                    ctx.Memo = "TestFileInstance Teardown: Attempting to delete File from Network (may already be deleted)";
                });
            }
            catch
            {
                //noop
            }
            await Client.DisposeAsync();
            NetworkCredentials.TestOutput?.WriteLine("TestFileInstance: TEARDOWN COMPLETED");
        }
    }
}