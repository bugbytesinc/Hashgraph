using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentials))]
    public class GetInfoTests
    {
        private readonly NetworkCredentials _network;
        public GetInfoTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Get Account Info: Can Get Info for Account")]
        public async Task CanGetInfoForAccountAsync()
        {
            await using var client = _network.NewClient();
            var account = _network.Payer;
            var info = await client.GetAccountInfoAsync(account);
            Assert.NotNull(info.Address);
            Assert.Equal(account.RealmNum, info.Address.RealmNum);
            Assert.Equal(account.ShardNum, info.Address.ShardNum);
            Assert.Equal(account.AccountNum, info.Address.AccountNum);
            Assert.NotNull(info.SmartContractId);
            Assert.False(info.Deleted);
            Assert.NotNull(info.Proxy);
            Assert.Equal(new Address(0, 0, 0), info.Proxy);
            Assert.Equal(0, info.ProxiedToAccount);
            Assert.Equal(new Endorsement(_network.PublicKey), info.Endorsement);
            Assert.True(info.Balance > 0);
            Assert.False(info.ReceiveSignatureRequired);
            Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
            Assert.True(info.Expiration > DateTime.MinValue);
        }
        [Fact(DisplayName = "Get Account Info: Can Get Info for Account Facet")]
        public async Task CanGetInfoForAccountFacet()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
            Assert.Equal(fxAccount.Record.Address, info.Address);
            Assert.NotNull(info.SmartContractId);
            Assert.False(info.Deleted);
            Assert.NotNull(info.Proxy);
            Assert.Equal(Address.None, info.Proxy);
            Assert.Equal(0, info.ProxiedToAccount);
            Assert.Equal(fxAccount.PublicKey, info.Endorsement);
            Assert.Equal(fxAccount.CreateParams.InitialBalance, info.Balance);
            Assert.Equal(fxAccount.CreateParams.RequireReceiveSignature, info.ReceiveSignatureRequired);
            Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
            Assert.True(info.Expiration > DateTime.MinValue);
            Assert.Equal(fxAccount.CreateParams.Memo, info.Memo);
        }
        [Fact(DisplayName = "Get Account Info: Can Get Info for Server Node")]
        public async Task CanGetInfoForGatewayAsync()
        {
            await using var client = _network.NewClient();
            var account = _network.Gateway;
            var info = await client.GetAccountInfoAsync(account);
            Assert.NotNull(info.Address);
            Assert.Equal(account.ShardNum, info.Address.ShardNum);
            Assert.Equal(account.RealmNum, info.Address.RealmNum);
            Assert.Equal(account.AccountNum, info.Address.AccountNum);
            Assert.NotNull(info.SmartContractId);
            Assert.False(info.Deleted);
            Assert.NotNull(info.Proxy);
            Assert.Equal(new Address(0, 0, 0), info.Proxy);
            Assert.True(info.ProxiedToAccount > -1);
            Assert.True(info.Balance > 0);
            Assert.False(info.ReceiveSignatureRequired);
            Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
            Assert.True(info.Expiration > DateTime.MinValue);
        }
        [Fact(DisplayName = "Get Account Info: Getting Account Info without paying signature fails.")]
        public async Task GetInfoWithoutPayingSignatureThrowsException()
        {
            await using var client = _network.NewClient();
            client.Configure(ctx => ctx.Signatory = null);
            var account = _network.Payer;
            var ioe = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await client.GetAccountInfoAsync(account);
            });
            Assert.StartsWith("The Payer's signatory (signing key/callback) has not been configured. This is required for retreiving records and other general network Queries. Please check that", ioe.Message);
        }
    }
}
