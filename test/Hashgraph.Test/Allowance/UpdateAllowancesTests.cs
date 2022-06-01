using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Allowance;

[Collection(nameof(NetworkCredentials))]
public class UpdateAllowancesTests
{
    private readonly NetworkCredentials _network;
    public UpdateAllowancesTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Update Allowances: Can Update a Crypto Allowance")]
    public async Task CanUpdateACryptoAllowance()
    {
        await using var fxOwner = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
        await using var fxAgent = await TestAccount.CreateAsync(_network);

        var amount = (long)fxOwner.CreateParams.InitialBalance / 4;
        await fxOwner.Client.AllocateAsync(new AllowanceParams
        {
            CryptoAllowances = new[] { new CryptoAllowance(fxOwner, fxAgent, amount) },
            Signatory = fxOwner
        }, ctx =>
        {
            ctx.Payer = fxOwner;
            ctx.Signatory = fxOwner;
        });

        var receipt = await fxOwner.Client.AllocateAsync(new AllowanceParams
        {
            CryptoAllowances = new[] { new CryptoAllowance(fxOwner, fxAgent, amount * 2) },
            Signatory = fxOwner
        }, ctx =>
        {
            ctx.Payer = fxOwner;
            ctx.Signatory = fxOwner;
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var systemAddress = await _network.GetGenisisAccountAddress();
        if (systemAddress is null)
        {
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await checkDetails(_network.Payer);
            });
            Assert.Equal(ResponseCode.NotSupported, pex.Status);
        }
        else
        {
            await checkDetails(systemAddress);
        }

        async Task checkDetails(Address payer)
        {
            var info = await fxOwner.Client.GetAccountDetailAsync(fxOwner, ctx => ctx.Payer = payer);
            Assert.Single(info.CryptoAllowances);
            var allowance = info.CryptoAllowances[0];
            Assert.Equal(info.Address, allowance.Owner);
            Assert.Equal(fxAgent.Record.Address, allowance.Agent);
            Assert.Equal(amount * 2, allowance.Amount);
        }
    }
    [Fact(DisplayName = "Update Allowances: Can Update a Crypto Allowance With Record")]
    public async Task CanUpdateACryptoAllowanceWithRecord()
    {
        await using var fxOwner = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 500_00_000_000);
        await using var fxAgent = await TestAccount.CreateAsync(_network);

        var amount = (long)fxOwner.CreateParams.InitialBalance / 3;
        await fxOwner.Client.AllocateAsync(new AllowanceParams
        {
            CryptoAllowances = new[] { new CryptoAllowance(fxOwner, fxAgent, amount) },
            Signatory = fxOwner
        }, ctx =>
        {
            ctx.Payer = fxOwner;
            ctx.Signatory = fxOwner;
        });

        var record = await fxOwner.Client.AllocateWithRecordAsync(new AllowanceParams
        {
            CryptoAllowances = new[] { new CryptoAllowance(fxOwner, fxAgent, amount * 2) },
            Signatory = fxOwner
        }, ctx =>
        {
            ctx.Payer = fxOwner;
            ctx.Signatory = fxOwner;
        });
        Assert.Equal(ResponseCode.Success, record.Status);

        var systemAddress = await _network.GetGenisisAccountAddress();
        if (systemAddress is null)
        {
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await checkDetails(_network.Payer);
            });
            Assert.Equal(ResponseCode.NotSupported, pex.Status);
        }
        else
        {
            await checkDetails(systemAddress);
        }

        async Task checkDetails(Address payer)
        {
            var info = await fxOwner.Client.GetAccountDetailAsync(fxOwner, ctx => ctx.Payer = payer);
            Assert.Single(info.CryptoAllowances);
            var allowance = info.CryptoAllowances[0];
            Assert.Equal(info.Address, allowance.Owner);
            Assert.Equal(fxAgent.Record.Address, allowance.Agent);
            Assert.Equal(amount * 2, allowance.Amount);
        }
    }
}