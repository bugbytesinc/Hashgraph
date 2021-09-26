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
    public class DeleteAssetTests
    {
        private readonly NetworkCredentials _network;
        public DeleteAssetTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Asset Delete: Can Delete Asset")]
        public async Task CanDeleteAsset()
        {
            await using var fx = await TestAsset.CreateAsync(_network, fx => fx.Metadata = null);

            var record = await fx.Client.DeleteTokenAsync(fx.Record.Token, fx.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.True(info.Deleted);
        }
        [Fact(DisplayName = "Asset Delete: Anyone with Admin Key Can Delete Asset")]
        public async Task AnyoneWithAdminKeyCanDeleteAsset()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var record = await fxAsset.Client.DeleteTokenAsync(fxAsset.Record.Token, fxAsset.AdminPrivateKey, ctx =>
            {
                ctx.Payer = fxAccount.Record.Address;
                ctx.Signatory = fxAccount.PrivateKey;
            });
            Assert.Equal(ResponseCode.Success, record.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.True(info.Deleted);
        }
        [Fact(DisplayName = "Asset Delete: Deleting Does Not Remove Asset Records")]
        public async Task DeletingDoesNotRemoveAssetRecords()
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

            await fxAsset.Client.TransferAsync(transferParams);

            var record = await fxAccount.Client.DeleteTokenAsync(fxAsset.Record.Token, fxAsset.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(initialCirculation, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
            Assert.True(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);

            var accountInfo = await fxAsset.Client.GetAccountInfoAsync(fxAccount.Record.Address);
            var asset = accountInfo.Tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
            Assert.NotNull(asset);
            Assert.Equal(fxAsset.Record.Token, asset.Token);
            Assert.Equal(fxAsset.Params.Symbol, asset.Symbol);
            Assert.Equal(xferAmount, asset.Balance);
            Assert.Equal(0U, asset.Decimals);
            Assert.Equal(TokenTradableStatus.Tradable, asset.TradableStatus);
            Assert.False(asset.AutoAssociated);
            Assert.Equal(TokenKycStatus.NotApplicable, asset.KycStatus);

            var treasuryInfo = await fxAsset.Client.GetAccountInfoAsync(fxAsset.TreasuryAccount.Record.Address);
            asset = treasuryInfo.Tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
            Assert.NotNull(asset);
            Assert.Equal(fxAsset.Record.Token, asset.Token);
            Assert.Equal(fxAsset.Params.Symbol, asset.Symbol);
            Assert.Equal(expectedTreasury, asset.Balance);
            Assert.Equal(0U, asset.Decimals);
            Assert.Equal(TokenTradableStatus.Tradable, asset.TradableStatus);
            Assert.False(asset.AutoAssociated);
            Assert.Equal(TokenKycStatus.NotApplicable, asset.KycStatus);
        }
        [Fact(DisplayName = "Asset Delete: Deleting Asset Prevents Asset Transfers")]
        public async Task DeletingAssetPreventsAssetTransfers()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

            var transferParams = new TransferParams
            {
                AssetTransfers = new AssetTransfer[] { new AssetTransfer(new Asset(fxAsset.Record.Token, 1), fxAsset.TreasuryAccount, fxAccount) },
                Signatory = fxAsset.TreasuryAccount
            };

            await fxAsset.Client.TransferAsync(transferParams);

            var record = await fxAccount.Client.DeleteTokenAsync(fxAsset.Record.Token, fxAsset.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            transferParams = new TransferParams
            {
                AssetTransfers = new AssetTransfer[] { new AssetTransfer(new Asset(fxAsset.Record.Token, 2), fxAsset.TreasuryAccount, fxAccount) },
                Signatory = fxAsset.TreasuryAccount
            };

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAsync(transferParams);
            });
            Assert.Equal(ResponseCode.TokenWasDeleted, tex.Status);
            Assert.StartsWith("Unable to execute transfers, status: TokenWasDeleted", tex.Message);
        }
        [Fact(DisplayName = "Asset Delete: Deleting Asset Prevents Asset Transfers Amongst Third Parties")]
        public async Task DeletingAssetPreventsAssetTransfersAmongstThirdParties()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);

            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset.Record.Token,1), fxAsset.TreasuryAccount.Record.Address, fxAccount1.Record.Address, fxAsset.TreasuryAccount.PrivateKey);
            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset.Record.Token,2), fxAsset.TreasuryAccount.Record.Address, fxAccount2.Record.Address, fxAsset.TreasuryAccount.PrivateKey);

            var record = await fxAccount1.Client.DeleteTokenAsync(fxAsset.Record.Token, fxAsset.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset.Record.Token,1), fxAccount1.Record.Address, fxAccount2.Record.Address, fxAccount1.PrivateKey);
            });
            Assert.Equal(ResponseCode.TokenWasDeleted, tex.Status);
            Assert.StartsWith("Unable to execute transfers, status: TokenWasDeleted", tex.Message);
        }
        [Fact(DisplayName = "Asset Delete: Calling Delete Without Admin Key Raises Error")]
        public async Task CallingDeleteWithoutAdminKeyRaisesError()
        {
            await using var fx = await TestAsset.CreateAsync(_network);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.DeleteTokenAsync(fx.Record.Token);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to Delete Token, status: InvalidSignature", tex.Message);

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "Asset Delete: Calling Delete on an Imutable Asset Raises an Error")]
        public async Task CallingDeleteOnAnImutableAssetRaisesAnError()
        {
            await using var fx = await TestAsset.CreateAsync(_network, ctx =>
            {
                ctx.Params.Administrator = null;
            });

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.DeleteTokenAsync(fx.Record.Token, fx.AdminPrivateKey);
            });
            Assert.Equal(ResponseCode.TokenIsImmutable, tex.Status);
            Assert.StartsWith("Unable to Delete Token, status: TokenIsImmutable", tex.Message);

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "Asset Delete: Can Delete Asset with One of Two Mult-Sig")]
        public async Task CanDeleteAssetWithOneOfTwoMultSig()
        {
            var (pubAdminKey2, privateAdminKey2) = Generator.KeyPair();
            await using var fx = await TestAsset.CreateAsync(_network, ctx =>
            {
                ctx.Params.Administrator = new Endorsement(1, ctx.AdminPublicKey, pubAdminKey2);
                ctx.Params.Signatory = new Signatory(ctx.Params.Signatory, privateAdminKey2);
            });

            var record = await fx.Client.DeleteTokenAsync(fx.Record.Token, fx.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.True(info.Deleted);
        }
        [Fact(DisplayName = "Asset Delete: Deleting a Deleted Asset Raises Error")]
        public async Task DeletingADeletedAssetRaiseesError()
        {
            await using var fx = await TestAsset.CreateAsync(_network);

            var receipt = await fx.Client.DeleteTokenAsync(fx.Record.Token, fx.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.True(info.Deleted);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.DeleteTokenAsync(fx.Record.Token, fx.AdminPrivateKey);
            });
            Assert.Equal(ResponseCode.TokenWasDeleted, tex.Status);
            Assert.StartsWith("Unable to Delete Token, status: TokenWasDeleted", tex.Message);
        }
        [Fact(DisplayName = "Asset Delete: Calling Delete with invalid ID raises Error")]
        public async Task CallingDeleteWithInvalidIDRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.DeleteTokenAsync(fxAccount.Record.Address, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.InvalidTokenId, tex.Status);
            Assert.StartsWith("Unable to Delete Token, status: InvalidTokenId", tex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "Asset Delete: Calling Delete with missing ID raises Error")]
        public async Task CallingDeleteWithMissingIDRaisesError()
        {
            await using var fx = await TestAsset.CreateAsync(_network);
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx.Client.DeleteTokenAsync(null, fx.AdminPrivateKey);
            });
            Assert.Equal("token", ane.ParamName);
            Assert.StartsWith("Token is missing. Please check that it is not null", ane.Message);

            ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx.Client.DeleteTokenAsync(Address.None, fx.AdminPrivateKey);
            });
            Assert.Equal("token", ane.ParamName);
            Assert.StartsWith("Token is missing. Please check that it is not null", ane.Message);
        }
        [Fact(DisplayName = "Asset Delete: Cannot Delete Treasury while Attached to Asset")]
        public async Task CannotDeleteTreasuryWhileAttachedToAsset()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            var circulation = (ulong) fxAsset.Metadata.Length;

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAccount1.Client.DeleteAccountAsync(fxAsset.TreasuryAccount, fxAccount1, ctx =>
                {
                    ctx.Payer = fxAccount1;
                    ctx.Signatory = new Signatory(fxAccount1, fxAsset.TreasuryAccount);
                });
            });
            Assert.Equal(ResponseCode.AccountIsTreasury, tex.Status);
            Assert.StartsWith("Unable to delete account, status: AccountIsTreasury", tex.Message);

            var serialNumbersTransfered = Enumerable.Range(1, (int)circulation).Select(i => (long)i);

            var transferParams = new TransferParams
            {
                AssetTransfers = serialNumbersTransfered.Select(sn => new AssetTransfer(new Asset(fxAsset.Record.Token, sn), fxAsset.TreasuryAccount, fxAccount2)),
                Signatory = fxAsset.TreasuryAccount
            };

            await fxAsset.Client.TransferAsync(transferParams);

            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAccount1.Client.DeleteAccountAsync(fxAsset.TreasuryAccount, fxAccount1, ctx =>
                {
                    ctx.Payer = fxAccount1;
                    ctx.Signatory = new Signatory(fxAccount1, fxAsset.TreasuryAccount);
                });
            });
            Assert.Equal(ResponseCode.AccountIsTreasury, tex.Status);
            Assert.StartsWith("Unable to delete account, status: AccountIsTreasury", tex.Message);

            // Confirm Assets still exist in account 2
            Assert.Equal(0ul, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(circulation, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));

            // What does the info say,
            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(circulation, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);

            // Move the Treasury, hmm...don't need treasury key?
            await fxAsset.Client.UpdateTokenAsync(new UpdateTokenParams
            {
                Token = fxAsset,
                Treasury = fxAccount1,
                Signatory = new Signatory(fxAsset.AdminPrivateKey, fxAccount1.PrivateKey)
            });

            // Double check balances
            Assert.Equal(0ul, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxAsset));
            Assert.Equal(circulation, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxAsset));

            // What does the info say now?
            info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAccount1.Record.Address, info.Treasury);
            Assert.Equal(circulation, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
        }
        [Fact(DisplayName = "Asset Delete: Can Delete Treasury after Deleting Asset")]
        public async Task CanDeleteTreasuryAfterDeletingAsset()
        {
            await using var fx = await TestAsset.CreateAsync(_network);

            var record = await fx.Client.DeleteTokenAsync(fx.Record.Token, fx.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.True(info.Deleted);

            var receipt = await fx.Client.DeleteAccountAsync(fx.TreasuryAccount.Record.Address, _network.Payer, fx.TreasuryAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
        }
        [Fact(DisplayName = "Asset Delete: Can Not Schedule a Delete Asset")]
        public async Task CanNotScheduleADeleteAsset()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.DeleteTokenAsync(fxAsset.Record.Token, new Signatory(fxAsset.AdminPrivateKey, new PendingParams
                {
                    PendingPayer = fxPayer,
                }));
            });
            Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
            Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
        }
    }
}
