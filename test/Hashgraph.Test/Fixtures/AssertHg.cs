namespace Hashgraph.Test.Fixtures;

public static class AssertHg
{
    public static async Task TokenStatusAsync(TestToken fxToken, TestAccount fxAccount, TokenKycStatus status)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var tokenRecord = tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
        Assert.NotNull(tokenRecord);

        Assert.Equal(status, tokenRecord.KycStatus);
    }

    public static async Task TokenStatusAsync(TestToken fxToken, TestAliasAccount fxAccount, TokenKycStatus status)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var tokenRecord = tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
        Assert.NotNull(tokenRecord);

        Assert.Equal(status, tokenRecord.KycStatus);
    }

    public static async Task AssetStatusAsync(TestAsset fxAsset, TestAccount fxAccount, TokenKycStatus status)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var tokenRecord = tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.NotNull(tokenRecord);

        Assert.Equal(status, tokenRecord.KycStatus);
    }

    public static async Task AssetStatusAsync(TestAsset fxAsset, TestAliasAccount fxAccount, TokenKycStatus status)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var tokenRecord = tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.NotNull(tokenRecord);

        Assert.Equal(status, tokenRecord.KycStatus);
    }

    public static async Task TokenStatusAsync(TestToken fxToken, TestAccount fxAccount, TokenTradableStatus status)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var tokenRecord = tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
        Assert.NotNull(tokenRecord);

        Assert.Equal(status, tokenRecord.FreezeStatus);
    }

    public static async Task TokenStatusAsync(TestToken fxToken, TestAliasAccount fxAccount, TokenTradableStatus status)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var tokenRecord = tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
        Assert.NotNull(tokenRecord);

        Assert.Equal(status, tokenRecord.FreezeStatus);
    }

    public static async Task AssetStatusAsync(TestAsset fxAsset, TestAccount fxAccount, TokenTradableStatus status)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var tokenRecord = tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.NotNull(tokenRecord);

        Assert.Equal(status, tokenRecord.FreezeStatus);
    }

    public static async Task AssetStatusAsync(TestAsset fxAsset, TestAliasAccount fxAccount, TokenTradableStatus status)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var tokenRecord = tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.NotNull(tokenRecord);

        Assert.Equal(status, tokenRecord.FreezeStatus);
    }

    public static async Task TokenPausedAsync(TestToken fxToken, TokenTradableStatus status)
    {
        var info = await fxToken.Client.GetTokenInfoAsync(fxToken);
        Assert.NotNull(info);

        Assert.Equal(status, info.PauseStatus);
    }

    public static async Task AssetPausedAsync(TestAsset fxAsset, TokenTradableStatus status)
    {
        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset);
        Assert.NotNull(info);

        Assert.Equal(status, info.PauseStatus);
    }

    public static async Task TokenBalanceAsync(TestToken fxToken, TestAccount fxAccount, ulong expectedBalance)
    {
        var balance = await fxAccount.GetTokenBalanceAsync(fxToken);
        Assert.Equal((long)expectedBalance, balance);
    }
    public static async Task AssetBalanceAsync(TestAsset fxAsset, TestAccount fxAccount, ulong expectedBalance)
    {
        var balance = await fxAccount.GetTokenBalanceAsync(fxAsset);
        Assert.Equal((long)expectedBalance, balance);
    }
    public static async Task AssetBalanceAsync(TestAsset fxAsset, TestAccount fxAccount, int expectedBalance)
    {
        var balance = await fxAccount.GetTokenBalanceAsync(fxAsset);
        Assert.Equal(expectedBalance, balance);
    }

    internal static async Task TokenNotAssociatedAsync(TestToken fxToken, TestAccount fxAccount)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var association = tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
        Assert.Null(association);
    }

    internal static async Task TokenNotAssociatedAsync(TestToken fxToken, TestAliasAccount fxAccount)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var association = tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
        Assert.Null(association);
    }

    internal static async Task<TokenHoldingData> TokenIsAssociatedAsync(TestToken fxToken, TestAccount fxAccount)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var association = tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
        Assert.NotNull(association);

        return association;
    }

    internal static async Task<TokenHoldingData> TokenIsAssociatedAsync(TestToken fxToken, TestAliasAccount fxAccount)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var association = tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
        Assert.NotNull(association);

        return association;
    }

    internal static async Task AssetNotAssociatedAsync(TestAsset fxAsset, TestAccount fxAccount)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var association = tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.Null(association);
    }

    internal static async Task AssetNotAssociatedAsync(TestAsset fxAsset, TestAliasAccount fxAccount)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var association = tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.Null(association);
    }

    internal static async Task<TokenHoldingData> AssetIsAssociatedAsync(TestAsset fxAsset, TestAccount fxAccount)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var association = tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.NotNull(association);

        return association;
    }
    internal static async Task<TokenHoldingData> AssetIsAssociatedAsync(TestAsset fxAsset, TestAliasAccount fxAccount)
    {
        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.NotNull(tokens);

        var association = tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.NotNull(association);

        return association;
    }

    public static Task CryptoBalanceAsync(TestAccount fxAccount, int expectedBalance)
    {
        return CryptoBalanceAsync(fxAccount, (ulong)expectedBalance);
    }
    public static async Task CryptoBalanceAsync(TestAccount fxAccount, ulong expectedBalance)
    {
        var balance = await fxAccount.Client.GetAccountBalanceAsync(fxAccount);
        Assert.Equal(expectedBalance, balance);
    }

    public static async Task CryptoContractBalanceAsync(PayableContract fxAccount, ulong expectedBalance)
    {
        var balance = await fxAccount.Client.GetContractBalanceAsync(fxAccount.ContractRecord.Contract);
        Assert.Equal(expectedBalance, balance);
    }

    internal static void ContainsRoyalty(TestToken fxToken, TestAccount fxPayer, TestAccount fxReceiver, int amount, ReadOnlyCollection<RoyaltyTransfer> royalties)
    {
        var token = fxToken.Record.Token;
        var payer = fxPayer.Record.Address;
        var receiver = fxReceiver.Record.Address;
        foreach (var entry in royalties)
        {
            if (amount == entry.Amount && token == entry.Token && receiver == entry.Receiver && entry.Payers.Contains(payer))
            {
                return;
            }
        }
        throw new Xunit.Sdk.XunitException($"Unable to find royalty payment using token {token} involving a payer {payer} to receiver {receiver} with amount {amount}.");
    }
    internal static void ContainsHbarRoyalty(TestAccount fxPayer, TestAccount fxReceiver, int amount, ReadOnlyCollection<RoyaltyTransfer> royalties)
    {
        var payer = fxPayer.Record.Address;
        var receiver = fxReceiver.Record.Address;
        foreach (var entry in royalties)
        {
            if (amount == entry.Amount && Address.None == entry.Token && receiver == entry.Receiver && entry.Payers.Contains(payer))
            {
                return;
            }
        }
        throw new Xunit.Sdk.XunitException($"Unable to find royalty payment using hBbar involving a payer {payer} to receiver {receiver} with amount {amount}.");
    }
    internal static void SingleAssociation(TestToken fxToken, TestAccount fxAccount, ReadOnlyCollection<Association> associations)
    {
        Assert.NotNull(associations);
        Assert.Single(associations);
        if (fxToken.Record.Token != associations[0].Token || fxAccount.Record.Address != associations[0].Account)
        {
            throw new Xunit.Sdk.XunitException($"Unable to find association record using token {fxToken.Record.Token} with account {fxAccount.Record.Address} .");
        }
    }
    internal static void SingleAssociation(TestAsset fxAsset, TestAccount fxAccount, ReadOnlyCollection<Association> associations)
    {
        Assert.NotNull(associations);
        Assert.Single(associations);
        if (fxAsset.Record.Token != associations[0].Token || fxAccount.Record.Address != associations[0].Account)
        {
            throw new Xunit.Sdk.XunitException($"Unable to find association record using asset {fxAsset.Record.Token} with account {fxAccount.Record.Address} .");
        }
    }

    internal static void SemanticVersionGreaterOrEqualThan(SemanticVersion expected, SemanticVersion actual)
    {
        if (expected.Major > actual.Major ||
            (expected.Major == actual.Major && expected.Minor > actual.Minor) ||
            (expected.Major == actual.Major && expected.Minor == actual.Minor && expected.Patch > actual.Patch))
        {
            throw new Xunit.Sdk.XunitException($"Semantic Version {actual.Major}.{actual.Minor}.{actual.Patch} is not greater than {expected.Major}.{expected.Minor}.{expected.Patch}");
        }

    }

    internal static void Empty(ReadOnlyMemory<byte> value)
    {
        if (!value.IsEmpty)
        {
            throw Xunit.Sdk.EmptyException.ForNonEmptyCollection(nameof(value));
        }
    }

    internal static void NotEmpty(ReadOnlyMemory<byte> value)
    {
        if (value.IsEmpty)
        {
            throw Xunit.Sdk.EmptyException.ForNonEmptyCollection(nameof(value));
        }
    }

    internal static void Equal(ReadOnlyMemory<byte> expected, ReadOnlyMemory<byte> actual)
    {
        var expectedBytes = Hex.FromBytes(expected.ToArray());
        var actualBytes = Hex.FromBytes(actual.ToArray());
        Assert.Equal(expectedBytes, actualBytes);
    }
}