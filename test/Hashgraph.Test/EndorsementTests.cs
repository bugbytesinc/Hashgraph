using Org.BouncyCastle.X509;

namespace Hashgraph.Tests;

public class EndorsementsTests
{
    [Fact(DisplayName = "Endorsements: Can Create Valid Ed25519 Endorsements Object")]
    public void CreateValidEd25519EndorsementsObject()
    {
        var (publicKey1, _) = Generator.Ed25519KeyPair();
        var (publicKey2, _) = Generator.Ed25519KeyPair();

        new Endorsement(publicKey1);
        new Endorsement(1, publicKey1);
        new Endorsement(publicKey1, publicKey2);
        new Endorsement(1, new Endorsement(1, publicKey1, publicKey2), new Endorsement(2, publicKey1, publicKey2));
    }
    [Fact(DisplayName = "Endorsements: Can Create Valid ECDSA Secp256K1 Endorsements Object")]
    public void CreateValidEcdsaSecp256K1EndorsementsObject()
    {
        var (publicKey1, _) = Generator.Secp256k1KeyPair();
        var (publicKey2, _) = Generator.Secp256k1KeyPair();

        new Endorsement(publicKey1);
        new Endorsement(1, publicKey1);
        new Endorsement(publicKey1, publicKey2);
        new Endorsement(1, new Endorsement(1, publicKey1, publicKey2), new Endorsement(2, publicKey1, publicKey2));
    }
    [Fact(DisplayName = "Endorsements: Too large of a requried count throws error.")]
    public void TooLargeRequiredCountThrowsError()
    {

        var (publicKey, _) = Generator.Ed25519KeyPair();
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Endorsement((uint)Generator.Integer(2, 4), publicKey);
        });
        Assert.Equal("requiredCount", exception.ParamName);
        Assert.StartsWith("The required number of keys for a valid signature cannot exceed the number of public keys provided.", exception.Message);
    }
    [Fact(DisplayName = "Endorsements: Empty Private key throws Exception")]
    public void EmptyValueForKeyThrowsError()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Endorsement();
        });
        Assert.Equal("endorsements", exception.ParamName);
        Assert.StartsWith("At least one endorsement in a list is required.", exception.Message);
    }
    [Fact(DisplayName = "Endorsements: Invalid Ed25519 Bytes in Private key throws Exception")]
    public void InvalidEd25519BytesForValueForKeyThrowsError()
    {
        var (originalKey, _) = Generator.Ed25519KeyPair();
        var invalidKey = originalKey.ToArray();
        invalidKey[0] = 0;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Endorsement(KeyType.Ed25519, invalidKey);
        });
        Assert.StartsWith("The public key does not appear to be encoded in a recognizable Ed25519 format.", exception.Message);
    }
    [Fact(DisplayName = "Endorsements: Invalid ECDSA Secp256K1 Bytes in Private key throws Exception")]
    public void InvalidEcdsaSecp256K1BytesForValueForKeyThrowsError()
    {
        var (originalKey, _) = Generator.Secp256k1KeyPair();
        var invalidKey = originalKey.ToArray();
        invalidKey[0] = 0;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Endorsement(KeyType.ECDSASecp256K1, invalidKey);
        });
        Assert.StartsWith("The public key was not provided in a recognizable ECDSA Secp256K1 format.", exception.Message);
    }
    [Fact(DisplayName = "Endorsements: Invalid Byte Length in Ed25519 Public key throws Exception")]
    public void InvalidEd25519ByteLengthForValueForKeyThrowsError()
    {
        var (originalKey, _) = Generator.Ed25519KeyPair();
        var invalidKey = originalKey.ToArray().Take(30).ToArray();
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Endorsement(KeyType.Ed25519, invalidKey);
        });
        Assert.StartsWith("The public key does not appear to be encoded in a recognizable Ed25519 format.", exception.Message);
    }
    [Fact(DisplayName = "Endorsements: Invalid Byte Length in ECDSA Secp256K1 Public key throws Exception")]
    public void InvalidEcdsaSecp256K1ByteLengthForValueForKeyThrowsError()
    {
        var (originalKey, _) = Generator.Secp256k1KeyPair();
        var invalidKey = originalKey.ToArray().Take(32).ToArray();
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Endorsement(KeyType.ECDSASecp256K1, invalidKey);
        });
        Assert.StartsWith("The public key was not provided in a recognizable ECDSA Secp256K1 format.", exception.Message);
    }
    [Fact(DisplayName = "Endorsements: Equivalent Ed25519 Endorsements are considered Equal")]
    public void EquivalentEd25519EndorsementsAreConsideredEqual()
    {
        var (publicKey1, _) = Generator.Ed25519KeyPair();
        var (publicKey2, _) = Generator.Ed25519KeyPair();
        var endorsement1 = new Endorsement(publicKey1);
        var endorsement2 = new Endorsement(publicKey1);
        Assert.Equal(endorsement1, endorsement2);
        Assert.True(endorsement1 == endorsement2);
        Assert.False(endorsement1 != endorsement2);

        endorsement1 = new Endorsement(publicKey1, publicKey2);
        endorsement2 = new Endorsement(publicKey1, publicKey2);
        Assert.Equal(endorsement1, endorsement2);
        Assert.True(endorsement1 == endorsement2);
        Assert.False(endorsement1 != endorsement2);

        object asObject1 = endorsement1;
        object asObject2 = endorsement2;
        Assert.Equal(asObject1, asObject2);
        Assert.True(endorsement1.Equals(asObject1));
        Assert.True(asObject1.Equals(endorsement1));
    }
    [Fact(DisplayName = "Endorsements: Equivalent ECDSA Secp256K1 Endorsements are considered Equal")]
    public void EquivalentECDSASecp256K1EndorsementsAreConsideredEqual()
    {
        var (publicKey1, _) = Generator.Secp256k1KeyPair();
        var (publicKey2, _) = Generator.Secp256k1KeyPair();
        var endorsement1 = new Endorsement(publicKey1);
        var endorsement2 = new Endorsement(publicKey1);
        Assert.Equal(endorsement1, endorsement2);
        Assert.True(endorsement1 == endorsement2);
        Assert.False(endorsement1 != endorsement2);

        endorsement1 = new Endorsement(publicKey1, publicKey2);
        endorsement2 = new Endorsement(publicKey1, publicKey2);
        Assert.Equal(endorsement1, endorsement2);
        Assert.True(endorsement1 == endorsement2);
        Assert.False(endorsement1 != endorsement2);

        object asObject1 = endorsement1;
        object asObject2 = endorsement2;
        Assert.Equal(asObject1, asObject2);
        Assert.True(endorsement1.Equals(asObject1));
        Assert.True(asObject1.Equals(endorsement1));
    }
    [Fact(DisplayName = "Endorsements: Disimilar Ed25519 Endorsements are not considered Equal")]
    public void DisimilarEd25519EndorsementsAreNotConsideredEqual()
    {
        var (publicKey1, _) = Generator.Ed25519KeyPair();
        var (publicKey2, _) = Generator.Ed25519KeyPair();
        var endorsements1 = new Endorsement(publicKey1);
        var endorsements2 = new Endorsement(publicKey2);
        Assert.NotEqual(endorsements1, endorsements2);
        Assert.False(endorsements1 == endorsements2);
        Assert.True(endorsements1 != endorsements2);

        endorsements1 = new Endorsement(publicKey1);
        endorsements2 = new Endorsement(publicKey1, publicKey2);
        Assert.NotEqual(endorsements1, endorsements2);
        Assert.False(endorsements1 == endorsements2);
        Assert.True(endorsements1 != endorsements2);

        endorsements1 = new Endorsement(publicKey1, publicKey2);
        endorsements2 = new Endorsement(1, publicKey1, publicKey2);
        Assert.NotEqual(endorsements1, endorsements2);
        Assert.False(endorsements1 == endorsements2);
        Assert.True(endorsements1 != endorsements2);
    }

    [Fact(DisplayName = "Endorsements: Disimilar ECDSA Secp256K1 Endorsements are not considered Equal")]
    public void DisimilarECDSASecp256K1EndorsementsAreNotConsideredEqual()
    {
        var (publicKey1, _) = Generator.Secp256k1KeyPair();
        var (publicKey2, _) = Generator.Secp256k1KeyPair();
        var endorsements1 = new Endorsement(publicKey1);
        var endorsements2 = new Endorsement(publicKey2);
        Assert.NotEqual(endorsements1, endorsements2);
        Assert.False(endorsements1 == endorsements2);
        Assert.True(endorsements1 != endorsements2);

        endorsements1 = new Endorsement(publicKey1);
        endorsements2 = new Endorsement(publicKey1, publicKey2);
        Assert.NotEqual(endorsements1, endorsements2);
        Assert.False(endorsements1 == endorsements2);
        Assert.True(endorsements1 != endorsements2);

        endorsements1 = new Endorsement(publicKey1, publicKey2);
        endorsements2 = new Endorsement(1, publicKey1, publicKey2);
        Assert.NotEqual(endorsements1, endorsements2);
        Assert.False(endorsements1 == endorsements2);
        Assert.True(endorsements1 != endorsements2);
    }

    [Fact(DisplayName = "Endorsements: Disimilar Multi-Key Endorsements are not considered Equal")]
    public void DisimilarMultiKeyEndorsementsAreNotConsideredEqual()
    {
        var (publicKey1, _) = Generator.Ed25519KeyPair();
        var (publicKey2, _) = Generator.Ed25519KeyPair();
        var (publicKey3, _) = Generator.Secp256k1KeyPair();
        var endorsements1 = new Endorsement(publicKey1, publicKey2);
        var endorsements2 = new Endorsement(publicKey2, publicKey3);
        Assert.NotEqual(endorsements1, endorsements2);
        Assert.False(endorsements1 == endorsements2);
        Assert.True(endorsements1 != endorsements2);

        endorsements1 = new Endorsement(1, publicKey1, publicKey2);
        endorsements2 = new Endorsement(2, publicKey2, publicKey3);
        Assert.NotEqual(endorsements1, endorsements2);
        Assert.False(endorsements1 == endorsements2);
        Assert.True(endorsements1 != endorsements2);

        endorsements1 = new Endorsement(1, publicKey1, publicKey2, publicKey3);
        endorsements2 = new Endorsement(2, publicKey1, publicKey2, publicKey3);
        Assert.NotEqual(endorsements1, endorsements2);
        Assert.False(endorsements1 == endorsements2);
        Assert.True(endorsements1 != endorsements2);

        endorsements1 = new Endorsement(2, publicKey1, publicKey2, publicKey3);
        endorsements2 = new Endorsement(3, publicKey1, publicKey2, publicKey3);
        Assert.NotEqual(endorsements1, endorsements2);
        Assert.False(endorsements1 == endorsements2);
        Assert.True(endorsements1 != endorsements2);
    }
    [Fact(DisplayName = "Endorsements: Default Can Create Ed25519 Type")]
    public void DefaultCanCreateEd25519Type()
    {
        var (publicKey1, _) = Generator.Ed25519KeyPair();

        var endorsement = new Endorsement(publicKey1);
        Assert.Equal(KeyType.Ed25519, endorsement.Type);
        Assert.Empty(endorsement.List);
        Assert.Equal(0U, endorsement.RequiredCount);
        Assert.Equal(publicKey1.ToArray(), endorsement.ToBytes().ToArray());
    }
    [Fact(DisplayName = "Endorsements: Default Can Create ECDSA Secp256K1 Type")]
    public void DefaultCanCreateECDSASecp256K1Type()
    {
        var (publicKey1, _) = Generator.Secp256k1KeyPair();

        var endorsement = new Endorsement(publicKey1);
        Assert.Equal(KeyType.ECDSASecp256K1, endorsement.Type);
        Assert.Empty(endorsement.List);
        Assert.Equal(0U, endorsement.RequiredCount);
        Assert.Equal(publicKey1.ToArray(), endorsement.ToBytes().ToArray());
    }
    [Fact(DisplayName = "Endorsements: Creating Ed25519 Type produces Ed25519 type")]
    public void CanCreateEd25519Type()
    {
        var (publicKey1, _) = Generator.Ed25519KeyPair();

        var endorsement = new Endorsement(KeyType.Ed25519, publicKey1);
        Assert.Equal(KeyType.Ed25519, endorsement.Type);
        Assert.Empty(endorsement.List);
        Assert.Equal(0U, endorsement.RequiredCount);
        Assert.Equal(publicKey1.ToArray(), endorsement.ToBytes().ToArray());
    }
    [Fact(DisplayName = "Endorsements: Creating ECDSA Secp256K1 Type produces ECDSA Secp256K1 type")]
    public void CanCreateECDSASecp256K1Type()
    {
        var (publicKey1, _) = Generator.Secp256k1KeyPair();

        var endorsement = new Endorsement(KeyType.ECDSASecp256K1, publicKey1);
        Assert.Equal(KeyType.ECDSASecp256K1, endorsement.Type);
        Assert.Empty(endorsement.List);
        Assert.Equal(0U, endorsement.RequiredCount);
        Assert.Equal(publicKey1.ToArray(), endorsement.ToBytes().ToArray());
    }
    [Fact(DisplayName = "Endorsements: Creating Contract Type from Bytes Produces Error.")]
    public void CreatingContractTypeFromBytesProducesError()
    {
        var contract = new Address(Generator.Integer(0, 100), Generator.Integer(0, 100), Generator.Integer(1000, 20000));
        var bytes = Abi.EncodeArguments(new[] { contract });

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Endorsement(KeyType.Contract, bytes);
        });
        Assert.Equal("type", exception.ParamName);
        Assert.StartsWith("Only endorsements representing single Ed25519 or ECDSASecp256K1 keys are supported with this constructor, please use the contract address constructor instead.", exception.Message);
    }
    [Fact(DisplayName = "Endorsements: Creating Contract Type produces Contract type")]
    public void CanCreateContractType()
    {
        var contract = new Address(Generator.Integer(0, 100), Generator.Integer(0, 100), Generator.Integer(1000, 20000));
        var endorsement = new Endorsement(contract);

        Assert.Equal(KeyType.Contract, endorsement.Type);
        Assert.Empty(endorsement.List);
        Assert.Equal(0U, endorsement.RequiredCount);
        AssertHg.Empty(endorsement.ToBytes(KeyFormat.Der));
        Assert.Equal(contract, endorsement.Contract);
    }
    [Fact(DisplayName = "Endorsements: Creating n of m List produces n of m List type")]
    public void CanCreateNofMList()
    {
        var n = (uint)Generator.Integer(1, 4);
        var m = Generator.Integer(5, 10);
        var keys = Enumerable.Range(0, m).Select(i => new Endorsement(Generator.Ed25519KeyPair().publicKey)).ToArray();
        var list = new Endorsement(n, keys);
        Assert.Equal(KeyType.List, list.Type);
        Assert.Equal(n, list.RequiredCount);
        Assert.Equal(m, list.List.Length);
        for (int i = 0; i < m; i++)
        {
            Assert.Equal(keys[i], list.List[i]);
        }
    }
    [Fact(DisplayName = "Endorsements: Can Enumerte a Tree")]
    public void CanEnumrateAnEndorsementTree()
    {
        var (publicKey1a, _) = Generator.Ed25519KeyPair();
        var (publicKey2a, _) = Generator.Ed25519KeyPair();
        var (publicKey3a, _) = Generator.Ed25519KeyPair();
        var (publicKey1b, _) = Generator.Ed25519KeyPair();
        var (publicKey2b, _) = Generator.Ed25519KeyPair();
        var (publicKey3b, _) = Generator.Ed25519KeyPair();
        var endorsements1 = new Endorsement(1, publicKey1a, publicKey1b);
        var endorsements2 = new Endorsement(1, publicKey2a, publicKey2b);
        var endorsements3 = new Endorsement(publicKey3a, publicKey3b);
        var tree = new Endorsement(endorsements1, endorsements2, endorsements3);

        Assert.Equal(KeyType.List, tree.Type);
        Assert.Equal(3U, tree.RequiredCount);
        Assert.Equal(3, tree.List.Length);
        Assert.Empty(tree.ToBytes(KeyFormat.Der).ToArray());

        Assert.Equal(KeyType.List, tree.List[0].Type);
        Assert.Equal(1U, tree.List[0].RequiredCount);
        Assert.Equal(2, tree.List[0].List.Length);
        Assert.Empty(tree.List[0].ToBytes(KeyFormat.Der).ToArray());

        Assert.Equal(KeyType.Ed25519, tree.List[0].List[0].Type);
        Assert.Equal(0U, tree.List[0].List[0].RequiredCount);
        Assert.Empty(tree.List[0].List[0].List);
        Assert.Equal(publicKey1a.ToArray(), tree.List[0].List[0].ToBytes().ToArray());

        Assert.Equal(KeyType.Ed25519, tree.List[0].List[1].Type);
        Assert.Equal(0U, tree.List[0].List[1].RequiredCount);
        Assert.Empty(tree.List[0].List[1].List);
        Assert.Equal(publicKey1b.ToArray(), tree.List[0].List[1].ToBytes().ToArray());

        Assert.Equal(KeyType.List, tree.List[1].Type);
        Assert.Equal(1U, tree.List[1].RequiredCount);
        Assert.Equal(2, tree.List[1].List.Length);
        Assert.Empty(tree.List[1].ToBytes(KeyFormat.Der).ToArray());

        Assert.Equal(KeyType.Ed25519, tree.List[1].List[0].Type);
        Assert.Equal(0U, tree.List[1].List[0].RequiredCount);
        Assert.Empty(tree.List[1].List[0].List);
        Assert.Equal(publicKey2a.ToArray(), tree.List[1].List[0].ToBytes().ToArray());

        Assert.Equal(KeyType.Ed25519, tree.List[1].List[1].Type);
        Assert.Equal(0U, tree.List[1].List[1].RequiredCount);
        Assert.Empty(tree.List[1].List[1].List);
        Assert.Equal(publicKey2b.ToArray(), tree.List[1].List[1].ToBytes().ToArray());

        Assert.Equal(KeyType.List, tree.List[2].Type);
        Assert.Equal(2U, tree.List[2].RequiredCount);
        Assert.Equal(2, tree.List[2].List.Length);
        Assert.Empty(tree.List[2].ToBytes(KeyFormat.Der).ToArray());

        Assert.Equal(KeyType.Ed25519, tree.List[2].List[0].Type);
        Assert.Equal(0U, tree.List[2].List[0].RequiredCount);
        Assert.Empty(tree.List[2].List[0].List);
        Assert.Equal(publicKey3a.ToArray(), tree.List[2].List[0].ToBytes().ToArray());

        Assert.Equal(KeyType.Ed25519, tree.List[2].List[1].Type);
        Assert.Equal(0U, tree.List[2].List[1].RequiredCount);
        Assert.Empty(tree.List[2].List[1].List);
        Assert.Equal(publicKey3b.ToArray(), tree.List[2].List[1].ToBytes().ToArray());
    }
    [Fact(DisplayName = "Endorsements: Make List Type from Key type constructor throws error.")]
    public void CreateListTypeFromKeyTypeConstructorThrowsError()
    {
        var (publicKey, _) = Generator.Ed25519KeyPair();
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Endorsement(KeyType.List, publicKey);
        });
        Assert.Equal("type", exception.ParamName);
        Assert.StartsWith("Only endorsements representing single Ed25519 or ECDSASecp256K1 keys are supported with this constructor, please use the list constructor instead.", exception.Message);
    }
    [Fact(DisplayName = "Endorsements: Make Contract Type from Key type constructor throws error.")]
    public void MakeContractTypeFromKeyTypeConstructorThrowsError()
    {
        var (publicKey, _) = Generator.Ed25519KeyPair();
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Endorsement(KeyType.Contract, publicKey);
        });
        Assert.Equal("type", exception.ParamName);
        Assert.StartsWith("Only endorsements representing single Ed25519 or ECDSASecp256K1 keys are supported with this constructor, please use the contract address constructor instead.", exception.Message);
    }
    [Fact(DisplayName = "Endorsements: Equivalent Contract Types are Considered Equal")]
    public void EquivalentContractEndorsementsAreConsideredEqual()
    {
        var contract = new Address(Generator.Integer(0, 100), Generator.Integer(0, 100), Generator.Integer(1000, 20000));

        var endorsement1 = new Endorsement(contract);
        var endorsement2 = new Endorsement(new Address(contract.ShardNum, contract.RealmNum, contract.AccountNum));
        Assert.Equal(endorsement1, endorsement2);
        Assert.True(endorsement1 == endorsement2);
        Assert.False(endorsement1 != endorsement2);

        object asObject1 = endorsement1;
        object asObject2 = endorsement2;
        Assert.Equal(asObject1, asObject2);
        Assert.True(endorsement1.Equals(asObject2));
        Assert.True(asObject1.Equals(endorsement2));
    }
    [Fact(DisplayName = "Endorsements: Disimilar Contract Types are Considered Equal")]
    public void DisimilarContractEndorsementsAreConsideredNotEqual()
    {
        var contract1 = new Address(Generator.Integer(0, 100), Generator.Integer(0, 100), Generator.Integer(1000, 20000));
        var contract2 = new Address(Generator.Integer(0, 100), Generator.Integer(0, 100), Generator.Integer(20001, 50000));

        var endorsement1 = new Endorsement(contract1);
        var endorsement2 = new Endorsement(contract2);
        Assert.NotEqual(endorsement1, endorsement2);
        Assert.False(endorsement1 == endorsement2);
        Assert.True(endorsement1 != endorsement2);

        object asObject1 = endorsement1;
        object asObject2 = endorsement2;
        Assert.NotEqual(asObject1, asObject2);
        Assert.False(endorsement1.Equals(asObject2));
        Assert.False(asObject1.Equals(endorsement2));
    }
    [Fact(DisplayName = "Endorsements: Contract Protobuf Plublic Key can Create Contract Type")]
    public void CanCreateContractEndorsementFromProtobufContractKeyType()
    {
        var contract = new Address(Generator.Integer(0, 100), Generator.Integer(0, 100), Generator.Integer(1000, 20000));
        var contractID = new Proto.ContractID(contract);
        var key = new Proto.Key { ContractID = contractID };
        var endorsement = key.ToEndorsement();

        Assert.Equal(KeyType.Contract, endorsement.Type);
        Assert.Empty(endorsement.List);
        Assert.Equal(0U, endorsement.RequiredCount);
        Assert.Equal(contract, endorsement.Contract);
    }

    [Fact(DisplayName = "Endorsements: Can Parse Ed25519 Der Encoded")]
    public void CanParseEd25519DerEncoded()
    {
        var publicKey = Hex.ToBytes("302a300506032b65700321001dd944db2def347f51ef46ab7bafba05e139ed3cadfa9786ce6ab034284d500d");

        var endorsement = new Endorsement(publicKey);
        Assert.Equal(KeyType.Ed25519, endorsement.Type);
        Assert.Empty(endorsement.List);
        Assert.Equal(0U, endorsement.RequiredCount);
        Assert.Equal(publicKey.ToArray(), endorsement.ToBytes().ToArray());
    }

    [Fact(DisplayName = "Endorsements: Can Parse Ed25519 From Der Encoding")]
    public void CanParseEd25519FromDerEncoding()
    {
        var derPublicKey = Hex.ToBytes("302a300506032b65700321001dd944db2def347f51ef46ab7bafba05e139ed3cadfa9786ce6ab034284d500d");

        var endorsement = new Endorsement(derPublicKey);
        Assert.Equal(KeyType.Ed25519, endorsement.Type);
        Assert.Empty(endorsement.List);
        Assert.Equal(0U, endorsement.RequiredCount);
        Assert.Equal(derPublicKey.ToArray(), endorsement.ToBytes().ToArray());
    }
    [Fact(DisplayName = "Endorsements: Can Parse Ed25519 Raw 32 bit key")]
    public void CanParseEd25519Raw32BitKey()
    {
        var derPublicKey = Hex.ToBytes("302a300506032b65700321001dd944db2def347f51ef46ab7bafba05e139ed3cadfa9786ce6ab034284d500d");
        var rawPublicKey = derPublicKey[^32..];

        var endorsement = new Endorsement(rawPublicKey);
        Assert.Equal(KeyType.Ed25519, endorsement.Type);
        Assert.Empty(endorsement.List);
        Assert.Equal(0U, endorsement.RequiredCount);
        Assert.Equal(derPublicKey.ToArray(), endorsement.ToBytes().ToArray());
    }

    [Fact(DisplayName = "Endorsements: Can Parse Secp256K1 From Extended Der Encoding")]
    public void CanParseSecp256K1FromExtendedDerEncoding()
    {
        var (derPublicKey, _) = Generator.Secp256k1KeyPair();

        var endorsement = new Endorsement(derPublicKey);
        Assert.Equal(KeyType.ECDSASecp256K1, endorsement.Type);
        Assert.Empty(endorsement.List);
        Assert.Equal(0U, endorsement.RequiredCount);
        Assert.Equal(derPublicKey.ToArray(), endorsement.ToBytes().ToArray());
    }

    [Fact(DisplayName = "Endorsements: Can Parse Secp256K1 From Compacted Der Encoding")]
    public void CanParseSecp256K1FromCompactedDerEncoding()
    {
        var derPublicKey = Hex.ToBytes("302d300706052b8104000a03220002ffd5a91eb6e55f584718a7da0bc168cddf9dd3dec2a968e574181a8fd9ab95ae");
        var longFormKey = Hex.ToBytes("308201333081ec06072a8648ce3d02013081e0020101302c06072a8648ce3d0101022100fffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f3044042000000000000000000000000000000000000000000000000000000000000000000420000000000000000000000000000000000000000000000000000000000000000704410479be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798483ada7726a3c4655da4fbfc0e1108a8fd17b448a68554199c47d08ffb10d4b8022100fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd036414102010103420004ffd5a91eb6e55f584718a7da0bc168cddf9dd3dec2a968e574181a8fd9ab95aee07205037c7be54a4b818c79eeec0a44e502a12abf2641e06554d643b7fb4516");

        var endorsement = new Endorsement(derPublicKey);
        Assert.Equal(KeyType.ECDSASecp256K1, endorsement.Type);
        Assert.Empty(endorsement.List);
        Assert.Equal(0U, endorsement.RequiredCount);
        Assert.Equal(longFormKey.ToArray(), endorsement.ToBytes().ToArray());
    }

    [Fact(DisplayName = "Endorsements: Can Parse Secp256K1 From Raw Form")]
    public void CanParseSecp256K1FromRawForm()
    {
        var derPublicKey = Hex.ToBytes("02ffd5a91eb6e55f584718a7da0bc168cddf9dd3dec2a968e574181a8fd9ab95ae");
        var longFormKey = Hex.ToBytes("308201333081ec06072a8648ce3d02013081e0020101302c06072a8648ce3d0101022100fffffffffffffffffffffffffffffffffffffffffffffffffffffffefffffc2f3044042000000000000000000000000000000000000000000000000000000000000000000420000000000000000000000000000000000000000000000000000000000000000704410479be667ef9dcbbac55a06295ce870b07029bfcdb2dce28d959f2815b16f81798483ada7726a3c4655da4fbfc0e1108a8fd17b448a68554199c47d08ffb10d4b8022100fffffffffffffffffffffffffffffffebaaedce6af48a03bbfd25e8cd036414102010103420004ffd5a91eb6e55f584718a7da0bc168cddf9dd3dec2a968e574181a8fd9ab95aee07205037c7be54a4b818c79eeec0a44e502a12abf2641e06554d643b7fb4516");

        var endorsement = new Endorsement(derPublicKey);
        Assert.Equal(KeyType.ECDSASecp256K1, endorsement.Type);
        Assert.Empty(endorsement.List);
        Assert.Equal(0U, endorsement.RequiredCount);
        Assert.Equal(longFormKey.ToArray(), endorsement.ToBytes().ToArray());
    }

    [Fact(DisplayName = "Endorsements: Can Extract Ed25519 Bytes in Various Formats")]
    public void CanExtractEd25519BytesInVariousFormats()
    {
        var derPublicKey = Hex.ToBytes("302a300506032b6570032100eeed21c291ef1d6860540370e9907ea9a7cb529dba1c0bfaa6dcf644f28aab31");
        var rawPublicKey = derPublicKey[^32..];
        var prtPublicKey = (new Proto.Key { Ed25519 = ByteString.CopyFrom(rawPublicKey.ToArray()) }).ToByteString().Memory;

        var endorsement = new Endorsement(rawPublicKey);

        AssertHg.Equal(derPublicKey, endorsement.ToBytes(KeyFormat.Default));
        AssertHg.Equal(rawPublicKey, endorsement.ToBytes(KeyFormat.Raw));
        AssertHg.Equal(derPublicKey, endorsement.ToBytes(KeyFormat.Der));
        AssertHg.Equal(prtPublicKey, endorsement.ToBytes(KeyFormat.Protobuf));
        AssertHg.Equal(derPublicKey, endorsement.ToBytes(KeyFormat.Hedera));
        AssertHg.Equal(rawPublicKey, endorsement.ToBytes(KeyFormat.Mirror));
    }
    [Fact(DisplayName = "Endorsements: Can Extract Secp256K1 Bytes in Various Formats")]
    public void CanExtractSecp256K1BytesInVariousFormats()
    {
        ReadOnlyMemory<byte> rawPublicKey = Hex.ToBytes("026866c9664a95af2e9d8e7109eb8ccbe74eb822d49be1242b1511d775d1826e2a");
        ReadOnlyMemory<byte> hdrPublicKey = Hex.ToBytes("302d300706052b8104000a032200026866c9664a95af2e9d8e7109eb8ccbe74eb822d49be1242b1511d775d1826e2a");
        ReadOnlyMemory<byte> derPublicKey = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(KeyUtils.ParsePublicEcdsaSecp256k1Key(rawPublicKey)).GetDerEncoded();
        ReadOnlyMemory<byte> prtPublicKey = (new Proto.Key { ECDSASecp256K1 = ByteString.CopyFrom(rawPublicKey.ToArray()) }).ToByteString().Memory;

        var endorsement = new Endorsement(KeyType.ECDSASecp256K1, rawPublicKey);

        AssertHg.Equal(derPublicKey, endorsement.ToBytes(KeyFormat.Default));
        AssertHg.Equal(rawPublicKey, endorsement.ToBytes(KeyFormat.Raw));
        AssertHg.Equal(derPublicKey, endorsement.ToBytes(KeyFormat.Der));
        AssertHg.Equal(prtPublicKey, endorsement.ToBytes(KeyFormat.Protobuf));
        AssertHg.Equal(hdrPublicKey, endorsement.ToBytes(KeyFormat.Hedera));
        AssertHg.Equal(rawPublicKey, endorsement.ToBytes(KeyFormat.Mirror));
    }
    [Fact(DisplayName = "Endorsements: Can Extract Contract Bytes in Various Formats")]
    public void CanExtractContractBytesInVariousFormats()
    {
        var contract = new Address(Generator.Integer(0, 5), Generator.Integer(1, 5), Generator.Integer(1001, 1000000));

        ReadOnlyMemory<byte> prtPublicKey = (new Proto.Key { ContractID = new Proto.ContractID(contract) }).ToByteString().Memory;

        var endorsement = new Endorsement(contract);

        AssertHg.Equal(prtPublicKey, endorsement.ToBytes(KeyFormat.Default));
        AssertHg.Equal(ReadOnlyMemory<byte>.Empty, endorsement.ToBytes(KeyFormat.Raw));
        AssertHg.Equal(ReadOnlyMemory<byte>.Empty, endorsement.ToBytes(KeyFormat.Der));
        AssertHg.Equal(prtPublicKey, endorsement.ToBytes(KeyFormat.Protobuf));
        AssertHg.Equal(prtPublicKey, endorsement.ToBytes(KeyFormat.Hedera));
        AssertHg.Equal(prtPublicKey, endorsement.ToBytes(KeyFormat.Mirror));
    }
    [Fact(DisplayName = "Endorsements: Can Extract List Bytes in Various Formats")]
    public void CanExtractListBytesInVariousFormats()
    {
        var (derPublicKey, _) = Generator.Secp256k1KeyPair();
        var inner = new Endorsement(derPublicKey);
        var endorsement = new Endorsement(1, inner);

        var contract = new Address(Generator.Integer(0, 5), Generator.Integer(1, 5), Generator.Integer(1001, 1000000));
        ReadOnlyMemory<byte> prtPublicKey = (new Proto.Key(endorsement)).ToByteString().Memory;

        AssertHg.Equal(prtPublicKey, endorsement.ToBytes(KeyFormat.Default));
        AssertHg.Equal(ReadOnlyMemory<byte>.Empty, endorsement.ToBytes(KeyFormat.Raw));
        AssertHg.Equal(ReadOnlyMemory<byte>.Empty, endorsement.ToBytes(KeyFormat.Der));
        AssertHg.Equal(prtPublicKey, endorsement.ToBytes(KeyFormat.Protobuf));
        AssertHg.Equal(prtPublicKey, endorsement.ToBytes(KeyFormat.Hedera));
        AssertHg.Equal(prtPublicKey, endorsement.ToBytes(KeyFormat.Mirror));
    }
    [Fact(DisplayName = "Endorsements: Can Verify a valid Ed25519 Signature")]
    public async Task CanVerifyAvalidEd25519Signature()
    {
        var (publicKey, privateKey) = Generator.Ed25519KeyPair();

        var endorsement = new Endorsement(publicKey);
        var signatory = new Signatory(privateKey);
        var message = Generator.Secp256k1KeyPair().publicKey;
        var sigMap = new SignatureMap();
        await sigMap.AddSignatureAsync(message, signatory);
        var signature = sigMap.SigPair[0].Ed25519.ToByteArray();

        Assert.True(endorsement.Verify(message, signature));
    }
    [Fact(DisplayName = "Endorsements: Can Not Verify an invalid Ed25519 Signature")]
    public async Task CannotVerifyAnInvalidEd25519Signature()
    {
        var (publicKey, _) = Generator.Ed25519KeyPair();
        var (_, privateKey) = Generator.Ed25519KeyPair();

        var endorsement = new Endorsement(publicKey);
        var signatory = new Signatory(privateKey);
        var message = Generator.Secp256k1KeyPair().publicKey;
        var sigMap = new SignatureMap();
        await sigMap.AddSignatureAsync(message, signatory);
        var signature = sigMap.SigPair[0].Ed25519.ToByteArray();

        Assert.False(endorsement.Verify(message, signature));
    }
    [Fact(DisplayName = "Endorsements: Can Verify a valid Secp256k1KeyPair Signature")]
    public async Task CanVerifyAvalidSecp256k1KeyPairSignature()
    {
        var (publicKey, privateKey) = Generator.Secp256k1KeyPair();

        var endorsement = new Endorsement(publicKey);
        var signatory = new Signatory(privateKey);
        var message = Generator.Ed25519KeyPair().publicKey;
        var sigMap = new SignatureMap();
        await sigMap.AddSignatureAsync(message, signatory);
        var signature = sigMap.SigPair[0].ECDSASecp256K1.ToByteArray();

        Assert.True(endorsement.Verify(message, signature));
    }
    [Fact(DisplayName = "Endorsements: Can Not Verify an invalid Secp256k1KeyPair Signature")]
    public async Task CannotVerifyAnInvalidSecp256k1KeyPairSignature()
    {
        var (publicKey, _) = Generator.Secp256k1KeyPair();
        var (_, privateKey) = Generator.Secp256k1KeyPair();

        var endorsement = new Endorsement(publicKey);
        var signatory = new Signatory(privateKey);
        var message = Generator.Ed25519KeyPair().publicKey;
        var sigMap = new SignatureMap();
        await sigMap.AddSignatureAsync(message, signatory);
        var signature = sigMap.SigPair[0].ECDSASecp256K1.ToByteArray();

        Assert.False(endorsement.Verify(message, signature));
    }
    [Fact(DisplayName = "Endorsements: Attempt to Verify A List Endorsment Raises Error")]
    public async Task AttemptToVerifyAListEndorsmentRaisesError()
    {
        var (publicKey1, privateKey1) = Generator.KeyPair();
        var (publicKey2, privateKey2) = Generator.KeyPair();

        var endorsement = new Endorsement(new Endorsement(publicKey1), new Endorsement(publicKey2));
        var signatory = new Signatory(new Signatory(privateKey1), new Signatory(privateKey2));
        var message = Generator.Ed25519KeyPair().publicKey;
        var sigMap = new SignatureMap();
        await sigMap.AddSignatureAsync(message, signatory);
        var signature = sigMap.SigPair[0].ECDSASecp256K1.ToByteArray();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            endorsement.Verify(message, signature);
        });
        Assert.StartsWith("Only endorsements representing single Ed25519 or ECDSASecp256K1 keys support validation of signatures, use SigPair.Satisfies for complex public key types.", ex.Message);
    }
    [Fact(DisplayName = "Endorsements: Attempt to Verify A Contract Endorsement Raises Error")]
    public async Task AttemptToVerifyAContractEndorsmentRaisesError()
    {
        var (_, privateKey) = Generator.KeyPair();

        var endorsement = new Endorsement(new Address(0, 0, Generator.Integer(1000, 2000)));
        var signatory = new Signatory(privateKey);
        var message = Generator.Ed25519KeyPair().publicKey;
        var sigMap = new SignatureMap();
        await sigMap.AddSignatureAsync(message, signatory);
        var signature = sigMap.SigPair[0].ECDSASecp256K1.ToByteArray();

        var ex = Assert.Throws<InvalidOperationException>(() =>
        {
            endorsement.Verify(message, signature);
        });
        Assert.StartsWith("Only endorsements representing single Ed25519 or ECDSASecp256K1 keys support validation of signatures, unable to validate Contract key type.", ex.Message);
    }
}