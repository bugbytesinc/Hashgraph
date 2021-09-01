using Hashgraph.Test.Fixtures;
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
                fx.Params.Commissions = new FixedCommission[]
                {
                    new FixedCommission(fxAccount, comToken, 100)
                };
            }, fxAccount);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Single(info.Commissions);

            Assert.Equal(fxToken.Params.Commissions.First(), info.Commissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Create Token with Fractional Commission")]
        public async Task CanCreateTokenWithFractionalCommission()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new FractionalCommission[]
                {
                    new FractionalCommission(fxAccount, 1, 2, 1, 100)
                };
                fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Single(info.Commissions);

            Assert.Equal(fxToken.Params.Commissions.First(), info.Commissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Create Token with Fixed and Fractional Commissions")]
        public async Task CanCreateTokenWithFixedAndFractionalCommissions()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var comToken = await TestToken.CreateAsync(_network, null, fxAccount);
            var fixedCommission = new FixedCommission(fxAccount, comToken, 100);
            var fractionalCommission = new FractionalCommission(fxAccount, 1, 2, 1, 100);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ICommission[] { fixedCommission, fractionalCommission };
                fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(2, info.Commissions.Count);

            Assert.Equal(fixedCommission, info.Commissions.First(f => f.CommissionType == CommissionType.Fixed));
            Assert.Equal(fractionalCommission, info.Commissions.First(f => f.CommissionType == CommissionType.Fractional));
        }
        [Fact(DisplayName = "Commissions: Can Add Fixed Commission to Token Definition")]
        public async Task CanAddFixedCommissionToTokenDefinition()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var comToken = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var fixedCommissions = new FixedCommission[] { new FixedCommission(fxAccount, comToken, 100) };
            var receipt = await fxToken.Client.UpdateCommissionsAsync(fxToken, fixedCommissions, fxToken.CommissionsPrivateKey);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Single(info.Commissions);

            Assert.Equal(fixedCommissions[0], info.Commissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Add Fractional Commission to Token Definition")]
        public async Task CanAddFractionalCommissionToTokenDefinition()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var fractionalCommissions = new FractionalCommission[] { new FractionalCommission(fxAccount, 1, 2, 1, 100) };
            var receipt = await fxToken.Client.UpdateCommissionsAsync(fxToken, fractionalCommissions, fxToken.CommissionsPrivateKey);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Single(info.Commissions);

            Assert.Equal(fractionalCommissions[0], info.Commissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Add Fixed and Fractional Commissions to Token Definition")]
        public async Task CanAddFixedAndFractionalCommissionsToTokenDefinition()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var comToken = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var fixedCommission = new FixedCommission(fxAccount, comToken, 100);
            var fractionalCommission = new FractionalCommission(fxAccount, 1, 2, 1, 100);
            var receipt = await fxToken.Client.UpdateCommissionsAsync(fxToken, new ICommission[] { fixedCommission, fractionalCommission }, fxToken.CommissionsPrivateKey);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(2, info.Commissions.Count);

            Assert.Equal(fixedCommission, info.Commissions.First(f => f.CommissionType == CommissionType.Fixed));
            Assert.Equal(fractionalCommission, info.Commissions.First(f => f.CommissionType == CommissionType.Fractional));
        }
        [Fact(DisplayName = "Commissions: Can Create Asset with Fixed Commission")]
        public async Task CanCreateAssetWithFixedCommission()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var comAsset = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new FixedCommission[]
                {
                    new FixedCommission(fxAccount, comAsset, 100)
                };
            }, fxAccount);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Single(info.Commissions);

            Assert.Equal(fxAsset.Params.Commissions.First(), info.Commissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Create Asset with Value Commission")]
        public async Task CanCreateAssetWithValueCommission()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var comAsset = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Commissions = new ValueCommission[]
                {
                    new ValueCommission(fxAccount, 1, 2, 1, comAsset.Record.Token)
                };
            }, fxAccount);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Single(info.Commissions);

            Assert.Equal(fxAsset.Params.Commissions.First(), info.Commissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Add Fixed Commission to Asset Definition")]
        public async Task CanAddFixedCommissionToAssetDefinition()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var comAsset = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

            var fixedCommissions = new FixedCommission[] { new FixedCommission(fxAccount, comAsset, 100) };
            var receipt = await fxAsset.Client.UpdateCommissionsAsync(fxAsset, fixedCommissions, fxAsset.CommissionsPrivateKey);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Single(info.Commissions);

            Assert.Equal(fixedCommissions[0], info.Commissions[0]);
        }
        [Fact(DisplayName = "Commissions: Can Not Add Fractional Commission to Asset Definition")]
        public async Task CanNotAddFractionalCommissionToAssetDefinition()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

            var fractionalCommissions = new FractionalCommission[] { new FractionalCommission(fxAccount, 1, 2, 1, 100) };

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.UpdateCommissionsAsync(fxAsset, fractionalCommissions, fxAsset.CommissionsPrivateKey);
            });
            Assert.Equal(ResponseCode.CustomFractionalFeeOnlyAllowedForFungibleCommon, tex.Status);
            Assert.StartsWith("Unable to Update Token Transfer Commissions, status: CustomFractionalFeeOnlyAllowedForFungibleCommon", tex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Empty(info.Commissions);
        }
        [Fact(DisplayName = "Commissions: Can Not Create Token with Royalty Commission")]
        public async Task CanNotCreateTokenWithRoyaltyCommission()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await TestToken.CreateAsync(_network, fx =>
                {
                    fx.Params.Commissions = new ValueCommission[]
                    {
                        new ValueCommission(fxAccount, 1, 2, 0, Address.None)
                    };
                    fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxAccount.PrivateKey);
                });
            });
            Assert.Equal(ResponseCode.CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique, tex.Status);
            Assert.StartsWith("Unable to create Token, status: CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique", tex.Message);
        }
        [Fact(DisplayName = "Commissions: Can Not Add Royalty Commission to Token Definition")]
        public async Task CanNotAddRoyaltyCommissionToTokenDefinition()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestToken.CreateAsync(_network, null, fxAccount);

            var valueCommissions = new ValueCommission[] { new ValueCommission(fxAccount, 1, 2, 0, Address.None) };

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.UpdateCommissionsAsync(fxAsset, valueCommissions, fxAsset.CommissionsPrivateKey);
            });
            Assert.Equal(ResponseCode.CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique, tex.Status);
            Assert.StartsWith("Unable to Update Token Transfer Commissions, status: CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique", tex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Empty(info.Commissions);
        }
    }
}
