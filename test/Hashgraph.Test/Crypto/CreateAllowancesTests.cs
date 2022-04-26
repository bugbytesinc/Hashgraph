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
            CryptoAllowances = new[] { new CryptoAllowanceGrant(fxOwner, fxAgent, amount) },
            Signatory = fxOwner
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
            Assert.Empty(info.TokenAllowances);
            Assert.Empty(info.AssetAllowances);
            var allowance = info.CryptoAllowances[0];
            Assert.Equal(fxAgent.Record.Address, allowance.Agent);
            Assert.Equal(amount, allowance.Amount);
        }
    }
    [Fact(DisplayName = "Create Allowances: Can Create a Crypto Allowance With Record")]
    public async Task CanCreateACryptoAllowanceWithRecord()
    {
        await using var fxOwner = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 500_00_000_000);
        await using var fxAgent = await TestAccount.CreateAsync(_network);

        var amount = (long)fxOwner.CreateParams.InitialBalance;
        var record = await fxOwner.Client.CreateAllowancesWithRecordAsync(new AllowanceParams
        {
            CryptoAllowances = new[] { new CryptoAllowanceGrant(fxOwner, fxAgent, amount) },
            Signatory = fxOwner
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
            Assert.Empty(info.TokenAllowances);
            Assert.Empty(info.AssetAllowances);
            var allowance = info.CryptoAllowances[0];
            Assert.Equal(fxAgent.Record.Address, allowance.Agent);
            Assert.Equal(amount, allowance.Amount);
        }
    }
    [Fact(DisplayName = "Create Allowances: Can Create a Token Allowance")]
    public async Task CanCreateATokenAllowance()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        await using var fxAgent = await TestAccount.CreateAsync(_network);

        var amount = (long)fxToken.Params.Circulation / 3 + 1;
        var receipt = await fxToken.Client.CreateAllowancesAsync(new AllowanceParams
        {
            TokenAllowances = new[] { new TokenAllowanceGrant(fxToken.Record.Token, fxToken.TreasuryAccount, fxAgent, amount) },
            Signatory = fxToken.TreasuryAccount.PrivateKey
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
            var info = await fxToken.Client.GetAccountDetailAsync(fxToken.TreasuryAccount, ctx => ctx.Payer = payer);
            Assert.Empty(info.CryptoAllowances);
            Assert.Single(info.TokenAllowances);
            Assert.Empty(info.AssetAllowances);
            var allowance = info.TokenAllowances[0];
            Assert.Equal(fxToken.Record.Token, allowance.Token);
            Assert.Equal(fxAgent.Record.Address, allowance.Agent);
            Assert.Equal(amount, allowance.Amount);
        }
    }
    [Fact(DisplayName = "Create Allowances: Can Create a Token Allowance With Record")]
    public async Task CanCreateATokenAllowanceWithRecord()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        await using var fxAgent = await TestAccount.CreateAsync(_network);

        var amount = (long)fxToken.Params.Circulation / 3 + 1;
        var record = await fxToken.Client.CreateAllowancesWithRecordAsync(new AllowanceParams
        {
            TokenAllowances = new[] { new TokenAllowanceGrant(fxToken.Record.Token, fxToken.TreasuryAccount, fxAgent, amount) },
            Signatory = fxToken.TreasuryAccount.PrivateKey
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
            var info = await fxToken.Client.GetAccountDetailAsync(fxToken.TreasuryAccount, ctx => ctx.Payer = payer);
            Assert.Empty(info.CryptoAllowances);
            Assert.Single(info.TokenAllowances);
            Assert.Empty(info.AssetAllowances);
            var allowance = info.TokenAllowances[0];
            Assert.Equal(fxToken.Record.Token, allowance.Token);
            Assert.Equal(fxAgent.Record.Address, allowance.Agent);
            Assert.Equal(amount, allowance.Amount);
        }
    }

    [Fact(DisplayName = "Create Allowances: Can Create An Asset Allowance")]
    public async Task CanCreateAnAssetAllowance()
    {
        await using var fxAgent = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAgent);

        var serialNumbers = new long[] { 1 };
        var receipt = await fxAsset.Client.CreateAllowancesAsync(new AllowanceParams
        {
            AssetAllowances = new[] { new AssetAllowanceGrant(fxAsset.Record.Token, fxAsset.TreasuryAccount, fxAgent, serialNumbers) },
            Signatory = fxAsset.TreasuryAccount.PrivateKey
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
            var info = await fxAsset.Client.GetAccountDetailAsync(fxAsset.TreasuryAccount, ctx => ctx.Payer = payer);
            Assert.Empty(info.CryptoAllowances);
            Assert.Empty(info.TokenAllowances);
            // This is a horrible HAPI design defect
            //Assert.Single(info.AssetAllowances);
            //var allowance = info.AssetAllowances[0];
            //Assert.Equal(fxAsset.Record.Token, allowance.Token);
            //Assert.Equal(fxAgent.Record.Address, allowance.Agent);
            //Assert.True(Enumerable.SequenceEqual(serialNumbers, allowance.SerialNumbers));
            Assert.Empty(info.AssetAllowances);
        }
    }
    [Fact(DisplayName = "Create Allowances: Can Create An Asset Allowance With Record")]
    public async Task CanCreateAnAssetAllowanceWithRecord()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network);
        await using var fxAgent = await TestAccount.CreateAsync(_network);

        var serialNumbers = new long[] { 1 };
        var record = await fxAsset.Client.CreateAllowancesWithRecordAsync(new AllowanceParams
        {
            AssetAllowances = new[] { new AssetAllowanceGrant(fxAsset.Record.Token, fxAsset.TreasuryAccount, fxAgent, serialNumbers) },
            Signatory = fxAsset.TreasuryAccount.PrivateKey
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
            var info = await fxAsset.Client.GetAccountDetailAsync(fxAsset.TreasuryAccount, ctx => ctx.Payer = payer);
            Assert.Empty(info.CryptoAllowances);
            Assert.Empty(info.TokenAllowances);
            // This is a horrible HAPI design defect
            //Assert.Single(info.AssetAllowances);
            //var allowance = info.AssetAllowances[0];
            //Assert.Equal(fxAsset.Record.Token, allowance.Token);
            //Assert.Equal(fxAgent.Record.Address, allowance.Agent);
            //Assert.True(Enumerable.SequenceEqual(serialNumbers, allowance.SerialNumbers));
            Assert.Empty(info.AssetAllowances);
        }
    }
    [Fact(DisplayName = "Create Allowances: Can Create An All Asset Allowance")]
    public async Task CanCreateAnAllAssetAllowance()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network);
        await using var fxAgent = await TestAccount.CreateAsync(_network);

        var receipt = await fxAsset.Client.CreateAllowancesAsync(new AllowanceParams
        {
            AssetAllowances = new[] { new AssetAllowanceGrant(fxAsset.Record.Token, fxAsset.TreasuryAccount, fxAgent) },
            Signatory = fxAsset.TreasuryAccount.PrivateKey
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
            var info = await fxAsset.Client.GetAccountDetailAsync(fxAsset.TreasuryAccount, ctx => ctx.Payer = payer);
            Assert.Empty(info.CryptoAllowances);
            Assert.Empty(info.TokenAllowances);
            Assert.Single(info.AssetAllowances);
            var allowance = info.AssetAllowances[0];
            Assert.Equal(fxAsset.Record.Token, allowance.Token);
            Assert.Equal(fxAgent.Record.Address, allowance.Agent);
            Assert.Null(allowance.SerialNumbers);
        }
    }
    [Fact(DisplayName = "Create Allowances: Can Create An All Asset Allowance With Record")]
    public async Task CanCreateAnAllAssetAllowanceWithRecord()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network);
        await using var fxAgent = await TestAccount.CreateAsync(_network);

        var record = await fxAsset.Client.CreateAllowancesWithRecordAsync(new AllowanceParams
        {
            AssetAllowances = new[] { new AssetAllowanceGrant(fxAsset.Record.Token, fxAsset.TreasuryAccount, fxAgent, null) },
            Signatory = fxAsset.TreasuryAccount.PrivateKey
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
            var info = await fxAsset.Client.GetAccountDetailAsync(fxAsset.TreasuryAccount, ctx => ctx.Payer = payer);
            Assert.Empty(info.CryptoAllowances);
            Assert.Empty(info.TokenAllowances);
            Assert.Single(info.AssetAllowances);
            var allowance = info.AssetAllowances[0];
            Assert.Equal(fxAsset.Record.Token, allowance.Token);
            Assert.Equal(fxAgent.Record.Address, allowance.Agent);
            Assert.Null(allowance.SerialNumbers);
        }
    }
    [Fact(DisplayName = "Create Allowances: Can Create a Crypto and Token Allowance With Record")]
    public async Task CanCreateACryptoAndtokenAllowanceWithRecord()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        await using var fxAgent = await TestAccount.CreateAsync(_network);
        long tokenAmount = (long)fxToken.Params.Circulation / 3 + 1;
        long hbarAmount = 500_00_000_000;
        await fxToken.Client.TransferAsync(_network.Payer, fxToken.TreasuryAccount, hbarAmount);

        var record = await fxToken.Client.CreateAllowancesWithRecordAsync(new AllowanceParams
        {
            CryptoAllowances = new[] { new CryptoAllowanceGrant(fxToken.TreasuryAccount, fxAgent, hbarAmount) },
            TokenAllowances = new[] { new TokenAllowanceGrant(fxToken.Record.Token, fxToken.TreasuryAccount, fxAgent, tokenAmount) },
            Signatory = fxToken.TreasuryAccount
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
            var info = await fxToken.Client.GetAccountDetailAsync(fxToken.TreasuryAccount, ctx => ctx.Payer = payer);
            Assert.Single(info.CryptoAllowances);
            Assert.Single(info.TokenAllowances);
            Assert.Empty(info.AssetAllowances);
            var cryptoAllowance = info.CryptoAllowances[0];
            Assert.Equal(fxAgent.Record.Address, cryptoAllowance.Agent);
            Assert.Equal(hbarAmount, cryptoAllowance.Amount);
            var tokenAllowance = info.TokenAllowances[0];
            Assert.Equal(fxToken.Record.Token, tokenAllowance.Token);
            Assert.Equal(fxAgent.Record.Address, tokenAllowance.Agent);
            Assert.Equal(tokenAmount, tokenAllowance.Amount);
        }
    }
    [Fact(DisplayName = "Create Allowances: Can Create a Crypto and Token and Assset Allowance With Record")]
    public async Task CanCreateACryptoAndtokenAndAssetAllowanceWithRecord()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        await using var fxAgent = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxToken.TreasuryAccount);
        var serialNumbers = new long[] { 1 };
        long tokenAmount = (long)fxToken.Params.Circulation / 3 + 1;
        long hbarAmount = 500_00_000_000;
        await fxToken.Client.TransferAsync(_network.Payer, fxToken.TreasuryAccount, hbarAmount);
        await fxToken.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxToken.TreasuryAccount, fxAsset.TreasuryAccount.PrivateKey);

        var record = await fxToken.Client.CreateAllowancesWithRecordAsync(new AllowanceParams
        {
            CryptoAllowances = new[] { new CryptoAllowanceGrant(fxToken.TreasuryAccount, fxAgent, hbarAmount) },
            TokenAllowances = new[] { new TokenAllowanceGrant(fxToken.Record.Token, fxToken.TreasuryAccount, fxAgent, tokenAmount) },
            AssetAllowances = new[] { new AssetAllowanceGrant(fxAsset.Record.Token, fxToken.TreasuryAccount, fxAgent, serialNumbers) },
            Signatory = fxToken.TreasuryAccount
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
            var info = await fxToken.Client.GetAccountDetailAsync(fxToken.TreasuryAccount, ctx => ctx.Payer = payer);
            Assert.Single(info.CryptoAllowances);
            Assert.Single(info.TokenAllowances);
            // This is a horrible HAPI design defect
            //Assert.Single(info.AssetAllowances);
            Assert.Empty(info.AssetAllowances);
            var cryptoAllowance = info.CryptoAllowances[0];
            Assert.Equal(fxAgent.Record.Address, cryptoAllowance.Agent);
            Assert.Equal(hbarAmount, cryptoAllowance.Amount);
            var tokenAllowance = info.TokenAllowances[0];
            Assert.Equal(fxToken.Record.Token, tokenAllowance.Token);
            Assert.Equal(fxAgent.Record.Address, tokenAllowance.Agent);
            Assert.Equal(tokenAmount, tokenAllowance.Amount);
            // This is a horrible HAPI design defect
            //var assetAllowance = info.AssetAllowances[0];
            //Assert.Equal(fxAsset.Record.Token, assetAllowance.Token);
            //Assert.Equal(fxAgent.Record.Address, assetAllowance.Agent);
            //Assert.True(Enumerable.SequenceEqual(serialNumbers, assetAllowance.SerialNumbers));
        }
    }
    [Fact(DisplayName = "Create Allowances: Can Create a Crypto and Token and All Assset Allowance")]
    public async Task CanCreateACryptoAndTokenAndAllAsssetAllowance()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);

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
            Assert.Single(info.AssetAllowances);
            var cryptoAllowance = info.CryptoAllowances[0];
            Assert.Equal(fxAllowances.Agent, cryptoAllowance.Agent);
            Assert.Equal((long)fxAllowances.Owner.CreateParams.InitialBalance, cryptoAllowance.Amount);
            var tokenAllowance = info.TokenAllowances[0];
            Assert.Equal(fxAllowances.TestToken.Record.Token, tokenAllowance.Token);
            Assert.Equal(fxAllowances.Agent, tokenAllowance.Agent);
            Assert.Equal((long)fxAllowances.TestToken.Params.Circulation, tokenAllowance.Amount);
            var assetAllowance = info.AssetAllowances[0];
            Assert.Equal(fxAllowances.TestAsset, assetAllowance.Token);
            Assert.Equal(fxAllowances.Agent, assetAllowance.Agent);
            Assert.Null(assetAllowance.SerialNumbers);
        }
    }
}