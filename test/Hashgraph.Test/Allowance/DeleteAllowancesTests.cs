using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Allowance;

[Collection(nameof(NetworkCredentials))]
public class DeleteAllowancesTests
{
    private readonly NetworkCredentials _network;
    public DeleteAllowancesTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Delete Allowances: Can Delete an Asset Allowance")]
    public async Task CanDeleteAnAssetAllowance()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);

        var receipt = await fxAllowances.Client.RevokeAssetAllowancesAsync(fxAllowances.TestAsset, fxAllowances.Owner, new long[] { 1 }, fxAllowances.Owner.PrivateKey);
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
            var info = await fxAllowances.Client.GetAccountDetailAsync(fxAllowances.Owner, ctx => ctx.Payer = payer);
            Assert.Single(info.CryptoAllowances);
            Assert.Single(info.TokenAllowances);
            // NETWORK 0.25 DEFECT (this should be empty)
            Assert.Single(info.AssetAllowances);
            // End bug
            var cryptoAllowance = info.CryptoAllowances[0];
            Assert.Equal(info.Address, cryptoAllowance.Owner);
            Assert.Equal(fxAllowances.Agent, cryptoAllowance.Agent);
            Assert.Equal((long)fxAllowances.Owner.CreateParams.InitialBalance, cryptoAllowance.Amount);
            var tokenAllowance = info.TokenAllowances[0];
            Assert.Equal(fxAllowances.TestToken.Record.Token, tokenAllowance.Token);
            Assert.Equal(fxAllowances.Agent, tokenAllowance.Agent);
            Assert.Equal((long)fxAllowances.TestToken.Params.Circulation, tokenAllowance.Amount);
            // NETWORK 0.25 DEFECT (this should be empty)
            var assetAllowance = info.AssetAllowances[0];
            Assert.Equal(fxAllowances.TestAsset, assetAllowance.Token);
            Assert.Equal(fxAllowances.Agent, assetAllowance.Agent);

        }
    }
    [Fact(DisplayName = "Delete Allowances: Can Delete an Asset Allowance With Record")]
    public async Task CanDeleteAnAssetAllowanceWithRecord()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);

        var record = await fxAllowances.Client.RevokeAssetAllowancesWithRecordAsync(fxAllowances.TestAsset, fxAllowances.Owner, new long[] { 1 }, fxAllowances.Owner.PrivateKey);
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
            var info = await fxAllowances.Client.GetAccountDetailAsync(fxAllowances.Owner, ctx => ctx.Payer = payer);
            Assert.Single(info.CryptoAllowances);
            Assert.Single(info.TokenAllowances);
            // NETWORK 0.25 DEFECT (this should be empty)
            Assert.Single(info.AssetAllowances);
            // end bug
            var cryptoAllowance = info.CryptoAllowances[0];
            Assert.Equal(info.Address, cryptoAllowance.Owner);
            Assert.Equal(fxAllowances.Agent, cryptoAllowance.Agent);
            Assert.Equal((long)fxAllowances.Owner.CreateParams.InitialBalance, cryptoAllowance.Amount);
            var tokenAllowance = info.TokenAllowances[0];
            Assert.Equal(fxAllowances.TestToken.Record.Token, tokenAllowance.Token);
            Assert.Equal(fxAllowances.Agent, tokenAllowance.Agent);
            Assert.Equal((long)fxAllowances.TestToken.Params.Circulation, tokenAllowance.Amount);
            // NETWORK 0.25 DEFECT (this should be empty)
            var assetAllowance = info.AssetAllowances[0];
            Assert.Equal(fxAllowances.TestAsset, assetAllowance.Token);
            Assert.Equal(fxAllowances.Agent, assetAllowance.Agent);
            // end bug
        }
    }


    [Fact(DisplayName = "Delete Allowances: Can Delete an Crypto Allowance")]
    public async Task CanDeleteACryptoAllowance()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);

        var receipt = await fxAllowances.Client.AllocateAsync(new AllowanceParams
        {
            CryptoAllowances = new[] { new CryptoAllowance(fxAllowances.Owner, fxAllowances.Agent, 0) },
            Signatory = fxAllowances.Owner.PrivateKey
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
            var info = await fxAllowances.Client.GetAccountDetailAsync(fxAllowances.Owner, ctx => ctx.Payer = payer);
            Assert.Empty(info.CryptoAllowances);
            Assert.Single(info.TokenAllowances);
            Assert.Single(info.AssetAllowances);
            var tokenAllowance = info.TokenAllowances[0];
            Assert.Equal(fxAllowances.TestToken.Record.Token, tokenAllowance.Token);
            Assert.Equal(fxAllowances.Agent, tokenAllowance.Agent);
            Assert.Equal((long)fxAllowances.TestToken.Params.Circulation, tokenAllowance.Amount);
            var assetAllowance = info.AssetAllowances[0];
            Assert.Equal(fxAllowances.TestAsset, assetAllowance.Token);
            Assert.Equal(fxAllowances.Agent, assetAllowance.Agent);
        }
    }

    [Fact(DisplayName = "Delete Allowances: Can Delete an Crypto Allowance with Record")]
    public async Task CanDeleteACryptoAllowanceWithRecord()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);

        var record = await fxAllowances.Client.AllocateWithRecordAsync(new AllowanceParams
        {
            CryptoAllowances = new[] { new CryptoAllowance(fxAllowances.Owner, fxAllowances.Agent, 0) },
            Signatory = fxAllowances.Owner.PrivateKey
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
            var info = await fxAllowances.Client.GetAccountDetailAsync(fxAllowances.Owner, ctx => ctx.Payer = payer);
            Assert.Empty(info.CryptoAllowances);
            Assert.Single(info.TokenAllowances);
            Assert.Single(info.AssetAllowances);
            var tokenAllowance = info.TokenAllowances[0];
            Assert.Equal(fxAllowances.TestToken.Record.Token, tokenAllowance.Token);
            Assert.Equal(fxAllowances.Agent, tokenAllowance.Agent);
            Assert.Equal((long)fxAllowances.TestToken.Params.Circulation, tokenAllowance.Amount);
            var assetAllowance = info.AssetAllowances[0];
            Assert.Equal(fxAllowances.TestAsset, assetAllowance.Token);
            Assert.Equal(fxAllowances.Agent, assetAllowance.Agent);
        }
    }

    [Fact(DisplayName = "Delete Allowances: Can Delete an Token Allowance")]
    public async Task CanDeleteATokenAllowance()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);

        var receipt = await fxAllowances.Client.AllocateAsync(new AllowanceParams
        {
            TokenAllowances = new[] { new TokenAllowance(fxAllowances.TestToken, fxAllowances.Owner, fxAllowances.Agent, 0) },
            Signatory = fxAllowances.Owner.PrivateKey
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
            var info = await fxAllowances.Client.GetAccountDetailAsync(fxAllowances.Owner, ctx => ctx.Payer = payer);
            Assert.Single(info.CryptoAllowances);
            Assert.Empty(info.TokenAllowances);
            Assert.Single(info.AssetAllowances);
            var cryptoAllowance = info.CryptoAllowances[0];
            Assert.Equal(info.Address, cryptoAllowance.Owner);
            Assert.Equal(fxAllowances.Agent, cryptoAllowance.Agent);
            Assert.Equal((long)fxAllowances.Owner.CreateParams.InitialBalance, cryptoAllowance.Amount);
            var assetAllowance = info.AssetAllowances[0];
            Assert.Equal(fxAllowances.TestAsset, assetAllowance.Token);
            Assert.Equal(fxAllowances.Agent, assetAllowance.Agent);
        }
    }

}