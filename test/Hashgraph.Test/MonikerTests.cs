using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests;

public class MonikerTests
{
    [Fact(DisplayName = "Moniker: Equivalent Monikeres are considered Equal")]
    public void EquivalentMonikerAreConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var bytes = Generator.KeyPair().publicKey[^20..];
        var moniker1 = new Moniker(shardNum, realmNum, bytes);
        var moniker2 = new Moniker(shardNum, realmNum, bytes);
        Assert.Equal(moniker1, moniker2);
        Assert.True(moniker1 == moniker2);
        Assert.False(moniker1 != moniker2);
        Assert.True(moniker1.Equals(moniker2));
        Assert.True(moniker2.Equals(moniker1));
        Assert.True(null as Moniker == null as Moniker);
        Assert.True(Moniker.None.Equals(Moniker.None));
    }
    [Fact(DisplayName = "Moniker: Disimilar Monikeres are not considered Equal")]
    public void DisimilarMonikeresAreNotConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var bytes1 = Generator.KeyPair().publicKey[^20..];
        var bytes2 = Generator.KeyPair().publicKey[^20..];
        var moniker1 = new Moniker(shardNum, realmNum, bytes1);
        Assert.NotEqual(moniker1, new Moniker(shardNum, realmNum + 1, bytes1));
        Assert.NotEqual(moniker1, new Moniker(shardNum + 1, realmNum, bytes1));
        Assert.NotEqual(moniker1, new Moniker(shardNum, realmNum, bytes2));
        Assert.False(moniker1 == new Moniker(shardNum, realmNum, bytes2));
        Assert.True(moniker1 != new Moniker(shardNum, realmNum, bytes2));
        Assert.False(moniker1.Equals(new Moniker(shardNum + 1, realmNum, bytes1)));
        Assert.False(moniker1.Equals(new Moniker(shardNum, realmNum + 1, bytes1)));
        Assert.False(moniker1.Equals(new Moniker(shardNum, realmNum, bytes2)));
    }
    [Fact(DisplayName = "Moniker: Comparing with null are not considered equal.")]
    public void NullMonikeresAreNotConsideredEqual()
    {
        object asNull = null;
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var bytes = Generator.KeyPair().publicKey[^20..];
        var moniker = new Moniker(shardNum, realmNum, bytes);
        Assert.False(moniker == null);
        Assert.False(null == moniker);
        Assert.True(moniker != null);
        Assert.False(moniker.Equals(null));
        Assert.False(moniker.Equals(asNull));
        Assert.False(moniker.Equals(Moniker.None));
    }
    [Fact(DisplayName = "Moniker: Comparing with other objects are not considered equal.")]
    public void OtherObjectsAreNotConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var bytes = Generator.KeyPair().publicKey[^20..];
        var moniker = new Moniker(shardNum, realmNum, bytes);
        Assert.False(moniker.Equals("Something that is not an Moniker"));
    }
    [Fact(DisplayName = "Moniker: Moniker cast as object still considered equivalent.")]
    public void MonikerCastAsObjectIsconsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var bytes = Generator.KeyPair().publicKey[^20..];
        var moniker = new Moniker(shardNum, realmNum, bytes);
        object equivalent = new Moniker(shardNum, realmNum, bytes);
        Assert.True(moniker.Equals(equivalent));
        Assert.True(equivalent.Equals(moniker));
    }
    [Fact(DisplayName = "Moniker: Moniker as objects but reference equal are same.")]
    public void ReferenceEqualIsconsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var bytes = Generator.KeyPair().publicKey[^20..];
        var moniker = new Moniker(shardNum, realmNum, bytes);
        object reference = moniker;
        Assert.True(moniker.Equals(reference));
        Assert.True(reference.Equals(moniker));
    }
    [Fact(DisplayName = "Moniker: Can Create Equivalent Moniker with Different Constructors")]
    public void CanCreateEquivalentMonikerWithDifferentConstructors()
    {
        var bytes = Generator.KeyPair().publicKey[^20..];
        var moniker1 = new Moniker(bytes);
        var moniker2 = new Moniker(0, 0, bytes);
        Assert.Equal(moniker1, moniker2);
        Assert.True(moniker1 == moniker2);
        Assert.False(moniker1 != moniker2);
        Assert.True(moniker1.Equals(moniker2));
    }
}