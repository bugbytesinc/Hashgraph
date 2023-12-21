namespace Hashgraph.Test.Token;

[Collection(nameof(NetworkCredentials))]
public class GetTokenInfoTests
{
    private readonly NetworkCredentials _network;
    public GetTokenInfoTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Token Info: Can Get Token Info")]
    public async Task CanGetTokenInfo()
    {
        await using var fx = await TestToken.CreateAsync(_network);
        Assert.Equal(ResponseCode.Success, fx.Record.Status);

        var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
        Assert.Equal(fx.Record.Token, info.Token);
        Assert.Equal(fx.Params.Symbol, info.Symbol);
        Assert.Equal(fx.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fx.Params.Circulation, info.Circulation);
        Assert.Equal(fx.Params.Decimals, info.Decimals);
        Assert.Equal(fx.Params.Ceiling, info.Ceiling);
        Assert.Equal(fx.Params.Administrator, info.Administrator);
        Assert.Equal(fx.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fx.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fx.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fx.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fx.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fx.Params.Symbol, info.Symbol);
    }
    [Fact(DisplayName = "Token Info: Null Token Identifier Raises Exception")]
    public async Task NullTokenIdentifierRaisesException()
    {
        await using var fx = await TestToken.CreateAsync(_network);

        var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await fx.Client.GetTokenInfoAsync(null);
        });
        Assert.Equal("token", ane.ParamName);
        Assert.StartsWith("Token is missing. Please check that it is not null", ane.Message);
    }
    [Fact(DisplayName = "Token Info: Empty Address Identifier Raises Exception")]
    public async Task EmptyAddressIdentifierRaisesException()
    {
        await using var fx = await TestToken.CreateAsync(_network);

        var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await fx.Client.GetTokenInfoAsync(Address.None);
        });
        Assert.Equal("token", ane.ParamName);
        Assert.StartsWith("Token is missing. Please check that it is not null", ane.Message);
    }
    [Fact(DisplayName = "Token Info: Account Address for Token Symbol Raises Error")]
    public async Task AccountAddressForTokenSymbolRaisesError()
    {
        await using var fx = await TestAccount.CreateAsync(_network);

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fx.Client.GetTokenInfoAsync(fx.Record.Address);
        });
        Assert.Equal(ResponseCode.InvalidTokenId, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: InvalidTokenId", pex.Message);
    }
    [Fact(DisplayName = "Token Info: Contract Address for Token Symbol Raises Error")]
    public async Task ContractAddressForTokenSymbolRaisesError()
    {
        await using var fx = await GreetingContract.CreateAsync(_network);

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fx.Client.GetTokenInfoAsync(fx.ContractRecord.Contract);
        });
        Assert.Equal(ResponseCode.InvalidTokenId, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: InvalidTokenId", pex.Message);
    }
    [Fact(DisplayName = "Token Info: Topic Address for Token Symbol Raises Error")]
    public async Task TopicAddressForTokenSymbolRaisesError()
    {
        await using var fx = await TestTopic.CreateAsync(_network);

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fx.Client.GetTokenInfoAsync(fx.Record.Topic);
        });
        Assert.Equal(ResponseCode.InvalidTokenId, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: InvalidTokenId", pex.Message);
    }
    [Fact(DisplayName = "Token Info: File Address for Token Symbol Raises Error")]
    public async Task FileAddressForTokenSymbolRaisesError()
    {
        await using var fx = await TestFile.CreateAsync(_network);

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fx.Client.GetTokenInfoAsync(fx.Record.File);
        });
        Assert.Equal(ResponseCode.InvalidTokenId, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: InvalidTokenId", pex.Message);
    }
}