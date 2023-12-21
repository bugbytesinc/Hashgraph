namespace Hashgraph.Test.AssetTokens;

[Collection(nameof(NetworkCredentials))]
public class GetAssetInfoTests
{
    private readonly NetworkCredentials _network;
    public GetAssetInfoTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Asset Info: Can Get Asset Info")]
    public async Task CanGetAssetInfo()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

        var asset = new Asset(fxAsset.Record.Token, 1);
        var receipt = await fxAsset.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount.Record.Address, fxAccount.Record.Address, fxAsset.TreasuryAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxAsset.Client.GetAssetInfoAsync(asset);
        Assert.Equal(asset, info.Asset);
        Assert.Equal(fxAccount.Record.Address, info.Owner);
        Assert.Equal(fxAsset.MintRecord.Concensus, info.Created);
        Assert.Equal(fxAsset.Metadata[0].ToArray(), info.Metadata.ToArray());
        AssertHg.NotEmpty(info.Ledger);
        Assert.Equal(Address.None, info.Agent);
    }

    [Fact(DisplayName = "Asset Info: Can Get Asset Info Having Delegated Allowance")]
    public async Task CanGetAssetInfoHavingDelegatedAllowance()
    {
        await using var fxAllowance = await TestAllowance.CreateAsync(_network);

        var asset = new Asset(fxAllowance.TestAsset.Record.Token, 1);

        var info = await fxAllowance.Client.GetAssetInfoAsync(asset);
        Assert.Equal(asset, info.Asset);
        Assert.Equal(fxAllowance.Owner.Record.Address, info.Owner);
        Assert.Equal(fxAllowance.TestAsset.MintRecord.Concensus, info.Created);
        Assert.Equal(fxAllowance.TestAsset.Metadata[0].ToArray(), info.Metadata.ToArray());
        AssertHg.NotEmpty(info.Ledger);
        Assert.Equal(fxAllowance.DelegatedAgent.Record.Address, info.Agent);
    }

    [Fact(DisplayName = "NETWORK V0.27.0 DEFECT: Asset Info: Can Get Asset Info Having Allowance")]
    public async Task CanGetAssetInfoHavingAllowanceDefect()
    {
        // https://github.com/hashgraph/hedera-services/issues/3486
        // tokenGetNftInfo does not return correct spenderID accountNum value.
        var testFailException = (await Assert.ThrowsAsync<Xunit.Sdk.EqualException>(CanGetAssetInfoHavingAllowAllAllowance));
        Assert.StartsWith("Assert.Equal() Failure", testFailException.Message);
        //[Fact(DisplayName = "Asset Info: Can Get Asset Info Having Allow All Allowance")]
        async Task CanGetAssetInfoHavingAllowAllAllowance()
        {
            await using var fxAllowance = await TestAllowance.CreateAsync(_network);

            var asset = new Asset(fxAllowance.TestAsset.Record.Token, 2);

            var info = await fxAllowance.Client.GetAssetInfoAsync(asset);
            Assert.Equal(asset, info.Asset);
            Assert.Equal(fxAllowance.Owner.Record.Address, info.Owner);
            Assert.Equal(fxAllowance.TestAsset.MintRecord.Concensus, info.Created);
            Assert.Equal(fxAllowance.TestAsset.Metadata[1].ToArray(), info.Metadata.ToArray());
            AssertHg.NotEmpty(info.Ledger);
            Assert.Equal(fxAllowance.Agent.Record.Address, info.Agent);
        }
    }

    [Fact(DisplayName = "Asset Info: Can Get Asset Info Having Explicit Allowance")]
    public async Task CanGetAssetInfoHavingExplicitAllowance()
    {
        await using var fxAllowance = await TestAllowance.CreateAsync(_network);
        await using var fxOtherAgent = await TestAccount.CreateAsync(_network);

        var asset = new Asset(fxAllowance.TestAsset.Record.Token, 1);

        await fxAllowance.Client.AllocateAsync(new AllowanceParams
        {
            AssetAllowances = new[]
            {
                new AssetAllowance(asset, fxAllowance.Owner, fxOtherAgent),
            },
            Signatory = fxAllowance.Owner.PrivateKey
        }); ;

        var info = await fxAllowance.Client.GetAssetInfoAsync(asset);
        Assert.Equal(asset, info.Asset);
        Assert.Equal(fxAllowance.Owner.Record.Address, info.Owner);
        Assert.Equal(fxAllowance.TestAsset.MintRecord.Concensus, info.Created);
        Assert.Equal(fxAllowance.TestAsset.Metadata[0].ToArray(), info.Metadata.ToArray());
        AssertHg.NotEmpty(info.Ledger);
        Assert.Equal(fxOtherAgent.Record.Address, info.Agent);
    }
}