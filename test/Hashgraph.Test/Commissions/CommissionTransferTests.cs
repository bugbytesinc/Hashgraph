using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token
{
    [Collection(nameof(NetworkCredentials))]
    public class CommissionTransferTests
    {
        private readonly NetworkCredentials _network;
        public CommissionTransferTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Commission Transfers: Transferring Token Applies Fixed Commision")]
        async Task TransferringTokenAppliesFixedCommision()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxComAccount, fxAccount1, fxAccount2);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new FixedCommission[]
                {
                    new FixedCommission(fxComAccount, fxComToken, 100)
                };
                fx.Params.GrantKycEndorsement = null;
            }, fxAccount1, fxAccount2);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            await fxToken.Client.AssociateTokenAsync(fxComToken, fxToken.TreasuryAccount, fxToken.TreasuryAccount);

            await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount1);
            await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount2);
            await AssertHg.TokenNotAssociatedAsync(fxToken, fxComAccount);
            await AssertHg.TokenIsAssociatedAsync(fxToken, fxToken.TreasuryAccount);

            await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount1);
            await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount2);
            await AssertHg.TokenIsAssociatedAsync(fxComToken, fxComAccount);
            await AssertHg.TokenIsAssociatedAsync(fxComToken, fxToken.TreasuryAccount);

            await fxComToken.Client.TransferTokensAsync(fxComToken, fxComToken.TreasuryAccount, fxAccount1, 100, fxComToken.TreasuryAccount);
            await fxComToken.Client.TransferTokensAsync(fxComToken, fxComToken.TreasuryAccount, fxToken.TreasuryAccount, 100, fxComToken.TreasuryAccount);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation);
            await AssertHg.TokenBalanceAsync(fxToken, fxComToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxToken.TreasuryAccount, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 200);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, 100, fxToken.TreasuryAccount);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxComToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxToken.TreasuryAccount, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 200);

            await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, 100, fxAccount1);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxComToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxToken.TreasuryAccount, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 200);
        }

        [Fact(DisplayName = "Commission Transfers: Transferring Token Applies Fixed Commision With Updated Fee Structure")]
        async Task TransferringTokenAppliesFixedCommisionWithUpdatedFeeStructure()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxComAccount, fxAccount1, fxAccount2);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            await AssertHg.TokenNotAssociatedAsync(fxToken, fxComAccount);
            await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount1);
            await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount2);
            await AssertHg.TokenIsAssociatedAsync(fxToken, fxToken.TreasuryAccount);

            await AssertHg.TokenIsAssociatedAsync(fxComToken, fxComAccount);
            await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount1);
            await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount2);
            await AssertHg.TokenNotAssociatedAsync(fxComToken, fxToken.TreasuryAccount);

            await fxComToken.Client.TransferTokensAsync(fxComToken, fxComToken.TreasuryAccount, fxAccount1, 100, fxComToken.TreasuryAccount);
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, 100, fxToken.TreasuryAccount);

            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);

            await fxToken.Client.UpdateCommissionsAsync(fxToken, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken, 100) }, fxToken.CommissionsPrivateKey);

            await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, 100, fxAccount1);

            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);
        }
        [Fact(DisplayName = "Commission Transfers: Transferring Token Applies Variable Commision")]
        async Task TransferringTokenAppliesVariableCommision()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new FractionalCommission[]
                {
                    new FractionalCommission(fxComAccount,1,2,1,100)
                };
                fx.Params.GrantKycEndorsement = null;
                fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxComAccount.PrivateKey);
            }, fxAccount1, fxAccount2);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount1);
            await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount2);
            await AssertHg.TokenIsAssociatedAsync(fxToken, fxComAccount);
            await AssertHg.TokenIsAssociatedAsync(fxToken, fxToken.TreasuryAccount);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, 100, fxToken.TreasuryAccount);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

            await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, 49, fxAccount1);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 51);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 25);
            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 24);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);
        }

        [Fact(DisplayName = "Commission Transfers: Transferring Token Applies Immutable Fixed Commision")]
        async Task TransferringTokenAppliesImmutableFixedCommision()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new FractionalCommission[]
                {
                    new FractionalCommission(fxComAccount,1,2,1,100)
                };
                fx.Params.GrantKycEndorsement = null;
                fx.Params.CommissionsEndorsement = null;
                fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxComAccount.PrivateKey);
            }, fxAccount1, fxAccount2);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            await AssertHg.TokenIsAssociatedAsync(fxToken, fxComAccount);
            await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount1);
            await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount2);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, 100, fxToken.TreasuryAccount);

            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

            await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, 50, fxAccount1);

            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 25);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 50);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 25);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);
        }


        [Fact(DisplayName = "Commission Transfers: Transferring Token From Treasury Does Not Apply Fixed Commision")]
        async Task TransferringTokenFromTreasuryDoesNotApplyFixedCommision()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new FractionalCommission[]
                {
                    new FractionalCommission(fx.TreasuryAccount,1,2,1,100)
                };
                fx.Params.GrantKycEndorsement = null;
                fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxAccount1.PrivateKey);
            }, fxAccount1, fxAccount2);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, 100, fxToken.TreasuryAccount);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

            await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, 50, fxAccount1);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 50);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 25);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 75);
        }

        [Fact(DisplayName = "Commission Transfers: Can Apply a Simple Imutable Token With Commissions")]
        async Task CanApplyASimpleImutableTokenWithCommissions()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxTreasury = await TestAccount.CreateAsync(_network);
            await using var fxCollector = await TestAccount.CreateAsync(_network);

            var token = (await fxTreasury.Client.CreateTokenAsync(new CreateTokenParams
            {
                Name = "A",
                Symbol = "B",
                Circulation = 1000,
                Treasury = fxTreasury,
                Expiration = DateTime.UtcNow.AddDays(70),
                Commissions = new FractionalCommission[]
                {
                    new FractionalCommission(fxCollector,1,2,1,100)
                },
                Memo = string.Empty,
                Signatory = new Signatory(fxTreasury, fxCollector)
            })).Token;

            await fxTreasury.Client.AssociateTokenAsync(token, fxAccount1, fxAccount1);
            await fxTreasury.Client.AssociateTokenAsync(token, fxAccount2, fxAccount2);

            await fxTreasury.Client.TransferTokensAsync(token, fxTreasury, fxAccount1, 100, fxTreasury);

            Assert.Equal(100ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount1, token));
            Assert.Equal(0ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount2, token));
            Assert.Equal(0ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxCollector, token));
            Assert.Equal(900ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxTreasury, token));

            await fxTreasury.Client.TransferTokensAsync(token, fxAccount1, fxAccount2, 50, fxAccount1);

            Assert.Equal(50ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount1, token));
            Assert.Equal(25ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount2, token));
            Assert.Equal(25ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxCollector, token));
            Assert.Equal(900ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxTreasury, token));
        }

        [Fact(DisplayName = "Commission Transfers: Commissions Are Applied when Payer Is Same Account as Sender")]
        public async Task CommissionsAreAppliedWhenPayerIsSameAccountAsSender()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxTreasury = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxCollector = await TestAccount.CreateAsync(_network);

            var token = (await fxTreasury.Client.CreateTokenAsync(new CreateTokenParams
            {
                Name = "A",
                Symbol = "B",
                Circulation = 1000,
                Treasury = fxTreasury,
                Expiration = DateTime.UtcNow.AddDays(70),
                Commissions = new FractionalCommission[]
                {
                    new FractionalCommission(fxCollector,1,2,1,100)
                },
                Signatory = new Signatory(fxTreasury, fxCollector)
            })).Token;

            await fxTreasury.Client.AssociateTokenAsync(token, fxAccount1, fxAccount1);
            await fxTreasury.Client.AssociateTokenAsync(token, fxAccount2, fxAccount2);

            await fxTreasury.Client.TransferTokensAsync(token, fxTreasury, fxAccount1, 100, ctx =>
             {
                 ctx.Payer = fxTreasury;
                 ctx.Signatory = fxTreasury;
             });

            Assert.Equal(100ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount1, token));
            Assert.Equal(0ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount2, token));
            Assert.Equal(0ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxCollector, token));
            Assert.Equal(900ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxTreasury, token));

            await fxTreasury.Client.TransferTokensAsync(token, fxAccount1, fxAccount2, 50, ctx =>
            {
                ctx.Payer = fxAccount1;
                ctx.Signatory = fxAccount1;
            });

            Assert.Equal(50ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount1, token));
            Assert.Equal(25ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount2, token));
            Assert.Equal(25ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxCollector, token));
            Assert.Equal(900ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxTreasury, token));
        }

        [Fact(DisplayName = "Commission Transfers: Inssuficient Commission Token Balance Prevents Transfer")]
        async Task InssuficientCommissionTokenBalancePreventsTransfer()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxComAccount, fxAccount1, fxAccount2);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new FixedCommission[]
                {
                    new FixedCommission(fxComAccount, fxComToken, 200)
                };
                fx.Params.GrantKycEndorsement = null;
            }, fxAccount1, fxAccount2);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            await fxToken.Client.AssociateTokenAsync(fxComToken, fxToken.TreasuryAccount, fxToken.TreasuryAccount);

            await fxComToken.Client.TransferTokensAsync(fxComToken, fxComToken.TreasuryAccount, fxAccount1, 100, fxComToken.TreasuryAccount);
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, 100, fxToken.TreasuryAccount);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxComToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxToken.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, 100, fxAccount1);
            });
            Assert.Equal(ResponseCode.InsufficientSenderAccountBalanceForCustomFee, tex.Status);
            Assert.StartsWith("Unable to execute transfers, status: InsufficientSenderAccountBalanceForCustomFee", tex.Message);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxComToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxToken.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);
        }

        [Fact(DisplayName = "Commission Transfers: Fractional Commissions Appear in Transaction Record")]
        async Task FractionalCommissionsAppearInTransactionRecord()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2, fxComAccount);

            var record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, 100, fxToken.TreasuryAccount);
            Assert.Empty(record.AssetTransfers);
            Assert.Empty(record.Commissions);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 100);

            await fxToken.Client.UpdateCommissionsAsync(fxToken, new FractionalCommission[] { new FractionalCommission(fxComAccount, 1, 2, 1, 100) }, fxToken.CommissionsPrivateKey);

            record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken, fxAccount1, fxAccount2, 100, fxAccount1);
            Assert.Empty(record.AssetTransfers);
            Assert.Single(record.Commissions);

            var commission = record.Commissions[0];
            Assert.Equal(fxToken.Record.Token, commission.Token);
            Assert.Equal(50U, commission.Amount);
            Assert.Equal(fxComAccount.Record.Address, commission.Receiver);
            Assert.Contains(fxAccount2.Record.Address, commission.Payers);

            Assert.Equal(3, record.TokenTransfers.Count);

            var xfer = record.TokenTransfers.FirstOrDefault(x => x.Address == fxAccount1);
            Assert.NotNull(xfer);
            Assert.Equal(fxToken.Record.Token, xfer.Token);
            Assert.Equal(-100, xfer.Amount);

            xfer = record.TokenTransfers.FirstOrDefault(x => x.Address == fxAccount2);
            Assert.NotNull(xfer);
            Assert.Equal(fxToken.Record.Token, xfer.Token);
            Assert.Equal(50, xfer.Amount);

            xfer = record.TokenTransfers.FirstOrDefault(x => x.Address == fxComAccount);
            Assert.NotNull(xfer);
            Assert.Equal(fxToken.Record.Token, xfer.Token);
            Assert.Equal(50, xfer.Amount);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 50);
            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 50);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);
        }

        [Fact(DisplayName = "Commission Transfers: Fixed Commissions Appear in Transaction Record")]
        async Task FixedCommissionsAppearInTransactionRecord()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2, fxComAccount);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);

            await fxToken.Client.TransferTokensAsync(fxComToken, fxComToken.TreasuryAccount, fxAccount1, 100, fxComToken.TreasuryAccount);
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, 100, fxToken.TreasuryAccount);

            await fxToken.Client.UpdateCommissionsAsync(fxToken, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken, 50) }, new Signatory(fxToken.CommissionsPrivateKey, fxComAccount.PrivateKey));

            var record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken, fxAccount1, fxAccount2, 100, fxAccount1);
            Assert.Empty(record.AssetTransfers);
            Assert.Single(record.Commissions);

            var commission = record.Commissions[0];
            Assert.Equal(fxComToken.Record.Token, commission.Token);
            Assert.Equal(50U, commission.Amount);
            Assert.Equal(fxComAccount.Record.Address, commission.Receiver);
            Assert.Contains(fxAccount1.Record.Address, commission.Payers);

            Assert.Equal(4, record.TokenTransfers.Count);

            var xfer = record.TokenTransfers.FirstOrDefault(x => x.Amount == -100);
            Assert.NotNull(xfer);
            Assert.Equal(fxToken.Record.Token, xfer.Token);
            Assert.Equal(fxAccount1.Record.Address, xfer.Address);

            xfer = record.TokenTransfers.FirstOrDefault(x => x.Amount == 100);
            Assert.NotNull(xfer);
            Assert.Equal(fxToken.Record.Token, xfer.Token);
            Assert.Equal(fxAccount2.Record.Address, xfer.Address);

            xfer = record.TokenTransfers.FirstOrDefault(x => x.Amount == -50);
            Assert.NotNull(xfer);
            Assert.Equal(fxComToken.Record.Token, xfer.Token);
            Assert.Equal(fxAccount1.Record.Address, xfer.Address);

            xfer = record.TokenTransfers.FirstOrDefault(x => x.Amount == 50);
            Assert.NotNull(xfer);
            Assert.Equal(fxComToken.Record.Token, xfer.Token);
            Assert.Equal(fxComAccount.Record.Address, xfer.Address);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 50);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 50);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);
        }
        [Fact(DisplayName = "Commission Transfers: Treasury Exempted from Nested Commissions")]
        async Task TreasuryExemptedFromNestedCommissions()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxComToken1 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount, fxComAccount);
            await using var fxComToken2 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount, fxComAccount);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

            await fxToken.Client.AssociateTokenAsync(fxComToken1, fxToken.TreasuryAccount, fxToken.TreasuryAccount);
            await fxToken.Client.AssociateTokenAsync(fxComToken2, fxToken.TreasuryAccount, fxToken.TreasuryAccount);

            await fxToken.Client.TransferTokensAsync(fxComToken1, fxComToken1.TreasuryAccount, fxToken.TreasuryAccount, 100, fxComToken1.TreasuryAccount);
            await fxToken.Client.TransferTokensAsync(fxComToken2, fxComToken2.TreasuryAccount, fxToken.TreasuryAccount, 100, fxComToken2.TreasuryAccount);

            await fxToken.Client.UpdateCommissionsAsync(fxToken, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken1, 50) }, new Signatory(fxToken.CommissionsPrivateKey, fxComAccount.PrivateKey));
            await fxToken.Client.UpdateCommissionsAsync(fxComToken1, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken2, 25) }, new Signatory(fxComToken1.CommissionsPrivateKey, fxComAccount.PrivateKey));

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxComToken1.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxComToken2.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation);

            await AssertHg.TokenBalanceAsync(fxComToken1, fxAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken1, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken1, fxComToken1.TreasuryAccount, fxComToken1.Params.Circulation - 100);
            await AssertHg.TokenBalanceAsync(fxComToken1, fxComToken2.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken1, fxToken.TreasuryAccount, 100);

            await AssertHg.TokenBalanceAsync(fxComToken2, fxAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken2, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken2, fxComToken1.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken2, fxComToken2.TreasuryAccount, fxComToken2.Params.Circulation - 100);
            await AssertHg.TokenBalanceAsync(fxComToken2, fxToken.TreasuryAccount, 100);

            var record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken, fxToken.TreasuryAccount, fxAccount, 100, fxToken.TreasuryAccount);
            Assert.Empty(record.AssetTransfers);
            Assert.Empty(record.Commissions);

            Assert.Equal(2, record.TokenTransfers.Count);

            var xfer = record.TokenTransfers.FirstOrDefault(x => x.Amount == -100);
            Assert.NotNull(xfer);
            Assert.Equal(fxToken.Record.Token, xfer.Token);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, xfer.Address);

            xfer = record.TokenTransfers.FirstOrDefault(x => x.Amount == 100);
            Assert.NotNull(xfer);
            Assert.Equal(fxToken.Record.Token, xfer.Token);
            Assert.Equal(fxAccount.Record.Address, xfer.Address);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount, 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxComToken1.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxComToken2.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

            await AssertHg.TokenBalanceAsync(fxComToken1, fxAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken1, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken1, fxComToken1.TreasuryAccount, fxComToken1.Params.Circulation - 100);
            await AssertHg.TokenBalanceAsync(fxComToken1, fxComToken2.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken1, fxToken.TreasuryAccount, 100);

            await AssertHg.TokenBalanceAsync(fxComToken2, fxAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken2, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken2, fxComToken1.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken2, fxComToken2.TreasuryAccount, fxComToken2.Params.Circulation - 100);
            await AssertHg.TokenBalanceAsync(fxComToken2, fxToken.TreasuryAccount, 100);
        }
        [Fact(DisplayName = "Commission Transfers: Transferring Asset Applies Fixed Commision With Updated Fee Structure")]
        async Task TransferringAssetAppliesFixedCommisionWithUpdatedFeeStructure()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxComAccount, fxAccount1, fxAccount2);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxComAccount);
            await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount1);
            await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount2);
            await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAsset.TreasuryAccount);

            await AssertHg.TokenIsAssociatedAsync(fxComToken, fxComAccount);
            await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount1);
            await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount2);
            await AssertHg.TokenNotAssociatedAsync(fxComToken, fxAsset.TreasuryAccount);

            await fxComToken.Client.TransferTokensAsync(fxComToken, fxComToken.TreasuryAccount, fxAccount1, 100, fxComToken.TreasuryAccount);
            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount1, fxAsset.TreasuryAccount);

            await AssertHg.AssetBalanceAsync(fxAsset, fxComAccount, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount1, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)fxAsset.Metadata.Length - 1);

            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);

            await fxAsset.Client.UpdateCommissionsAsync(fxAsset, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken, 100) }, fxAsset.CommissionsPrivateKey);

            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAccount1, fxAccount2, fxAccount1);

            await AssertHg.AssetBalanceAsync(fxAsset, fxComAccount, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount1, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount2, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)fxAsset.Metadata.Length - 1);

            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);
        }
        [Fact(DisplayName = "Commission Transfers: Can Schedule Asset Trnasfer with Fixed Commision")]
        async Task CanScheduleAssetTrnasferWithFixedCommision()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxComAccount, fxAccount1, fxAccount2);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxComAccount);
            await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount1);
            await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount2);
            await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAsset.TreasuryAccount);

            await AssertHg.TokenIsAssociatedAsync(fxComToken, fxComAccount);
            await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount1);
            await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount2);
            await AssertHg.TokenNotAssociatedAsync(fxComToken, fxAsset.TreasuryAccount);

            await fxComToken.Client.TransferTokensAsync(fxComToken, fxComToken.TreasuryAccount, fxAccount1, 100, fxComToken.TreasuryAccount);
            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount1, fxAsset.TreasuryAccount);

            await AssertHg.AssetBalanceAsync(fxAsset, fxComAccount, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount1, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)fxAsset.Metadata.Length - 1);

            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);

            await fxAsset.Client.UpdateCommissionsAsync(fxAsset, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken, 100) }, fxAsset.CommissionsPrivateKey);

            var schedulingReceipt = await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAccount1, fxAccount2, new Signatory(fxAccount1, new PendingParams { PendingPayer = fxAccount2 }));

            await AssertHg.AssetBalanceAsync(fxAsset, fxComAccount, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount1, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)fxAsset.Metadata.Length - 1);

            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);

            var counterReceipt = await fxAsset.Client.SignPendingTransactionAsync(schedulingReceipt.Pending.Id, fxAccount2);
            Assert.Equal(ResponseCode.Success, counterReceipt.Status);

            TransactionReceipt pendingReceipt = await fxAsset.Client.GetReceiptAsync(schedulingReceipt.Pending.TxId);
            Assert.Equal(ResponseCode.Success, pendingReceipt.Status);

            await AssertHg.AssetBalanceAsync(fxAsset, fxComAccount, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount1, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount2, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)fxAsset.Metadata.Length - 1);

            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);
        }
        [Fact(DisplayName = "Commission Transfers: Transferring Multiple Assets Applies Fixed Commision For Each Asset Transferred")]
        async Task TransferringMultipleAssetsAppliesFixedCommisionForEachAssetTransferred()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxComAccount, fxAccount1, fxAccount2);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);

            await fxComToken.Client.TransferTokensAsync(fxComToken, fxComToken.TreasuryAccount, fxAccount1, 100, fxComToken.TreasuryAccount);
            await fxAsset.Client.TransferAsync(new TransferParams
            {
                AssetTransfers = new AssetTransfer[]
                {
                    new AssetTransfer(new Asset(fxAsset,1),fxAsset.TreasuryAccount,fxAccount1),
                    new AssetTransfer(new Asset(fxAsset,2),fxAsset.TreasuryAccount,fxAccount1)
                },
                Signatory = fxAsset.TreasuryAccount.PrivateKey
            });

            await AssertHg.AssetBalanceAsync(fxAsset, fxComAccount, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount1, 2);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)fxAsset.Metadata.Length - 2);

            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 100);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);

            await fxAsset.Client.UpdateCommissionsAsync(fxAsset, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken, 10) }, fxAsset.CommissionsPrivateKey);

            await fxAsset.Client.TransferAsync(new TransferParams
            {
                AssetTransfers = new AssetTransfer[]
                {
                    new AssetTransfer(new Asset(fxAsset,1),fxAccount1, fxAccount2),
                    new AssetTransfer(new Asset(fxAsset,2),fxAccount1, fxAccount2)
                },
                Signatory = fxAccount1.PrivateKey
            });

            await AssertHg.AssetBalanceAsync(fxAsset, fxComAccount, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount1, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAccount2, 2);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)fxAsset.Metadata.Length - 2);

            await AssertHg.TokenBalanceAsync(fxComToken, fxComAccount, 20);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 80);
            await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);
        }
        [Fact(DisplayName = "Commission Transfers: Treasury Does Pay Second Degree Comission")]
        async Task TreasuryDoesPaySecondDegreeComission()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxComToken1 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount, fxAccount2, fxComAccount);
            await using var fxComToken2 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount, fxAccount2, fxComAccount);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount, fxAccount2);

            await fxToken.Client.UpdateCommissionsAsync(fxToken, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken1, 50) }, new Signatory(fxToken.CommissionsPrivateKey, fxComAccount.PrivateKey));
            await fxToken.Client.UpdateCommissionsAsync(fxComToken1, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken2, 25) }, new Signatory(fxComToken1.CommissionsPrivateKey, fxComAccount.PrivateKey));

            await fxToken.Client.TransferTokensAsync(fxComToken1, fxComToken1.TreasuryAccount, fxAccount, 100, fxComToken1.TreasuryAccount);
            await fxToken.Client.TransferTokensAsync(fxComToken2, fxComToken2.TreasuryAccount, fxAccount, 100, fxComToken2.TreasuryAccount);
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, 100, fxToken.TreasuryAccount);

            var record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken, fxAccount, fxAccount2, 100, fxAccount);
            Assert.Empty(record.AssetTransfers);

            Assert.Equal(2, record.Commissions.Count);
            AssertHg.ContainsCommission(fxComToken1, fxAccount, fxComAccount, 50, record.Commissions);
            AssertHg.ContainsCommission(fxComToken2, fxAccount, fxComAccount, 25, record.Commissions);

            Assert.Equal(6, record.TokenTransfers.Count);
            Assert.Contains(new TokenTransfer(fxToken, fxAccount, -100), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxToken, fxAccount2, 100), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxComToken1, fxAccount, -50), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxComToken1, fxComAccount, 50), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxComToken2, fxAccount, -25), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxComToken2, fxComAccount, 25), record.TokenTransfers);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 100);
            await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

            await AssertHg.TokenBalanceAsync(fxComToken1, fxAccount, 50);
            await AssertHg.TokenBalanceAsync(fxComToken1, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken1, fxComAccount, 50);
            await AssertHg.TokenBalanceAsync(fxComToken1, fxComToken1.TreasuryAccount, fxComToken1.Params.Circulation - 100);

            await AssertHg.TokenBalanceAsync(fxComToken2, fxAccount, 75);
            await AssertHg.TokenBalanceAsync(fxComToken2, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxComToken2, fxComAccount, 25);
            await AssertHg.TokenBalanceAsync(fxComToken2, fxComToken2.TreasuryAccount, fxComToken2.Params.Circulation - 100);
        }
        [Fact(DisplayName = "Commission Transfers: Three Fixed Commissions Plus Royalty When hBar Value Exchanged")]
        public async Task TransferringAssetAppliesMultipleFixedCommisionDeductionDestinations()
        {
            await using var fxBuyer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
            await using var fxSeller = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor3 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor4 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxGasToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.Decimals = 2;
                fx.Params.Circulation = 1_000_00;
            }, fxBenefactor1, fxBenefactor2, fxBenefactor3, fxBenefactor4, fxBuyer, fxSeller);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ICommission[]
                {
                    new ValueCommission(fxBenefactor4, 1, 5, 50, fxGasToken),
                    new FixedCommission(fxBenefactor1, fxGasToken, 20),
                    new FixedCommission(fxBenefactor2, fxGasToken, 20),
                    new FixedCommission(fxBenefactor3, fxGasToken, 40),
                };
                fx.Params.GrantKycEndorsement = null;
            }, fxBuyer, fxSeller);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var movedAsset = new Asset(fxAsset, 1);

            await fxGasToken.Client.TransferTokensAsync(fxGasToken, fxGasToken.TreasuryAccount, fxSeller, 100_00, fxGasToken.TreasuryAccount);
            await fxGasToken.Client.TransferAssetAsync(movedAsset, fxAsset.TreasuryAccount, fxSeller, fxAsset.TreasuryAccount);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 100_00);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 100_00);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 10_00_000_000);
            await AssertHg.CryptoBalanceAsync(fxSeller, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor4, 0);

            var record = await fxAsset.Client.TransferWithRecordAsync(new TransferParams
            {
                AssetTransfers = new[] {
                    new AssetTransfer(movedAsset, fxSeller, fxBuyer)
                },
                CryptoTransfers = new Dictionary<Address, long> {
                    { fxBuyer.Record.Address, -5_00_000_000 },
                    { fxSeller.Record.Address, 5_00_000_000 }
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
            Assert.Contains(new TokenTransfer(fxGasToken, fxSeller, -80), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor1, 20), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor2, 20), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor3, 40), record.TokenTransfers);
            Assert.Single(record.AssetTransfers);
            Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
            Assert.Equal(4, record.Commissions.Count);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor1, 20, record.Commissions);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor2, 20, record.Commissions);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor3, 40, record.Commissions);
            AssertHg.ContainsHbarCommission(fxSeller, fxBenefactor4, 1_00_000_000, record.Commissions);
            Assert.Equal(-5_00_000_000L, record.Transfers[fxBuyer]);
            Assert.Equal(4_00_000_000L, record.Transfers[fxSeller]);
            Assert.Equal(1_00_000_000L, record.Transfers[fxBenefactor4]);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 99_20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 40);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 100_00);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 5_00_000_000);
            await AssertHg.CryptoBalanceAsync(fxSeller, 4_00_000_000L);
            await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor4, 1_00_000_000L);
        }
        [Fact(DisplayName = "Commission Transfers: Three Fixed Commissions Plus Royalty When Fungible Token Value Exchanged")]
        public async Task ThreeFixedCommissionsPlusRoyaltyWhenFungibleTokenValueExchanged()
        {
            await using var fxBuyer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
            await using var fxSeller = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor3 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor4 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxGasToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.Decimals = 2;
                fx.Params.Circulation = 1_000_00;
            }, fxBenefactor1, fxBenefactor2, fxBenefactor3, fxBenefactor4, fxBuyer, fxSeller);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ICommission[]
                {
                    new ValueCommission(fxBenefactor4, 1, 5, 50, fxGasToken),
                    new FixedCommission(fxBenefactor1, fxGasToken, 20),
                    new FixedCommission(fxBenefactor2, fxGasToken, 20),
                    new FixedCommission(fxBenefactor3, fxGasToken, 40),
                };
                fx.Params.GrantKycEndorsement = null;
            }, fxBuyer, fxSeller);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var movedAsset = new Asset(fxAsset, 1);

            await fxGasToken.Client.TransferTokensAsync(fxGasToken, fxGasToken.TreasuryAccount, fxBuyer, 100_00, fxGasToken.TreasuryAccount);
            await fxGasToken.Client.TransferAssetAsync(movedAsset, fxAsset.TreasuryAccount, fxSeller, fxAsset.TreasuryAccount);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 100_00);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 100_00);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 10_00_000_000);
            await AssertHg.CryptoBalanceAsync(fxSeller, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor4, 0);

            var record = await fxAsset.Client.TransferWithRecordAsync(new TransferParams
            {
                AssetTransfers = new[] {
                    new AssetTransfer(movedAsset, fxSeller, fxBuyer)
                },
                TokenTransfers = new[] {
                    new TokenTransfer(fxGasToken, fxBuyer, -10_00),
                    new TokenTransfer(fxGasToken, fxSeller, 10_00),
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
            Assert.Equal(6, record.TokenTransfers.Count);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBuyer, -10_00), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxSeller, 7_20), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor1, 20), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor2, 20), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor3, 40), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor4, 2_00), record.TokenTransfers);
            Assert.Single(record.AssetTransfers);
            Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
            Assert.Equal(4, record.Commissions.Count);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor1, 20, record.Commissions);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor2, 20, record.Commissions);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor3, 40, record.Commissions);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor4, 2_00, record.Commissions);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 90_00);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 7_20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 40);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor4, 2_00);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 100_00);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 10_00_000_000L);
            await AssertHg.CryptoBalanceAsync(fxSeller, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor4, 0);
        }
        [Fact(DisplayName = "Commission Transfers: Three Fixed Commissions Plus Royalty When Alternate Fungible Token Value Exchanged")]
        public async Task ThreeFixedCommissionsPlusRoyaltyWhenAlternateFungibleTokenValueExchanged()
        {
            await using var fxBuyer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
            await using var fxSeller = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor3 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor4 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxGasToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.Decimals = 2;
                fx.Params.Circulation = 1_000_00;
            }, fxBenefactor1, fxBenefactor2, fxBenefactor3, fxBenefactor4, fxBuyer, fxSeller);
            await using var fxPaymentToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.Decimals = 2;
                fx.Params.Circulation = 1_000_00;
            }, fxBenefactor1, fxBenefactor2, fxBenefactor3, fxBenefactor4, fxBuyer, fxSeller);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ICommission[]
                {
                    new ValueCommission(fxBenefactor4, 1, 5, 50, fxGasToken),
                    new FixedCommission(fxBenefactor1, fxGasToken, 20),
                    new FixedCommission(fxBenefactor2, fxGasToken, 20),
                    new FixedCommission(fxBenefactor3, fxGasToken, 40),
                };
                fx.Params.GrantKycEndorsement = null;
            }, fxBuyer, fxSeller);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var movedAsset = new Asset(fxAsset, 1);

            await fxGasToken.Client.TransferTokensAsync(fxGasToken, fxGasToken.TreasuryAccount, fxSeller, 100_00, fxGasToken.TreasuryAccount);
            await fxGasToken.Client.TransferTokensAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxBuyer, 100_00, fxPaymentToken.TreasuryAccount);
            await fxGasToken.Client.TransferAssetAsync(movedAsset, fxAsset.TreasuryAccount, fxSeller, fxAsset.TreasuryAccount);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 100_00);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 100_00);

            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 100_00);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor1, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor2, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor3, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor4, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 100_00);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 10_00_000_000);
            await AssertHg.CryptoBalanceAsync(fxSeller, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor4, 0);

            var record = await fxAsset.Client.TransferWithRecordAsync(new TransferParams
            {
                AssetTransfers = new[] {
                    new AssetTransfer(movedAsset, fxSeller, fxBuyer)
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
            Assert.Equal(7, record.TokenTransfers.Count);
            Assert.Contains(new TokenTransfer(fxGasToken, fxSeller, -80), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor1, 20), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor2, 20), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor3, 40), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxPaymentToken, fxBuyer, -10_00), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxPaymentToken, fxSeller, 8_00), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxPaymentToken, fxBenefactor4, 2_00), record.TokenTransfers);
            Assert.Single(record.AssetTransfers);
            Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
            Assert.Equal(4, record.Commissions.Count);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor1, 20, record.Commissions);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor2, 20, record.Commissions);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor3, 40, record.Commissions);
            AssertHg.ContainsCommission(fxPaymentToken, fxSeller, fxBenefactor4, 2_00, record.Commissions);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 99_20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 40);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor4, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 100_00);

            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBuyer, 90_00);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxSeller, 8_00);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor1, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor2, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor3, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxBenefactor4, 2_00);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxPaymentToken, fxPaymentToken.TreasuryAccount, fxPaymentToken.Params.Circulation - 100_00);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 10_00_000_000L);
            await AssertHg.CryptoBalanceAsync(fxSeller, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor4, 0);
        }
        [Fact(DisplayName = "Commission Transfers: Three Fixed Commissions Plus Royalty When No Value Exchanged")]
        public async Task ThreeFixedCommissionsPlusRoyaltyWhenNoValueExchanged()
        {
            await using var fxBuyer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxSeller = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor3 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor4 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxGasToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.Decimals = 2;
                fx.Params.Circulation = 1_000_00;
            }, fxBenefactor1, fxBenefactor2, fxBenefactor3, fxBenefactor4, fxBuyer, fxSeller);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ICommission[]
                {
                    new FixedCommission(fxBenefactor1, fxGasToken, 20),
                    new FixedCommission(fxBenefactor2, fxGasToken, 20),
                    new FixedCommission(fxBenefactor3, fxGasToken, 40),
                    new ValueCommission(fxBenefactor4, 1, 5, 50, fxGasToken),
                };
                fx.Params.GrantKycEndorsement = null;
            }, fxBuyer, fxSeller);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var movedAsset = new Asset(fxAsset, 1);

            await fxGasToken.Client.TransferTokensAsync(fxGasToken, fxGasToken.TreasuryAccount, fxSeller, 100_00, fxGasToken.TreasuryAccount);
            await fxGasToken.Client.TransferTokensAsync(fxGasToken, fxGasToken.TreasuryAccount, fxBuyer, 100_00, fxGasToken.TreasuryAccount);
            await fxGasToken.Client.TransferAssetAsync(movedAsset, fxAsset.TreasuryAccount, fxSeller, fxAsset.TreasuryAccount);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 100_00);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 100_00);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 200_00);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 0);
            await AssertHg.CryptoBalanceAsync(fxSeller, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor4, 0);

            var record = await fxAsset.Client.TransferWithRecordAsync(new TransferParams
            {
                AssetTransfers = new[] {
                    new AssetTransfer(movedAsset, fxSeller, fxBuyer)
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
            Assert.Equal(6, record.TokenTransfers.Count);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBuyer, -50), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxSeller, -80), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor1, 20), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor2, 20), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor3, 40), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor4, 50), record.TokenTransfers);
            Assert.Single(record.AssetTransfers);
            Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
            Assert.Equal(4, record.Commissions.Count);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor1, 20, record.Commissions);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor2, 20, record.Commissions);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor3, 40, record.Commissions);
            AssertHg.ContainsCommission(fxGasToken, fxBuyer, fxBenefactor4, 50, record.Commissions);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 99_50);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 99_20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 40);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor4, 50);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 200_00);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 0);
            await AssertHg.CryptoBalanceAsync(fxSeller, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor4, 0);
        }
        [Fact(DisplayName = "Commission Transfers: Three Fixed Commissions Plus Royalty When hBar Given in Addition")]
        public async Task ThreeFixedCommissionsPlusRoyaltyWhenhBarGiveninAddition()
        {
            await using var fxBuyer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxSeller = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
            await using var fxBenefactor1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor3 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxBenefactor4 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
            await using var fxGasToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.Decimals = 2;
                fx.Params.Circulation = 1_000_00;
            }, fxBenefactor1, fxBenefactor2, fxBenefactor3, fxBenefactor4, fxBuyer, fxSeller);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ICommission[]
                {
                    new ValueCommission(fxBenefactor4, 1, 5, 50, fxGasToken),
                    new FixedCommission(fxBenefactor1, fxGasToken, 20),
                    new FixedCommission(fxBenefactor2, fxGasToken, 20),
                    new FixedCommission(fxBenefactor3, fxGasToken, 40),
                };
                fx.Params.GrantKycEndorsement = null;
            }, fxBuyer, fxSeller);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var movedAsset = new Asset(fxAsset, 1);

            await fxGasToken.Client.TransferTokensAsync(fxGasToken, fxGasToken.TreasuryAccount, fxBuyer, 100_00, fxGasToken.TreasuryAccount);
            await fxGasToken.Client.TransferTokensAsync(fxGasToken, fxGasToken.TreasuryAccount, fxSeller, 100_00, fxGasToken.TreasuryAccount);
            await fxGasToken.Client.TransferAssetAsync(movedAsset, fxAsset.TreasuryAccount, fxSeller, fxAsset.TreasuryAccount);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 100_00);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 100_00);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 200_00);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 0);
            await AssertHg.CryptoBalanceAsync(fxSeller, 10_00_000_000);
            await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor4, 0);

            var record = await fxAsset.Client.TransferWithRecordAsync(new TransferParams
            {
                AssetTransfers = new[] {
                    new AssetTransfer(movedAsset, fxSeller, fxBuyer)
                },
                CryptoTransfers = new Dictionary<Address, long> {
                    { fxSeller.Record.Address, -5_00_000_000 },
                    { fxBuyer.Record.Address, 5_00_000_000 }
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
            Assert.Equal(6, record.TokenTransfers.Count);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBuyer, -50), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxSeller, -80), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor1, 20), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor2, 20), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor3, 40), record.TokenTransfers);
            Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor4, 50), record.TokenTransfers);
            Assert.Single(record.AssetTransfers);
            Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
            Assert.Equal(4, record.Commissions.Count);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor1, 20, record.Commissions);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor2, 20, record.Commissions);
            AssertHg.ContainsCommission(fxGasToken, fxSeller, fxBenefactor3, 40, record.Commissions);
            AssertHg.ContainsCommission(fxGasToken, fxBuyer, fxBenefactor4, 50, record.Commissions);
            Assert.Equal(5_00_000_000L, record.Transfers[fxBuyer]);
            Assert.Equal(-5_00_000_000L, record.Transfers[fxSeller]);

            await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor4, 0);
            await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
            await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

            await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 99_50);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 99_20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 20);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 40);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor4, 50);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxAsset.TreasuryAccount, 0);
            await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 200_00);

            await AssertHg.CryptoBalanceAsync(fxBuyer, 5_00_000_000);
            await AssertHg.CryptoBalanceAsync(fxSeller, 5_00_000_000L);
            await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
            await AssertHg.CryptoBalanceAsync(fxBenefactor4, 0);
        }
    }
}
