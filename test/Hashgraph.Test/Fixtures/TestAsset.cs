using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Fixtures
{
    public class TestAsset : IAsyncDisposable
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
        public ReadOnlyMemory<byte> RoyaltiesPublickKey;
        public ReadOnlyMemory<byte> RoyaltiesPrivateKey;
        public TestAccount TreasuryAccount;
        public TestAccount RenewAccount;
        public Address Payer;
        public Client Client;
        public CreateAssetParams Params;
        public CreateTokenRecord Record;
        public NetworkCredentials Network;
        public ReadOnlyMemory<byte>[] Metadata;
        public AssetMintRecord MintRecord;

        public static async Task<TestAsset> CreateAsync(NetworkCredentials networkCredentials, Action<TestAsset> customize = null, params TestAccount[] associate)
        {
            var maxSupply = (long)(Generator.Integer(10, 20) * 1000);
            var fx = new TestAsset
            {
                Network = networkCredentials
            };
            fx.Network.Output?.WriteLine("STARTING SETUP: Test Asset Instance");
            (fx.AdminPublicKey, fx.AdminPrivateKey) = Generator.KeyPair();
            (fx.GrantPublicKey, fx.GrantPrivateKey) = Generator.KeyPair();
            (fx.SuspendPublicKey, fx.SuspendPrivateKey) = Generator.KeyPair();
            (fx.ConfiscatePublicKey, fx.ConfiscatePrivateKey) = Generator.KeyPair();
            (fx.SupplyPublicKey, fx.SupplyPrivateKey) = Generator.KeyPair();
            (fx.RoyaltiesPublickKey, fx.RoyaltiesPrivateKey) = Generator.KeyPair();
            fx.Payer = networkCredentials.Payer;
            fx.Client = networkCredentials.NewClient();
            fx.TreasuryAccount = await TestAccount.CreateAsync(networkCredentials);
            fx.RenewAccount = await TestAccount.CreateAsync(networkCredentials);
            fx.Metadata = Enumerable.Range(1, Generator.Integer(3, 9)).Select(_ => Generator.SHA384Hash()).ToArray();
            fx.Params = new CreateAssetParams
            {
                Name = Generator.Code(50),
                Symbol = Generator.UppercaseAlphaCode(20),
                Treasury = fx.TreasuryAccount.Record.Address,
                Ceiling = maxSupply,
                Administrator = fx.AdminPublicKey,
                GrantKycEndorsement = fx.GrantPublicKey,
                SuspendEndorsement = fx.SuspendPublicKey,
                ConfiscateEndorsement = fx.ConfiscatePublicKey,
                SupplyEndorsement = fx.SupplyPublicKey,
                RoyaltiesEndorsement = fx.RoyaltiesPublickKey,
                InitializeSuspended = false,
                Expiration = Generator.TruncatedFutureDate(2000, 3000),
                RenewAccount = fx.RenewAccount.Record.Address,
                RenewPeriod = TimeSpan.FromDays(90),
                Signatory = new Signatory(fx.AdminPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey),
                Memo = "Test Asset: " + Generator.Code(20)
            };
            customize?.Invoke(fx);
            try
            {
                fx.Record = await fx.Client.CreateTokenWithRecordAsync(fx.Params, ctx =>
                {
                    ctx.Memo = "TestAsset Setup: " + fx.Params.Symbol ?? "(null symbol)";
                });
            }
            catch (TransactionException ex) when (ex.Message?.StartsWith("The Network Changed the price of Retrieving a Record while attempting to retrieve this record") == true)
            {
                var record = await fx.Client.GetTransactionRecordAsync(ex.TxId) as CreateTokenRecord;
                if (record is not null)
                {
                    fx.Record = record;
                }
                else
                {
                    throw;
                }
            }
            Assert.Equal(ResponseCode.Success, fx.Record.Status);
            await fx.AssociateAccounts(associate);
            if (fx.Metadata is not null && fx.Metadata.Length > 0)
            {
                fx.MintRecord = await fx.Client.MintAssetWithRecordAsync(fx.Record.Token, fx.Metadata, fx.SupplyPrivateKey);
            }
            networkCredentials.Output?.WriteLine("SETUP COMPLETED: Test Asset Instance");
            return fx;
        }
        public async ValueTask DisposeAsync()
        {
            Network.Output?.WriteLine("STARTING TEARDOWN: Test Asset Instance");
            try
            {
                await Client.DeleteTokenAsync(Record.Token, AdminPrivateKey, ctx =>
                {
                    ctx.Memo = "TestAssetInstance Teardown: Attempting to delete Asset from Network (may already be deleted)";
                });
            }
            catch
            {
                //noop
            }
            await Client.DisposeAsync();
            await TreasuryAccount.DisposeAsync();
            await RenewAccount.DisposeAsync();
            Network.Output?.WriteLine("TEARDOWN COMPLETED Test Asset Instance");
        }

        public static implicit operator Address(TestAsset fxAsset)
        {
            return fxAsset.Record.Token;
        }

        public async Task AssociateAccounts(params TestAccount[] accounts)
        {
            foreach (var account in accounts)
            {
                await Client.AssociateTokenAsync(Record.Token, account.Record.Address, account.PrivateKey);
            }
        }
    }
}