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

        [Fact(DisplayName = "NETWORK BUG: Commissions: Transferring Token Applies Fixed Commision")]
        public async Task TransferringTokenAppliesFixedCommisionNetworkBug()
        {
            var testFailException = (await Assert.ThrowsAsync<TransactionException>(TransferringTokenAppliesFixedCommision));
            Assert.StartsWith("Unable to execute transfers, status: TokenNotAssociatedToAccount", testFailException.Message);

            //[Fact(DisplayName = "Commissions: Transferring Token Applies Fixed Commision")]
            async Task TransferringTokenAppliesFixedCommision()
            {
                await using var fxAccount1 = await TestAccount.CreateAsync(_network);
                await using var fxAccount2 = await TestAccount.CreateAsync(_network);
                await using var fxAccount3 = await TestAccount.CreateAsync(_network);
                await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2, fxAccount3);
                await using var fxToken = await TestToken.CreateAsync(_network, fx =>
                {
                    fx.Params.FixedCommissions = new FixedCommission[]
                    {
                    new FixedCommission(fxAccount1, fxComToken, 100)
                    };
                    fx.Params.GrantKycEndorsement = null;
                }, fxAccount1, fxAccount2, fxAccount3);
                Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

                // Note: this should not be necessary
                await fxToken.Client.AssociateTokenAsync(fxComToken, fxToken.TreasuryAccount, fxToken.TreasuryAccount);

                await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount1);
                await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount2);
                await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount3);

                await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount1);
                await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount2);
                await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount3);

                await AssertHg.TokenIsAssociatedAsync(fxComToken, fxToken.TreasuryAccount);

                await fxComToken.Client.TransferTokensAsync(fxComToken, fxComToken.TreasuryAccount, fxAccount2, 100, fxComToken.TreasuryAccount);
                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount2, 100, fxToken.TreasuryAccount);

                await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 100);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount3, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

                await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 0);
                await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 100);
                await AssertHg.TokenBalanceAsync(fxComToken, fxAccount3, 0);
                await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);

                await fxToken.Client.TransferTokensAsync(fxToken, fxAccount2, fxAccount3, 100, fxAccount2);

                await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount3, 100);
                await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

                await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 100);
                await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
                await AssertHg.TokenBalanceAsync(fxComToken, fxAccount3, 0);
                await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);
            }
        }

        [Fact(DisplayName = "NETWORK BUG: Commissions: Transferring Token Applies Fixed Commision With Updated Fee Structure")]
        public async Task TransferringTokenAppliesFixedCommisionWithUpdatedFeeStructureNetworkBug()
        {
            var testFailException = (await Assert.ThrowsAsync<TransactionException>(TransferringTokenAppliesFixedCommisionWithUpdatedFeeStructure));
            Assert.StartsWith("Unable to execute transfers, status: TokenNotAssociatedToAccount", testFailException.Message);

            //[Fact(DisplayName = "Commissions: Transferring Token Applies Fixed Commision With Updated Fee Structure")]
            async Task TransferringTokenAppliesFixedCommisionWithUpdatedFeeStructure()
            {
                await using var fxAccount1 = await TestAccount.CreateAsync(_network);
                await using var fxAccount2 = await TestAccount.CreateAsync(_network);
                await using var fxAccount3 = await TestAccount.CreateAsync(_network);
                await using var fxComToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2, fxAccount3);
                await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2, fxAccount3);
                Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

                await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount1);
                await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount2);
                await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount3);

                await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount1);
                await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount2);
                await AssertHg.TokenIsAssociatedAsync(fxComToken, fxAccount3);

                await fxComToken.Client.TransferTokensAsync(fxComToken, fxComToken.TreasuryAccount, fxAccount2, 100, fxComToken.TreasuryAccount);
                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount2, 100, fxToken.TreasuryAccount);

                await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 100);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount3, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

                await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 0);
                await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 100);
                await AssertHg.TokenBalanceAsync(fxComToken, fxAccount3, 0);
                await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);

                await fxToken.Client.UpdateCommissionsAsync(fxToken, new FixedCommission[] { new FixedCommission(fxAccount1, fxComToken, 100) }, null, fxToken.CommissionsPrivateKey);

                await fxToken.Client.TransferTokensAsync(fxToken, fxAccount2, fxAccount3, 100, fxAccount2);

                await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount3, 100);
                await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

                await AssertHg.TokenBalanceAsync(fxComToken, fxAccount1, 100);
                await AssertHg.TokenBalanceAsync(fxComToken, fxAccount2, 0);
                await AssertHg.TokenBalanceAsync(fxComToken, fxAccount3, 0);
                await AssertHg.TokenBalanceAsync(fxComToken, fxComToken.TreasuryAccount, fxComToken.Params.Circulation - 100);
            }
        }
        [Fact(DisplayName = "NETWORK BUG: Commissions: Transferring Token Applies Fixed Commision")]
        public async Task TransferringTokenAppliesVariableCommisionNetworkBug()
        {
            var testFailException = (await Assert.ThrowsAsync<TransactionException>(TransferringTokenAppliesVariableCommision));
            Assert.StartsWith("Unable to execute transfers, status: TokenNotAssociatedToAccount", testFailException.Message);

            //[Fact(DisplayName = "Commissions: Transferring Token Applies Fixed Commision")]
            async Task TransferringTokenAppliesVariableCommision()
            {
                await using var fxAccount1 = await TestAccount.CreateAsync(_network);
                await using var fxAccount2 = await TestAccount.CreateAsync(_network);
                await using var fxAccount3 = await TestAccount.CreateAsync(_network);
                await using var fxToken = await TestToken.CreateAsync(_network, fx =>
                {
                    fx.Params.VariableCommissions = new VariableCommission[]
                    {
                    new VariableCommission(fxAccount1,1,2,1,100)
                    };
                    fx.Params.GrantKycEndorsement = null;
                    fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxAccount1.PrivateKey);
                }, fxAccount2, fxAccount3);
                Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

                await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount1);
                await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount2);
                await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount3);

                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount2, 100, fxToken.TreasuryAccount);

                await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 100);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount3, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

                await fxToken.Client.TransferTokensAsync(fxToken, fxAccount2, fxAccount3, 100, fxAccount2);

                await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 50);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount3, 50);
                await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);
            }
        }

        [Fact(DisplayName = "NETWORK BUG: Commissions: Transferring Token Applies Immutable Fixed Commision")]
        public async Task TransferringTokenAppliesImmutableFixedCommisionNetworkBug()
        {
            var testFailException = (await Assert.ThrowsAsync<TransactionException>(TransferringTokenAppliesImmutableFixedCommision));
            Assert.StartsWith("Unable to execute transfers, status: TokenNotAssociatedToAccount", testFailException.Message);

            //[Fact(DisplayName = "Commissions: Transferring Token Applies Immutable Fixed Commision")]
            async Task TransferringTokenAppliesImmutableFixedCommision()
            {
                await using var fxAccount1 = await TestAccount.CreateAsync(_network);
                await using var fxAccount2 = await TestAccount.CreateAsync(_network);
                await using var fxAccount3 = await TestAccount.CreateAsync(_network);
                await using var fxToken = await TestToken.CreateAsync(_network, fx =>
                {
                    fx.Params.VariableCommissions = new VariableCommission[]
                    {
                    new VariableCommission(fxAccount1,1,2,1,100)
                    };
                    fx.Params.GrantKycEndorsement = null;
                    fx.Params.CommissionsEndorsement = null;
                    fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxAccount1.PrivateKey);
                }, fxAccount2, fxAccount3);
                Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

                await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount1);
                await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount2);
                await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount3);

                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount2, 100, fxToken.TreasuryAccount);

                await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 50);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 50);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount3, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);

                await fxToken.Client.TransferTokensAsync(fxToken, fxAccount2, fxAccount3, 50, fxAccount2);

                await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 75);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount3, 25);
                await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 100);
            }
        }

        [Fact(DisplayName = "NETWORK BUG: Commissions: Can Apply a Simple Imutable Token With Commissions")]
        public async Task TransferringTokenAppliesFixedCommisionToTreasuryNetworkBug()
        {
            var testFailException = (await Assert.ThrowsAsync<TransactionException>(TransferringTokenAppliesFixedCommisionToTreasury));
            Assert.StartsWith("Unable to execute transfers, status: TokenNotAssociatedToAccount", testFailException.Message);

            //[Fact(DisplayName = "Commissions: Transferring Token Applies Fixed Commision To Treasury")]
            async Task TransferringTokenAppliesFixedCommisionToTreasury()
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

                await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 50);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 50);

                await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, 50, fxAccount1);

                await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 0);
                await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 25);
                await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - 25);
            }
        }

        [Fact(DisplayName = "NETWORK BUG: Commissions: Can Apply a Simple Imutable Token With Commissions")]
        public async Task CanApplyASimpleImutableTokenWithCommissionsNetworkBug()
        {
            var testFailException = (await Assert.ThrowsAsync<TransactionException>(CanApplyASimpleImutableTokenWithCommissions));
            Assert.StartsWith("Unable to execute transfers, status: TokenNotAssociatedToAccount", testFailException.Message);

            //[Fact(DisplayName = "Commissions: Can Apply a Simple Imutable Token With Commissions")]
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

                await fxTreasury.Client.TransferTokensAsync(token, fxTreasury, fxAccount1, 100, fxTreasury);

                Assert.Equal(50ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount1, token));
                Assert.Equal(0ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount2, token));
                Assert.Equal(50ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxCollector, token));
                Assert.Equal(900ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxTreasury, token));

                await fxTreasury.Client.TransferTokensAsync(token, fxAccount1, fxAccount2, 50, fxAccount1);

                Assert.Equal(0ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount1, token));
                Assert.Equal(25ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount2, token));
                Assert.Equal(75ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxCollector, token));
                Assert.Equal(900ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxTreasury, token));
            }
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
            Assert.Equal(50ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxCollector, token));
            Assert.Equal(850ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxTreasury, token));

            await fxTreasury.Client.TransferTokensAsync(token, fxAccount1, fxAccount2, 66, ctx =>
            {
                ctx.Payer = fxAccount1;
                ctx.Signatory = fxAccount1;
            });

            Assert.Equal(1ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount1, token));
            Assert.Equal(66ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxAccount2, token));
            Assert.Equal(83ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxCollector, token));
            Assert.Equal(850ul, await fxTreasury.Client.GetAccountTokenBalanceAsync(fxTreasury, token));
        }
    }
}
