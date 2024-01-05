
namespace Hashgraph.Test;

[Collection(nameof(NetworkCredentials))]
public class FeatureTests
{
    private readonly NetworkCredentials _network;
    public FeatureTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Features: Token Balances Are Disabled")]
    public async Task TokenBalancesAreDisabled()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var xferAmount = 2 * fxToken.Params.Circulation / 3;

        var receipt = await fxToken.Client.TransferTokensAsync(fxToken.Record.Token, fxToken.TreasuryAccount.Record.Address, fxAccount.Record.Address, (long)xferAmount, fxToken.TreasuryAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        IMessage interceptedMessage = null;
        await fxToken.Client.GetAccountBalanceAsync(fxAccount, ctx => ctx.OnResponseReceived += (int arg1, IMessage message) => interceptedMessage = message);

        var balances = (interceptedMessage as Response)?.CryptogetAccountBalance;
        Assert.NotNull(balances);
        Assert.Equal(fxAccount.CreateParams.InitialBalance, balances.Balance);
#pragma warning disable CS0612 // Type or member is obsolete
        Assert.Empty(balances.TokenBalances);
#pragma warning restore CS0612 // Type or member is obsolete
    }
}