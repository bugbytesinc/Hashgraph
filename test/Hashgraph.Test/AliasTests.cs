using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests;

public class AliasTests
{
    [Fact(DisplayName = "Alias: Equivalent Aliases are considered Equal")]
    public void EquivalentAliasAreConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var alias1 = new Alias(shardNum, realmNum, publicKey);
        var alias2 = new Alias(shardNum, realmNum, publicKey);
        Assert.Equal(alias1, alias2);
        Assert.True(alias1 == alias2);
        Assert.False(alias1 != alias2);
        Assert.True(alias1.Equals(alias2));
        Assert.True(alias2.Equals(alias1));
        Assert.True(null as Alias == null as Alias);
    }
    [Fact(DisplayName = "Alias: Disimilar Aliases are not considered Equal")]
    public void DisimilarAliasesAreNotConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey1, _) = Generator.KeyPair();
        var (publicKey2, _) = Generator.KeyPair();
        var alias1 = new Alias(shardNum, realmNum, publicKey1);
        Assert.NotEqual(alias1, new Alias(shardNum, realmNum + 1, publicKey1));
        Assert.NotEqual(alias1, new Alias(shardNum + 1, realmNum, publicKey1));
        Assert.NotEqual(alias1, new Alias(shardNum, realmNum, publicKey2));
        Assert.False(alias1 == new Alias(shardNum, realmNum, publicKey2));
        Assert.True(alias1 != new Alias(shardNum, realmNum, publicKey2));
        Assert.False(alias1.Equals(new Alias(shardNum + 1, realmNum, publicKey1)));
        Assert.False(alias1.Equals(new Alias(shardNum, realmNum + 1, publicKey1)));
        Assert.False(alias1.Equals(new Alias(shardNum, realmNum, publicKey2)));
    }
    [Fact(DisplayName = "Alias: Comparing with null are not considered equal.")]
    public void NullAliasesAreNotConsideredEqual()
    {
        object asNull = null;
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var alias = new Alias(shardNum, realmNum, publicKey);
        Assert.False(alias == null);
        Assert.False(null == alias);
        Assert.True(alias != null);
        Assert.False(alias.Equals(null));
        Assert.False(alias.Equals(asNull));
    }
    [Fact(DisplayName = "Alias: Comparing with other objects are not considered equal.")]
    public void OtherObjectsAreNotConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var alias = new Alias(shardNum, realmNum, publicKey);
        Assert.False(alias.Equals("Something that is not an Alias"));
    }
    [Fact(DisplayName = "Alias: Alias cast as object still considered equivalent.")]
    public void AliasCastAsObjectIsconsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var alias = new Alias(shardNum, realmNum, publicKey);
        object equivalent = new Alias(shardNum, realmNum, publicKey);
        Assert.True(alias.Equals(equivalent));
        Assert.True(equivalent.Equals(alias));
    }
    [Fact(DisplayName = "Alias: Alias as objects but reference equal are same.")]
    public void ReferenceEqualIsconsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var alias = new Alias(shardNum, realmNum, publicKey);
        object reference = alias;
        Assert.True(alias.Equals(reference));
        Assert.True(reference.Equals(alias));
    }
    [Fact(DisplayName = "Alias: Can Create Equivalent Alias with Different Constructors")]
    public void CanCreateEquivalentAliasWithDifferentConstructors()
    {
        var (publicKey, _) = Generator.KeyPair();
        var endorsement = new Endorsement(publicKey);
        var alias1 = new Alias(publicKey);
        var alias2 = new Alias(endorsement);
        var alias3 = new Alias(0, 0, publicKey);
        var alias4 = new Alias(0, 0, endorsement);
        Assert.Equal(alias1, alias2);
        Assert.Equal(alias1, alias3);
        Assert.Equal(alias1, alias4);
        Assert.Equal(alias2, alias3);
        Assert.Equal(alias2, alias4);
        Assert.Equal(alias3, alias4);
        Assert.True(alias1 == alias2);
        Assert.True(alias1 == alias3);
        Assert.True(alias1 == alias4);
        Assert.True(alias2 == alias3);
        Assert.True(alias2 == alias4);
        Assert.True(alias3 == alias4);
        Assert.False(alias1 != alias2);
        Assert.False(alias1 != alias3);
        Assert.False(alias1 != alias4);
        Assert.False(alias2 != alias3);
        Assert.False(alias2 != alias4);
        Assert.False(alias3 != alias4);
        Assert.True(alias1.Equals(alias2));
        Assert.True(alias1.Equals(alias3));
        Assert.True(alias1.Equals(alias4));
        Assert.True(alias2.Equals(alias3));
        Assert.True(alias2.Equals(alias4));
        Assert.True(alias3.Equals(alias4));
    }
}