using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests;

public class AddressOrAliasTests
{
    [Fact(DisplayName = "AddressOrAlias: Equivalent Address are considered Equal")]
    public void EquivalentAddressAreConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var address1 = new Address(shardNum, realmNum, accountNum);
        var address2 = new Address(shardNum, realmNum, accountNum);
        var addressOrAlias1 = new AddressOrAlias(address1);
        var addressOrAlias2 = new AddressOrAlias(address2);
        Assert.Equal(addressOrAlias1, addressOrAlias2);
        Assert.True(addressOrAlias1 == addressOrAlias2);
        Assert.False(addressOrAlias1 != addressOrAlias2);
        Assert.True(addressOrAlias1.Equals(addressOrAlias2));
        Assert.True(addressOrAlias2.Equals(addressOrAlias1));
        Assert.True(null as AddressOrAlias == null as AddressOrAlias);
    }
    [Fact(DisplayName = "AddressOrAlias: Equivalent Aliases are considered Equal")]
    public void EquivalentAliasAreConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var addressOrAlias1 = new AddressOrAlias(new Alias(shardNum, realmNum, publicKey));
        var addressOrAlias2 = new AddressOrAlias(new Alias(shardNum, realmNum, publicKey));
        Assert.Equal(addressOrAlias1, addressOrAlias2);
        Assert.True(addressOrAlias1 == addressOrAlias2);
        Assert.False(addressOrAlias1 != addressOrAlias2);
        Assert.True(addressOrAlias1.Equals(addressOrAlias2));
        Assert.True(addressOrAlias2.Equals(addressOrAlias1));
        Assert.True(null as Alias == null as Alias);
    }
    [Fact(DisplayName = "AddressOrAlias: Disimilar Address are not considered Equal")]
    public void DisimilarAddressAreNotConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var addressOrAlias1 = new AddressOrAlias(new Address(shardNum, realmNum, accountNum));
        Assert.NotEqual(addressOrAlias1, new AddressOrAlias(new Address(shardNum, realmNum + 1, accountNum)));
        Assert.NotEqual(addressOrAlias1, new AddressOrAlias(new Address(shardNum + 1, realmNum, accountNum)));
        Assert.NotEqual(addressOrAlias1, new AddressOrAlias(new Address(shardNum, realmNum, accountNum + 1)));
        Assert.False(addressOrAlias1 == new AddressOrAlias(new Address(shardNum, realmNum, accountNum + 1)));
        Assert.True(addressOrAlias1 != new AddressOrAlias(new Address(shardNum, realmNum, accountNum + 1)));
        Assert.False(addressOrAlias1.Equals(new AddressOrAlias(new Address(shardNum + 1, realmNum, accountNum))));
        Assert.False(addressOrAlias1.Equals(new AddressOrAlias(new Address(shardNum, realmNum + 1, accountNum))));
        Assert.False(addressOrAlias1.Equals(new AddressOrAlias(new Address(shardNum, realmNum, accountNum + 1))));
    }
    [Fact(DisplayName = "AddressOrAlias: Disimilar Aliases are not considered Equal")]
    public void DisimilarAliasesAreNotConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey1, _) = Generator.KeyPair();
        var (publicKey2, _) = Generator.KeyPair();
        var alias1 = new AddressOrAlias(new Alias(shardNum, realmNum, publicKey1));
        Assert.NotEqual(alias1, new AddressOrAlias(new Alias(shardNum, realmNum + 1, publicKey1)));
        Assert.NotEqual(alias1, new AddressOrAlias(new Alias(shardNum + 1, realmNum, publicKey1)));
        Assert.NotEqual(alias1, new AddressOrAlias(new Alias(shardNum, realmNum, publicKey2)));
        Assert.False(alias1 == new AddressOrAlias(new Alias(shardNum, realmNum, publicKey2)));
        Assert.True(alias1 != new AddressOrAlias(new Alias(shardNum, realmNum, publicKey2)));
        Assert.False(alias1.Equals(new AddressOrAlias(new Alias(shardNum + 1, realmNum, publicKey1))));
        Assert.False(alias1.Equals(new AddressOrAlias(new Alias(shardNum, realmNum + 1, publicKey1))));
        Assert.False(alias1.Equals(new AddressOrAlias(new Alias(shardNum, realmNum, publicKey2))));
    }
    [Fact(DisplayName = "AddressOrAlias: Address and Alias Types are not Equivalent")]
    public void AddressAndAliasTypesAreNotEquivalent()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var acctNum = Generator.Integer(0, 200);
        var (publicKey1, _) = Generator.KeyPair();
        var (publicKey2, _) = Generator.KeyPair();
        var addressOrAlias1 = new AddressOrAlias(new Alias(shardNum, realmNum, publicKey1));
        var addressOrAlias2 = new AddressOrAlias(new Address(shardNum, realmNum, acctNum));
        Assert.NotEqual(addressOrAlias1, addressOrAlias2);
        Assert.False(addressOrAlias1 == addressOrAlias2);
        Assert.True(addressOrAlias1 != addressOrAlias2);
        Assert.False(addressOrAlias1.Equals(addressOrAlias2));
    }
    [Fact(DisplayName = "AddressOrAlias: Comparing with null are not considered equal.")]
    public void NullAddressAreNotConsideredEqual()
    {
        object asNull = null;
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var addressOrAlias = new AddressOrAlias(new Address(shardNum, realmNum, accountNum));
        Assert.False(addressOrAlias == null);
        Assert.False(null == addressOrAlias);
        Assert.True(addressOrAlias != null);
        Assert.False(addressOrAlias.Equals(null));
        Assert.False(addressOrAlias.Equals(asNull));
    }
    [Fact(DisplayName = "AddressOrAlias: Comparing with null are not considered equal.")]
    public void NullAliasesAreNotConsideredEqual()
    {
        object asNull = null;
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var addressOrAlias = new AddressOrAlias(new Alias(shardNum, realmNum, publicKey));
        Assert.False(addressOrAlias == null);
        Assert.False(null == addressOrAlias);
        Assert.True(addressOrAlias != null);
        Assert.False(addressOrAlias.Equals(null));
        Assert.False(addressOrAlias.Equals(asNull));
    }
    [Fact(DisplayName = "AddressOrAlias: Comparing with other objects are not considered equal.")]
    public void OtherObjectsAreNotConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var addressOrAlias1 = new AddressOrAlias(new Address(shardNum, realmNum, accountNum));
        var addressOrAlias2 = new AddressOrAlias(new Alias(shardNum, realmNum, publicKey));
        Assert.False(addressOrAlias1.Equals("Something that is not an addressOrAlias"));
        Assert.False(addressOrAlias2.Equals("Something that is not an Alias"));
    }
    [Fact(DisplayName = "AddressOrAlias: Address cast as object still considered equivalent.")]
    public void AddressCastAsObjectIsconsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var addressOrAlias = new AddressOrAlias(new Address(shardNum, realmNum, accountNum));
        object equivalent = new AddressOrAlias(new Address(shardNum, realmNum, accountNum));
        Assert.True(addressOrAlias.Equals(equivalent));
        Assert.True(equivalent.Equals(addressOrAlias));
    }
    [Fact(DisplayName = "AddressOrAlias: Alias cast as object still considered equivalent.")]
    public void AliasCastAsObjectIsconsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var alias = new AddressOrAlias(new Alias(shardNum, realmNum, publicKey));
        object equivalent = new AddressOrAlias(new Alias(shardNum, realmNum, publicKey));
        Assert.True(alias.Equals(equivalent));
        Assert.True(equivalent.Equals(alias));
    }
    [Fact(DisplayName = "AddressOrAlias: AddressOrAlias as objects but reference equal are same.")]
    public void ReferenceEqualIsconsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var addressOrAlias1 = new AddressOrAlias(new Address(shardNum, realmNum, accountNum));
        var addressOrAlias2 = new AddressOrAlias(new Alias(shardNum, realmNum, publicKey));
        object reference1 = addressOrAlias1;
        object reference2 = addressOrAlias2;
        Assert.True(addressOrAlias1.Equals(reference1));
        Assert.True(reference1.Equals(addressOrAlias1));
        Assert.True(addressOrAlias2.Equals(reference2));
        Assert.True(reference2.Equals(addressOrAlias2));
    }
    [Fact(DisplayName = "AddressOrAlias: Can Create Equivalent Alias with Different Constructors")]
    public void CanCreateEquivalentAliasWithDifferentConstructors()
    {
        var (publicKey, _) = Generator.KeyPair();
        var endorsement = new Endorsement(publicKey);
        var alias1 = new AddressOrAlias(new Alias(publicKey));
        var alias2 = new AddressOrAlias(new Alias(endorsement));
        var alias3 = new AddressOrAlias(new Alias(0, 0, publicKey));
        var alias4 = new AddressOrAlias(new Alias(0, 0, endorsement));
        var alias5 = new AddressOrAlias(endorsement);
        var alias6 = new AddressOrAlias(publicKey);
        Assert.Equal(alias1, alias2);
        Assert.Equal(alias1, alias3);
        Assert.Equal(alias1, alias4);
        Assert.Equal(alias1, alias5);
        Assert.Equal(alias1, alias6);
        Assert.Equal(alias2, alias3);
        Assert.Equal(alias2, alias4);
        Assert.Equal(alias2, alias5);
        Assert.Equal(alias2, alias6);
        Assert.Equal(alias3, alias4);
        Assert.Equal(alias3, alias5);
        Assert.Equal(alias3, alias6);
        Assert.Equal(alias4, alias5);
        Assert.Equal(alias4, alias6);
        Assert.Equal(alias5, alias6);
        Assert.True(alias1 == alias2);
        Assert.True(alias1 == alias3);
        Assert.True(alias1 == alias4);
        Assert.True(alias1 == alias5);
        Assert.True(alias1 == alias6);
        Assert.True(alias2 == alias3);
        Assert.True(alias2 == alias4);
        Assert.True(alias2 == alias5);
        Assert.True(alias2 == alias6);
        Assert.True(alias3 == alias4);
        Assert.True(alias3 == alias5);
        Assert.True(alias3 == alias6);
        Assert.True(alias4 == alias5);
        Assert.True(alias4 == alias6);
        Assert.True(alias5 == alias6);
        Assert.False(alias1 != alias2);
        Assert.False(alias1 != alias3);
        Assert.False(alias1 != alias4);
        Assert.False(alias1 != alias5);
        Assert.False(alias1 != alias6);
        Assert.False(alias2 != alias3);
        Assert.False(alias2 != alias4);
        Assert.False(alias2 != alias5);
        Assert.False(alias2 != alias6);
        Assert.False(alias3 != alias4);
        Assert.False(alias3 != alias5);
        Assert.False(alias3 != alias6);
        Assert.False(alias4 != alias5);
        Assert.False(alias4 != alias6);
        Assert.False(alias5 != alias6);
        Assert.True(alias1.Equals(alias2));
        Assert.True(alias1.Equals(alias3));
        Assert.True(alias1.Equals(alias4));
        Assert.True(alias1.Equals(alias5));
        Assert.True(alias1.Equals(alias6));
        Assert.True(alias2.Equals(alias3));
        Assert.True(alias2.Equals(alias4));
        Assert.True(alias2.Equals(alias5));
        Assert.True(alias2.Equals(alias6));
        Assert.True(alias3.Equals(alias4));
        Assert.True(alias3.Equals(alias5));
        Assert.True(alias3.Equals(alias6));
        Assert.True(alias4.Equals(alias5));
        Assert.True(alias4.Equals(alias6));
        Assert.True(alias5.Equals(alias6));
    }
}