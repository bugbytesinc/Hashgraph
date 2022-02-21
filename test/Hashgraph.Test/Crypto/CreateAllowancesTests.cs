using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class CreateAllowancesTests
{
    private readonly NetworkCredentials _network;
    public CreateAllowancesTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Create Allowances: Can Create a Crypto Allowance")]
    public async Task CanCreateACryptoAllowance()
    {
        await using var fxOwner = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
        await using var fxAgent = await TestAccount.CreateAsync(_network);

        var amount = (long)fxOwner.CreateParams.InitialBalance;
        var receipt = await fxOwner.Client.CreateAllowancesAsync(new AllowanceParams
        {
            CryptoAllowances = new[] { new CryptoAllowance(fxOwner, fxAgent, (long)fxOwner.CreateParams.InitialBalance) },
            Signatory = fxOwner
        }, ctx => {
            ctx.Payer = fxOwner;
            ctx.Signatory = fxOwner;
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxOwner.Client.GetAccountInfoAsync(fxOwner);
        Assert.Single(info.CryptoAllowances);
        var allowance = info.CryptoAllowances[0];
        // vvv NETWORK 0.25 DEFECT
        // The following should work but it does not
        // Assert.Equal(fxOwner.Record.Address, allowance.Owner);
        Assert.Equal(Address.None, allowance.Owner);
        // ^^^ END DEFECT
        Assert.Equal(fxAgent.Record.Address, allowance.Agent);
        Assert.Equal(amount, allowance.Amount);
    }
    [Fact(DisplayName = "Create Allowances: Can Create a Crypto Allowance With Record")]
    public async Task CanCreateACryptoAllowanceWithRecord()
    {
        await using var fxOwner = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 500_00_000_000);
        await using var fxAgent = await TestAccount.CreateAsync(_network);

        var amount = (long)fxOwner.CreateParams.InitialBalance;
        var record = await fxOwner.Client.CreateAllowancesWithRecordAsync(new AllowanceParams
        {
            CryptoAllowances = new[] { new CryptoAllowance(fxOwner, fxAgent, (long)fxOwner.CreateParams.InitialBalance) },
            Signatory = fxOwner
        }, ctx => {
            ctx.Payer = fxOwner;
            ctx.Signatory = fxOwner;
        });
        Assert.Equal(ResponseCode.Success, record.Status);
        // vvv NETWORK 0.25 DEFECT
        // The following should work but it does not
        //Assert.Single(record.CryptoAllowances);
        //var allowance = record.CryptoAllowances[0];
        //Assert.Equal(fxOwner.Record.Address, allowance.Owner);
        //Assert.Equal(fxAgent.Record.Address, allowance.Agent);
        //Assert.Equal(amount, allowance.Amount);
        Assert.Empty(record.CryptoAllowances);
        // ^^^ END DEFECT

        var info = await fxOwner.Client.GetAccountInfoAsync(fxOwner);
        Assert.Single(info.CryptoAllowances);
        var allowance = info.CryptoAllowances[0];
        // vvv NETWORK 0.25 DEFECT
        // The following should work but it does not
        // Assert.Equal(fxOwner.Record.Address, allowance.Owner);
        Assert.Equal(Address.None, allowance.Owner);
        // ^^^ END DEFECT
        Assert.Equal(fxAgent.Record.Address, allowance.Agent);
        Assert.Equal(amount, allowance.Amount);
    }
}