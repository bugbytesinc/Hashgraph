using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token;

[Collection(nameof(NetworkCredentials))]
public class RoyaltyTransferTests
{
    private readonly NetworkCredentials _network;
    public RoyaltyTransferTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Royalty Transfers: Transferring Token Applies Fixed Commision")]
    async Task TransferringTokenAppliesFixedCommision()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxComAccount = await TestAccount.CreateAsync(_network);
        await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxComAccount, fxAccount1, fxAccount2);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                    new FixedRoyalty(fxComAccount, fxComToken, 100)
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

    [Fact(DisplayName = "Royalty Transfers: Transferring Token Applies Fixed Commision With Updated Fee Structure")]
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

        await fxToken.Client.UpdateRoyaltiesAsync(fxToken, new FixedRoyalty[] { new FixedRoyalty(fxComAccount, fxComToken, 100) }, fxToken.RoyaltiesPrivateKey);

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
    [Fact(DisplayName = "Royalty Transfers: Transferring Token Applies Fractional Commision")]
    async Task TransferringTokenAppliesFractionalCommision()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxComAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new TokenRoyalty[]
            {
                    new TokenRoyalty(fxComAccount, 1, 2, 1, 100)
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

    [Fact(DisplayName = "Royalty Transfers: Transferring Token Applies Fractional Commision As Surcharge")]
    async Task TransferringTokenAppliesFractionalCommisionAsSurcharge()
    {
        await using var fxSender = await TestAccount.CreateAsync(_network);
        await using var fxReceiver = await TestAccount.CreateAsync(_network);
        await using var fxComAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new TokenRoyalty[]
            {
                    new TokenRoyalty(fxComAccount, 1, 2, 1, 100, true)
            };
            fx.Params.GrantKycEndorsement = null;
            fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxComAccount.PrivateKey);
        }, fxSender, fxReceiver);
        Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

        await AssertHg.TokenIsAssociatedAsync(fxToken, fxSender);
        await AssertHg.TokenIsAssociatedAsync(fxToken, fxReceiver);
        await AssertHg.TokenIsAssociatedAsync(fxToken, fxComAccount);
        await AssertHg.TokenIsAssociatedAsync(fxToken, fxToken.TreasuryAccount);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxSender, 100, fxToken.TreasuryAccount);

        await AssertHg.TokenBalanceAsync(fxToken, fxSender, 100);
        await AssertHg.TokenBalanceAsync(fxToken, fxReceiver, 0);
        await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 0);
        await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

        await fxToken.Client.TransferTokensAsync(fxToken, fxSender, fxReceiver, 49, fxSender);

        await AssertHg.TokenBalanceAsync(fxToken, fxSender, 27);
        await AssertHg.TokenBalanceAsync(fxToken, fxReceiver, 49);
        await AssertHg.TokenBalanceAsync(fxToken, fxComAccount, 24);
        await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);
    }

    [Fact(DisplayName = "Royalty Transfers: Transferring Token Applies Immutable Fixed Commision")]
    async Task TransferringTokenAppliesImmutableFixedCommision()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxComAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new TokenRoyalty[]
            {
                    new TokenRoyalty(fxComAccount,1,2,1,100)
            };
            fx.Params.GrantKycEndorsement = null;
            fx.Params.RoyaltiesEndorsement = null;
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


    [Fact(DisplayName = "Royalty Transfers: Transferring Token From Treasury Does Not Apply Fixed Commision")]
    async Task TransferringTokenFromTreasuryDoesNotApplyFixedCommision()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new TokenRoyalty[]
            {
                    new TokenRoyalty(fx.TreasuryAccount,1,2,1,100)
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

    [Fact(DisplayName = "Royalty Transfers: Can Apply a Simple Imutable Token With Royalties")]
    async Task CanApplyASimpleImutableTokenWithRoyalties()
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
            Royalties = new TokenRoyalty[]
            {
                    new TokenRoyalty(fxCollector,1,2,1,100)
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

    [Fact(DisplayName = "Royalty Transfers: Royalties Are Applied when Payer Is Same Account as Sender")]
    public async Task RoyaltiesAreAppliedWhenPayerIsSameAccountAsSender()
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
            Royalties = new TokenRoyalty[]
            {
                    new TokenRoyalty(fxCollector,1,2,1,100)
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

    [Fact(DisplayName = "Royalty Transfers: Inssuficient Royalty Token Balance Prevents Transfer")]
    async Task InssuficientRoyaltyTokenBalancePreventsTransfer()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxComAccount = await TestAccount.CreateAsync(_network);
        await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxComAccount, fxAccount1, fxAccount2);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                    new FixedRoyalty(fxComAccount, fxComToken, 200)
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

    [Fact(DisplayName = "Royalty Transfers: Fractional Royalties Appear in Transaction Record")]
    async Task FractionalRoyaltiesAppearInTransactionRecord()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxComAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2, fxComAccount);

        var record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, 100, fxToken.TreasuryAccount);
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Associations);
        Assert.Empty(record.Royalties);
        await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 100);

        await fxToken.Client.UpdateRoyaltiesAsync(fxToken, new TokenRoyalty[] { new TokenRoyalty(fxComAccount, 1, 2, 1, 100) }, fxToken.RoyaltiesPrivateKey);

        record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken, fxAccount1, fxAccount2, 100, fxAccount1);
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Associations);
        Assert.Single(record.Royalties);

        var royalty = record.Royalties[0];
        Assert.Equal(fxToken.Record.Token, royalty.Token);
        Assert.Equal(50U, royalty.Amount);
        Assert.Equal(fxComAccount.Record.Address, royalty.Receiver);
        Assert.Contains(fxAccount2.Record.Address, royalty.Payers);

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

    [Fact(DisplayName = "Royalty Transfers: Fixed Royalties Appear in Transaction Record")]
    async Task FixedRoyaltiesAppearInTransactionRecord()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxComAccount = await TestAccount.CreateAsync(_network);
        await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2, fxComAccount);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);

        await fxToken.Client.TransferTokensAsync(fxComToken, fxComToken.TreasuryAccount, fxAccount1, 100, fxComToken.TreasuryAccount);
        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, 100, fxToken.TreasuryAccount);

        await fxToken.Client.UpdateRoyaltiesAsync(fxToken, new FixedRoyalty[] { new FixedRoyalty(fxComAccount, fxComToken, 50) }, new Signatory(fxToken.RoyaltiesPrivateKey, fxComAccount.PrivateKey));

        var record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken, fxAccount1, fxAccount2, 100, fxAccount1);
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Associations);
        Assert.Single(record.Royalties);

        var royalty = record.Royalties[0];
        Assert.Equal(fxComToken.Record.Token, royalty.Token);
        Assert.Equal(50U, royalty.Amount);
        Assert.Equal(fxComAccount.Record.Address, royalty.Receiver);
        Assert.Contains(fxAccount1.Record.Address, royalty.Payers);

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
    [Fact(DisplayName = "Royalty Transfers: Treasury Exempted from Nested Royalties")]
    async Task TreasuryExemptedFromNestedRoyalties()
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

        await fxToken.Client.UpdateRoyaltiesAsync(fxToken, new FixedRoyalty[] { new FixedRoyalty(fxComAccount, fxComToken1, 50) }, new Signatory(fxToken.RoyaltiesPrivateKey, fxComAccount.PrivateKey));
        await fxToken.Client.UpdateRoyaltiesAsync(fxComToken1, new FixedRoyalty[] { new FixedRoyalty(fxComAccount, fxComToken2, 25) }, new Signatory(fxComToken1.RoyaltiesPrivateKey, fxComAccount.PrivateKey));

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
        Assert.Empty(record.Royalties);
        Assert.Empty(record.Associations);

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
    [Fact(DisplayName = "Royalty Transfers: Transferring Asset Applies Fixed Commision With Updated Fee Structure")]
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

        await fxAsset.Client.UpdateRoyaltiesAsync(fxAsset, new FixedRoyalty[] { new FixedRoyalty(fxComAccount, fxComToken, 100) }, fxAsset.RoyaltiesPrivateKey);

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
    [Fact(DisplayName = "Royalty Transfers: Can Schedule Asset Trnasfer with Fixed Commision")]
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

        await fxAsset.Client.UpdateRoyaltiesAsync(fxAsset, new FixedRoyalty[] { new FixedRoyalty(fxComAccount, fxComToken, 100) }, fxAsset.RoyaltiesPrivateKey);

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
    [Fact(DisplayName = "Royalty Transfers: Transferring Multiple Assets Applies Fixed Commision For Each Asset Transferred")]
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

        await fxAsset.Client.UpdateRoyaltiesAsync(fxAsset, new FixedRoyalty[] { new FixedRoyalty(fxComAccount, fxComToken, 10) }, fxAsset.RoyaltiesPrivateKey);

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
    [Fact(DisplayName = "Royalty Transfers: Treasury Does Pay Second Degree Comission")]
    async Task TreasuryDoesPaySecondDegreeComission()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxComAccount = await TestAccount.CreateAsync(_network);
        await using var fxComToken1 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount, fxAccount2, fxComAccount);
        await using var fxComToken2 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount, fxAccount2, fxComAccount);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount, fxAccount2);

        await fxToken.Client.UpdateRoyaltiesAsync(fxToken, new FixedRoyalty[] { new FixedRoyalty(fxComAccount, fxComToken1, 50) }, new Signatory(fxToken.RoyaltiesPrivateKey, fxComAccount.PrivateKey));
        await fxToken.Client.UpdateRoyaltiesAsync(fxComToken1, new FixedRoyalty[] { new FixedRoyalty(fxComAccount, fxComToken2, 25) }, new Signatory(fxComToken1.RoyaltiesPrivateKey, fxComAccount.PrivateKey));

        await fxToken.Client.TransferTokensAsync(fxComToken1, fxComToken1.TreasuryAccount, fxAccount, 100, fxComToken1.TreasuryAccount);
        await fxToken.Client.TransferTokensAsync(fxComToken2, fxComToken2.TreasuryAccount, fxAccount, 100, fxComToken2.TreasuryAccount);
        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, 100, fxToken.TreasuryAccount);

        var record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken, fxAccount, fxAccount2, 100, fxAccount);
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Associations);

        Assert.Equal(2, record.Royalties.Count);
        AssertHg.ContainsRoyalty(fxComToken1, fxAccount, fxComAccount, 50, record.Royalties);
        AssertHg.ContainsRoyalty(fxComToken2, fxAccount, fxComAccount, 25, record.Royalties);

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
    [Fact(DisplayName = "Royalty Transfers: Three Fixed Royalties Plus Royalty When hBar Value Exchanged")]
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
            fx.Params.Royalties = new IRoyalty[]
            {
                    new AssetRoyalty(fxBenefactor4, 1, 5, 50, fxGasToken),
                    new FixedRoyalty(fxBenefactor1, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor2, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor3, fxGasToken, 40),
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
            CryptoTransfers = new Dictionary<AddressOrAlias, long> {
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
        Assert.Empty(record.Associations);
        Assert.Equal(4, record.Royalties.Count);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor1, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor2, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor3, 40, record.Royalties);
        AssertHg.ContainsHbarRoyalty(fxSeller, fxBenefactor4, 1_00_000_000, record.Royalties);
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
    [Fact(DisplayName = "Royalty Transfers: Three Fixed Royalties Plus Royalty When Fungible Token Value Exchanged")]
    public async Task ThreeFixedRoyaltiesPlusRoyaltyWhenFungibleTokenValueExchanged()
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
            fx.Params.Royalties = new IRoyalty[]
            {
                    new AssetRoyalty(fxBenefactor4, 1, 5, 50, fxGasToken),
                    new FixedRoyalty(fxBenefactor1, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor2, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor3, fxGasToken, 40),
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
        Assert.Empty(record.Associations);
        Assert.Equal(4, record.Royalties.Count);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor1, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor2, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor3, 40, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor4, 2_00, record.Royalties);

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
    [Fact(DisplayName = "Royalty Transfers: Three Fixed Royalties Plus Royalty When Alternate Fungible Token Value Exchanged")]
    public async Task ThreeFixedRoyaltiesPlusRoyaltyWhenAlternateFungibleTokenValueExchanged()
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
            fx.Params.Royalties = new IRoyalty[]
            {
                    new AssetRoyalty(fxBenefactor4, 1, 5, 50, fxGasToken),
                    new FixedRoyalty(fxBenefactor1, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor2, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor3, fxGasToken, 40),
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
        Assert.Empty(record.Associations);
        Assert.Equal(4, record.Royalties.Count);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor1, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor2, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor3, 40, record.Royalties);
        AssertHg.ContainsRoyalty(fxPaymentToken, fxSeller, fxBenefactor4, 2_00, record.Royalties);

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
    [Fact(DisplayName = "Royalty Transfers: Three Fixed Royalties Plus Royalty When No Value Exchanged")]
    public async Task ThreeFixedRoyaltiesPlusRoyaltyWhenNoValueExchanged()
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
            fx.Params.Royalties = new IRoyalty[]
            {
                    new FixedRoyalty(fxBenefactor1, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor2, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor3, fxGasToken, 40),
                    new AssetRoyalty(fxBenefactor4, 1, 5, 50, fxGasToken),
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
        Assert.Empty(record.Associations);
        Assert.Equal(4, record.Royalties.Count);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor1, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor2, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor3, 40, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxBuyer, fxBenefactor4, 50, record.Royalties);

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
    [Fact(DisplayName = "Royalty Transfers: Three Fixed Royalties Plus Royalty When hBar Given in Addition")]
    public async Task ThreeFixedRoyaltiesPlusRoyaltyWhenhBarGiveninAddition()
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
            fx.Params.Royalties = new IRoyalty[]
            {
                    new AssetRoyalty(fxBenefactor4, 1, 5, 50, fxGasToken),
                    new FixedRoyalty(fxBenefactor1, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor2, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor3, fxGasToken, 40),
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
            CryptoTransfers = new Dictionary<AddressOrAlias, long> {
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
        Assert.Empty(record.Associations);
        Assert.Equal(4, record.Royalties.Count);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor1, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor2, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor3, 40, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxBuyer, fxBenefactor4, 50, record.Royalties);
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
    [Fact(DisplayName = "Royalty Transfers: Three Fixed Royalties Plus Royalty with Bystander Wallet")]
    public async Task ThreeFixedRoyaltiesPlusRoyaltyWithBystanderWallet()
    {
        await using var fxBuyer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
        await using var fxSeller = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await using var fxBystander = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
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
            fx.Params.Royalties = new IRoyalty[]
            {
                    new AssetRoyalty(fxBenefactor4, 1, 5, 50, fxGasToken),
                    new FixedRoyalty(fxBenefactor1, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor2, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor3, fxGasToken, 40),
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
        await AssertHg.AssetBalanceAsync(fxAsset, fxBystander, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 100_00);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 100_00);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBystander, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxAsset.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 200_00);

        await AssertHg.CryptoBalanceAsync(fxBuyer, 10_00_000_000);
        await AssertHg.CryptoBalanceAsync(fxSeller, 0);
        await AssertHg.CryptoBalanceAsync(fxBystander, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor4, 0);

        var record = await fxAsset.Client.TransferWithRecordAsync(new TransferParams
        {
            AssetTransfers = new[] {
                    new AssetTransfer(movedAsset, fxSeller, fxBuyer)
                },
            CryptoTransfers = new Dictionary<AddressOrAlias, long> {
                    { fxBuyer.Record.Address, -5_00_000_000 },
                    { fxBystander.Record.Address, 5_00_000_000 }
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
        Assert.Empty(record.Associations);
        Assert.Equal(4, record.Royalties.Count);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor1, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor2, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor3, 40, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxBuyer, fxBenefactor4, 50, record.Royalties);
        Assert.Equal(5_00_000_000L, record.Transfers[fxBystander]);
        Assert.Equal(-5_00_000_000L, record.Transfers[fxBuyer]);

        await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBystander, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor4, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 99_50);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 99_20);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBystander, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 20);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 20);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 40);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor4, 50);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxAsset.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 200_00);

        await AssertHg.CryptoBalanceAsync(fxBuyer, 5_00_000_000);
        await AssertHg.CryptoBalanceAsync(fxSeller, 0);
        await AssertHg.CryptoBalanceAsync(fxBystander, 5_00_000_000L);
        await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor4, 0);
    }
    [Fact(DisplayName = "Royalty Transfers: Social Token with Three Fixed Royalties Can Be Exchanged")]
    public async Task SocialTokenWithThreeFixedRoyaltiesCanBeExchanged()
    {
        await using var fxSender = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await using var fxReceiver = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await using var fxBenefactor1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await using var fxBenefactor2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await using var fxBenefactor3 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await using var fxGasToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.Decimals = 2;
            fx.Params.Circulation = 1_000_00;
        }, fxBenefactor1, fxBenefactor2, fxBenefactor3, fxSender, fxReceiver);
        await using var fxSocialToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.Decimals = 2;
            fx.Params.Circulation = 1_000_00;
            fx.Params.Royalties = new IRoyalty[]
            {
                    new FixedRoyalty(fxBenefactor1, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor2, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor3, fxGasToken, 40),
            };
            fx.Params.GrantKycEndorsement = null;
        }, fxSender, fxReceiver);

        await fxGasToken.Client.TransferTokensAsync(fxGasToken, fxGasToken.TreasuryAccount, fxReceiver, 100_00, fxGasToken.TreasuryAccount);
        await fxGasToken.Client.TransferTokensAsync(fxGasToken, fxGasToken.TreasuryAccount, fxSender, 100_00, fxGasToken.TreasuryAccount);
        await fxGasToken.Client.TransferTokensAsync(fxSocialToken, fxSocialToken.TreasuryAccount, fxSender, 500_00, fxSocialToken.TreasuryAccount);

        await AssertHg.TokenBalanceAsync(fxSocialToken, fxSender, 500_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxReceiver, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor1, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor2, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor3, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxSocialToken.TreasuryAccount, fxSocialToken.Params.Circulation - 500_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxGasToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxGasToken, fxSender, 100_00);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxReceiver, 100_00);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxSocialToken.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 200_00);

        await AssertHg.CryptoBalanceAsync(fxSender, 0);
        await AssertHg.CryptoBalanceAsync(fxReceiver, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);

        var record = await fxSocialToken.Client.TransferWithRecordAsync(new TransferParams
        {
            TokenTransfers = new[] {
                    new TokenTransfer(fxSocialToken, fxSender, -200_00),
                    new TokenTransfer(fxSocialToken, fxReceiver, 200_00),
                },
            Signatory = new Signatory(fxSender, fxReceiver)
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
        Assert.Contains(new TokenTransfer(fxSocialToken, fxSender, -200_00), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxSocialToken, fxReceiver, 200_00), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxGasToken, fxSender, -80), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor1, 20), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor2, 20), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor3, 40), record.TokenTransfers);
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Associations);
        Assert.Equal(3, record.Royalties.Count);
        AssertHg.ContainsRoyalty(fxGasToken, fxSender, fxBenefactor1, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSender, fxBenefactor2, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSender, fxBenefactor3, 40, record.Royalties);

        await AssertHg.TokenBalanceAsync(fxSocialToken, fxSender, 300_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxReceiver, 200_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor1, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor2, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor3, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxSocialToken.TreasuryAccount, fxSocialToken.Params.Circulation - 500_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxGasToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxGasToken, fxSender, 99_20);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxReceiver, 100_00);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 20);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 20);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 40);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxSocialToken.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 200_00);

        await AssertHg.CryptoBalanceAsync(fxSender, 0);
        await AssertHg.CryptoBalanceAsync(fxReceiver, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);

        record = await fxSocialToken.Client.TransferWithRecordAsync(new TransferParams
        {
            TokenTransfers = new[] {
                    new TokenTransfer(fxSocialToken, fxSender, -100_00),
                    new TokenTransfer(fxSocialToken, fxReceiver, 100_00),
                },
            Signatory = new Signatory(fxSender, fxReceiver)
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
        Assert.Contains(new TokenTransfer(fxSocialToken, fxSender, -100_00), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxSocialToken, fxReceiver, 100_00), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxGasToken, fxSender, -80), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor1, 20), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor2, 20), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor3, 40), record.TokenTransfers);
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Associations);
        Assert.Equal(3, record.Royalties.Count);
        AssertHg.ContainsRoyalty(fxGasToken, fxSender, fxBenefactor1, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSender, fxBenefactor2, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSender, fxBenefactor3, 40, record.Royalties);

        await AssertHg.TokenBalanceAsync(fxSocialToken, fxSender, 200_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxReceiver, 300_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor1, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor2, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor3, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxSocialToken.TreasuryAccount, fxSocialToken.Params.Circulation - 500_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxGasToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxGasToken, fxSender, 98_40);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxReceiver, 100_00);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 40);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 40);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 80);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxSocialToken.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 200_00);

        await AssertHg.CryptoBalanceAsync(fxSender, 0);
        await AssertHg.CryptoBalanceAsync(fxReceiver, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
    }
    [Fact(DisplayName = "Royalty Transfers: Asset can be Exchanged for Social Token with Three Fixed Royalties")]
    public async Task AssetCanBeExchangedForSocialTokenWithThreeFixedRoyalties()
    {
        await using var fxSeller = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await using var fxBuyer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await using var fxBenefactor1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await using var fxBenefactor2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await using var fxBenefactor3 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await using var fxBenefactor4 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await using var fxGasToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.Decimals = 2;
            fx.Params.Circulation = 1_000_00;
        }, fxBenefactor1, fxBenefactor2, fxBenefactor3, fxBenefactor4, fxSeller, fxBuyer);
        await using var fxSocialToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.Decimals = 2;
            fx.Params.Circulation = 1_000_00;
            fx.Params.Royalties = new IRoyalty[]
            {
                    new FixedRoyalty(fxBenefactor1, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor2, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor3, fxGasToken, 40),
            };
            fx.Params.GrantKycEndorsement = null;
        }, fxBenefactor4, fxSeller, fxBuyer);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new IRoyalty[]
            {
                    new FixedRoyalty(fxBenefactor1, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor2, fxGasToken, 20),
                    new FixedRoyalty(fxBenefactor3, fxGasToken, 40),
                    new AssetRoyalty(fxBenefactor4, 15, 100, 10_00, fxGasToken),
            };
            fx.Params.GrantKycEndorsement = null;
        }, fxSeller, fxBuyer);

        var movedAsset = new Asset(fxAsset, 1);

        await fxGasToken.Client.TransferTokensAsync(fxGasToken, fxGasToken.TreasuryAccount, fxBuyer, 100_00, fxGasToken.TreasuryAccount);
        await fxGasToken.Client.TransferTokensAsync(fxGasToken, fxGasToken.TreasuryAccount, fxSeller, 100_00, fxGasToken.TreasuryAccount);
        await fxGasToken.Client.TransferTokensAsync(fxSocialToken, fxSocialToken.TreasuryAccount, fxBuyer, 500_00, fxSocialToken.TreasuryAccount);
        await fxGasToken.Client.TransferAssetAsync(movedAsset, fxAsset.TreasuryAccount, fxSeller, fxAsset.TreasuryAccount);

        await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor4, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxSocialToken, fxSeller, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBuyer, 500_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor1, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor2, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor3, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor4, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxSocialToken.TreasuryAccount, fxSocialToken.Params.Circulation - 500_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxGasToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 100_00);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 100_00);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor4, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxSocialToken.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 200_00);

        await AssertHg.CryptoBalanceAsync(fxSeller, 0);
        await AssertHg.CryptoBalanceAsync(fxBuyer, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor4, 0);

        var record = await fxSocialToken.Client.TransferWithRecordAsync(new TransferParams
        {
            AssetTransfers = new[] {
                    new AssetTransfer(movedAsset, fxSeller, fxBuyer)
                },
            TokenTransfers = new[] {
                    new TokenTransfer(fxSocialToken, fxBuyer, -200_00),
                    new TokenTransfer(fxSocialToken, fxSeller, 200_00),
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
        Assert.Equal(8, record.TokenTransfers.Count);
        Assert.Contains(new TokenTransfer(fxSocialToken, fxBuyer, -200_00), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxSocialToken, fxSeller, 170_00), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxSocialToken, fxBenefactor4, 30_00), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxGasToken, fxSeller, -80), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxGasToken, fxSeller, -80), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor1, 40), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor2, 40), record.TokenTransfers);
        Assert.Contains(new TokenTransfer(fxGasToken, fxBenefactor3, 80), record.TokenTransfers);
        Assert.Single(record.AssetTransfers);
        Assert.Contains(new AssetTransfer(movedAsset, fxSeller, fxBuyer), record.AssetTransfers);
        Assert.Empty(record.Associations);
        Assert.Equal(7, record.Royalties.Count);
        // Note: This behavior does not match the intent of the protobuf, it will probably
        // change, right now it is being reported in detail, but the protobuf allows for the
        // payer to be listed as a repeating element, so it _could_ be reported in aggregate.
        // But for now the network is returning detailed results.
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor1, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor2, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxSeller, fxBenefactor3, 40, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxBuyer, fxBenefactor1, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxBuyer, fxBenefactor2, 20, record.Royalties);
        AssertHg.ContainsRoyalty(fxGasToken, fxBuyer, fxBenefactor3, 40, record.Royalties);
        AssertHg.ContainsRoyalty(fxSocialToken, fxSeller, fxBenefactor4, 30_00, record.Royalties);

        await AssertHg.AssetBalanceAsync(fxAsset, fxBuyer, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxSeller, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor1, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor2, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor3, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxBenefactor4, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, fxAsset.Metadata.Length - 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxGasToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxSocialToken, fxSeller, 170_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBuyer, 300_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor1, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor2, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor3, 0);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxBenefactor4, 30_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxSocialToken.TreasuryAccount, fxSocialToken.Params.Circulation - 500_00);
        await AssertHg.TokenBalanceAsync(fxSocialToken, fxGasToken.TreasuryAccount, 0);

        await AssertHg.TokenBalanceAsync(fxGasToken, fxSeller, 99_20);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBuyer, 99_20);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor1, 40);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor2, 40);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor3, 80);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxBenefactor4, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxSocialToken.TreasuryAccount, 0);
        await AssertHg.TokenBalanceAsync(fxGasToken, fxGasToken.TreasuryAccount, fxGasToken.Params.Circulation - 200_00);

        await AssertHg.CryptoBalanceAsync(fxSeller, 0);
        await AssertHg.CryptoBalanceAsync(fxBuyer, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor1, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor2, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor3, 0);
        await AssertHg.CryptoBalanceAsync(fxBenefactor4, 0);
    }
}