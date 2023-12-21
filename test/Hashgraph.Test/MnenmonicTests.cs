namespace Hashgraph.Tests;

public class MnenmonicTests
{
    [Fact(DisplayName = "Mnenmonic: Can Generate HashPack Ed25519 Key from Mnenmonic")]
    public void CanGenerateHashPackEd25519KeyFromMnenmonic()
    {
        var words = "absorb radio panic chunk rough person burden place dune submit wagon strong dog coyote more multiply noble impulse fiscal coach cook door goat judge".Split(" ");
        var mnemonic = new Mnenmonic(words, "");
        var (publicKey, privateKey) = mnemonic.GenerateKeyPair(KeyDerivitationPath.HashPack);

        var expectedPublicKey = Hex.ToBytes("302a300506032b6570032100e8fbc76f27d87092a1f37fc53caf0c084ac2df1b2693c0bef24e350b0843b39f");
        var expectedPrivateKey = Hex.ToBytes("302e020100300506032b6570042204200bfe68bfa4b8048a6c6cd67c33a482ac01b061810f3b443ddf1e15919bd39bfe");

        AssertHg.Equal(expectedPublicKey, publicKey);
        AssertHg.Equal(expectedPrivateKey, privateKey);
    }
    [Fact(DisplayName = "Mnenmonic: Can Generate Calaxy Ed25519 Key from Mnenmonic")]
    public void CanGenerateCalaxyEd25519KeyFromMnenmonic()
    {
        var words = "recipe harsh clever agent snow diagram rain use hybrid demand pumpkin dynamic".Split(" ");
        var expectedEndorsement = new Endorsement(KeyType.Ed25519, Hex.ToBytes("1df12df4508a18239333240a92f97287bf1f49ec8f9ff88e6dc996d3b7ab6de7"));
        var mnemonic = new Mnenmonic(words, "");
        var (publicKey, _) = mnemonic.GenerateKeyPair(KeyDerivitationPath.Calaxy);
        var endorsement = new Endorsement(publicKey);

        Assert.Equal(expectedEndorsement, endorsement);
    }
    [Fact(DisplayName = "Mnenmonic: Can Generate Blade ECDSA Secp 256K1 Key from Mnenmonic")]
    public void CanGenerateBladeECDSASecp256K1KeyFromMnenmonic()
    {
        var words = "pass tumble pencil hat grape abstract apple shallow leg embrace truth royal".Split(" ");
        var expectedSignatory = new Signatory(KeyType.ECDSASecp256K1, Hex.ToBytes("13fa7ab3d0b22d66e940b5147af4059408a1ce51ae2415641daad400d2a3f3fb"));
        var expectedEndorsement = new Endorsement(KeyType.ECDSASecp256K1, Hex.ToBytes("0377abfad2ff0e83b10e64852565ffe2296aad79c7e83218672b8719f3dbaeda90"));
        var expectedMoniker = new Moniker(Hex.ToBytes("69a5b0c36547c5cccce4bbf662e4594ebc2e0a00"));

        var mnemonic = new Mnenmonic(words, "");
        var (publicKey, privateKey) = mnemonic.GenerateKeyPair(KeyDerivitationPath.Blade);

        var signatory = new Signatory(privateKey);
        var endorsement = new Endorsement(publicKey);
        var moniker = new Moniker(endorsement);

        Assert.Equal(expectedSignatory, signatory);
        Assert.Equal(expectedEndorsement, endorsement);
        Assert.Equal(expectedMoniker, moniker);
    }

    [Fact(DisplayName = "Mnenmonic: Can Generate WallaWallet Ed25519 Key from Mnenmonic")]
    public void CanGenerateWallaWalletE25519KeyFromMnenmonic()
    {
        var words = "flame tower stand popular farm response vacant theory ticket enemy priority wreck".Split(" ");
        var expectedSignatory = new Signatory(KeyType.Ed25519, Hex.ToBytes("431ec28f4a7907fd4bdfa4a02b054addc0c5c20f93dfa34c8e5dfe96f9c0924b"));
        var expectedEndorsement = new Endorsement(KeyType.Ed25519, Hex.ToBytes("6a74a7d3498a3ea407830f31c2b1a6ed15140c5a0934c85a029b2d1fc1d9dfc6"));

        var mnemonic = new Mnenmonic(words, "");
        var (publicKey, privateKey) = mnemonic.GenerateKeyPair(KeyDerivitationPath.WallaWallet);

        var signatory = new Signatory(privateKey);
        var endorsement = new Endorsement(publicKey);

        Assert.Equal(expectedSignatory, signatory);
        Assert.Equal(expectedEndorsement, endorsement);
    }

}