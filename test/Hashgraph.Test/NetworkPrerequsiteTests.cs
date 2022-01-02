using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace Hashgraph.Tests
{
    [Collection(nameof(NetworkCredentials))]
    public class NetworkPrerequsiteTests
    {
        private readonly NetworkCredentials _networkCredentials;

        public NetworkPrerequsiteTests(NetworkCredentials networkCredentials)
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

        [Fact(DisplayName = "Test Prerequisites: Mirror Address Exists and Resolves to IP Address")]
        public void MirrorkAddressExists()
        {
            Assert.NotNull(_networkCredentials.MirrorAddress);
            var ip = Dns.GetHostAddresses(_networkCredentials.MirrorAddress);
            Assert.True(ip.Length > 0, "Unable to resolve Mirror Address to live IP Address");
        }

        [Fact(DisplayName = "Test Prerequisites: Mirror Port Exists")]
        public void MirrorPortExists()
        {
            Assert.True(_networkCredentials.MirrorPort > 0, "Mirror Port Number should be greater than zero.");
        }

        [Fact(DisplayName = "Test Prerequisites: Mirror Node is Reachable on Configured Port")]
        public void MirrorIsReachable()
        {
            using var client = new TcpClient();
            client.Connect(_networkCredentials.MirrorAddress, _networkCredentials.MirrorPort);
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
            Assert.False(_networkCredentials.PrivateKey.IsEmpty);
        }
        [Fact(DisplayName = "Test Prerequisites: Test Account Public Key is not Empty")]
        public void AccountAccountPublicKeyIsNotEmpty()
        {
            Assert.False(_networkCredentials.PublicKey.IsEmpty);
        }
        [Fact(DisplayName = "Test Prerequisites: Test Account Public and Private Keys Match")]
        public void PublicAndPrivateKeysMatch()
        {
            var privateKey = Ed25519Util.PrivateParamsFromDerOrRaw(_networkCredentials.PrivateKey);
            var generatedPublicKey = privateKey.GeneratePublicKey();
            var publicKey = Ed25519Util.PublicParamsFromDerOrRaw(_networkCredentials.PublicKey);
            Assert.Equal(generatedPublicKey.GetEncoded(), publicKey.GetEncoded());
        }
        [Fact(DisplayName = "Test Prerequisites: Can Generate Key Pair from Bouncy Castle")]
        public void CanGenerateKeyPairFromBouncyCastle()
        {
            var privateKeyPrefix = Hex.ToBytes("302e020100300506032b657004220420").ToArray();
            var publicKeyPrefix = Hex.ToBytes("302a300506032b6570032100").ToArray();

            var keyPairGenerator = new Ed25519KeyPairGenerator();
            keyPairGenerator.Init(new Ed25519KeyGenerationParameters(new SecureRandom()));
            var keyPair = keyPairGenerator.GenerateKeyPair();

            var privateKey = keyPair.Private as Ed25519PrivateKeyParameters;
            var publicKey = keyPair.Public as Ed25519PublicKeyParameters;

            var pubKey = Hex.FromBytes(publicKeyPrefix.Concat(publicKey.GetEncoded()).ToArray());
            var priKey = Hex.FromBytes(privateKeyPrefix.Concat(privateKey.GetEncoded()).ToArray());

            var checkPrivateKey = Ed25519Util.PrivateParamsFromDerOrRaw(Hex.ToBytes(priKey));
            var checkPublicKey = Ed25519Util.ToDerBytes(checkPrivateKey.GeneratePublicKey());
            var checkPublicHex = Hex.FromBytes(checkPublicKey);

            Assert.Equal(pubKey, checkPublicHex);
        }
    }
}
