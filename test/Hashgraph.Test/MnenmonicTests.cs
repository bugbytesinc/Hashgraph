using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests;

public class MnenmonicTests
{
    [Fact(DisplayName = "Mnenmonic: Can Generate Ed25519 Key from Mnenmonic")]
    public void CanGenerateEd25519KeyFromMnenmonic()
    {
        var words = "absorb radio panic chunk rough person burden place dune submit wagon strong dog coyote more multiply noble impulse fiscal coach cook door goat judge".Split(" ");
        var mnemonic = new Mnenmonic(words, "");
        var (publicKey, privateKey) = mnemonic.GenerateEd25519KeyPair(KeyDerivitationPath.HashPack);

        var expectedPublicKey = Hex.ToBytes("302a300506032b6570032100e8fbc76f27d87092a1f37fc53caf0c084ac2df1b2693c0bef24e350b0843b39f");
        var expectedPrivateKey = Hex.ToBytes("302e020100300506032b6570042204200bfe68bfa4b8048a6c6cd67c33a482ac01b061810f3b443ddf1e15919bd39bfe");

        AssertHg.Equal(expectedPublicKey, publicKey);
        AssertHg.Equal(expectedPrivateKey, privateKey);
    }
}