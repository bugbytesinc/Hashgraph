using Hashgraph.Test.Fixtures;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace Hashgraph.Tests
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class NetworkEnvironmentPrerequisiteTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;

        public NetworkEnvironmentPrerequisiteTests(NetworkCredentialsFixture networkCredentials)
        {
            _networkCredentials = networkCredentials;
        }

        [Fact(DisplayName = "Test Prerequisites: Network Credentials For Tests Exist")]
        public void NetworkCredentialsExist()
        {
            Assert.NotNull(_networkCredentials);
        }

        [Fact(DisplayName = "Test Prerequisites: Network Address Exists and Resolves to IP Address")]
        public void NetworkAddressExists()
        {
            Assert.NotNull(_networkCredentials.NetworkAddress);
            var ip = Dns.GetHostAddresses(_networkCredentials.NetworkAddress);
            Assert.True(ip.Length > 0, "Unable to resolve Network Address to live IP Address");
        }

        [Fact(DisplayName = "Test Prerequisites: Network Port Exists")]
        public void NetworkPortExists()
        {
            Assert.True(_networkCredentials.NetworkPort > 0, "Network Port Number should be greater than zero.");
        }

        [Fact(DisplayName = "Test Prerequisites: Test Network is Reachable on Configured Port")]
        public void NetworkIsReachable()
        {
            using var client = new TcpClient();
            client.Connect(_networkCredentials.NetworkAddress, _networkCredentials.NetworkPort);
        }
        [Fact(DisplayName = "Test Prerequisites: Server Realm is Non-Negative")]
        public void ServerRealmIsNonNegative()
        {
            Assert.True(_networkCredentials.ServerRealm >= 0, "Server Node Realm should be greater than or equal to zero.");
        }
        [Fact(DisplayName = "Test Prerequisites: Server Shard is Non-Negative")]
        public void ServerShardIsNonNegative()
        {
            Assert.True(_networkCredentials.ServerShard >= 0, "Server Node Shard should be greater than or equal to zero.");
        }
        [Fact(DisplayName = "Test Prerequisites: Server Account Number is Non-Negative")]
        public void ServerAccountNumberIsNonNegative()
        {
            Assert.True(_networkCredentials.ServerNumber >= 0, "Server Node Account Number should be greater than or equal to zero.");
        }
        [Fact(DisplayName = "Test Prerequisites: Test Account Realm is Non-Negative")]
        public void AccountRealmIsNonNegative()
        {
            Assert.True(_networkCredentials.AccountRealm >= 0, "Test Account Realm should be greater than or equal to zero.");
        }
        [Fact(DisplayName = "Test Prerequisites: Test Account Shard is Non-Negative")]
        public void AccountShardIsNonNegative()
        {
            Assert.True(_networkCredentials.AccountShard >= 0, "Test Account Shard should be greater than or equal to zero.");
        }
        [Fact(DisplayName = "Test Prerequisites: Test Account Account Number is Non-Negative")]
        public void AccountAccountNumberIsNonNegative()
        {
            Assert.True(_networkCredentials.AccountNumber >= 0, "Test Account Number should be greater than or equal to zero.");
        }
        [Fact(DisplayName = "Test Prerequisites: Test Account Private Key is not Empty")]
        public void AccountAccountPrivateKeyIsNotEmpty()
        {
            Assert.False(_networkCredentials.AccountPrivateKey.IsEmpty);
        }
        [Fact(DisplayName = "Test Prerequisites: Test Account Public Key is not Empty")]
        public void AccountAccountPublicKeyIsNotEmpty()
        {
            Assert.False(_networkCredentials.AccountPublicKey.IsEmpty);
        }
    }
}
