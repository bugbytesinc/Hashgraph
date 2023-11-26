using System;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Fixtures;

public class TestToken : IAsyncDisposable
{
    public ReadOnlyMemory<byte> AdminPublicKey;
    public ReadOnlyMemory<byte> AdminPrivateKey;
    public ReadOnlyMemory<byte> GrantPublicKey;
    public ReadOnlyMemory<byte> GrantPrivateKey;
    public ReadOnlyMemory<byte> SuspendPublicKey;
    public ReadOnlyMemory<byte> SuspendPrivateKey;
    public ReadOnlyMemory<byte> PausePublicKey;
    public ReadOnlyMemory<byte> PausePrivateKey;
    public ReadOnlyMemory<byte> ConfiscatePublicKey;
    public ReadOnlyMemory<byte> ConfiscatePrivateKey;
    public ReadOnlyMemory<byte> SupplyPublicKey;
    public ReadOnlyMemory<byte> SupplyPrivateKey;
    public ReadOnlyMemory<byte> RoyaltiesPublicKey;
    public ReadOnlyMemory<byte> RoyaltiesPrivateKey;
    public TestAccount TreasuryAccount;
    public TestAccount RenewAccount;
    public Address Payer;
    public Client Client;
    public CreateTokenParams Params;
    public CreateTokenRecord Record;
    public NetworkCredentials Network;

    public static async Task<TestToken> CreateAsync(NetworkCredentials networkCredentials, Action<TestToken> customize = null, params TestAccount[] associate)
    {
        var wholeTokens = (ulong)(Generator.Integer(10, 20) * 100000);
        var decimals = (uint)Generator.Integer(2, 5);
        var circulation = wholeTokens * (ulong)Math.Pow(10, decimals);
        var maxSupply = (long)(circulation * Generator.Double(2.1, 2.8));
        var fx = new TestToken
        {
            Network = networkCredentials
        };
        fx.Network.Output?.WriteLine("STARTING SETUP: Test Token Instance");
        (fx.AdminPublicKey, fx.AdminPrivateKey) = Generator.KeyPair();
        (fx.GrantPublicKey, fx.GrantPrivateKey) = Generator.KeyPair();
        (fx.SuspendPublicKey, fx.SuspendPrivateKey) = Generator.KeyPair();
        (fx.PausePublicKey, fx.PausePrivateKey) = Generator.KeyPair();
        (fx.ConfiscatePublicKey, fx.ConfiscatePrivateKey) = Generator.KeyPair();
        (fx.SupplyPublicKey, fx.SupplyPrivateKey) = Generator.KeyPair();
        (fx.RoyaltiesPublicKey, fx.RoyaltiesPrivateKey) = Generator.KeyPair();
        fx.Payer = networkCredentials.Payer;
        fx.Client = networkCredentials.NewClient();
        fx.TreasuryAccount = await TestAccount.CreateAsync(networkCredentials);
        fx.RenewAccount = await TestAccount.CreateAsync(networkCredentials);
        fx.Params = new CreateTokenParams
        {
            Name = Generator.Code(100),
            Symbol = Generator.Code(100),
            Circulation = circulation,
            Decimals = decimals,
            Ceiling = maxSupply,
            Treasury = fx.TreasuryAccount.Record.Address,
            Administrator = fx.AdminPublicKey,
            GrantKycEndorsement = fx.GrantPublicKey,
            SuspendEndorsement = fx.SuspendPublicKey,
            PauseEndorsement = fx.PausePublicKey,
            ConfiscateEndorsement = fx.ConfiscatePublicKey,
            SupplyEndorsement = fx.SupplyPublicKey,
            RoyaltiesEndorsement = fx.RoyaltiesPublicKey,
            InitializeSuspended = false,
            Expiration = Generator.TruncatedFutureDate(2000, 3000),
            RenewAccount = fx.RenewAccount.Record.Address,
            RenewPeriod = TimeSpan.FromDays(90),
            Signatory = new Signatory(fx.AdminPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey),
            Memo = ("Test Token: " + Generator.Code(20)).TruncateMemo()
        };
        customize?.Invoke(fx);
        fx.Record = await networkCredentials.RetryForKnownNetworkIssuesAsync(async () =>
        {
            return await fx.Client.CreateTokenWithRecordAsync(fx.Params, ctx =>
            {
                ctx.Memo = (".NET SDK Test: " + fx.Params.Symbol ?? "(null symbol)").TruncateMemo();
            });
        });
        Assert.Equal(ResponseCode.Success, fx.Record.Status);
        await fx.AssociateAccounts(associate);
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
                ctx.Memo = ".NET SDK Test: Attempting to delete Token from Network (may already be deleted)";
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

    public async Task AssociateAccounts(params TestAccount[] accounts)
    {
        foreach (var account in accounts)
        {
            await Client.AssociateTokenAsync(Record.Token, account.Record.Address, account.PrivateKey);
        }
    }
}