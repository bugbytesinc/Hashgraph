namespace Hashgraph.Tests;

public class SignatureMapExtensionTests
{
    [Fact(DisplayName = "SignatureMap: Can Add Ed25519 Signature to Empty Signature Map")]
    public async Task CanAddEd25519SignaturetoEmptySignatureMap()
    {
        var (publicKey, privateKey) = Generator.Ed25519KeyPair();

        var endorsement = new Endorsement(publicKey);
        var signatory = new Signatory(privateKey);
        var message = Generator.Secp256k1KeyPair().publicKey;
        var sigMap = new SignatureMap();
        await sigMap.AddSignatureAsync(message, signatory);

        Assert.Single(sigMap.SigPair);
        var sigPair = sigMap.SigPair[0];
        AssertHg.Equal(endorsement.ToBytes(KeyFormat.Raw), sigPair.PubKeyPrefix.Memory);
        Assert.NotNull(sigPair.Ed25519);
        Assert.True(endorsement.Verify(message, sigPair.Ed25519.Memory));
    }
    [Fact(DisplayName = "SignatureMap: Can Add ECDSASecp256K1 Signature to Empty Signature Map")]
    public async Task CanAddSecp256k1KeyPairSignaturetoEmptySignatureMap()
    {
        var (publicKey, privateKey) = Generator.Secp256k1KeyPair();

        var endorsement = new Endorsement(publicKey);
        var signatory = new Signatory(privateKey);
        var message = Generator.Ed25519KeyPair().publicKey;
        var sigMap = new SignatureMap();
        await sigMap.AddSignatureAsync(message, signatory);

        Assert.Single(sigMap.SigPair);
        var sigPair = sigMap.SigPair[0];
        AssertHg.Equal(endorsement.ToBytes(KeyFormat.Raw), sigPair.PubKeyPrefix.Memory);
        Assert.NotNull(sigPair.ECDSASecp256K1);
        Assert.True(endorsement.Verify(message, sigPair.ECDSASecp256K1.Memory));
    }
    [Fact(DisplayName = "SignatureMap: Can Add Multiple Signatures Map")]
    public async Task CanAddMultipleSignaturesMap()
    {
        var count = 10;
        var sigMap = new SignatureMap();
        var message = Generator.Secp256k1KeyPair().publicKey;
        for (var i = 0; i < count; i++)
        {
            var (_, privateKey) = Generator.KeyPair();
            var signatory = new Signatory(privateKey);
            await sigMap.AddSignatureAsync(message, signatory);
        }
        Assert.NotNull(sigMap.SigPair);
        Assert.Equal(count, sigMap.SigPair.Count);
    }
    [Fact(DisplayName = "SignatureMap: Can Satisfy Two Signatures")]
    public async Task CanSatisfyWithTwoSignatures()
    {
        var (publicKey1, privateKey1) = Generator.KeyPair();
        var (publicKey2, privateKey2) = Generator.KeyPair();

        var endorsement = new Endorsement(new Endorsement(publicKey1), new Endorsement(publicKey2));
        var signatory = new Signatory(new Signatory(privateKey1), new Signatory(privateKey2));
        var message = Generator.Ed25519KeyPair().publicKey;
        var sigMap = new SignatureMap();
        await sigMap.AddSignatureAsync(message, signatory);

        Assert.True(sigMap.Satisfies(message, endorsement));
    }
    [Fact(DisplayName = "SignatureMap: Cannot Satisfy when Missing a Signature")]
    public async Task CannotSatisfyWhenMissingASignature()
    {
        var (publicKey1, privateKey1) = Generator.KeyPair();
        var (publicKey2, _) = Generator.KeyPair();

        var endorsement = new Endorsement(new Endorsement(publicKey1), new Endorsement(publicKey2));
        var signatory = new Signatory(privateKey1);
        var message = Generator.Ed25519KeyPair().publicKey;
        var sigMap = new SignatureMap();
        await sigMap.AddSignatureAsync(message, signatory);

        Assert.False(sigMap.Satisfies(message, endorsement));
    }
    [Fact(DisplayName = "SignatureMap: Can Satisfy Complex Key Requirements")]
    public async Task CanSatisfyComplexKeyRequirements()
    {
        var (publicKey1, privateKey1) = Generator.KeyPair();
        var (publicKey2, privateKey2) = Generator.KeyPair();
        var (publicKey3, privateKey3) = Generator.KeyPair();
        var (publicKey4, privateKey4) = Generator.KeyPair();
        var (publicKey5, privateKey5) = Generator.KeyPair();

        var endorsement = new Endorsement(
            new Endorsement(new Endorsement(publicKey1), new Endorsement(publicKey2)),
            new Endorsement(1, new Endorsement(publicKey3), new Endorsement(publicKey4)),
            new Endorsement(publicKey5));
        var signatory = new Signatory(
            new Signatory(privateKey1),
            new Signatory(privateKey2),
            new Signatory(privateKey3),
            new Signatory(privateKey4),
            new Signatory(privateKey5));
        var message = Generator.Ed25519KeyPair().publicKey;
        var sigMap = new SignatureMap();
        await sigMap.AddSignatureAsync(message, signatory);

        Assert.True(sigMap.Satisfies(message, endorsement));
    }
    [Fact(DisplayName = "SignatureMap: Can Fail Complex Key Requirements")]
    public async Task CanFailComplexKeyRequirements()
    {
        var (publicKey1, privateKey1) = Generator.KeyPair();
        var (publicKey2, privateKey2) = Generator.KeyPair();
        var (publicKey3, privateKey3) = Generator.KeyPair();
        var (publicKey4, privateKey4) = Generator.KeyPair();
        var (publicKey5, _) = Generator.KeyPair();

        var endorsement = new Endorsement(
            new Endorsement(new Endorsement(publicKey1), new Endorsement(publicKey2)),
            new Endorsement(1, new Endorsement(publicKey3), new Endorsement(publicKey4)),
            new Endorsement(publicKey5));
        var signatory = new Signatory(
            new Signatory(privateKey1),
            new Signatory(privateKey2),
            new Signatory(privateKey3),
            new Signatory(privateKey4));
        var message = Generator.Ed25519KeyPair().publicKey;
        var sigMap = new SignatureMap();
        await sigMap.AddSignatureAsync(message, signatory);

        Assert.False(sigMap.Satisfies(message, endorsement));
    }
    [Fact(DisplayName = "SignatureMap: Satisfies Ignores Invalid Signatures")]
    public async Task SatisfiesIgnoresInvalidSignatures()
    {
        var (publicKey1, privateKey1) = Generator.KeyPair();
        var (publicKey2, privateKey2) = Generator.KeyPair();

        var endorsement = new Endorsement(new Endorsement(publicKey1), new Endorsement(publicKey2));
        var signatory = new Signatory(new Signatory(privateKey1), new Signatory(privateKey2));
        var message = Generator.Ed25519KeyPair().publicKey;
        var sigMap = new SignatureMap();
        await sigMap.AddSignatureAsync(message, signatory);
        for (var i = 0; i < 10; i++)
        {
            await sigMap.AddSignatureAsync(message, new Signatory(Generator.KeyPair().privateKey));
        }

        Assert.True(sigMap.Satisfies(message, endorsement));
    }
}