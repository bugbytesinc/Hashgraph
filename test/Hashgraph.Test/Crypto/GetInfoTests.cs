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
        Assert.NotNull(info.ContractId);
        Assert.False(info.Deleted);
        Assert.Equal(0, info.ContractNonce);
        Assert.Equal(new Endorsement(_network.PublicKey), info.Endorsement);
        Assert.True(info.Balance > 0);
        Assert.False(info.ReceiveSignatureRequired);
        Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, info.AutoRenewAccount);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(0, info.AssetCount);
        Assert.Equal(0, info.AutoAssociationLimit);
        Assert.NotEqual(Alias.None, info.Alias);
        // HIP-583 Churn
        //Assert.Empty(info.Monikers);
        AssertHg.NotEmpty(info.Ledger);
        Assert.NotNull(info.StakingInfo);
        Assert.False(info.StakingInfo.Declined);
        Assert.True(ConsensusTimeStamp.MinValue <= info.StakingInfo.PeriodStart);
        Assert.Equal(0, info.StakingInfo.PendingReward);
        Assert.Equal(0, info.StakingInfo.Proxied);
        Assert.Equal(Address.None, info.StakingInfo.Proxy);
        Assert.True(0 <= info.StakingInfo.Node);
    }
    [Fact(DisplayName = "Get Account Info: Can Get Info for Account Facet")]
    public async Task CanGetInfoForAccountFacet()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.Equal(fxAccount.Record.Address, info.Address);
        Assert.NotNull(info.ContractId);
        Assert.False(info.Deleted);
        Assert.Equal(0, info.ContractNonce);
        Assert.Equal(fxAccount.PublicKey, info.Endorsement);
        Assert.Equal(fxAccount.CreateParams.InitialBalance, info.Balance);
        Assert.Equal(fxAccount.CreateParams.RequireReceiveSignature, info.ReceiveSignatureRequired);
        Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, info.AutoRenewAccount);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(fxAccount.CreateParams.Memo, info.Memo);
        Assert.Equal(0, info.AssetCount);
        Assert.Equal(fxAccount.CreateParams.AutoAssociationLimit, info.AutoAssociationLimit);
        Assert.Equal(Alias.None, info.Alias);
        // HIP-583 Churn
        //Assert.Empty(info.Monikers);
        AssertHg.NotEmpty(info.Ledger);
        Assert.NotNull(info.StakingInfo);
        Assert.False(info.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, info.StakingInfo.PeriodStart);
        Assert.Equal(0, info.StakingInfo.PendingReward);
        Assert.Equal(0, info.StakingInfo.Proxied);
        Assert.Equal(Address.None, info.StakingInfo.Proxy);
        Assert.Equal(0, info.StakingInfo.Node);
    }
    [Fact(DisplayName = "Get Account Info: Can Get Info for Alias Facet")]
    public async Task CanGetInfoForAliasFacet()
    {
        await using var fxAccount = await TestAliasAccount.CreateAsync(_network);
        var infoFromAddress = await fxAccount.Client.GetAccountInfoAsync(fxAccount.CreateRecord.Address);
        Assert.Equal(fxAccount.CreateRecord.Address, infoFromAddress.Address);
        Assert.NotNull(infoFromAddress.ContractId);
        Assert.False(infoFromAddress.Deleted);
        Assert.Equal(0, infoFromAddress.ContractNonce);
        Assert.Equal(fxAccount.PublicKey, infoFromAddress.Endorsement);
        Assert.True(infoFromAddress.Balance > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, infoFromAddress.AutoRenewAccount);
        Assert.False(infoFromAddress.ReceiveSignatureRequired);
        Assert.True(infoFromAddress.AutoRenewPeriod.TotalSeconds > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, infoFromAddress.AutoRenewAccount);
        Assert.True(infoFromAddress.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal("auto-created account", infoFromAddress.Memo);
        Assert.Equal(0, infoFromAddress.AssetCount);
        Assert.Equal(0, infoFromAddress.AutoAssociationLimit);
        Assert.Equal(fxAccount.Alias, infoFromAddress.Alias);
        // HIP-583 Churn
        //Assert.Empty(infoFromAddress.Monikers);
        AssertHg.NotEmpty(infoFromAddress.Ledger);
        Assert.NotNull(infoFromAddress.StakingInfo);
        Assert.False(infoFromAddress.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAddress.StakingInfo.PeriodStart);
        Assert.Equal(0, infoFromAddress.StakingInfo.PendingReward);
        Assert.Equal(0, infoFromAddress.StakingInfo.Proxied);
        Assert.Equal(Address.None, infoFromAddress.StakingInfo.Proxy);
        Assert.Equal(0, infoFromAddress.StakingInfo.Node);

        var infoFromAlias = await fxAccount.Client.GetAccountInfoAsync(fxAccount.Alias);
        Assert.Equal(fxAccount.CreateRecord.Address, infoFromAlias.Address);
        Assert.NotNull(infoFromAlias.ContractId);
        Assert.False(infoFromAlias.Deleted);
        Assert.Equal(0, infoFromAlias.ContractNonce);
        Assert.Equal(fxAccount.PublicKey, infoFromAlias.Endorsement);
        Assert.True(infoFromAlias.Balance > 0);
        Assert.False(infoFromAlias.ReceiveSignatureRequired);
        Assert.True(infoFromAlias.AutoRenewPeriod.TotalSeconds > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, infoFromAlias.AutoRenewAccount);
        Assert.True(infoFromAlias.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal("auto-created account", infoFromAlias.Memo);
        Assert.Equal(0, infoFromAlias.AssetCount);
        Assert.Equal(0, infoFromAlias.AutoAssociationLimit);
        Assert.Equal(fxAccount.Alias, infoFromAlias.Alias);
        // HIP-583 Churn
        //Assert.Empty(infoFromAlias.Monikers);
        AssertHg.Equal(infoFromAddress.Ledger, infoFromAlias.Ledger);
        Assert.NotNull(infoFromAlias.StakingInfo);
        Assert.False(infoFromAlias.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, infoFromAlias.StakingInfo.PeriodStart);
        Assert.Equal(0, infoFromAlias.StakingInfo.PendingReward);
        Assert.Equal(0, infoFromAlias.StakingInfo.Proxied);
        Assert.Equal(Address.None, infoFromAlias.StakingInfo.Proxy);
        Assert.Equal(0, infoFromAlias.StakingInfo.Node);
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
        Assert.NotNull(info.ContractId);
        Assert.False(info.Deleted);
        Assert.Equal(0, info.ContractNonce);
        Assert.True(info.Balance > 0);
        Assert.False(info.ReceiveSignatureRequired);
        Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, info.AutoRenewAccount);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(0, info.AssetCount);
        Assert.Equal(Alias.None, info.Alias);
        // HIP-583 Churn
        //Assert.Empty(info.Monikers);
        AssertHg.NotEmpty(info.Ledger);
        Assert.NotNull(info.StakingInfo);
        Assert.False(info.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, info.StakingInfo.PeriodStart);
        Assert.Equal(0, info.StakingInfo.PendingReward);
        Assert.True(info.StakingInfo.Proxied > -1);
        Assert.Equal(Address.None, info.StakingInfo.Proxy);
        Assert.Equal(0, info.StakingInfo.Node);
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
        Assert.NotNull(info.ContractId);
        Assert.False(info.Deleted);
        Assert.Equal(0, info.ContractNonce);
        Assert.True(info.Balance > 0);
        Assert.False(info.ReceiveSignatureRequired);
        Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, info.AutoRenewAccount);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(fxAsset.Metadata.Length, info.AssetCount);
        Assert.Equal(fxAsset.TreasuryAccount.CreateParams.AutoAssociationLimit, info.AutoAssociationLimit);
        Assert.Equal(Alias.None, info.Alias);
        // HIP-583 Churn
        //Assert.Empty(info.Monikers);
        AssertHg.NotEmpty(info.Ledger);
        Assert.NotNull(info.StakingInfo);
        Assert.False(info.StakingInfo.Declined);
        Assert.Equal(ConsensusTimeStamp.MinValue, info.StakingInfo.PeriodStart);
        Assert.Equal(0, info.StakingInfo.PendingReward);
        Assert.Equal(0, info.StakingInfo.Proxied);
        Assert.Equal(Address.None, info.StakingInfo.Proxy);
        Assert.Equal(0, info.StakingInfo.Node);
    }
    [Fact(DisplayName = "Get Account Info: Can Get Info for Ed25519 Account")]
    public async Task CanGetInfoForAccountEd25519Async()
    {
        await using var client = _network.NewClient();
        var initialBalance = (ulong)Generator.Integer(10, 200);
        var (publicKey, privateKey) = Generator.Ed25519KeyPair();
        var account = (await client.CreateAccountAsync(new CreateAccountParams
        {
            InitialBalance = initialBalance,
            Endorsement = publicKey
        })).Address;
        var info = await client.GetAccountInfoAsync(account);
        Assert.NotNull(info.Address);
        Assert.Equal(account.RealmNum, info.Address.RealmNum);
        Assert.Equal(account.ShardNum, info.Address.ShardNum);
        Assert.Equal(account.AccountNum, info.Address.AccountNum);
        Assert.NotNull(info.ContractId);
        Assert.False(info.Deleted);
        Assert.Equal(0, info.ContractNonce);
        Assert.Equal(new Endorsement(KeyType.Ed25519, publicKey), info.Endorsement);
        Assert.True(info.Balance > 0);
        Assert.False(info.ReceiveSignatureRequired);
        Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, info.AutoRenewAccount);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(0, info.AssetCount);
        Assert.Equal(0, info.AutoAssociationLimit);
        Assert.Equal(Alias.None, info.Alias);
        // HIP-583 Churn
        //Assert.Empty(info.Monikers);
        AssertHg.NotEmpty(info.Ledger);
        Assert.NotNull(info.StakingInfo);
        Assert.False(info.StakingInfo.Declined);
        Assert.True(ConsensusTimeStamp.MinValue <= info.StakingInfo.PeriodStart);
        Assert.Equal(0, info.StakingInfo.PendingReward);
        Assert.Equal(0, info.StakingInfo.Proxied);
        Assert.Equal(Address.None, info.StakingInfo.Proxy);
        Assert.True(0 <= info.StakingInfo.Node);
    }
    [Fact(DisplayName = "Get Account Info: Can Get Info for Secp256K1 Account")]
    public async Task CanGetInfoForAccountSecp256K1Async()
    {
        await using var client = _network.NewClient();
        var initialBalance = (ulong)Generator.Integer(10, 200);
        var (publicKey, privateKey) = Generator.Secp256k1KeyPair();
        var account = (await client.CreateAccountAsync(new CreateAccountParams
        {
            InitialBalance = initialBalance,
            Endorsement = publicKey
        })).Address;
        var info = await client.GetAccountInfoAsync(account);
        Assert.NotNull(info.Address);
        Assert.Equal(account.RealmNum, info.Address.RealmNum);
        Assert.Equal(account.ShardNum, info.Address.ShardNum);
        Assert.Equal(account.AccountNum, info.Address.AccountNum);
        Assert.NotNull(info.ContractId);
        Assert.False(info.Deleted);
        Assert.Equal(0, info.ContractNonce);
        Assert.Equal(new Endorsement(KeyType.ECDSASecp256K1, publicKey), info.Endorsement);
        Assert.True(info.Balance > 0);
        Assert.False(info.ReceiveSignatureRequired);
        Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
        // v0.34.0 Churn
        //Assert.Equal(Address.None, info.AutoRenewAccount);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(0, info.AssetCount);
        Assert.Equal(0, info.AutoAssociationLimit);
        Assert.Equal(Alias.None, info.Alias);
        // HIP-583 Churn
        //Assert.Empty(info.Monikers);
        AssertHg.NotEmpty(info.Ledger);
        Assert.NotNull(info.StakingInfo);
        Assert.False(info.StakingInfo.Declined);
        Assert.True(ConsensusTimeStamp.MinValue <= info.StakingInfo.PeriodStart);
        Assert.Equal(0, info.StakingInfo.PendingReward);
        Assert.Equal(0, info.StakingInfo.Proxied);
        Assert.Equal(Address.None, info.StakingInfo.Proxy);
        Assert.True(0 <= info.StakingInfo.Node);
    }
}