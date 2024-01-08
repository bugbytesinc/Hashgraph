namespace Hashgraph.Test.Allowance;

[Collection(nameof(NetworkCredentials))]
public class ExecuteAllowancesTests
{
    private readonly NetworkCredentials _network;
    public ExecuteAllowancesTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Execute Allowances: Can Spend an Asset Allowance")]
    public async Task CanSpendAnAssetAllowance()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);
        await using var fxDestination = await TestAccount.CreateAsync(_network);

        await fxDestination.Client.AssociateTokenAsync(fxAllowances.TestAsset, fxDestination, fxDestination.PrivateKey);

        var client = fxAllowances.Client.Clone(ctx =>
        {
            ctx.Payer = fxAllowances.Agent;
            ctx.Signatory = fxAllowances.Agent.PrivateKey;
        });

        var receipt = await client.TransferAsync(new TransferParams
        {
            AssetTransfers = new[] { new AssetTransfer(new Asset(fxAllowances.TestAsset, 1), fxAllowances.Owner, fxDestination, true) }
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.AssetBalanceAsync(fxAllowances.TestAsset, fxDestination, 1);
    }

    [Fact(DisplayName = "Execute Allowances: Can not Spend an Asset Allowance Without Delegated Flag")]
    public async Task CanNotSpendAnAssetAllowanceWithoutDelegatedFlag()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);
        await using var fxDestination = await TestAccount.CreateAsync(_network);

        await fxDestination.Client.AssociateTokenAsync(fxAllowances.TestToken, fxDestination, fxDestination.PrivateKey);

        var client = fxAllowances.Client.Clone(ctx =>
        {
            ctx.Payer = fxAllowances.Agent;
            ctx.Signatory = fxAllowances.Agent.PrivateKey;
        });

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await client.TransferAsync(new TransferParams
            {
                AssetTransfers = new[] { new AssetTransfer(new Asset(fxAllowances.TestAsset, 1), fxAllowances.Owner, fxDestination, false) }
            });
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);

        await AssertHg.AssetNotAssociatedAsync(fxAllowances.TestAsset, fxDestination);
    }

    [Fact(DisplayName = "Execute Allowances: Can Spend a Token Allowance")]
    public async Task CanSpendATokenAllowance()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);
        await using var fxDestination = await TestAccount.CreateAsync(_network);

        await fxDestination.Client.AssociateTokenAsync(fxAllowances.TestToken, fxDestination, fxDestination.PrivateKey);

        var xferAmount = fxAllowances.TestToken.Params.Circulation / 3 + 1;

        var client = fxAllowances.Client.Clone(ctx =>
        {
            ctx.Payer = fxAllowances.Agent;
            ctx.Signatory = fxAllowances.Agent.PrivateKey;
        });

        var receipt = await client.TransferAsync(new TransferParams
        {
            TokenTransfers = new[] {
                new TokenTransfer(fxAllowances.TestToken, fxAllowances.Owner, - (long) xferAmount, true),
                new TokenTransfer(fxAllowances.TestToken, fxDestination, (long) xferAmount, false)
            }
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.TokenBalanceAsync(fxAllowances.TestToken, fxAllowances.Owner, fxAllowances.TestToken.Params.Circulation - xferAmount);
        await AssertHg.TokenBalanceAsync(fxAllowances.TestToken, fxDestination, xferAmount);

    }

    [Fact(DisplayName = "Execute Allowances: Can Not Spend a Token Allowance Without Delegated Flag")]
    public async Task CanNotSpendATokenAllowanceWithoutDelegatedFlag()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);
        await using var fxDestination = await TestAccount.CreateAsync(_network);

        await fxDestination.Client.AssociateTokenAsync(fxAllowances.TestToken, fxDestination, fxDestination.PrivateKey);

        var xferAmount = fxAllowances.TestToken.Params.Circulation / 3 + 1;

        var client = fxAllowances.Client.Clone(ctx =>
        {
            ctx.Payer = fxAllowances.Agent;
            ctx.Signatory = fxAllowances.Agent.PrivateKey;
        });

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await client.TransferAsync(new TransferParams
            {
                TokenTransfers = new[] {
                new TokenTransfer(fxAllowances.TestToken, fxAllowances.Owner, - (long) xferAmount),
                new TokenTransfer(fxAllowances.TestToken, fxDestination, (long) xferAmount)
            }
            });
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);

        await AssertHg.TokenBalanceAsync(fxAllowances.TestToken, fxAllowances.Owner, fxAllowances.TestToken.Params.Circulation);
        await AssertHg.TokenBalanceAsync(fxAllowances.TestToken, fxDestination, 0);
    }

    [Fact(DisplayName = "Execute Allowances: Can Spend a Crypto Allowance")]
    public async Task CanSpendACryptoAllowance()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);
        await using var fxDestination = await TestAccount.CreateAsync(_network);

        var initialOwnerBalance = await fxAllowances.Client.GetAccountBalanceAsync(fxAllowances.Owner);
        var initialDestination = await fxDestination.Client.GetAccountBalanceAsync(fxDestination);
        var xferAmount = initialOwnerBalance / 3 + 1;

        var client = fxAllowances.Client.Clone(ctx =>
        {
            ctx.Payer = fxAllowances.Agent;
            ctx.Signatory = fxAllowances.Agent.PrivateKey;
        });

        var receipt = await client.TransferAsync(new TransferParams
        {
            CryptoTransfers = new[]
            {
                new CryptoTransfer(fxAllowances.Owner, - (long) xferAmount, true),
                new CryptoTransfer(fxDestination, (long) xferAmount, false)
            }
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.CryptoBalanceAsync(fxAllowances.Owner, initialOwnerBalance - xferAmount);
        await AssertHg.CryptoBalanceAsync(fxDestination, initialDestination + xferAmount);
    }

    [Fact(DisplayName = "Execute Allowances: Can Not Spend a Crypto Allowance without Delegated Flag")]
    public async Task CanNotSpendACryptoAllowanceWithoutDelegatedFlag()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);
        await using var fxDestination = await TestAccount.CreateAsync(_network);

        var initialOwnerBalance = await fxAllowances.Client.GetAccountBalanceAsync(fxAllowances.Owner);
        var initialDestination = await fxDestination.Client.GetAccountBalanceAsync(fxDestination);
        var xferAmount = initialOwnerBalance / 3 + 1;

        var client = fxAllowances.Client.Clone(ctx =>
        {
            ctx.Payer = fxAllowances.Agent;
            ctx.Signatory = fxAllowances.Agent.PrivateKey;
        });

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await client.TransferAsync(new TransferParams
            {
                CryptoTransfers = new[]
                {
                    new CryptoTransfer(fxAllowances.Owner, - (long) xferAmount, false),
                    new CryptoTransfer(fxDestination, (long) xferAmount, false)
                }
            });
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);

        await AssertHg.CryptoBalanceAsync(fxAllowances.Owner, initialOwnerBalance);
        await AssertHg.CryptoBalanceAsync(fxDestination, initialDestination);
    }

    [Fact(DisplayName = "Execute Allowances: Can Spend an Explicit Asset")]
    public async Task CanGetAssetInfoHavingExplicitAllowance()
    {
        await using var fxAllowance = await TestAllowance.CreateAsync(_network);
        await using var fxOtherAgent = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 50_00_000_000);
        await using var fxDestination = await TestAccount.CreateAsync(_network);

        var asset = new Asset(fxAllowance.TestAsset.Record.Token, 1);

        await fxAllowance.Client.AllocateAsync(new AllowanceParams
        {
            AssetAllowances = new[]
            {
                new AssetAllowance(asset, fxAllowance.Owner, fxOtherAgent),
            },
            Signatory = fxAllowance.Owner.PrivateKey
        });

        var info = await fxAllowance.Client.GetAssetInfoAsync(asset);
        Assert.Equal(asset, info.Asset);
        Assert.Equal(fxAllowance.Owner.Record.Address, info.Owner);
        Assert.Equal(fxAllowance.TestAsset.MintRecord.Concensus, info.Created);
        Assert.Equal(fxAllowance.TestAsset.Metadata[0].ToArray(), info.Metadata.ToArray());
        AssertHg.NotEmpty(info.Ledger);
        Assert.Equal(fxOtherAgent.Record.Address, info.Agent);

        var client = fxAllowance.Client.Clone(ctx =>
        {
            ctx.Payer = fxOtherAgent;
            ctx.Signatory = fxOtherAgent.PrivateKey;
        });

        var receipt = await client.TransferAsync(new TransferParams
        {
            AssetTransfers = new[] { new AssetTransfer(asset, fxAllowance.Owner, fxDestination, true) }
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.AssetBalanceAsync(fxAllowance.TestAsset, fxDestination, 1);

        info = await fxAllowance.Client.GetAssetInfoAsync(asset);
        Assert.Equal(asset, info.Asset);
        Assert.Equal(fxDestination.Record.Address, info.Owner);
        Assert.Equal(fxAllowance.TestAsset.MintRecord.Concensus, info.Created);
        Assert.Equal(fxAllowance.TestAsset.Metadata[0].ToArray(), info.Metadata.ToArray());
        AssertHg.NotEmpty(info.Ledger);
        Assert.Equal(Address.None, info.Agent);

    }
    [Fact(DisplayName = "Execute Allowances: Can Spend an Implicit Asset when Explicit Allowance Exists")]
    public async Task CanSpendAnImplicitAssetWhenExplicitAllowanceExists()
    {
        await using var fxAllowance = await TestAllowance.CreateAsync(_network);
        await using var fxOtherAgent = await TestAccount.CreateAsync(_network);
        await using var fxDestination = await TestAccount.CreateAsync(_network);

        var asset = new Asset(fxAllowance.TestAsset.Record.Token, 1);

        await fxAllowance.Client.AllocateAsync(new AllowanceParams
        {
            AssetAllowances = new[]
            {
                new AssetAllowance(asset, fxAllowance.Owner, fxOtherAgent),
            },
            Signatory = fxAllowance.Owner.PrivateKey
        });

        var info = await fxAllowance.Client.GetAssetInfoAsync(asset);
        Assert.Equal(asset, info.Asset);
        Assert.Equal(fxAllowance.Owner.Record.Address, info.Owner);
        Assert.Equal(fxAllowance.TestAsset.MintRecord.Concensus, info.Created);
        Assert.Equal(fxAllowance.TestAsset.Metadata[0].ToArray(), info.Metadata.ToArray());
        AssertHg.NotEmpty(info.Ledger);
        Assert.Equal(fxOtherAgent.Record.Address, info.Agent);

        var client = fxAllowance.Client.Clone(ctx =>
        {
            ctx.Payer = fxAllowance.Agent;
            ctx.Signatory = fxAllowance.Agent.PrivateKey;
        });

        var receipt = await client.TransferAsync(new TransferParams
        {
            AssetTransfers = new[] { new AssetTransfer(asset, fxAllowance.Owner, fxDestination, true) }
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.AssetBalanceAsync(fxAllowance.TestAsset, fxDestination, 1);

        info = await fxAllowance.Client.GetAssetInfoAsync(asset);
        Assert.Equal(asset, info.Asset);
        Assert.Equal(fxDestination.Record.Address, info.Owner);
        Assert.Equal(fxAllowance.TestAsset.MintRecord.Concensus, info.Created);
        Assert.Equal(fxAllowance.TestAsset.Metadata[0].ToArray(), info.Metadata.ToArray());
        AssertHg.NotEmpty(info.Ledger);
        Assert.Equal(Address.None, info.Agent);

    }

    [Fact(DisplayName = "Execute Allowances: Can Spend an Asset Allowance From Delegate")]
    public async Task CanSpendAnAssetAllowanceFromDelegate()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);
        await using var fxDestination = await TestAccount.CreateAsync(_network);

        await fxDestination.Client.AssociateTokenAsync(fxAllowances.TestAsset, fxDestination, fxDestination.PrivateKey);

        var client = fxAllowances.Client.Clone(ctx =>
        {
            ctx.Payer = fxAllowances.DelegatedAgent;
            ctx.Signatory = fxAllowances.DelegatedAgent.PrivateKey;
        });

        var receipt = await client.TransferAsync(new TransferParams
        {
            AssetTransfers = new[] { new AssetTransfer(new Asset(fxAllowances.TestAsset, 1), fxAllowances.Owner, fxDestination, true) }
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.AssetBalanceAsync(fxAllowances.TestAsset, fxDestination, 1);
    }

    [Fact(DisplayName = "Execute Allowances: Can Spend an Asset Having Delegate by Original Agent")]
    public async Task CanSpendAnAssetHavingDelegateByOriginalAgent()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);
        await using var fxDestination = await TestAccount.CreateAsync(_network);

        await fxDestination.Client.AssociateTokenAsync(fxAllowances.TestAsset, fxDestination, fxDestination.PrivateKey);

        var client = fxAllowances.Client.Clone(ctx =>
        {
            ctx.Payer = fxAllowances.Agent;
            ctx.Signatory = fxAllowances.Agent.PrivateKey;
        });

        var receipt = await client.TransferAsync(new TransferParams
        {
            AssetTransfers = new[] { new AssetTransfer(new Asset(fxAllowances.TestAsset, 1), fxAllowances.Owner, fxDestination, true) }
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.AssetBalanceAsync(fxAllowances.TestAsset, fxDestination, 1);
    }

    [Fact(DisplayName = "Execute Allowances: Can Not Spend an Asset Allowance From Delegate Without Permission")]
    public async Task CanNotSpendAnAssetAllowanceFromDelegateWithoutPermission()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);
        await using var fxDestination = await TestAccount.CreateAsync(_network);

        await fxDestination.Client.AssociateTokenAsync(fxAllowances.TestAsset, fxDestination, fxDestination.PrivateKey);

        var client = fxAllowances.Client.Clone(ctx =>
        {
            ctx.Payer = fxAllowances.DelegatedAgent;
            ctx.Signatory = fxAllowances.DelegatedAgent.PrivateKey;
        });

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            var receipt = await client.TransferAsync(new TransferParams
            {
                AssetTransfers = new[] { new AssetTransfer(new Asset(fxAllowances.TestAsset, 2), fxAllowances.Owner, fxDestination, true) }
            });
        });
        Assert.Equal(ResponseCode.SpenderDoesNotHaveAllowance, tex.Status);

        await AssertHg.AssetBalanceAsync(fxAllowances.TestAsset, fxAllowances.Owner, fxAllowances.TestAsset.Metadata.Length);
        await AssertHg.AssetBalanceAsync(fxAllowances.TestAsset, fxDestination, 0);
    }
    [Fact(DisplayName = "Execute Allowances: Can Delete Account having Allowance")]
    public async Task CanDeleteAccountHavingAllowance()
    {
        await using var fxAllowances = await TestAllowance.CreateAsync(_network);
        await using var fxDestination = await TestAccount.CreateAsync(_network);

        await fxDestination.Client.AssociateTokenAsync(fxAllowances.TestToken, fxDestination, fxDestination.PrivateKey);

        var xferAmount = (long)fxAllowances.TestToken.Params.Circulation / 2;

        var client = fxAllowances.Client.Clone(ctx =>
        {
            ctx.Payer = fxAllowances.Agent;
            ctx.Signatory = fxAllowances.Agent;
        });

        var receipt = await client.TransferAsync(new TransferParams
        {
            TokenTransfers = new[] {
                new TokenTransfer(fxAllowances.TestToken, fxAllowances.Owner, -xferAmount, true),
                new TokenTransfer(fxAllowances.TestToken, fxDestination, xferAmount, false)
            }
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.TokenBalanceAsync(fxAllowances.TestToken, fxDestination, (ulong)xferAmount);

        var del = await client.DeleteAccountAsync(fxAllowances.Agent, _network.Payer);
        Assert.Equal(ResponseCode.Success, del.Status);
    }

    [Fact(DisplayName = "Execute Allowances: Can Spend a Token Allowance With Reimbursement")]
    public async Task CanSpendATokenAllowanceWithReimbursement()
    {
        await using var fxAgent = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxDestination = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxDestination);

        await fxToken.Client.AllocateAsync(new AllowanceParams
        {
            CryptoAllowances = new[] { new CryptoAllowance(fxToken.TreasuryAccount, fxAgent, 50) },
            TokenAllowances = new[] { new TokenAllowance(fxToken, fxToken.TreasuryAccount, fxAgent, (long)fxToken.Params.Circulation - 1) },
            Signatory = fxToken.TreasuryAccount.PrivateKey
        });

        var xferAmount = fxToken.Params.Circulation / 3 + 1;
        await using var client = fxToken.Client.Clone(ctx => { ctx.Payer = fxAgent; ctx.Signatory = fxAgent; });

        var receipt = await client.TransferAsync(new TransferParams
        {
            CryptoTransfers = new[]
            {
                new CryptoTransfer(fxToken.TreasuryAccount, -10, true),
                new CryptoTransfer(fxAgent, 10)
            },
            TokenTransfers = new[] {
                new TokenTransfer(fxToken, fxToken.TreasuryAccount, - (long) xferAmount, true),
                new TokenTransfer(fxToken, fxDestination, (long) xferAmount)
            }
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - xferAmount);
        await AssertHg.TokenBalanceAsync(fxToken, fxDestination, xferAmount);

        await client.DeleteAccountAsync(fxAgent, fxDestination);

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await client.TransferAsync(new TransferParams
            {
                CryptoTransfers = new[]
                {
                new CryptoTransfer(fxToken.TreasuryAccount, -10, true),
                new CryptoTransfer(fxAgent, 10)
            },
                TokenTransfers = new[] {
                new TokenTransfer(fxToken, fxToken.TreasuryAccount, - (long) xferAmount, true),
                new TokenTransfer(fxToken, fxDestination, (long) xferAmount)
            }
            });
        });
        Assert.Equal(ResponseCode.PayerAccountNotFound, pex.Status);

        await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - xferAmount);
        await AssertHg.TokenBalanceAsync(fxToken, fxDestination, xferAmount);
    }
}