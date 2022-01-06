using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.AssetTokens
{
    [Collection(nameof(NetworkCredentials))]
    public class TransferAssetTests
    {
        private readonly NetworkCredentials _network;
        public TransferAssetTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Transfer Assets: Can Transfer Asset Coins")]
        public async Task CanTransferAssets()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

            var asset = new Asset(fxAsset.Record.Token, 1);
            var receipt = await fxAsset.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount.Record.Address, fxAccount.Record.Address, fxAsset.TreasuryAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0UL, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(fxAsset.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
            Assert.Empty(info.Royalties);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
            // NETWORK V0.21.0 UNSUPPORTED vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(info.Ledger.ToArray());
            // NETWORK V0.21.0 UNSUPPORTED ^^^^

            var balances = await fxAccount.Client.GetAccountBalancesAsync(fxAccount.Record.Address);
            Assert.Equal(fxAccount.Record.Address, balances.Address);
            Assert.Equal(fxAccount.CreateParams.InitialBalance, balances.Crypto);
            Assert.Single(balances.Tokens);
            Assert.Equal(1ul, balances.Tokens[fxAsset.Record.Token].Balance);

            balances = await fxAccount.Client.GetAccountBalancesAsync(fxAsset.TreasuryAccount.Record.Address);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, balances.Address);
            Assert.Equal(fxAsset.TreasuryAccount.CreateParams.InitialBalance, balances.Crypto);
            Assert.Single(balances.Tokens);
            Assert.Equal((ulong)(fxAsset.Metadata.Length - 1), balances.Tokens[fxAsset.Record.Token].Balance);
        }
        [Fact(DisplayName = "Transfer Assets: Can Transfer Asset Coins to Alias Account")]
        public async Task CanTransferAssetsToAliasAccount()
        {
            await using var fxAccount = await TestAliasAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
            await fxAsset.Client.AssociateTokenAsync(fxAsset.Record.Token, fxAccount, fxAccount.PrivateKey);

            var asset = new Asset(fxAsset.Record.Token, 1);
            var receipt = await fxAsset.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount.Record.Address, fxAccount.Alias, fxAsset.TreasuryAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0UL, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(fxAsset.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
            Assert.Empty(info.Royalties);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
            // NETWORK V0.21.0 UNSUPPORTED vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(info.Ledger.ToArray());
            // NETWORK V0.21.0 UNSUPPORTED ^^^^

            var balances = await fxAccount.Client.GetAccountBalancesAsync(fxAccount.CreateRecord.Address);
            Assert.Equal(fxAccount.CreateRecord.Address, balances.Address);
            Assert.True(balances.Crypto > 0);
            Assert.Single(balances.Tokens);
            Assert.Equal(1ul, balances.Tokens[fxAsset.Record.Token].Balance);

            balances = await fxAccount.Client.GetAccountBalancesAsync(fxAsset.TreasuryAccount.Record.Address);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, balances.Address);
            Assert.Equal(fxAsset.TreasuryAccount.CreateParams.InitialBalance, balances.Crypto);
            Assert.Single(balances.Tokens);
            Assert.Equal((ulong)(fxAsset.Metadata.Length - 1), balances.Tokens[fxAsset.Record.Token].Balance);
        }
        [Fact(DisplayName = "Transfer Assets: Can Transfer Asset and Get Record")]
        public async Task CanTransferAssetAndGetRecord()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

            var asset = new Asset(fxAsset.Record.Token, 1);

            var record = await fxAsset.Client.TransferAssetWithRecordAsync(asset, fxAsset.TreasuryAccount.Record.Address, fxAccount.Record.Address, fxAsset.TreasuryAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Empty(record.TokenTransfers);
            Assert.Single(record.AssetTransfers);
            Assert.Empty(record.Royalties);
            Assert.Empty(record.Associations);

            var xfer = record.AssetTransfers.First(x => x.To == fxAccount.Record.Address);
            Assert.NotNull(xfer);
            Assert.Equal(fxAsset.Record.Token, xfer.Asset);
            Assert.Equal(1, xfer.Asset.SerialNum);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
            // NETWORK V0.21.0 UNSUPPORTED vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(info.Ledger.ToArray());
            // NETWORK V0.21.0 UNSUPPORTED ^^^^

            var balances = await fxAccount.Client.GetAccountBalancesAsync(fxAccount.Record.Address);
            Assert.Equal(fxAccount.Record.Address, balances.Address);
            Assert.Equal(fxAccount.CreateParams.InitialBalance, balances.Crypto);
            Assert.Single(balances.Tokens);
            Assert.Equal(1UL, balances.Tokens[fxAsset.Record.Token]);

            balances = await fxAccount.Client.GetAccountBalancesAsync(fxAsset.TreasuryAccount.Record.Address);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, balances.Address);
            Assert.Equal(fxAsset.TreasuryAccount.CreateParams.InitialBalance, balances.Crypto);
            Assert.Single(balances.Tokens);
            Assert.Equal((ulong)fxAsset.Metadata.Length - 1, balances.Tokens[fxAsset.Record.Token]);
            Assert.Equal((ulong)fxAsset.Metadata.Length - 1, balances.Tokens[fxAsset.Record.Token].Balance);
            Assert.Equal(0U, balances.Tokens[fxAsset.Record.Token].Decimals);
        }
        [Fact(DisplayName = "Transfer Assets: Can Transfer Asset Coins and Get Record with signatories in context param")]
        public async Task CanTransferAssetsAndGetRecordWithSignatoriesInContextParam()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

            var asset = new Asset(fxAsset.Record.Token, 1);

            var record = await fxAsset.Client.TransferAssetWithRecordAsync(asset, fxAsset.TreasuryAccount.Record.Address, fxAccount.Record.Address, fxAsset.TreasuryAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Empty(record.TokenTransfers);
            Assert.Single(record.AssetTransfers);
            Assert.Empty(record.Royalties);
            Assert.Empty(record.Associations);

            var xfer = record.AssetTransfers.First(x => x.To == fxAccount.Record.Address);
            Assert.NotNull(xfer);
            Assert.Equal(fxAsset.Record.Token, xfer.Asset);
            Assert.Equal(1, xfer.Asset.SerialNum);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
            // NETWORK V0.21.0 UNSUPPORTED vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(info.Ledger.ToArray());
            // NETWORK V0.21.0 UNSUPPORTED ^^^^

            var balances = await fxAccount.Client.GetAccountBalancesAsync(fxAccount.Record.Address);
            Assert.Equal(fxAccount.Record.Address, balances.Address);
            Assert.Equal(fxAccount.CreateParams.InitialBalance, balances.Crypto);
            Assert.Single(balances.Tokens);
            Assert.Equal(1UL, balances.Tokens[fxAsset.Record.Token]);

            balances = await fxAccount.Client.GetAccountBalancesAsync(fxAsset.TreasuryAccount.Record.Address);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, balances.Address);
            Assert.Equal(fxAsset.TreasuryAccount.CreateParams.InitialBalance, balances.Crypto);
            Assert.Single(balances.Tokens);
            Assert.Equal((ulong)fxAsset.Metadata.Length - 1, balances.Tokens[fxAsset.Record.Token]);
            Assert.Equal((ulong)fxAsset.Metadata.Length - 1, balances.Tokens[fxAsset.Record.Token].Balance);
            Assert.Equal(0U, balances.Tokens[fxAsset.Record.Token].Decimals);
        }
        [Fact(DisplayName = "Transfer Assets: Can Execute Multi-Transfer Asset Coins")]
        public async Task CanExecuteMultiTransferAssets()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            var transfers = new TransferParams
            {
                AssetTransfers = new AssetTransfer[]
                {
                    new AssetTransfer(new Asset(fxAsset,1),fxAsset.TreasuryAccount, fxAccount1),
                    new AssetTransfer(new Asset(fxAsset,2),fxAsset.TreasuryAccount, fxAccount2)
                },
                Signatory = fxAsset.TreasuryAccount.PrivateKey
            };
            var receipt = await fxAsset.Client.TransferAsync(transfers);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(1UL, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length - 2, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Transfer Assets: Can Execute Multi-Transfer Asset with Record")]
        public async Task CanExecuteMultiTransferAssetsWithRecord()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            var transfers = new TransferParams
            {
                AssetTransfers = new AssetTransfer[]
                {
                    new AssetTransfer(new Asset(fxAsset,1),fxAsset.TreasuryAccount, fxAccount1),
                    new AssetTransfer(new Asset(fxAsset,2),fxAsset.TreasuryAccount, fxAccount2)
                },
                Signatory = fxAsset.TreasuryAccount.PrivateKey
            };
            var record = await fxAsset.Client.TransferWithRecordAsync(transfers);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Equal(2, record.AssetTransfers.Count);
            Assert.Null(record.ParentTransactionConcensus);

            var xferTo1 = record.AssetTransfers.First(x => x.To == fxAccount1.Record.Address);
            Assert.NotNull(xferTo1);
            Assert.Equal(fxAsset.Record.Token, xferTo1.Asset);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, xferTo1.From);
            Assert.Equal(1U, xferTo1.Asset.SerialNum);

            var xferTo2 = record.AssetTransfers.First(x => x.To == fxAccount2.Record.Address);
            Assert.NotNull(xferTo2);
            Assert.Equal(fxAsset.Record.Token, xferTo2.Asset);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, xferTo2.From);
            Assert.Equal(2U, xferTo2.Asset.SerialNum);

            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(1UL, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length - 2, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Transfer Assets: Can Execute Multi-Transfer Assets and Crypto")]
        public async Task CanExecuteMultiTransferAssetsAndCrypto()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);

            var cryptoAmount = (long)Generator.Integer(100, 200);
            var transfers = new TransferParams
            {
                CryptoTransfers = new Dictionary<AddressOrAlias, long>
                {
                    { _network.Payer, -2 * cryptoAmount },
                    { fxAccount1, cryptoAmount },
                    { fxAccount2, cryptoAmount }
                },
                AssetTransfers = new AssetTransfer[]
                {
                    new AssetTransfer(new Asset(fxAsset,1),fxAsset.TreasuryAccount, fxAccount1),
                    new AssetTransfer(new Asset(fxAsset,2),fxAsset.TreasuryAccount, fxAccount2)
                },
                Signatory = new Signatory(_network.Signatory, fxAsset.TreasuryAccount.PrivateKey)
            };
            var receipt = await fxAsset.Client.TransferAsync(transfers);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.NotNull(receipt.CurrentExchangeRate);
            Assert.NotNull(receipt.NextExchangeRate);
            Assert.Equal(_network.Payer, receipt.Id.Address);

            Assert.Equal(fxAccount1.CreateParams.InitialBalance + (ulong)cryptoAmount, await fxAccount1.Client.GetAccountBalanceAsync(fxAccount1));
            Assert.Equal(fxAccount2.CreateParams.InitialBalance + (ulong)cryptoAmount, await fxAccount2.Client.GetAccountBalanceAsync(fxAccount2));
            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(1UL, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length - 2, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Transfer Assets: Can Execute Multi-Transfer Asset Coins and Crypto with Record")]
        public async Task CanExecuteMultiTransferAssetsAndCryptoWithRecord()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            var cryptoAmount = (long)Generator.Integer(100, 200);
            var transfers = new TransferParams
            {
                CryptoTransfers = new Dictionary<AddressOrAlias, long>
                {
                    { _network.Payer, -2 * cryptoAmount },
                    { fxAccount1, cryptoAmount },
                    { fxAccount2, cryptoAmount }
                },
                AssetTransfers = new AssetTransfer[]
                {
                    new AssetTransfer(new Asset(fxAsset,1),fxAsset.TreasuryAccount, fxAccount1),
                    new AssetTransfer(new Asset(fxAsset,2),fxAsset.TreasuryAccount, fxAccount2)
                },
                Signatory = new Signatory(_network.Signatory, fxAsset.TreasuryAccount.PrivateKey)
            };
            var record = await fxAsset.Client.TransferWithRecordAsync(transfers);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Equal(5, record.Transfers.Count);
            Assert.Equal(2, record.AssetTransfers.Count);

            Assert.Equal(cryptoAmount, record.Transfers[fxAccount1]);
            Assert.Equal(cryptoAmount, record.Transfers[fxAccount2]);

            var xferTo1 = record.AssetTransfers.First(x => x.To == fxAccount1.Record.Address);
            Assert.NotNull(xferTo1);
            Assert.Equal(fxAsset.Record.Token, xferTo1.Asset);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, xferTo1.From);
            Assert.Equal(1U, xferTo1.Asset.SerialNum);

            var xferTo2 = record.AssetTransfers.First(x => x.To == fxAccount2.Record.Address);
            Assert.NotNull(xferTo2);
            Assert.Equal(fxAsset.Record.Token, xferTo2.Asset);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, xferTo2.From);
            Assert.Equal(2U, xferTo2.Asset.SerialNum);

            Assert.Equal(fxAccount1.CreateParams.InitialBalance + (ulong)cryptoAmount, await fxAccount1.Client.GetAccountBalanceAsync(fxAccount1));
            Assert.Equal(fxAccount2.CreateParams.InitialBalance + (ulong)cryptoAmount, await fxAccount2.Client.GetAccountBalanceAsync(fxAccount2));
            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(1UL, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length - 2, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Transfer Assets: Can Pass an Asset")]
        public async Task CanPassAnAsset()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);

            var asset = new Asset(fxAsset, 1);

            await fxAsset.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount, fxAccount1, fxAsset.TreasuryAccount);
            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));

            await fxAsset.Client.TransferAssetAsync(asset, fxAccount1, fxAccount2, fxAccount1);
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(1UL, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
        }
        [Fact(DisplayName = "Transfer Assets: Receive Signature Requirement Applies to Assets")]
        public async Task ReceiveSignatureRequirementAppliesToAssets()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network, fx =>
            {
                fx.CreateParams.RequireReceiveSignature = true;
                fx.CreateParams.Signatory = fx.PrivateKey;
            });
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);

            var asset = new Asset(fxAsset, 1);

            await fxAsset.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount, fxAccount1, fxAsset.TreasuryAccount);
            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(asset, fxAccount1, fxAccount2, fxAccount1);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: InvalidSignature", tex.Message);

            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));

            await fxAsset.Client.TransferAssetAsync(asset, fxAccount1, fxAccount2, new Signatory(fxAccount1, fxAccount2));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(1UL, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
        }
        [Fact(DisplayName = "Transfer Assets: Cannot Pass to A Frozen Account")]
        public async Task CannotPassToAFrozenAccount()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);

            var asset = new Asset(fxAsset, 1);

            await fxAsset.Client.SuspendTokenAsync(fxAsset, fxAccount2, fxAsset.SuspendPrivateKey);

            await fxAsset.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount, fxAccount1, fxAsset.TreasuryAccount);
            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(asset, fxAccount1, fxAccount2, fxAccount1);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
        }
        [Fact(DisplayName = "Transfer Assets: Cannot Pass to A KYC Non Granted Account")]
        public async Task CannotPassToAKCYNonGrantedAccount()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount1, fxAccount2);

            await fxAsset.Client.GrantTokenKycAsync(fxAsset, fxAccount1, fxAsset.GrantPrivateKey);

            var asset = new Asset(fxAsset, 1);

            await fxAsset.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount, fxAccount1, fxAsset.TreasuryAccount);
            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(asset, fxAccount1, fxAccount2, fxAccount1);
            });
            Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountKycNotGrantedForToken", tex.Message);

            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
        }
        [Fact(DisplayName = "Transfer Assets: Can Transfer Assets After Resume")]
        public async Task CanTransferAssetsAfterResume()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = true;
            }, fxAccount1, fxAccount2);

            var asset = new Asset(fxAsset, 1);

            // Account one, by default should not recievie coins.
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount, fxAccount1, fxAsset.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

            // Resume Participating Accounts
            await fxAsset.Client.ResumeTokenAsync(fxAsset.Record.Token, fxAccount1, fxAsset.SuspendPrivateKey);
            await fxAsset.Client.ResumeTokenAsync(fxAsset.Record.Token, fxAccount2, fxAsset.SuspendPrivateKey);

            // Move coins to account 2 via 1
            await fxAsset.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount, fxAccount1, fxAsset.TreasuryAccount);
            await fxAsset.Client.TransferAssetAsync(asset, fxAccount1, fxAccount2, fxAccount1);

            // Check our Balances
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length - 1, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));

            // Suppend Account One from Receiving Coins
            await fxAsset.Client.SuspendTokenAsync(fxAsset.Record.Token, fxAccount1, fxAsset.SuspendPrivateKey);
            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(asset, fxAccount2, fxAccount1, fxAccount2);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

            // Can we suspend the treasury?
            await fxAsset.Client.SuspendTokenAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.SuspendPrivateKey);
            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(asset, fxAccount2, fxAsset.TreasuryAccount, fxAccount2);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

            // Double Check can't send from frozen treasury.
            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 2), fxAsset.TreasuryAccount, fxAccount2, fxAsset.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

            // Balances should not have changed
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length - 1, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Transfer Assets: Can Transfer Assets After Suspend")]
        public async Task CannotTransferAssetsAfterSuspend()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = false;
            }, fxAccount1, fxAccount2);

            var asset = new Asset(fxAsset, 1);

            // Move coins to account 2 via 1
            await fxAsset.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount, fxAccount1, fxAsset.TreasuryAccount);
            await fxAsset.Client.TransferAssetAsync(asset, fxAccount1, fxAccount2, fxAccount1);

            // Check our Balances
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length - 1, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));

            // Suppend Account One from Receiving Coins
            await fxAsset.Client.SuspendTokenAsync(fxAsset.Record.Token, fxAccount1, fxAsset.SuspendPrivateKey);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(asset, fxAccount2, fxAccount1, fxAccount2);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

            // Can we suspend the treasury?
            await fxAsset.Client.SuspendTokenAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.SuspendPrivateKey);
            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(asset, fxAccount2, fxAsset.TreasuryAccount, fxAccount2);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

            // Double Check can't send from frozen treasury.
            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 2), fxAsset.TreasuryAccount, fxAccount2, fxAsset.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

            // Balances should not have changed
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length - 1, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));

            // Resume Participating Accounts
            await fxAsset.Client.ResumeTokenAsync(fxAsset, fxAccount1, fxAsset.SuspendPrivateKey);
            await fxAsset.Client.ResumeTokenAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.SuspendPrivateKey);

            // Move coins to back via 1
            await fxAsset.Client.TransferAssetAsync(asset, fxAccount2, fxAccount1, fxAccount2);
            await fxAsset.Client.TransferAssetAsync(asset, fxAccount1, fxAsset.TreasuryAccount, fxAccount1);

            // Check our Final Balances
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Associate Assets: Can Transfer Assets to Contract")]
        public async Task CanTransferAssetsToContract()
        {
            await using var fxContract = await GreetingContract.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = false;
            });
            await fxContract.Client.AssociateTokenAsync(fxAsset.Record.Token, fxContract.ContractRecord.Contract, fxContract.PrivateKey);

            var asset = new Asset(fxAsset, 1);

            var receipt = await fxAsset.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount, fxContract.ContractRecord.Contract, fxAsset.TreasuryAccount);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxContract.Client.GetContractInfoAsync(fxContract);
            var association = info.Tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
            Assert.NotNull(association);
            Assert.Equal(fxAsset.Record.Token, association.Token);
            Assert.Equal(fxAsset.Params.Symbol, association.Symbol);
            Assert.Equal(1U, association.Balance);
            Assert.Equal(0U, association.Decimals);
            Assert.Equal(TokenKycStatus.NotApplicable, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);
        }
        [Fact(DisplayName = "Transfer Assets: Can Transfer Asset Coins to Contract and Back")]
        public async Task CanTransferAssetsToContractAndBack()
        {
            await using var fxContract = await GreetingContract.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

            await fxAsset.Client.AssociateTokenAsync(fxAsset, fxContract, fxContract.PrivateKey);

            var asset = new Asset(fxAsset, 1);

            var receipt = await fxAsset.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount, fxContract.ContractRecord.Contract, fxAsset.TreasuryAccount);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var contractBalance = await fxContract.Client.GetContractBalancesAsync(fxContract.ContractRecord.Contract);
            Assert.Equal(fxContract.ContractRecord.Contract, contractBalance.Address);
            Assert.Equal(0UL, contractBalance.Crypto);
            Assert.Single(contractBalance.Tokens);
            Assert.Equal(1UL, contractBalance.Tokens[fxAsset.Record.Token]);
            Assert.Equal(1UL, await fxContract.Client.GetContractTokenBalanceAsync(fxContract, fxAsset));

            var treasuryBalance = await fxContract.Client.GetAccountBalancesAsync(fxAsset.TreasuryAccount.Record.Address);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, treasuryBalance.Address);
            Assert.Equal(fxAsset.TreasuryAccount.CreateParams.InitialBalance, treasuryBalance.Crypto);
            Assert.Single(treasuryBalance.Tokens);
            Assert.Equal((ulong)fxAsset.Metadata.Length - 1, treasuryBalance.Tokens[fxAsset.Record.Token]);
            Assert.Equal((ulong)fxAsset.Metadata.Length - 1, await fxContract.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));

            receipt = await fxAsset.Client.TransferAssetAsync(asset, fxContract.ContractRecord.Contract, fxAsset.TreasuryAccount, fxContract.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            Assert.Equal((ulong)fxAsset.Metadata.Length, await fxContract.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal(0UL, await fxContract.Client.GetContractTokenBalanceAsync(fxContract, fxAsset));
        }
        [Fact(DisplayName = "Transfer Assets: Can Change Treasury After Emptying")]
        public async Task CanChangeTreasuryAfterEmptying()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxNewTreasury = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.ConfiscateEndorsement = null;
            }, fxAccount, fxNewTreasury);

            var serialNumbers = Enumerable.Range(1, fxAsset.Metadata.Length).Select(i => (long)i);

            var transfers = new TransferParams
            {
                AssetTransfers = serialNumbers.Select(sn => new AssetTransfer(new Asset(fxAsset, sn), fxAsset.TreasuryAccount, fxAccount)),
                Signatory = fxAsset.TreasuryAccount.PrivateKey
            };
            var receipt = await fxAsset.Client.TransferAsync(transfers);

            // Double check balances.
            Assert.Equal((ulong)fxAsset.Metadata.Length, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset));
            Assert.Equal(0UL, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal(0UL, await fxAccount.Client.GetAccountTokenBalanceAsync(fxNewTreasury, fxAsset));

            // Can Not move the treasury to an existing account already having tokens
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(new UpdateTokenParams
                {
                    Token = fxAsset,
                    Treasury = fxAccount,
                    Signatory = new Signatory(fxAsset.AdminPrivateKey, fxAccount.PrivateKey)
                });
            });
            Assert.Equal(ResponseCode.TransactionRequiresZeroTokenBalances, tex.Status);
            Assert.Equal(ResponseCode.TransactionRequiresZeroTokenBalances, tex.Receipt.Status);
            Assert.StartsWith("Unable to update Token, status: TransactionRequiresZeroTokenBalances", tex.Message);

            // Coins have not moved.
            Assert.Equal((ulong)fxAsset.Metadata.Length, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset));
            Assert.Equal(0UL, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal(0UL, await fxAccount.Client.GetAccountTokenBalanceAsync(fxNewTreasury, fxAsset));

            // Move the treasury to a new account having zero token balance
            await fxAsset.Client.UpdateTokenAsync(new UpdateTokenParams
            {
                Token = fxAsset,
                Treasury = fxNewTreasury,
                Signatory = new Signatory(fxAsset.AdminPrivateKey, fxNewTreasury.PrivateKey)
            });

            // Coins have not moved.
            Assert.Equal((ulong)fxAsset.Metadata.Length, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset));
            Assert.Equal(0UL, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal(0UL, await fxAccount.Client.GetAccountTokenBalanceAsync(fxNewTreasury, fxAsset));

            // What does the info say now?
            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxNewTreasury.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
            // NETWORK V0.21.0 UNSUPPORTED vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(info.Ledger.ToArray());
            // NETWORK V0.21.0 UNSUPPORTED ^^^^
        }
        [Fact(DisplayName = "Transfer Assets: Can Schedule Multi-Transfer Asset Coins")]
        public async Task CanScheduleMultiTransferAssetCoins()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);

            var transfers = new TransferParams
            {
                AssetTransfers = new AssetTransfer[]
                {
                    new AssetTransfer(new Asset(fxAsset,1),fxAsset.TreasuryAccount, fxAccount1),
                    new AssetTransfer(new Asset(fxAsset,2),fxAsset.TreasuryAccount, fxAccount2)
                },
                Signatory = new Signatory(
                    fxAsset.TreasuryAccount.PrivateKey,
                    new PendingParams
                    {
                        PendingPayer = fxPayer
                    })
            };
            var schedulingReceipt = await fxAsset.Client.TransferAsync(transfers);
            Assert.Equal(ResponseCode.Success, schedulingReceipt.Status);

            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(0UL, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));

            var counterReceipt = await fxPayer.Client.SignPendingTransactionAsync(schedulingReceipt.Pending.Id, fxPayer);
            Assert.Equal(ResponseCode.Success, counterReceipt.Status);

            var transferReceipt = await fxPayer.Client.GetReceiptAsync(schedulingReceipt.Pending.TxId);
            Assert.Equal(ResponseCode.Success, schedulingReceipt.Status);

            Assert.Equal(1UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(1UL, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));
            Assert.Equal((ulong)fxAsset.Metadata.Length - 2, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
        }
        [Fact(DisplayName = "Transfer Assets: Metadata and Serial Numbers Transfer Properly")]
        async Task MetadataAndSerialNumbersTransferProperly()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

            var circulation = (ulong)fxAsset.Metadata.Length;
            var serialNumbers = Enumerable.Range(1, fxAsset.Metadata.Length).Where(i => i % 2 == 0).Select(i => (long)i).ToArray();
            var xferCount = (ulong)serialNumbers.Length;
            var expectedTreasury = circulation - xferCount;

            var transfers = new TransferParams
            {
                AssetTransfers = serialNumbers.Select(sn => new AssetTransfer(new Asset(fxAsset, sn), fxAsset.TreasuryAccount, fxAccount)),
                Signatory = fxAsset.TreasuryAccount.PrivateKey
            };
            var receipt = await fxAsset.Client.TransferAsync(transfers);

            // Double check balances.
            Assert.Equal(xferCount, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset));
            Assert.Equal(circulation - xferCount, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));

            // Double Check Metadata
            for (long sn = 1; sn <= (long)circulation; sn++)
            {
                var id = new Asset(fxAsset.Record.Token, sn);
                var asset = await fxAccount.Client.GetAssetInfoAsync(id);
                Assert.Equal(sn, asset.Asset.SerialNum);
                Assert.Equal(fxAsset.Record.Token, asset.Asset);
                Assert.Equal(asset.Asset.SerialNum % 2 == 0 ? fxAccount.Record.Address : fxAsset.TreasuryAccount.Record.Address, asset.Owner);
                Assert.True(fxAsset.Metadata[sn - 1].Span.SequenceEqual(asset.Metadata.Span));
            }
        }
    }
}
