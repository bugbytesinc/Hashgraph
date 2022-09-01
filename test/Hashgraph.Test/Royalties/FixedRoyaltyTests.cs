using Hashgraph.Test.Fixtures;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token;

[Collection(nameof(NetworkCredentials))]
public class FixedRoyaltyTests
{
    private readonly NetworkCredentials _network;
    public FixedRoyaltyTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Royalty Fixed Transfers: Transferring Asset Applies Single Fixed Commision")]
    async Task TransferringAssetAppliesSingleFixedCommision()
    {
        await using var fxBuyer = await TestAccount.CreateAsync(_network);
        await using var fxSeller = await TestAccount.CreateAsync(_network);
        await using var fxBenefactor = await TestAccount.CreateAsync(_network);
        await using var fxPaymentToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxBenefactor, fxBuyer, fxSeller);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                        new FixedRoyalty(fxBenefactor, fxPaymentToken, 10)
            };
            fx.Params.GrantKycEndorsement = null;
        }, fxBuyer, fxSeller);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var movedAsset = new Asset(fxAsset, 1);

        await fxPaymentToken.Client.TransferTokensAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxBuyer, 100, fxPaymentToken.TreasuryAccount);
        await fxPaymentToken.Client.TransferAssetAsync(movedAsset, fxAsset.TreasuryAccount, fxSeller, fxAsset.TreasuryAccount);

        await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxPaymentToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 100);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 100);

        var record = await fxAsset.Client.TransferWithRecordAsync(new TransferParams
        {
            AssetTransfers = new[] {
                    new AssetTransfer(movedAsset,fxSeller,fxBuyer)
                },
            TokenTransfers = new[] {
                    new TokenTransfer(fxPaymentToken, fxBuyer, -100),
                    new TokenTransfer(fxPaymentToken, fxSeller, 100),
                },
            Signatory = new Signatory(fxBuyer, fxSeller)
        });

        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Equal(3, record.TokenTransfers.Count);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxBuyer, -100), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxSeller, 90), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxBenefactor, 10), record.TokenTransfers);
        Assert.Single(record.AssetTransfers);
        Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
        Assert.Empty(record.Associations);
        Assert.Single(record.Royalties);
        AssertHg.ContainsRoyalty(fxPaymentToken, fxSeller, fxBenefactor, 10, record.Royalties);

        await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxPaymentToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 90);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor, 10);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 100);
    }

    [Fact(DisplayName = "Royalty Fixed Transfers: Transferring Asset Applies Single Fixed Hbar Commision")]
    async Task TransferringAssetAppliesSingleFixedHbarCommision()
    {
        await using var fxBuyer = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 10_00_000_000);
        await using var fxSeller = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
        await using var fxBenefactor = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                        new FixedRoyalty(fxBenefactor, Address.None, 1_00_000_000)
            };
            fx.Params.GrantKycEndorsement = null;
        }, fxBuyer, fxSeller);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var movedAsset = new Asset(fxAsset, 1);

        await fxAsset.Client.TransferAssetAsync(movedAsset, fxAsset.TreasuryAccount, fxSeller, fxAsset.TreasuryAccount);

        await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);

        await AssertHg.CryptoBalanceAsync(fxBuyer, 10_00_000_000);
        await AssertHg.CryptoBalanceAsync(fxSeller, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor, 0);

        var record = await fxAsset.Client.TransferWithRecordAsync(new TransferParams
        {
            AssetTransfers = new[] {
                    new AssetTransfer(movedAsset,fxSeller,fxBuyer)
                },
            CryptoTransfers = new [] {
                    new CryptoTransfer( fxBuyer.Record.Address, -5_00_000_000 ),
                    new CryptoTransfer(fxSeller.Record.Address, 5_00_000_000 )
                },
            Signatory = new Signatory(fxBuyer, fxSeller)
        });

        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Empty(record.TokenTransfers);
        Assert.Single(record.AssetTransfers);
        Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
        Assert.Single(record.Royalties);
        AssertHg.ContainsHbarRoyalty(fxSeller, fxBenefactor, 1_00_000_000, record.Royalties);

        await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);

        await AssertHg.CryptoBalanceAsync(fxBuyer, 5_00_000_000);
        await AssertHg.CryptoBalanceAsync(fxSeller, 4_00_000_000);
        await AssertHg.CryptoBalanceAsync(fxBenefactor, 1_00_000_000);
    }

    [Fact(DisplayName = "Royalty Fixed Transfers: Transferring Asset Applies Fixed Commisions when Token And HBar Exchanged")]
    async Task TransferringAssetAppliesFixedCommisionsWhenTokenAndHBarExchanged()
    {
        await using var fxBuyer = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 10_00_000_000);
        await using var fxSeller = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
        await using var fxBenefactor = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
        await using var fxPaymentToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxBenefactor, fxBuyer, fxSeller);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                        new FixedRoyalty(fxBenefactor, fxPaymentToken, 50)
            };
            fx.Params.GrantKycEndorsement = null;
        }, fxBuyer, fxSeller);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var movedAsset = new Asset(fxAsset, 1);

        await fxPaymentToken.Client.TransferTokensAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxBuyer, 200, fxPaymentToken.TreasuryAccount);
        await fxPaymentToken.Client.TransferAssetAsync(movedAsset, fxAsset.TreasuryAccount, fxSeller, fxAsset.TreasuryAccount);

        await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxPaymentToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 200);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 200);

        await AssertHg.CryptoBalanceAsync(fxBuyer, 10_00_000_000);
        await AssertHg.CryptoBalanceAsync(fxSeller, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor, 0);

        var record = await fxAsset.Client.TransferWithRecordAsync(new TransferParams
        {
            AssetTransfers = new[] {
                    new AssetTransfer(movedAsset,fxSeller,fxBuyer)
                },
            TokenTransfers = new[] {
                    new TokenTransfer(fxPaymentToken, fxBuyer, -100),
                    new TokenTransfer(fxPaymentToken, fxSeller, 100),
                },
            CryptoTransfers = new[] {
                    new CryptoTransfer( fxBuyer.Record.Address, -10_00_000_000 ),
                    new CryptoTransfer(fxSeller.Record.Address, 10_00_000_000 )
                },
            Signatory = new Signatory(fxBuyer, fxSeller)
        });

        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Equal(3, record.TokenTransfers.Count);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxBuyer, -100), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxSeller, 50), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxBenefactor, 50), record.TokenTransfers);
        Assert.Single(record.AssetTransfers);
        Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
        Assert.Empty(record.Associations);
        Assert.Single(record.Royalties);
        AssertHg.ContainsRoyalty(fxPaymentToken, fxSeller, fxBenefactor, 50, record.Royalties);

        await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxPaymentToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 100);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 50);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor, 50);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 200);

        await AssertHg.CryptoBalanceAsync(fxBuyer, 0);
        await AssertHg.CryptoBalanceAsync(fxSeller, 10_00_000_000);
        await AssertHg.CryptoBalanceAsync(fxBenefactor, 0);
    }

    [Fact(DisplayName = "Royalty Fixed Transfers: Transferring Asset Applies Multiple Fixed Commision Deduction Destinations")]
    public async Task TransferringAssetAppliesMultipleFixedCommisionDeductionDestinations()
    {
        await using var fxBuyer = await TestAccount.CreateAsync(_network);
        await using var fxSeller = await TestAccount.CreateAsync(_network);
        await using var fxBenefactor1 = await TestAccount.CreateAsync(_network);
        await using var fxBenefactor2 = await TestAccount.CreateAsync(_network);
        await using var fxBenefactor3 = await TestAccount.CreateAsync(_network);
        await using var fxPaymentToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.Decimals = 2;
            fx.Params.Circulation = 1_000_00;
        }, fxBenefactor1, fxBenefactor2, fxBenefactor3, fxBuyer, fxSeller);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                        new FixedRoyalty(fxBenefactor1, fxPaymentToken, 20),
                        new FixedRoyalty(fxBenefactor2, fxPaymentToken, 20),
                        new FixedRoyalty(fxBenefactor3, fxPaymentToken, 40),
            };
            fx.Params.GrantKycEndorsement = null;
        }, fxBuyer, fxSeller);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var movedAsset = new Asset(fxAsset, 1);

        await fxPaymentToken.Client.TransferTokensAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxBuyer, 100_00, fxPaymentToken.TreasuryAccount);
        await fxPaymentToken.Client.TransferAssetAsync(movedAsset, fxAsset.TreasuryAccount, fxSeller, fxAsset.TreasuryAccount);

        await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxPaymentToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 100_00);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor1, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor2, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor3, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 100_00);

        var record = await fxAsset.Client.TransferWithRecordAsync(new TransferParams
        {
            AssetTransfers = new[] {
                    new AssetTransfer(movedAsset,fxSeller,fxBuyer)
                },
            TokenTransfers = new[] {
                    new TokenTransfer(fxPaymentToken, fxBuyer, -10_00),
                    new TokenTransfer(fxPaymentToken, fxSeller, 10_00),
                },
            Signatory = new Signatory(fxBuyer, fxSeller)
        });

        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Equal(5, record.TokenTransfers.Count);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxBuyer, -10_00), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxSeller, 9_20), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxBenefactor1, 20), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxBenefactor2, 20), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxBenefactor3, 40), record.TokenTransfers);
        Assert.Single(record.AssetTransfers);
        Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
        Assert.Empty(record.Associations);
        Assert.Equal(3, record.Royalties.Count);
        AssertHg.ContainsRoyalty(fxPaymentToken, fxSeller, fxBenefactor1, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxPaymentToken, fxSeller, fxBenefactor2, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxPaymentToken, fxSeller, fxBenefactor3, 40, record.Royalties);

        await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxPaymentToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 90_00);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 9_20);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor1, 20);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor2, 20);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor3, 40);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 100_00);
    }

    [Fact(DisplayName = "Royalty Fixed Transfers: Transferring Asset Applies Multiple Fixed Commision Fee Even Without Payment")]
    public async Task TransferringAssetAppliesMultipleFixedCommisionFeeEvenWithoutPayment()
    {
        await using var fxBuyer = await TestAccount.CreateAsync(_network);
        await using var fxSeller = await TestAccount.CreateAsync(_network);
        await using var fxBenefactor1 = await TestAccount.CreateAsync(_network);
        await using var fxBenefactor2 = await TestAccount.CreateAsync(_network);
        await using var fxBenefactor3 = await TestAccount.CreateAsync(_network);
        await using var fxPaymentToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.Decimals = 2;
            fx.Params.Circulation = 1_000_00;
        }, fxBenefactor1, fxBenefactor2, fxBenefactor3, fxBuyer, fxSeller);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                        new FixedRoyalty(fxBenefactor1, fxPaymentToken, 20),
                        new FixedRoyalty(fxBenefactor2, fxPaymentToken, 20),
                        new FixedRoyalty(fxBenefactor3, fxPaymentToken, 40),
            };
            fx.Params.GrantKycEndorsement = null;
        }, fxBuyer, fxSeller);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var movedAsset = new Asset(fxAsset, 1);

        await fxPaymentToken.Client.TransferTokensAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxSeller, 100_00, fxPaymentToken.TreasuryAccount);
        await fxPaymentToken.Client.TransferAssetAsync(movedAsset, fxAsset.TreasuryAccount, fxSeller, fxAsset.TreasuryAccount);

        await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxPaymentToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 100_00);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor1, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor2, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor3, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 100_00);

        var record = await fxAsset.Client.TransferWithRecordAsync(new TransferParams
        {
            AssetTransfers = new[] {
                    new AssetTransfer(movedAsset,fxSeller,fxBuyer)
                },
            Signatory = new Signatory(fxBuyer, fxSeller)
        });

        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Equal(4, record.TokenTransfers.Count);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxSeller, -80), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxBenefactor1, 20), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxBenefactor2, 20), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxPaymentToken, fxBenefactor3, 40), record.TokenTransfers);
        Assert.Single(record.AssetTransfers);
        Assert.Empty(record.Associations);
        Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
        Assert.Equal(3, record.Royalties.Count);
        AssertHg.ContainsRoyalty(fxPaymentToken, fxSeller, fxBenefactor1, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxPaymentToken, fxSeller, fxBenefactor2, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxPaymentToken, fxSeller, fxBenefactor3, 40, record.Royalties);

        await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxPaymentToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 99_20);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor1, 20);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor2, 20);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor3, 40);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 100_00);
    }
}