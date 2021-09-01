using Hashgraph.Test.Fixtures;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token
{
    [Collection(nameof(NetworkCredentials))]
    public class CommissionValueTests
    {
        private readonly NetworkCredentials _network;
        public CommissionValueTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Commission Value Transfers: Transferring Asset Applies Single Value Commision")]
        async Task TransferringAssetAppliesSingleValueCommision()
        {
            await using var fxBuyer = await TestAccount.CreateAsync(_network);
            await using var fxSeller = await TestAccount.CreateAsync(_network);
            await using var fxBenefactor = await TestAccount.CreateAsync(_network);
            await using var fxPaymentToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxBenefactor, fxBuyer, fxSeller);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ValueCommission[]
                {
                        new ValueCommission(fxBenefactor, 1, 2, 0, Address.None)
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
            Assert.Contains(new TokenTransfer(fxPaymentToken, fxSeller, 50), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxPaymentToken, fxBenefactor, 50), record.TokenTransfers);
            Assert.Single(record.AssetTransfers);
            Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
            Assert.Single(record.Commissions);
            AssertHg.ContainsCommission(fxPaymentToken, fxSeller, fxBenefactor, 50, record.Commissions);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxPaymentToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 50);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor, 50);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 100);
        }

        [Fact(DisplayName = "Commission Value Transfers: Transferring Asset Applies Single Value Hbar Commision")]
        async Task TransferringAssetAppliesSingleValueHbarCommision()
        {
            await using var fxBuyer = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 10_00_000_000);
            await using var fxSeller = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ValueCommission[]
                {
                        new ValueCommission(fxBenefactor, 1, 2, 0, Address.None)
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
                CryptoTransfers = new Dictionary<Address, long> {
                    { fxBuyer.Record.Address, -10_00_000_000 },
                    { fxSeller.Record.Address, 10_00_000_000 }
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
            Assert.Single(record.Commissions);
            AssertHg.ContainsHbarCommission(fxSeller, fxBenefactor, 5_00_000_000, record.Commissions);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 0);
            await AssertHg.CryptoBalanceAsync(fxSeller, 5_00_000_000);
            await AssertHg.CryptoBalanceAsync(fxBenefactor, 5_00_000_000);
        }

        [Fact(DisplayName = "Commission Value Transfers: Transferring Asset Applies Value Commisions when Token And HBar Exchanged")]
        async Task TransferringAssetAppliesValueCommisionsWhenTokenAndHBarExchanged()
        {
            await using var fxBuyer = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 10_00_000_000);
            await using var fxSeller = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
            await using var fxPaymentToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxBenefactor, fxBuyer, fxSeller);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ValueCommission[]
                {
                        new ValueCommission(fxBenefactor, 1, 2, 0, Address.None)
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
                CryptoTransfers = new Dictionary<Address, long> {
                    { fxBuyer.Record.Address, -10_00_000_000 },
                    { fxSeller.Record.Address, 10_00_000_000 }
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
            Assert.Equal(2, record.Commissions.Count);
            AssertHg.ContainsCommission(fxPaymentToken, fxSeller, fxBenefactor, 50, record.Commissions);
            AssertHg.ContainsHbarCommission(fxSeller, fxBenefactor, 5_00_000_000, record.Commissions);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxPaymentToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 50);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor, 50);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 100);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 0);
            await AssertHg.CryptoBalanceAsync(fxSeller, 5_00_000_000);
            await AssertHg.CryptoBalanceAsync(fxBenefactor, 5_00_000_000);
        }

        [Fact(DisplayName = "Commission Value Transfers: Transferring Asset Applies Single Value Commision Without Fallback")]
        async Task TransferringAssetAppliesSingleValueCommisionWithoutFallback()
        {
            await using var fxBuyer = await TestAccount.CreateAsync(_network);
            await using var fxSeller = await TestAccount.CreateAsync(_network);
            await using var fxBenefactor = await TestAccount.CreateAsync(_network);
            await using var fxPaymentToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxBenefactor, fxBuyer, fxSeller);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ValueCommission[]
                {
                        new ValueCommission(fxBenefactor, 1, 2, 10_00_000_000, Address.None)
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
            Assert.Contains(new TokenTransfer(fxPaymentToken, fxSeller, 50), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxPaymentToken, fxBenefactor, 50), record.TokenTransfers);
            Assert.Single(record.AssetTransfers);
            Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
            Assert.Single(record.Commissions);
            AssertHg.ContainsCommission(fxPaymentToken, fxSeller, fxBenefactor, 50, record.Commissions);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxPaymentToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 50);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor, 50);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 100);
        }

        [Fact(DisplayName = "Commission Value Transfers: Transferring Asset Applies Single Value Commision With hBar Fallback")]
        async Task TransferringAssetAppliesSingleValueCommisionWithHBarFallback()
        {
            await using var fxBuyer = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 10_00_000_000);
            await using var fxSeller = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ValueCommission[]
                {
                        new ValueCommission(fxBenefactor, 1, 2, 10_00_000_000, Address.None)
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
            Assert.Single(record.Commissions);
            AssertHg.ContainsHbarCommission(fxBuyer, fxBenefactor, 10_00_000_000, record.Commissions);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 0);
            await AssertHg.CryptoBalanceAsync(fxSeller, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor, 10_00_000_000);
        }
        [Fact(DisplayName = "Commission Value Transfers: Transferring Asset Applies Single Value Commision With Token Fallback")]
        async Task TransferringAssetAppliesSingleValueCommisionWithTokenFallback()
        {
            await using var fxBuyer = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
            await using var fxSeller = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
            await using var fxPaymentToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxBenefactor, fxBuyer, fxSeller);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ValueCommission[]
                {
                        new ValueCommission(fxBenefactor, 1, 2, 10, fxPaymentToken)
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

            await AssertHg.CryptoBalanceAsync(fxBuyer, 0);
            await AssertHg.CryptoBalanceAsync(fxSeller, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor, 0);

            var record = await fxAsset.Client.TransferWithRecordAsync(new TransferParams
            {
                AssetTransfers = new[] {
                    new AssetTransfer(movedAsset,fxSeller,fxBuyer)
                },
                Signatory = new Signatory(fxSeller, fxBuyer)
            });

            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Equal(2, record.TokenTransfers.Count);
            Assert.Contains(new TokenTransfer(fxPaymentToken, fxBuyer, -10), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxPaymentToken, fxBenefactor, 10), record.TokenTransfers);
            Assert.Single(record.AssetTransfers);
            Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
            Assert.Single(record.Commissions);
            AssertHg.ContainsCommission(fxPaymentToken, fxBuyer, fxBenefactor, 10, record.Commissions);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxPaymentToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 90);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor, 10);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 100);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 0);
            await AssertHg.CryptoBalanceAsync(fxSeller, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor, 0);
        }
        [Fact(DisplayName = "Commission Value Transfers: No Commission for Single Transfer When No Fallback")]
        async Task NoCommissionForSingleTransferWhenNoFallback()
        {
            await using var fxBuyer = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 10_00_000_000);
            await using var fxSeller = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 0);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ValueCommission[]
                {
                        new ValueCommission(fxBenefactor, 1, 2, 0, Address.None)
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
                Signatory = new Signatory(fxSeller)
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
            Assert.Empty(record.Commissions);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 10_00_000_000);
            await AssertHg.CryptoBalanceAsync(fxSeller, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor, 0);
        }
    }
}
