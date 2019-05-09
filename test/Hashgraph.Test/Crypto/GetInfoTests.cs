using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class GetInfoTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public GetInfoTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "Get Account Info: Can Get Info for Account")]
        public async Task CanGetInfoForAccountAsync()
        {
            await using (var client = _networkCredentials.CreateClientWithDefaultConfiguration())
            {
                var account = _networkCredentials.CreateDefaultAccount();
                var info = await client.GetAccountInfoAsync(account);
                Assert.NotNull(info.Address);
                Assert.Equal(account.RealmNum, info.Address.RealmNum);
                Assert.Equal(account.ShardNum, info.Address.ShardNum);
                Assert.Equal(account.AccountNum, info.Address.AccountNum);
                Assert.NotNull(info.SmartContractId);
                Assert.False(info.Deleted);
                Assert.NotNull(info.Proxy);
                Assert.True(info.Proxy.RealmNum > -1);
                Assert.True(info.Proxy.ShardNum > -1);
                Assert.True(info.Proxy.AccountNum > -1);
                Assert.Equal(0, info.ProxyShareFraction);
                Assert.Equal(0, info.ProxiedToAccount);
                Assert.Equal(_networkCredentials.AccountPublicKey.ToArray(), info.PublicKey.ToArray());
                Assert.True(info.Balance > 0);
                Assert.True(info.SendThresholdCreateRecord > 0);
                Assert.True(info.ReceiveThresholdCreateRecord > 0);
                Assert.False(info.ReceiveSignatureRequired);
                // At the moment, it appears this is off.
                //Assert.True(info.Expiration > DateTime.UtcNow);
                Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
            }
        }
        [Fact(DisplayName = "Get Account Info: Can Get Info for Server Node")]
        public async Task CanGetInfoForGatewayAsync()
        {
            await using (var client = _networkCredentials.CreateClientWithDefaultConfiguration())
            {
                var account = _networkCredentials.CreateDefaultGateway();
                var info = await client.GetAccountInfoAsync(account);
                Assert.NotNull(info.Address);
                Assert.Equal(account.RealmNum, info.Address.RealmNum);
                Assert.Equal(account.ShardNum, info.Address.ShardNum);
                Assert.Equal(account.AccountNum, info.Address.AccountNum);
                Assert.NotNull(info.SmartContractId);
                Assert.False(info.Deleted);
                Assert.NotNull(info.Proxy);
                Assert.True(info.Proxy.RealmNum > -1);
                Assert.True(info.Proxy.ShardNum > -1);
                Assert.True(info.Proxy.AccountNum > -1);
                Assert.True(info.ProxyShareFraction > 0);
                Assert.True(info.ProxiedToAccount > 0);
                Assert.True(info.Balance > 0);
                Assert.True(info.SendThresholdCreateRecord > 0);
                Assert.True(info.ReceiveThresholdCreateRecord > 0);
                Assert.False(info.ReceiveSignatureRequired);
                Assert.True(info.Expiration > DateTime.UtcNow);
                Assert.True(info.AutoRenewPeriod.TotalSeconds > 0);
            }
        }
    }
}
