using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class GetDetailTests
{
    private readonly NetworkCredentials _network;
    public GetDetailTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Get Account Detail: Can Get Detail for Account")]
    public async Task CanGetDetailForAccountAsync()
    {
        await using var client = _network.NewClient();
        var account = _network.Payer;

        var systemAddress = await _network.GetGenisisAccountAddress();
        if (systemAddress is null)
        {
            await checkDetails(_network.Payer);
        }
        else
        {
            await checkDetails(systemAddress);
        }

        async Task checkDetails(Address payer)
        {
            var detail = await client.GetAccountDetailAsync(account, ctx => ctx.Payer = payer);
            Assert.NotNull(detail.Address);
            Assert.Equal(account.RealmNum, detail.Address.RealmNum);
            Assert.Equal(account.ShardNum, detail.Address.ShardNum);
            Assert.Equal(account.AccountNum, detail.Address.AccountNum);
            Assert.NotNull(detail.SmartContractId);
            Assert.False(detail.Deleted);
            Assert.NotNull(detail.Proxy);
            Assert.Equal(new Address(0, 0, 0), detail.Proxy);
            Assert.Equal(0, detail.ProxiedToAccount);
            Assert.Equal(new Endorsement(_network.PublicKey), detail.Endorsement);
            Assert.True(detail.Balance > 0);
            Assert.False(detail.ReceiveSignatureRequired);
            Assert.True(detail.AutoRenewPeriod.TotalSeconds > 0);
            Assert.True(detail.Expiration > DateTime.MinValue);
            Assert.Equal(0, detail.AssetCount);
            Assert.Equal(0, detail.AutoAssociationLimit);
            Assert.Equal(Alias.None, detail.Alias);
            AssertHg.NotEmpty(detail.Ledger);
            Assert.Empty(detail.CryptoAllowances);
            Assert.Empty(detail.TokenAllowances);
            Assert.Empty(detail.AssetAllowances);
        }
    }
    [Fact(DisplayName = "Get Account Detail: Can Get Detail for Account Facet")]
    public async Task CanGetDetailForAccountFacet()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);

        var systemAddress = await _network.GetGenisisAccountAddress();
        if (systemAddress is null)
        {
            await checkDetails(_network.Payer);
        }
        else
        {
            await checkDetails(systemAddress);
        }

        async Task checkDetails(Address payer)
        {
            var detail = await fxAccount.Client.GetAccountDetailAsync(fxAccount, ctx => ctx.Payer = payer);
            Assert.Equal(fxAccount.Record.Address, detail.Address);
            Assert.NotNull(detail.SmartContractId);
            Assert.False(detail.Deleted);
            Assert.NotNull(detail.Proxy);
            Assert.Equal(Address.None, detail.Proxy);
            Assert.Equal(0, detail.ProxiedToAccount);
            Assert.Equal(fxAccount.PublicKey, detail.Endorsement);
            Assert.Equal(fxAccount.CreateParams.InitialBalance, detail.Balance);
            Assert.Equal(fxAccount.CreateParams.RequireReceiveSignature, detail.ReceiveSignatureRequired);
            Assert.True(detail.AutoRenewPeriod.TotalSeconds > 0);
            Assert.True(detail.Expiration > DateTime.MinValue);
            Assert.Equal(fxAccount.CreateParams.Memo, detail.Memo);
            Assert.Equal(0, detail.AssetCount);
            Assert.Equal(fxAccount.CreateParams.AutoAssociationLimit, detail.AutoAssociationLimit);
            Assert.Equal(Alias.None, detail.Alias);
            AssertHg.NotEmpty(detail.Ledger);
            Assert.Empty(detail.CryptoAllowances);
            Assert.Empty(detail.TokenAllowances);
            Assert.Empty(detail.AssetAllowances);
        }
    }
    [Fact(DisplayName = "Get Account Detail: Can Get Detail for Alias Facet")]
    public async Task CanGetDetailForAliasFacet()
    {
        await using var fxAccount = await TestAliasAccount.CreateAsync(_network);

        var systemAddress = await _network.GetGenisisAccountAddress();
        if (systemAddress is null)
        {
            await checkDetails(_network.Payer);
        }
        else
        {
            await checkDetails(systemAddress);
        }

        async Task checkDetails(Address payer)
        {
            var detailFromAddress = await fxAccount.Client.GetAccountDetailAsync(fxAccount.CreateRecord.Address, ctx => ctx.Payer = payer);
            Assert.Equal(fxAccount.CreateRecord.Address, detailFromAddress.Address);
            Assert.NotNull(detailFromAddress.SmartContractId);
            Assert.False(detailFromAddress.Deleted);
            Assert.NotNull(detailFromAddress.Proxy);
            Assert.Equal(Address.None, detailFromAddress.Proxy);
            Assert.Equal(0, detailFromAddress.ProxiedToAccount);
            Assert.Equal(fxAccount.PublicKey, detailFromAddress.Endorsement);
            Assert.True(detailFromAddress.Balance > 0);
            Assert.False(detailFromAddress.ReceiveSignatureRequired);
            Assert.True(detailFromAddress.AutoRenewPeriod.TotalSeconds > 0);
            Assert.True(detailFromAddress.Expiration > DateTime.MinValue);
            Assert.Equal("auto-created account", detailFromAddress.Memo);
            Assert.Equal(0, detailFromAddress.AssetCount);
            Assert.Equal(0, detailFromAddress.AutoAssociationLimit);
            Assert.Equal(fxAccount.Alias, detailFromAddress.Alias);
            AssertHg.NotEmpty(detailFromAddress.Ledger);
            Assert.Empty(detailFromAddress.CryptoAllowances);
            Assert.Empty(detailFromAddress.TokenAllowances);
            Assert.Empty(detailFromAddress.AssetAllowances);

            var detailFromAlias = await fxAccount.Client.GetAccountDetailAsync(fxAccount.Alias, ctx => ctx.Payer = payer);
            Assert.Equal(fxAccount.CreateRecord.Address, detailFromAlias.Address);
            Assert.NotNull(detailFromAlias.SmartContractId);
            Assert.False(detailFromAlias.Deleted);
            Assert.NotNull(detailFromAlias.Proxy);
            Assert.Equal(Address.None, detailFromAlias.Proxy);
            Assert.Equal(0, detailFromAlias.ProxiedToAccount);
            Assert.Equal(fxAccount.PublicKey, detailFromAlias.Endorsement);
            Assert.True(detailFromAlias.Balance > 0);
            Assert.False(detailFromAlias.ReceiveSignatureRequired);
            Assert.True(detailFromAlias.AutoRenewPeriod.TotalSeconds > 0);
            Assert.True(detailFromAlias.Expiration > DateTime.MinValue);
            Assert.Equal("auto-created account", detailFromAlias.Memo);
            Assert.Equal(0, detailFromAlias.AssetCount);
            Assert.Equal(0, detailFromAlias.AutoAssociationLimit);
            Assert.Equal(fxAccount.Alias, detailFromAlias.Alias);
            AssertHg.Equal(detailFromAddress.Ledger, detailFromAlias.Ledger);
            Assert.NotStrictEqual(detailFromAddress.CryptoAllowances, detailFromAlias.CryptoAllowances);
            Assert.NotStrictEqual(detailFromAddress.TokenAllowances, detailFromAlias.TokenAllowances);
            Assert.NotStrictEqual(detailFromAddress.AssetAllowances, detailFromAlias.AssetAllowances);
        }
    }
    [Fact(DisplayName = "Get Account Detail: Can Get Detail for Server Node")]
    public async Task CanGetDetailForGatewayAsync()
    {
        await using var client = _network.NewClient();
        var account = _network.Gateway;

        var systemAddress = await _network.GetGenisisAccountAddress();
        if (systemAddress is null)
        {
            await checkDetails(_network.Payer);
        }
        else
        {
            await checkDetails(systemAddress);
        }

        async Task checkDetails(Address payer)
        {
            var detail = await client.GetAccountDetailAsync(account, ctx=>ctx.Payer = payer);
            Assert.NotNull(detail.Address);
            Assert.Equal(account.ShardNum, detail.Address.ShardNum);
            Assert.Equal(account.RealmNum, detail.Address.RealmNum);
            Assert.Equal(account.AccountNum, detail.Address.AccountNum);
            Assert.NotNull(detail.SmartContractId);
            Assert.False(detail.Deleted);
            Assert.NotNull(detail.Proxy);
            Assert.Equal(new Address(0, 0, 0), detail.Proxy);
            Assert.True(detail.ProxiedToAccount > -1);
            Assert.True(detail.Balance > 0);
            Assert.False(detail.ReceiveSignatureRequired);
            Assert.True(detail.AutoRenewPeriod.TotalSeconds > 0);
            Assert.True(detail.Expiration > DateTime.MinValue);
            Assert.Equal(0, detail.AssetCount);
            Assert.Equal(Alias.None, detail.Alias);
            AssertHg.NotEmpty(detail.Ledger);
            Assert.Empty(detail.CryptoAllowances);
            Assert.Empty(detail.TokenAllowances);
            Assert.Empty(detail.AssetAllowances);
        }
    }
    [Fact(DisplayName = "Get Account Detail: Getting Account Detail without paying signature fails.")]
    public async Task GetDetailWithoutPayingSignatureThrowsException()
    {
        await using var client = _network.NewClient();
        client.Configure(ctx => ctx.Signatory = null);
        var account = _network.Payer;
        var ioe = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await client.GetAccountDetailAsync(account);
        });
        Assert.StartsWith("The Payer's signatory (signing key/callback) has not been configured. This is required for retreiving records and other general network Queries. Please check that", ioe.Message);
    }
    [Fact(DisplayName = "Get Account Detail: Can Get Detail for Asset Treasury Account")]
    public async Task CanGetDetailForAssetTreasuryAccount()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network);

        var systemAddress = await _network.GetGenisisAccountAddress();
        if (systemAddress is null)
        {
            await checkDetails(_network.Payer);
        }
        else
        {
            await checkDetails(systemAddress);
        }

        async Task checkDetails(Address payer)
        {
            var detail = await fxAsset.Client.GetAccountDetailAsync(fxAsset.TreasuryAccount.Record.Address, ctx => ctx.Payer = payer);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, detail.Address);
            Assert.NotNull(detail.SmartContractId);
            Assert.False(detail.Deleted);
            Assert.NotNull(detail.Proxy);
            Assert.Equal(new Address(0, 0, 0), detail.Proxy);
            Assert.True(detail.ProxiedToAccount > -1);
            Assert.True(detail.Balance > 0);
            Assert.False(detail.ReceiveSignatureRequired);
            Assert.True(detail.AutoRenewPeriod.TotalSeconds > 0);
            Assert.True(detail.Expiration > DateTime.MinValue);
            Assert.Equal(fxAsset.Metadata.Length, detail.AssetCount);
            Assert.Equal(fxAsset.TreasuryAccount.CreateParams.AutoAssociationLimit, detail.AutoAssociationLimit);
            Assert.Equal(Alias.None, detail.Alias);
            AssertHg.NotEmpty(detail.Ledger);
            Assert.Empty(detail.CryptoAllowances);
            Assert.Empty(detail.TokenAllowances);
            Assert.Empty(detail.AssetAllowances);
        }
    }
}