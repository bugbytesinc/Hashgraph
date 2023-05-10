using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Linq;
using Xunit;

namespace Hashgraph.Tests;

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

        var checkPrivateKey = KeyUtils.ParsePrivateEd25519Key(Hex.ToBytes(priKey));
        var checkPublicKey = KeyUtils.EncodeAsDer(checkPrivateKey.GeneratePublicKey());
        var checkPublicHex = Hex.FromBytes(checkPublicKey);

        Assert.Equal(pubKey, checkPublicHex);
    }
}