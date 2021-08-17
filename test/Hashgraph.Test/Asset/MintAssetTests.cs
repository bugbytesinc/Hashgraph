using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.AssetTokens
{
    [Collection(nameof(NetworkCredentials))]
    public class MintAssetTests
    {
        private readonly NetworkCredentials _network;
        public MintAssetTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Mint Assets: Can Mint Assets")]
        public async Task CanMintAssets()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Metadata = null);
            Assert.NotNull(fxAsset.Record);
            Assert.NotNull(fxAsset.Record.Token);
            Assert.True(fxAsset.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var metadata = Enumerable.Range(1, Generator.Integer(2, 10)).Select(_ => Generator.SHA384Hash()).ToArray();
            var receipt = await fxAsset.Client.MintAssetAsync(fxAsset.Record.Token, metadata, fxAsset.SupplyPrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(metadata.Length, receipt.SerialNumbers.Count);
            foreach (var serialNumber in receipt.SerialNumbers)
            {
                Assert.True(serialNumber > 0);
            }

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)metadata.Length, info.Circulation);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(fxAsset.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.Commissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);

            Assert.Equal((ulong)metadata.Length, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Mint Assets: Can Mint Asset Coins (No Extra Signatory)")]
        public async Task CanMintAssetsWithouExtraSignatory()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Metadata = null);
            Assert.NotNull(fxAsset.Record);
            Assert.NotNull(fxAsset.Record.Token);
            Assert.True(fxAsset.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var metadata = Enumerable.Range(1, Generator.Integer(2, 10)).Select(_ => Generator.SHA384Hash()).ToArray();
            var receipt = await fxAsset.Client.MintAssetAsync(fxAsset.Record.Token, metadata, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxAsset.SupplyPrivateKey));
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(metadata.Length, receipt.SerialNumbers.Count);
            foreach (var serialNumber in receipt.SerialNumbers)
            {
                Assert.True(serialNumber > 0);
            }

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);

            Assert.Equal((ulong)metadata.Length, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Mint Assets: Can Mint Asset Coins and get Record")]
        public async Task CanMintAssetsAndGetRecord()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Metadata = null);
            Assert.NotNull(fxAsset.Record);
            Assert.NotNull(fxAsset.Record.Token);
            Assert.True(fxAsset.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var metadata = Enumerable.Range(1, Generator.Integer(2, 10)).Select(_ => Generator.SHA384Hash()).ToArray();
            var record = await fxAsset.Client.MintAssetWithRecordAsync(fxAsset.Record.Token, metadata, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxAsset.SupplyPrivateKey));
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.Equal(metadata.Length, record.SerialNumbers.Count);
            foreach (var serialNumber in record.SerialNumbers)
            {
                Assert.True(serialNumber > 0);
            }
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Equal(metadata.Length, record.SerialNumbers.Count);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            // Note: we doubled the circulation
            Assert.Equal((ulong)metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);

            Assert.Equal((ulong)metadata.Length, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Mint Assets: Can Mint Asset Coins from Any Account with Supply Key")]
        public async Task CanMintAssetsFromAnyAccountWithSupplyKey()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 100_00_000_000);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Metadata = null);
            Assert.NotNull(fxAsset.Record);
            Assert.NotNull(fxAsset.Record.Token);
            Assert.True(fxAsset.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var metadata = Enumerable.Range(1, Generator.Integer(2, 10)).Select(_ => Generator.SHA384Hash()).ToArray();
            var receipt = await fxAsset.Client.MintAssetAsync(fxAsset.Record.Token, metadata, ctx =>
            {
                ctx.Payer = fxAccount.Record.Address;
                ctx.Signatory = new Signatory(fxAccount.PrivateKey, fxAsset.SupplyPrivateKey);
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(metadata.Length, receipt.SerialNumbers.Count);
            foreach (var serialNumber in receipt.SerialNumbers)
            {
                Assert.True(serialNumber > 0);
            }
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(metadata.Length, receipt.SerialNumbers.Count);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            // Note: we doubled the circulation
            Assert.Equal((ulong)metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);

            Assert.Equal((ulong)metadata.Length, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Mint Assets: Mint Asset Record Includes Asset Transfers")]
        public async Task MintAssetRecordIncludesAssetTransfers()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Metadata = null);

            var metadata = Enumerable.Range(1, Generator.Integer(2, 10)).Select(_ => Generator.SHA384Hash()).ToArray();
            var record = await fxAsset.Client.MintAssetWithRecordAsync(fxAsset.Record.Token, metadata, fxAsset.SupplyPrivateKey);
            Assert.Equal(metadata.Length, record.SerialNumbers.Count);
            Assert.Empty(record.TokenTransfers);
            Assert.Equal(metadata.Length, record.AssetTransfers.Count);
            Assert.Empty(record.Commissions);

            for (var i = 0; i < metadata.Length; i++)
            {
                var ssn = record.SerialNumbers[i];
                var xfer = record.AssetTransfers.FirstOrDefault(x => x.Asset.SerialNum == ssn);
                Assert.NotNull(xfer);
                Assert.Equal(fxAsset.Record.Token, xfer.Asset);
                Assert.Equal(Address.None, xfer.From);
                Assert.Equal(fxAsset.TreasuryAccount.Record.Address, xfer.To);
            }
            Assert.Equal((ulong)metadata.Length, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal((ulong)metadata.Length, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
        }
        [Fact(DisplayName = "Mint Assets: Mint Asset Requires Signature by Supply Key")]
        public async Task MintAssetRequiresSignatureBySupplyKey()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Metadata = null);
            var metadata = Enumerable.Range(1, Generator.Integer(2, 10)).Select(_ => Generator.SHA384Hash()).ToArray();
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.MintAssetAsync(fxAsset.Record.Token, metadata);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to Mint Token Coins, status: InvalidSignature", tex.Message);

            Assert.Equal(0UL, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal(0UL, (await fxAsset.Client.GetTokenInfoAsync(fxAsset)).Circulation);
        }
        [Fact(DisplayName = "Mint Assets: Can Not More Mint Assets than Ceiling")]
        public async Task CanNotMoreMintAssetsThanCeiling()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.Ceiling = fx.Metadata.Length);

            var metadata = new ReadOnlyMemory<byte>[] { Generator.SHA384Hash() };

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.MintAssetAsync(fxAsset.Record.Token, metadata, fxAsset.SupplyPrivateKey);
            });
            Assert.Equal(ResponseCode.TokenMaxSupplyReached, tex.Status);
            Assert.StartsWith("Unable to Mint Token Coins, status: TokenMaxSupplyReached", tex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Params.Ceiling, (long)info.Circulation);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
        }
        [Fact(DisplayName = "Mint Assets: Can Schedule Mint Asset Coins")]
        public async Task CanScheduleMintAssetCoins()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            var metadata = Enumerable.Range(1, Generator.Integer(2, 10)).Select(_ => Generator.SHA384Hash()).ToArray();
            var pendingReceipt = await fxAsset.Client.MintAssetAsync(
                    fxAsset.Record.Token,
                    metadata,
                    new Signatory(
                        fxAsset.SupplyPrivateKey,
                        new PendingParams
                        {
                            PendingPayer = fxPayer
                        }));

            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)fxAsset.Metadata.Length);

            var schedulingReceipt = await fxAsset.Client.SignPendingTransactionAsync(pendingReceipt.Pending.Id, fxPayer.PrivateKey); //as AssetMintReceipt;
            Assert.Equal(ResponseCode.Success, schedulingReceipt.Status);
            // Should be able to do this but can't yet
            //Assert.Equal(metadata.Length, schedulingReceipt.SerialNumbers.Count);
            //foreach (var serialNumber in schedulingReceipt.SerialNumbers)
            //{
            //    Assert.True(serialNumber > 0);
            //}

            // Can also get them via Record.
            var record = await fxAsset.Client.GetTransactionRecordAsync(pendingReceipt.Pending.TxId) as AssetMintRecord;
            Assert.Equal(metadata.Length, record.SerialNumbers.Count);
            foreach (var serialNumber in record.SerialNumbers)
            {
                Assert.True(serialNumber > 0);
            }

            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)(metadata.Length + fxAsset.Metadata.Length));

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)(metadata.Length + fxAsset.Metadata.Length), info.Circulation);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(fxAsset.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.Commissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
        }
    }
}
