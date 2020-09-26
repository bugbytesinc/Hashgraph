using System;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Fixtures
{
    public class TestToken : IAsyncDisposable
    {
        public ReadOnlyMemory<byte> AdminPublicKey;
        public ReadOnlyMemory<byte> AdminPrivateKey;
        public ReadOnlyMemory<byte> GrantPublicKey;
        public ReadOnlyMemory<byte> GrantPrivateKey;
        public ReadOnlyMemory<byte> SuspendPublicKey;
        public ReadOnlyMemory<byte> SuspendPrivateKey;
        public ReadOnlyMemory<byte> ConfiscatePublicKey;
        public ReadOnlyMemory<byte> ConfiscatePrivateKey;
        public ReadOnlyMemory<byte> SupplyPublicKey;
        public ReadOnlyMemory<byte> SupplyPrivateKey;
        public TestAccount TreasuryAccount;
        public TestAccount RenewAccount;
        public Address Payer;
        public Client Client;
        public CreateTokenParams Params;
        public CreateTokenRecord Record;
        public NetworkCredentials Network;

        public static async Task<TestToken> CreateAsync(NetworkCredentials networkCredentials, Action<TestToken> customize = null)
        {
            var fx = new TestToken();
            fx.Network = networkCredentials;
            fx.Network.Output?.WriteLine("STARTING SETUP: Test Token Instance");
            (fx.AdminPublicKey, fx.AdminPrivateKey) = Generator.KeyPair();
            (fx.GrantPublicKey, fx.GrantPrivateKey) = Generator.KeyPair();
            (fx.SuspendPublicKey, fx.SuspendPrivateKey) = Generator.KeyPair();
            (fx.ConfiscatePublicKey, fx.ConfiscatePrivateKey) = Generator.KeyPair();
            (fx.SupplyPublicKey, fx.SupplyPrivateKey) = Generator.KeyPair();
            fx.Payer = networkCredentials.Payer;
            fx.Client = networkCredentials.NewClient();
            fx.TreasuryAccount = await TestAccount.CreateAsync(networkCredentials);
            fx.RenewAccount = await TestAccount.CreateAsync(networkCredentials);
            fx.Params = new CreateTokenParams
            {
                Name = Generator.Code(50),
                Symbol = Generator.UppercaseAlphaCode(20),
                Circulation = (ulong)(Generator.Integer(10, 20) * 100000),
                Decimals = (uint)Generator.Integer(2, 5),
                Treasury = fx.TreasuryAccount.Record.Address,
                Administrator = fx.AdminPublicKey,
                GrantKycEndorsement = fx.GrantPublicKey,
                SuspendEndorsement = fx.SuspendPublicKey,
                ConfiscateEndorsement = fx.ConfiscatePublicKey,
                SupplyEndorsement = fx.SupplyPublicKey,
                InitializeSuspended = false,
                Expiration = Generator.TruncatedFutureDate(2000, 3000),
                RenewAccount = fx.RenewAccount.Record.Address,
                RenewPeriod = TimeSpan.FromDays(90),
                Signatory = new Signatory(fx.AdminPrivateKey, fx.GrantPrivateKey, fx.SuspendPrivateKey, fx.ConfiscatePrivateKey, fx.SupplyPrivateKey, fx.RenewAccount.PrivateKey)
            };
            customize?.Invoke(fx);
            fx.Record = await fx.Client.CreateTokenWithRecordAsync(fx.Params, ctx =>
            {
                ctx.Memo = "TestToken Setup: " + fx.Params.Symbol ?? "(null symbol)";
            });
            Assert.Equal(ResponseCode.Success, fx.Record.Status);
            networkCredentials.Output?.WriteLine("SETUP COMPLETED: Test Token Instance");
            return fx;
        }

        public async ValueTask DisposeAsync()
        {
            Network.Output?.WriteLine("STARTING TEARDOWN: Test Token Instance");
            try
            {
                await Client.DeleteTokenAsync(Record.Token, AdminPrivateKey, ctx =>
                {
                    ctx.Memo = "TestTokenInstance Teardown: Attempting to delete Token from Network (may already be deleted)";
                });
            }
            catch
            {
                //noop
            }
            await Client.DisposeAsync();
            await TreasuryAccount.DisposeAsync();
            await RenewAccount.DisposeAsync();
            Network.Output?.WriteLine("TEARDOWN COMPLETED Test Token Instance");
        }

        public static implicit operator Address(TestToken fxToken)
        {
            return fxToken.Record.Token;
        }
        public static implicit operator TokenIdentifier(TestToken fxToken)
        {
            return new TokenIdentifier(fxToken.Record.Token);
        }
    }
}