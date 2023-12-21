namespace Hashgraph.Test.AssetToken;

[Collection(nameof(NetworkCredentials))]
public class ConfiscateAssetTests
{
    private readonly NetworkCredentials _network;
    public ConfiscateAssetTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Confiscate Assets: Can Confiscate Asset Coins")]
    public async Task CanConfiscateAssetCoins()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var circulation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = circulation / (ulong)Generator.Integer(3, 5) + 1;
        var expectedTreasury = (ulong)fxAsset.Metadata.Length - xferAmount;
        var serialNumbersToConfiscate = Enumerable.Range(1, (int)xferAmount).Select(i => (long)i);

        var xferRecipt = await fxAsset.Client.TransferAsync(new TransferParams
        {
            AssetTransfers = serialNumbersToConfiscate.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        });

        await _network.WaitForMirrorConsensusAsync(xferRecipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(circulation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var receipt = await fxAsset.Client.ConfiscateAssetsAsync(fxAsset.Record.Token, serialNumbersToConfiscate, fxAccount, fxAsset.ConfiscatePrivateKey);

        await _network.WaitForMirrorConsensusAsync(receipt);

        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(expectedTreasury, receipt.Circulation);

        Assert.Equal(0, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(expectedTreasury, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Assets: Can Confiscate Single Asset")]
    public async Task CanConfiscateSingleAsset()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var circulation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = circulation / (ulong)Generator.Integer(3, 5) + 1;
        var expectedTreasury = (ulong)fxAsset.Metadata.Length - xferAmount;
        var expectedCirculation = (ulong)fxAsset.Metadata.Length - 1;
        var serialNumbersToXfer = Enumerable.Range(1, (int)xferAmount).Select(i => (long)i);

        var xferReceipt = await fxAsset.Client.TransferAsync(new TransferParams
        {
            AssetTransfers = serialNumbersToXfer.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        });

        await _network.WaitForMirrorConsensusAsync(xferReceipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(circulation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var receipt = await fxAsset.Client.ConfiscateAssetAsync(new Asset(fxAsset.Record.Token, 1), fxAccount, ctx =>
        {
            ctx.Signatory = new Signatory(ctx.Signatory, fxAsset.ConfiscatePrivateKey);
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(expectedCirculation, receipt.Circulation);
        await _network.WaitForMirrorConsensusAsync(receipt);

        Assert.Equal((long)(xferAmount - 1), await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(expectedCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Assets: Can Confiscate Asset Coins from Alias")]
    public async Task CanConfiscateAssetCoinsFromAlias()
    {
        await using var fxAccount = await TestAliasAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
        await fxAsset.Client.AssociateTokenAsync(fxAsset, fxAccount, fxAccount.PrivateKey);
        var circulation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = circulation / (ulong)Generator.Integer(3, 5) + 1;
        var expectedTreasury = (ulong)fxAsset.Metadata.Length - xferAmount;
        var serialNumbersToConfiscate = Enumerable.Range(1, (int)xferAmount).Select(i => (long)i);

        var xfrReceipt = await fxAsset.Client.TransferAsync(new TransferParams
        {
            AssetTransfers = serialNumbersToConfiscate.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        });
        await _network.WaitForMirrorConsensusAsync(xfrReceipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(circulation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var receipt = await fxAsset.Client.ConfiscateAssetsAsync(fxAsset.Record.Token, serialNumbersToConfiscate, fxAccount.Alias, fxAsset.ConfiscatePrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(expectedTreasury, receipt.Circulation);

        await _network.WaitForMirrorConsensusAsync(receipt);

        Assert.Equal(0, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(expectedTreasury, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Assets: Can Confiscate A Single Asset")]
    public async Task CanConfiscateASingleAsset()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

        var initialCirculation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = 2ul;
        var expectedTreasury = initialCirculation - xferAmount;
        var serialNumbersTransfered = Enumerable.Range(1, 2).Select(i => (long)i);

        var transferParams = new TransferParams
        {
            AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        };

        var xferReceipt = await fxAsset.Client.TransferAsync(transferParams);

        await _network.WaitForMirrorConsensusAsync(xferReceipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var receipt = await fxAsset.Client.ConfiscateAssetAsync(new Asset(fxAsset.Record.Token, 1), fxAccount, fxAsset.ConfiscatePrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(initialCirculation - 1, receipt.Circulation);

        await _network.WaitForMirrorConsensusAsync(receipt);

        Assert.Equal(1, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation - 1, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Assets: Can Confiscate an Asset Get Record")]
    public async Task CanConfiscateAnAssetGetRecord()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

        var initialCirculation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = 2ul;
        var expectedTreasury = initialCirculation - xferAmount;
        var serialNumbersTransfered = Enumerable.Range(1, 2).Select(i => (long)i);

        var transferParams = new TransferParams
        {
            AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        };

        var xferReceipt = await fxAsset.Client.TransferAsync(transferParams);

        await _network.WaitForMirrorConsensusAsync(xferReceipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var record = await fxAsset.Client.ConfiscateAssetWithRecordAsync(new Asset(fxAsset.Record.Token, 1), fxAccount, fxAsset.ConfiscatePrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Equal(initialCirculation - 1, record.Circulation);

        await _network.WaitForMirrorConsensusAsync(record);

        Assert.Equal(1, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation - 1, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
        Assert.Null(record.ParentTransactionConcensus);
    }
    [Fact(DisplayName = "Confiscate Assets: Can Confiscate an Assets and Get Record")]
    public async Task CanConfiscateAnAssetsAndGetRecord()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

        var initialCirculation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = 2ul;
        var expectedTreasury = initialCirculation - xferAmount;
        var expectedCirculation = initialCirculation - xferAmount;
        var serialNumbersTransfered = Enumerable.Range(1, 2).Select(i => (long)i);

        var transferParams = new TransferParams
        {
            AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        };

        var xferReceipt = await fxAsset.Client.TransferAsync(transferParams);

        await _network.WaitForMirrorConsensusAsync(xferReceipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var record = await fxAsset.Client.ConfiscateAssetsWithRecordAsync(fxAsset, serialNumbersTransfered, fxAccount, fxAsset.ConfiscatePrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Equal(expectedCirculation, record.Circulation);

        await _network.WaitForMirrorConsensusAsync(record);

        Assert.Equal(0, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(expectedCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
        Assert.Null(record.ParentTransactionConcensus);
    }
    [Fact(DisplayName = "Confiscate Assets: Can Confiscate A Small Amount Assets and get Record without Extra Signatory")]
    public async Task CanConfiscateASmallAmountAssetCoinsAndGetRecordWithoutExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

        var initialCirculation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = 2ul;
        var expectedTreasury = initialCirculation - xferAmount;
        var serialNumbersTransfered = Enumerable.Range(1, 2).Select(i => (long)i);

        var transferParams = new TransferParams
        {
            AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        };

        var xferReceipt = await fxAsset.Client.TransferAsync(transferParams);

        await _network.WaitForMirrorConsensusAsync(xferReceipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var record = await fxAsset.Client.ConfiscateAssetsWithRecordAsync(fxAsset.Record.Token, serialNumbersTransfered, fxAccount, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxAsset.ConfiscatePrivateKey));
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Equal(expectedTreasury, record.Circulation);

        await _network.WaitForMirrorConsensusAsync(record);

        Assert.Equal(0, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(expectedTreasury, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Assets: Can Confiscate Single Asset and get Record without Extra Signatory")]
    public async Task CanConfiscateSingleAssetAndGetRecordWithoutExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

        var initialCirculation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = 2ul;
        var expectedTreasury = initialCirculation - xferAmount;
        var expectedCirculation = initialCirculation - 1;
        var serialNumbersTransfered = Enumerable.Range(1, 2).Select(i => (long)i);

        var transferParams = new TransferParams
        {
            AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        };

        var receipt = await fxAsset.Client.TransferAsync(transferParams);

        await _network.WaitForMirrorConsensusAsync(receipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var record = await fxAsset.Client.ConfiscateAssetWithRecordAsync(new Asset(fxAsset.Record.Token, 1), fxAccount, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxAsset.ConfiscatePrivateKey));
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Equal(expectedCirculation, record.Circulation);

        await _network.WaitForMirrorConsensusAsync(record);

        Assert.Equal((long)(xferAmount - 1), await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(expectedCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Assets: Can Confiscate an Asset from Any Account with Confiscate Key")]
    public async Task CanConfiscateAnAssetFromAnyAccountWithConfiscateKey()
    {
        await using var fxOther = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);


        var initialCirculation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = 2ul;
        var expectedTreasury = initialCirculation - xferAmount;
        var serialNumbersTransfered = Enumerable.Range(1, 2).Select(i => (long)i);

        var transferParams = new TransferParams
        {
            AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        };

        var xferReceipt = await fxAsset.Client.TransferAsync(transferParams);

        await _network.WaitForMirrorConsensusAsync(xferReceipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var receipt = await fxAsset.Client.ConfiscateAssetAsync(new Asset(fxAsset, 1), fxAccount, fxAsset.ConfiscatePrivateKey, ctx =>
         {
             ctx.Payer = fxOther.Record.Address;
             ctx.Signatory = fxOther.PrivateKey;
         });
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(initialCirculation - 1, receipt.Circulation);

        await _network.WaitForMirrorConsensusAsync(receipt);

        Assert.Equal(1, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation - 1, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Assets: Can Not Confiscate More Assets than Account Has")]
    public async Task CanNotConfiscateMoreAssetsThanAccountHas()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

        var initialCirculation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = 2ul;
        var expectedTreasury = initialCirculation - xferAmount;
        var serialNumbersTransfered = Enumerable.Range(1, 2).Select(i => (long)i);

        var transferParams = new TransferParams
        {
            AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        };

        var xferReceipt = await fxAsset.Client.TransferAsync(transferParams);

        await _network.WaitForMirrorConsensusAsync(xferReceipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.ConfiscateAssetsAsync(fxAsset, new long[] { 1, 2, 3 }, fxAccount, fxAsset.ConfiscatePrivateKey);
        });
        Assert.Equal(ResponseCode.AccountDoesNotOwnWipedNft, tex.Status);
        Assert.Equal(ResponseCode.AccountDoesNotOwnWipedNft, tex.Receipt.Status);
        Assert.StartsWith("Unable to Confiscate Token, status: AccountDoesNotOwnWipedNft", tex.Message);

        await _network.WaitForMirrorConsensusAsync(tex);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Assets: Confiscate Record Includes Asset Transfers")]
    public async Task ConfiscateRecordIncludesAssetTransfers()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

        var initialCirculation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = 2ul;
        var expectedTreasury = initialCirculation - xferAmount;
        var serialNumbersTransfered = Enumerable.Range(1, 2).Select(i => (long)i);

        var transferParams = new TransferParams
        {
            AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        };

        var xferReceipt = await fxAsset.Client.TransferAsync(transferParams);

        await _network.WaitForMirrorConsensusAsync(xferReceipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var record = await fxAsset.Client.ConfiscateAssetWithRecordAsync(new Asset(fxAsset, 1), fxAccount, fxAsset.ConfiscatePrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.Equal(initialCirculation - 1, record.Circulation);
        Assert.Empty(record.TokenTransfers);
        Assert.Single(record.AssetTransfers);

        var xfer = record.AssetTransfers[0];
        Assert.Equal(fxAsset.Record.Token, xfer.Asset);
        Assert.Equal(fxAccount.Record.Address, xfer.From);
        Assert.Equal(Address.None, xfer.To);
    }
    [Fact(DisplayName = "Confiscate Assets: Confiscation Requires Confiscate Key Signature")]
    public async Task ConfiscationRequiresConfiscateKeySignature()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

        var initialCirculation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = 2ul;
        var expectedTreasury = initialCirculation - xferAmount;
        var serialNumbersTransfered = Enumerable.Range(1, 2).Select(i => (long)i);

        var transferParams = new TransferParams
        {
            AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        };

        var xferReceipt = await fxAsset.Client.TransferAsync(transferParams);

        await _network.WaitForMirrorConsensusAsync(xferReceipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.ConfiscateAssetsAsync(fxAsset, new long[] { 1 }, fxAccount);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
        Assert.StartsWith("Unable to Confiscate Token, status: InvalidSignature", tex.Message);

        await _network.WaitForMirrorConsensusAsync(tex);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Assets: Cannot Confiscate when no Confiscation Endorsement")]
    public async Task CannotConfiscateWhenNoConfiscationEndorsement()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.ConfiscateEndorsement = null;
        }, fxAccount);

        var initialCirculation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = 2ul;
        var expectedTreasury = initialCirculation - xferAmount;
        var serialNumbersTransfered = Enumerable.Range(1, 2).Select(i => (long)i);

        var transferParams = new TransferParams
        {
            AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        };

        var xferRecipt = await fxAsset.Client.TransferAsync(transferParams);

        await _network.WaitForMirrorConsensusAsync(xferRecipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.ConfiscateAssetAsync(new Asset(fxAsset, 1), fxAccount, fxAsset.ConfiscatePrivateKey);
        });
        Assert.Equal(ResponseCode.TokenHasNoWipeKey, tex.Status);
        Assert.Equal(ResponseCode.TokenHasNoWipeKey, tex.Receipt.Status);
        Assert.StartsWith("Unable to Confiscate Token, status: TokenHasNoWipeKey", tex.Message);

        await _network.WaitForMirrorConsensusAsync(tex);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Assets: Can Not Schedule Confiscate Asset Coins")]
    public async Task CanNotScheduleConfiscateAssetCoins()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);


        var initialCirculation = (ulong)fxAsset.Metadata.Length;
        var xferAmount = 2ul;
        var expectedTreasury = initialCirculation - xferAmount;
        var serialNumbersTransfered = Enumerable.Range(1, 2).Select(i => (long)i);

        var transferParams = new TransferParams
        {
            AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount)),
            Signatory = fxAsset.TreasuryAccount
        };

        var xferReceipt = await fxAsset.Client.TransferAsync(transferParams);

        await _network.WaitForMirrorConsensusAsync(xferReceipt);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal((long)expectedTreasury, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
        Assert.Equal(initialCirculation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.ConfiscateAssetsAsync(
                fxAsset,
                new long[] { 1 },
                fxAccount,
                new Signatory(
                    fxAsset.ConfiscatePrivateKey,
                    new PendingParams
                    {
                        PendingPayer = fxPayer
                    })
            );
        });
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Receipt.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }
    [Fact(DisplayName = "Confiscate Assets: Can Confiscate Asset From Treasury")]
    public async Task CanConfiscateAssetFromTreasury()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
        var circulation = (ulong)fxAsset.Metadata.Length;

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.ConfiscateAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAsset.ConfiscatePrivateKey);
        });
        Assert.Equal(ResponseCode.CannotWipeTokenTreasuryAccount, tex.Status);
        Assert.Equal(ResponseCode.CannotWipeTokenTreasuryAccount, tex.Receipt.Status);
        Assert.StartsWith("Unable to Confiscate Token, status: CannotWipeTokenTreasuryAccount", tex.Message);

        Assert.Equal(circulation, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
    }
}