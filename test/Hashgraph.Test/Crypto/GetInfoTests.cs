using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class GetInfoTests
{
    private readonly NetworkCredentials _network;
    public GetInfoTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Get Account Info: Can Get Info for Account")]
    public async Task CanGetInfoForAccountAsync()
    {
        await using var client = _network.NewClient();
        var account = _network.Payer;
        var info = await client.GetAccountInfoAsync(account);
        Assert.NotNull(info.Address);
        Assert.Equal(account.RealmNum, info.Address.RealmNum);
        Assert.Equal(account.ShardNum, info.Address.ShardNum);
        Assert.Equal(account.AccountNum, info.Address.AccountNum);
        Assert.NotNull(info.SmartContractId);
        Assert.False(info.Deleted);
        Assert.NotNull(info.Proxy);
        Assert.Equal(new Address(0, 0, 0), info.Proxy);
        Assert.Equal(0, info.ProxiedToAccount);
        Assert.Equal(new Endorsement(_network.PublicKey), info.Endorsement);
        Assert.True(info.Balance > 0);
        Assert.False(info.ReceiveSignatureRequired);
        Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        Assert.True(info.Expiration > DateTime.MinValue);
        Assert.Equal(0, info.AssetCount);
        Assert.Equal(0, info.AutoAssociationLimit);
        Assert.Equal(Alias.None, info.Alias);
        AssertHg.NotEmpty(info.Ledger);
        Assert.Empty(info.CryptoAllowances);
        Assert.Empty(info.TokenAllowances);
        Assert.Empty(info.AssetAllowances);
    }
    [Fact(DisplayName = "Get Account Info: Can Get Info for Account Facet")]
    public async Task CanGetInfoForAccountFacet()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.Equal(fxAccount.Record.Address, info.Address);
        Assert.NotNull(info.SmartContractId);
        Assert.False(info.Deleted);
        Assert.NotNull(info.Proxy);
        Assert.Equal(Address.None, info.Proxy);
        Assert.Equal(0, info.ProxiedToAccount);
        Assert.Equal(fxAccount.PublicKey, info.Endorsement);
        Assert.Equal(fxAccount.CreateParams.InitialBalance, info.Balance);
        Assert.Equal(fxAccount.CreateParams.RequireReceiveSignature, info.ReceiveSignatureRequired);
        Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        Assert.True(info.Expiration > DateTime.MinValue);
        Assert.Equal(fxAccount.CreateParams.Memo, info.Memo);
        Assert.Equal(0, info.AssetCount);
        Assert.Equal(fxAccount.CreateParams.AutoAssociationLimit, info.AutoAssociationLimit);
        Assert.Equal(Alias.None, info.Alias);
        AssertHg.NotEmpty(info.Ledger);
        Assert.Empty(info.CryptoAllowances);
        Assert.Empty(info.TokenAllowances);
        Assert.Empty(info.AssetAllowances);
    }
    [Fact(DisplayName = "Get Account Info: Can Get Info for Alias Facet")]
    public async Task CanGetInfoForAliasFacet()
    {
        await using var fxAccount = await TestAliasAccount.CreateAsync(_network);
        var infoFromAddress = await fxAccount.Client.GetAccountInfoAsync(fxAccount.CreateRecord.Address);
        Assert.Equal(fxAccount.CreateRecord.Address, infoFromAddress.Address);
        Assert.NotNull(infoFromAddress.SmartContractId);
        Assert.False(infoFromAddress.Deleted);
        Assert.NotNull(infoFromAddress.Proxy);
        Assert.Equal(Address.None, infoFromAddress.Proxy);
        Assert.Equal(0, infoFromAddress.ProxiedToAccount);
        Assert.Equal(fxAccount.PublicKey, infoFromAddress.Endorsement);
        Assert.True(infoFromAddress.Balance > 0);
        Assert.False(infoFromAddress.ReceiveSignatureRequired);
        Assert.True(infoFromAddress.AutoRenewPeriod.TotalSeconds > 0);
        Assert.True(infoFromAddress.Expiration > DateTime.MinValue);
        Assert.Equal("auto-created account", infoFromAddress.Memo);
        Assert.Equal(0, infoFromAddress.AssetCount);
        Assert.Equal(0, infoFromAddress.AutoAssociationLimit);
        Assert.Equal(fxAccount.Alias, infoFromAddress.Alias);
        AssertHg.NotEmpty(infoFromAddress.Ledger);
        Assert.Empty(infoFromAddress.CryptoAllowances);
        Assert.Empty(infoFromAddress.TokenAllowances);
        Assert.Empty(infoFromAddress.AssetAllowances);

        var infoFromAlias = await fxAccount.Client.GetAccountInfoAsync(fxAccount.Alias);
        Assert.Equal(fxAccount.CreateRecord.Address, infoFromAlias.Address);
        Assert.NotNull(infoFromAlias.SmartContractId);
        Assert.False(infoFromAlias.Deleted);
        Assert.NotNull(infoFromAlias.Proxy);
        Assert.Equal(Address.None, infoFromAlias.Proxy);
        Assert.Equal(0, infoFromAlias.ProxiedToAccount);
        Assert.Equal(fxAccount.PublicKey, infoFromAlias.Endorsement);
        Assert.True(infoFromAlias.Balance > 0);
        Assert.False(infoFromAlias.ReceiveSignatureRequired);
        Assert.True(infoFromAlias.AutoRenewPeriod.TotalSeconds > 0);
        Assert.True(infoFromAlias.Expiration > DateTime.MinValue);
        Assert.Equal("auto-created account", infoFromAlias.Memo);
        Assert.Equal(0, infoFromAlias.AssetCount);
        Assert.Equal(0, infoFromAlias.AutoAssociationLimit);
        Assert.Equal(fxAccount.Alias, infoFromAlias.Alias);
        AssertHg.Equal(infoFromAddress.Ledger, infoFromAlias.Ledger);
        Assert.NotStrictEqual(infoFromAddress.CryptoAllowances, infoFromAlias.CryptoAllowances);
        Assert.NotStrictEqual(infoFromAddress.TokenAllowances, infoFromAlias.TokenAllowances);
        Assert.NotStrictEqual(infoFromAddress.AssetAllowances, infoFromAlias.AssetAllowances);
    }
    [Fact(DisplayName = "Get Account Info: Can Get Info for Server Node")]
    public async Task CanGetInfoForGatewayAsync()
    {
        await using var client = _network.NewClient();
        var account = _network.Gateway;
        var info = await client.GetAccountInfoAsync(account);
        Assert.NotNull(info.Address);
        Assert.Equal(account.ShardNum, info.Address.ShardNum);
        Assert.Equal(account.RealmNum, info.Address.RealmNum);
        Assert.Equal(account.AccountNum, info.Address.AccountNum);
        Assert.NotNull(info.SmartContractId);
        Assert.False(info.Deleted);
        Assert.NotNull(info.Proxy);
        Assert.Equal(new Address(0, 0, 0), info.Proxy);
        Assert.True(info.ProxiedToAccount > -1);
        Assert.True(info.Balance > 0);
        Assert.False(info.ReceiveSignatureRequired);
        Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        Assert.True(info.Expiration > DateTime.MinValue);
        Assert.Equal(0, info.AssetCount);
        Assert.Equal(Alias.None, info.Alias);
        AssertHg.NotEmpty(info.Ledger);
        Assert.Empty(info.CryptoAllowances);
        Assert.Empty(info.TokenAllowances);
        Assert.Empty(info.AssetAllowances);
    }
    [Fact(DisplayName = "Get Account Info: Getting Account Info without paying signature fails.")]
    public async Task GetInfoWithoutPayingSignatureThrowsException()
    {
        await using var client = _network.NewClient();
        client.Configure(ctx => ctx.Signatory = null);
        var account = _network.Payer;
        var ioe = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await client.GetAccountInfoAsync(account);
        });
        Assert.StartsWith("The Payer's signatory (signing key/callback) has not been configured. This is required for retreiving records and other general network Queries. Please check that", ioe.Message);
    }
    [Fact(DisplayName = "Get Account Info: Can Get Info for Asset Treasury Account")]
    public async Task CanGetInfoForAssetTreasuryAccount()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network);

        var info = await fxAsset.Client.GetAccountInfoAsync(fxAsset.TreasuryAccount.Record.Address);
        Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Address);
        Assert.NotNull(info.SmartContractId);
        Assert.False(info.Deleted);
        Assert.NotNull(info.Proxy);
        Assert.Equal(new Address(0, 0, 0), info.Proxy);
        Assert.True(info.ProxiedToAccount > -1);
        Assert.True(info.Balance > 0);
        Assert.False(info.ReceiveSignatureRequired);
        Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        Assert.True(info.Expiration > DateTime.MinValue);
        Assert.Equal(fxAsset.Metadata.Length, info.AssetCount);
        Assert.Equal(fxAsset.TreasuryAccount.CreateParams.AutoAssociationLimit, info.AutoAssociationLimit);
        Assert.Equal(Alias.None, info.Alias);
        AssertHg.NotEmpty(info.Ledger);
        Assert.Empty(info.CryptoAllowances);
        Assert.Empty(info.TokenAllowances);
        Assert.Empty(info.AssetAllowances);
    }
}