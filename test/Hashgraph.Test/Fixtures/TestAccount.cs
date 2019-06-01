using System;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Fixtures
{
    public class TestAccount : IAsyncDisposable
    {
        public ReadOnlyMemory<byte> PublicKey;
        public ReadOnlyMemory<byte> PrivateKey;
        public CreateAccountParams CreateParams;
        public AccountRecord Record;
        public NetworkCredentials Network;
        public Client Client;

        public static async Task<TestAccount> CreateAsync(NetworkCredentials networkCredentials)
        {
            var fx = new TestAccount();
            fx.Network = networkCredentials;
            fx.Network.Output?.WriteLine("STARTING SETUP: Test Account Instance");
            (fx.PublicKey, fx.PrivateKey) = Generator.KeyPair();
            fx.CreateParams = new CreateAccountParams
            {
                PublicKey = fx.PublicKey,
                InitialBalance = (ulong)Generator.Integer(10, 20)
            };
            fx.Client = networkCredentials.NewClient();
            fx.Record = await fx.Client.CreateAccountWithRecordAsync(fx.CreateParams, ctx =>
             {
                 ctx.Memo = "Test Account Instance: Creating Test Account on Network";
             });
            Assert.Equal(ResponseCode.Success, fx.Record.Status);
            networkCredentials.Output?.WriteLine("SETUP COMPLETED: Test Account Instance");
            return fx;
        }

        public async ValueTask DisposeAsync()
        {
            Network.Output?.WriteLine("STARTING TEARDOWN: Test Account Instance");
            try
            {
                await Client.DeleteAccountAsync(new Account(Record.Address, PrivateKey), Network.Payer, ctx =>
                  {
                      ctx.Memo = "Test Account Instance Teardown: Attempting to delete Account from Network (if exists)";
                  });
            }
            catch
            {
                //noop
            }
            await Client.DisposeAsync();
            Network.Output?.WriteLine("TEARDOWN COMPLETED: Test Account Instance");
        }
    }
}