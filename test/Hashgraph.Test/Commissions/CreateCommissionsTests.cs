using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token
{
    [Collection(nameof(NetworkCredentials))]
    public class CreateCommissionsTests
    {
        private readonly NetworkCredentials _network;
        public CreateCommissionsTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Commissions: Can Create Token with Fixed Commission")]
        public async Task CanCreateTokenWithFixedCommission()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var comToken = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.FixedCommissions = new FixedCommission[]
                {
                    new FixedCommission(fxAccount, comToken, 100)
                };
            }, fxAccount);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Single(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);

            Assert.Equal(fxToken.Params.FixedCommissions.First(), info.FixedCommissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Create Token with Variable Commission")]
        public async Task CanCreateTokenWithVariableCommission()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.VariableCommissions = new VariableCommission[]
                {
                    new VariableCommission(fxAccount, 1, 2, 1, 100)
                };
                fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Empty(info.FixedCommissions);
            Assert.Single(info.VariableCommissions);

            Assert.Equal(fxToken.Params.VariableCommissions.First(), info.VariableCommissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Create Token with Fixed and Variable Commissions")]
        public async Task CanCreateTokenWithFixedAndVariableCommissions()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var comToken = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.FixedCommissions = new FixedCommission[]
                {
                    new FixedCommission(fxAccount, comToken, 100)
                };
                fx.Params.VariableCommissions = new VariableCommission[]
                {
                    new VariableCommission(fxAccount, 1, 2, 1, 100)
                };
                fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Single(info.FixedCommissions);
            Assert.Single(info.VariableCommissions);

            Assert.Equal(fxToken.Params.FixedCommissions.First(), info.FixedCommissions[0]);
            Assert.Equal(fxToken.Params.VariableCommissions.First(), info.VariableCommissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Add Fixed Commission to Token Definition")]
        public async Task CanAddFixedCommissionToTokenDefinition()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var comToken = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var fixedCommissions = new FixedCommission[] { new FixedCommission(fxAccount, comToken, 100) };
            var receipt = await fxToken.Client.UpdateCommissionsAsync(fxToken, fixedCommissions, null, fxToken.CommissionsPrivateKey);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Single(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);

            Assert.Equal(fixedCommissions[0], info.FixedCommissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Add Variable Commission to Token Definition")]
        public async Task CanAddVariableCommissionToTokenDefinition()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var variableCommissions = new VariableCommission[] { new VariableCommission(fxAccount, 1, 2, 1, 100) };
            var receipt = await fxToken.Client.UpdateCommissionsAsync(fxToken, null, variableCommissions, fxToken.CommissionsPrivateKey);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Empty(info.FixedCommissions);
            Assert.Single(info.VariableCommissions);

            Assert.Equal(variableCommissions[0], info.VariableCommissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Add Fixed and Variable Commissions to Token Definition")]
        public async Task CanAddFixedAndVariableCommissionsToTokenDefinition()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var comToken = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var fixedCommissions = new FixedCommission[] { new FixedCommission(fxAccount, comToken, 100) };
            var variableCommissions = new VariableCommission[] { new VariableCommission(fxAccount, 1, 2, 1, 100) };
            var receipt = await fxToken.Client.UpdateCommissionsAsync(fxToken, fixedCommissions, variableCommissions, fxToken.CommissionsPrivateKey);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Single(info.FixedCommissions);
            Assert.Single(info.VariableCommissions);

            Assert.Equal(fixedCommissions[0], info.FixedCommissions[0]);
            Assert.Equal(variableCommissions[0], info.VariableCommissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Create Asset with Fixed Commission")]
        public async Task CanCreateAssetWithFixedCommission()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var comAsset = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.FixedCommissions = new FixedCommission[]
                {
                    new FixedCommission(fxAccount, comAsset, 100)
                };
            }, fxAccount);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Single(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);

            Assert.Equal(fxAsset.Params.FixedCommissions.First(), info.FixedCommissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Add Fixed Commission to Asset Definition")]
        public async Task CanAddFixedCommissionToAssetDefinition()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var comAsset = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

            var fixedCommissions = new FixedCommission[] { new FixedCommission(fxAccount, comAsset, 100) };
            var receipt = await fxAsset.Client.UpdateCommissionsAsync(fxAsset, fixedCommissions, null, fxAsset.CommissionsPrivateKey);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Single(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);

            Assert.Equal(fixedCommissions[0], info.FixedCommissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Not Add Variable Commission to Asset Definition")]
        public async Task CanNotAddVariableCommissionToAssetDefinition()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

            var variableCommissions = new VariableCommission[] { new VariableCommission(fxAccount, 1, 2, 1, 100) };

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.UpdateCommissionsAsync(fxAsset, null, variableCommissions, fxAsset.CommissionsPrivateKey);
            });
            Assert.Equal(ResponseCode.CustomFractionalFeeOnlyAllowedForFungibleCommon, tex.Status);
            Assert.StartsWith("Unable to Update Token Transfer Commissions, status: CustomFractionalFeeOnlyAllowedForFungibleCommon", tex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Empty(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);
        }

        [Fact(DisplayName = "Commissions: Transferring Token Applies Fixed Commision")]
        async Task TransferringTokenAppliesFixedCommision()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxComAccount, fxAccount1, fxAccount2);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.FixedCommissions = new FixedCommission[]
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

        [Fact(DisplayName = "Commissions: Transferring Token Applies Fixed Commision With Updated Fee Structure")]
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

            await fxToken.Client.UpdateCommissionsAsync(fxToken, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken, 100) }, null, fxToken.CommissionsPrivateKey);

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
        [Fact(DisplayName = "Commissions: Transferring Token Applies Variable Commision")]
        async Task TransferringTokenAppliesVariableCommision()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.VariableCommissions = new VariableCommission[]
                {
                    new VariableCommission(fxComAccount,1,2,1,100)
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

        [Fact(DisplayName = "Commissions: Transferring Token Applies Immutable Fixed Commision")]
        async Task TransferringTokenAppliesImmutableFixedCommision()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.VariableCommissions = new VariableCommission[]
                {
                    new VariableCommission(fxComAccount,1,2,1,100)
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


        [Fact(DisplayName = "Commissions: Transferring Token From Treasury Does Not Apply Fixed Commision")]
        async Task TransferringTokenFromTreasuryDoesNotApplyFixedCommision()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.VariableCommissions = new VariableCommission[]
                {
                    new VariableCommission(fx.TreasuryAccount,1,2,1,100)
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

        [Fact(DisplayName = "Commissions: Can Apply a Simple Imutable Token With Commissions")]
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
                VariableCommissions = new VariableCommission[]
                {
                    new VariableCommission(fxCollector,1,2,1,100)
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

        [Fact(DisplayName = "Commissions: Commissions Are Applied when Payer Is Same Account as Sender")]
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
                VariableCommissions = new VariableCommission[]
                {
                    new VariableCommission(fxCollector,1,2,1,100)
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

        [Fact(DisplayName = "Commissions: Inssuficient Commission Token Balance Prevents Transfer")]
        async Task InssuficientCommissionTokenBalancePreventsTransfer()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxComAccount, fxAccount1, fxAccount2);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.FixedCommissions = new FixedCommission[]
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
            Assert.Equal(ResponseCode.InsufficientPayerBalanceForCustomFee, tex.Status);
            Assert.StartsWith("Unable to execute transfers, status: InsufficientPayerBalanceForCustomFee", tex.Message);

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

        [Fact(DisplayName = "Commissions: Variable Commissions Appear in Transaction Record")]
        async Task VariableCommissionsAppearInTransactionRecord()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2, fxComAccount);

            var record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, 100, fxToken.TreasuryAccount);
            Assert.Empty(record.AssetTransfers);
            Assert.Empty(record.Commissions);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 100);

            await fxToken.Client.UpdateCommissionsAsync(fxToken, null, new VariableCommission[] { new VariableCommission(fxComAccount, 1, 2, 1, 100) }, fxToken.CommissionsPrivateKey);

            record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken, fxAccount1, fxAccount2, 100, fxAccount1);
            Assert.Empty(record.AssetTransfers);
            Assert.Single(record.Commissions);

            var commission = record.Commissions[0];
            Assert.Equal(fxToken.Record.Token, commission.Token);
            Assert.Equal(50U, commission.Amount);
            Assert.Equal(fxComAccount.Record.Address, commission.Address);

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

        [Fact(DisplayName = "Commissions: Fixed Commissions Appear in Transaction Record")]
        async Task FixedCommissionsAppearInTransactionRecord()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2, fxComAccount);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);

            await fxToken.Client.TransferTokensAsync(fxComToken, fxComToken.TreasuryAccount, fxAccount1, 100, fxComToken.TreasuryAccount);
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, 100, fxToken.TreasuryAccount);

            await fxToken.Client.UpdateCommissionsAsync(fxToken, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken, 50) }, null, new Signatory(fxToken.CommissionsPrivateKey, fxComAccount.PrivateKey));

            var record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken, fxAccount1, fxAccount2, 100, fxAccount1);
            Assert.Empty(record.AssetTransfers);
            Assert.Single(record.Commissions);

            var commission = record.Commissions[0];
            Assert.Equal(fxComToken.Record.Token, commission.Token);
            Assert.Equal(50U, commission.Amount);
            Assert.Equal(fxComAccount.Record.Address, commission.Address);

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
        [Fact(DisplayName = "Commissions: Treasury Exempted from Nested Commissions")]
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

            await fxToken.Client.UpdateCommissionsAsync(fxToken, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken1, 50) }, null, new Signatory(fxToken.CommissionsPrivateKey, fxComAccount.PrivateKey));
            await fxToken.Client.UpdateCommissionsAsync(fxComToken1, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken2, 25) }, null, new Signatory(fxComToken1.CommissionsPrivateKey, fxComAccount.PrivateKey));

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
        [Fact(DisplayName = "Commissions: Transferring Asset Applies Fixed Commision With Updated Fee Structure")]
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

            await fxAsset.Client.UpdateCommissionsAsync(fxAsset, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken, 100) }, null, fxAsset.CommissionsPrivateKey);

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
        [Fact(DisplayName = "Commissions: Can Schedule Asset Trnasfer with Fixed Commision")]
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

            await fxAsset.Client.UpdateCommissionsAsync(fxAsset, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken, 100) }, null, fxAsset.CommissionsPrivateKey);

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
        [Fact(DisplayName = "Commissions: Transferring Multiple Assets Applies Fixed Commision For Each Asset Transferred")]
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

            await fxAsset.Client.UpdateCommissionsAsync(fxAsset, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken, 10) }, null, fxAsset.CommissionsPrivateKey);

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
        [Fact(DisplayName = "Commissions: Treasury Does Pay Second Degree Comission")]
        async Task TreasuryDoesPaySecondDegreeComission()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxComAccount = await TestAccount.CreateAsync(_network);
            await using var fxComToken1 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount, fxAccount2, fxComAccount);
            await using var fxComToken2 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount, fxAccount2, fxComAccount);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount, fxAccount2);

            await fxToken.Client.UpdateCommissionsAsync(fxToken, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken1, 50) }, null, new Signatory(fxToken.CommissionsPrivateKey, fxComAccount.PrivateKey));
            await fxToken.Client.UpdateCommissionsAsync(fxComToken1, new FixedCommission[] { new FixedCommission(fxComAccount, fxComToken2, 25) }, null, new Signatory(fxComToken1.CommissionsPrivateKey, fxComAccount.PrivateKey));

            await fxToken.Client.TransferTokensAsync(fxComToken1, fxComToken1.TreasuryAccount, fxAccount, 100, fxComToken1.TreasuryAccount);
            await fxToken.Client.TransferTokensAsync(fxComToken2, fxComToken2.TreasuryAccount, fxAccount, 100, fxComToken2.TreasuryAccount);
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, 100, fxToken.TreasuryAccount);

            var record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken, fxAccount, fxAccount2, 100, fxAccount);
            Assert.Empty(record.AssetTransfers);
            Assert.Equal(2, record.Commissions.Count);

            var commission = record.Commissions.FirstOrDefault(c => c.Token == fxComToken1.Record.Token);
            Assert.NotNull(commission);
            Assert.Equal(50U, commission.Amount);
            Assert.Equal(fxComAccount.Record.Address, commission.Address);

            commission = record.Commissions.FirstOrDefault(c => c.Token == fxComToken2.Record.Token);
            Assert.NotNull(commission);
            Assert.Equal(25U, commission.Amount);
            Assert.Equal(fxComAccount.Record.Address, commission.Address);

            Assert.Equal(6, record.TokenTransfers.Count);

            var xfer = record.TokenTransfers.FirstOrDefault(x => x.Amount == -100);
            Assert.NotNull(xfer);
            Assert.Equal(fxToken.Record.Token, xfer.Token);
            Assert.Equal(fxAccount.Record.Address, xfer.Address);

            xfer = record.TokenTransfers.FirstOrDefault(x => x.Amount == 100);
            Assert.NotNull(xfer);
            Assert.Equal(fxToken.Record.Token, xfer.Token);
            Assert.Equal(fxAccount2.Record.Address, xfer.Address);

            xfer = record.TokenTransfers.FirstOrDefault(x => x.Amount == -50);
            Assert.NotNull(xfer);
            Assert.Equal(fxComToken1.Record.Token, xfer.Token);
            Assert.Equal(fxAccount.Record.Address, xfer.Address);

            xfer = record.TokenTransfers.FirstOrDefault(x => x.Amount == 50);
            Assert.NotNull(xfer);
            Assert.Equal(fxComToken1.Record.Token, xfer.Token);
            Assert.Equal(fxComAccount.Record.Address, xfer.Address);

            xfer = record.TokenTransfers.FirstOrDefault(x => x.Amount == -25);
            Assert.NotNull(xfer);
            Assert.Equal(fxComToken2.Record.Token, xfer.Token);
            Assert.Equal(fxAccount.Record.Address, xfer.Address);

            xfer = record.TokenTransfers.FirstOrDefault(x => x.Amount == 25);
            Assert.NotNull(xfer);
            Assert.Equal(fxComToken2.Record.Token, xfer.Token);
            Assert.Equal(fxComAccount.Record.Address, xfer.Address);

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
    }
}
