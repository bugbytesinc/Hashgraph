using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Fixtures
{
    public class TestFile : IAsyncDisposable
    {
        public byte[] Contents;
        public ReadOnlyMemory<byte> PublicKey;
        public ReadOnlyMemory<byte> PrivateKey;
        public DateTime Expiration;
        public Account Payer;
        public Client Client;
        public FileRecord Record;
        public NetworkCredentials Network;

        public static async Task<TestFile> CreateAsync(NetworkCredentials networkCredentials)
        {
            var test = new TestFile();
            test.Network = networkCredentials;
            test.Network.Output?.WriteLine("STARTING SETUP: Test File Instance");
            test.Contents = Encoding.Unicode.GetBytes("Hello Hashgraph " + Generator.Code(50));
            (test.PublicKey, test.PrivateKey) = Generator.KeyPair();
            test.Expiration = Generator.TruncatedFutureDate(2, 4);
            test.Payer = new Account(
                    networkCredentials.AccountRealm,
                    networkCredentials.AccountShard,
                    networkCredentials.AccountNumber,
                    networkCredentials.PrivateKey,
                    test.PrivateKey);
            test.Client = networkCredentials.NewClient();
            test.Client.Configure(ctx => { ctx.Payer = test.Payer; });
            test.Record = await test.Client.CreateFileWithRecordAsync(new CreateFileParams
            {
                Expiration = test.Expiration,
                Endorsements = new Endorsement[] { test.PublicKey },
                Contents = test.Contents
            }, ctx =>
            {
                ctx.Memo = "TestFileInstance Setup: Creating Test File on Network";
            });
            Assert.Equal(ResponseCode.Success, test.Record.Status);
            networkCredentials.Output?.WriteLine("SETUP COMPLETED: Test File Instance");
            return test;
        }

        public async ValueTask DisposeAsync()
        {
            Network.Output?.WriteLine("STARTING TEARDOWN: Test File Instance");
            try
            {
                await Client.DeleteFileAsync(Record.File, ctx =>
                {
                    ctx.Memo = "TestFileInstance Teardown: Attempting to delete File from Network (may already be deleted)";
                });
            }
            catch
            {
                //noop
            }
            await Client.DisposeAsync();
            Network.Output?.WriteLine("TEARDOWN COMPLETED Test File Instance");
        }
    }
}