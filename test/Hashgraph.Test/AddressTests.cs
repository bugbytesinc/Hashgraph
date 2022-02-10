using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests;

public class AddressTests
{
    [Fact(DisplayName = "Address: Equivalent Addresses are considered Equal")]
    public void EquivalentAddressAreConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var address1 = new Address(shardNum, realmNum, accountNum);
        var address2 = new Address(shardNum, realmNum, accountNum);
        Assert.Equal(address1, address2);
        Assert.True(address1 == address2);
        Assert.False(address1 != address2);
        Assert.True(address1.Equals(address2));
        Assert.True(address2.Equals(address1));
        Assert.True(null as Address == null as Address);
    }
    [Fact(DisplayName = "Address: Disimilar Addresses are not considered Equal")]
    public void DisimilarAddressesAreNotConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var address1 = new Address(shardNum, realmNum, accountNum);
        Assert.NotEqual(address1, new Address(shardNum, realmNum + 1, accountNum));
        Assert.NotEqual(address1, new Address(shardNum + 1, realmNum, accountNum));
        Assert.NotEqual(address1, new Address(shardNum, realmNum, accountNum + 1));
        Assert.False(address1 == new Address(shardNum, realmNum, accountNum + 1));
        Assert.True(address1 != new Address(shardNum, realmNum, accountNum + 1));
        Assert.False(address1.Equals(new Address(shardNum + 1, realmNum, accountNum)));
        Assert.False(address1.Equals(new Address(shardNum, realmNum + 1, accountNum)));
        Assert.False(address1.Equals(new Address(shardNum, realmNum, accountNum + 1)));

        Assert.False(address1.TryGetAlias(out Alias alias));
        Assert.Null(alias);

        Assert.False(address1.TryGetMoniker(out Moniker moniker));
        Assert.Null(moniker);
    }
    [Fact(DisplayName = "Address: Comparing with null are not considered equal.")]
    public void NullAddressesAreNotConsideredEqual()
    {
        object asNull = null;
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var address = new Address(shardNum, realmNum, accountNum);
        Assert.False(address == null);
        Assert.False(null == address);
        Assert.True(address != null);
        Assert.False(address.Equals(null));
        Assert.False(address.Equals(asNull));
    }
    [Fact(DisplayName = "Address: Comparing with other objects are not considered equal.")]
    public void OtherObjectsAreNotConsideredEqualToAddress()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var address = new Address(shardNum, realmNum, accountNum);
        Assert.False(address.Equals("Something that is not an address"));
    }
    [Fact(DisplayName = "Address: Address cast as object still considered equivalent.")]
    public void AddressCastAsObjectIsconsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var address = new Address(shardNum, realmNum, accountNum);
        object equivalent = new Address(shardNum, realmNum, accountNum);
        Assert.True(address.Equals(equivalent));
        Assert.True(equivalent.Equals(address));
    }
    [Fact(DisplayName = "Address: Address as objects but reference equal are same.")]
    public void ReferenceEqualIsconsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var address = new Address(shardNum, realmNum, accountNum);
        object reference = address;
        Assert.True(address.Equals(reference));
        Assert.True(reference.Equals(address));
    }

    [Fact(DisplayName = "Address: Equivalent Aliases Addresses are considered Equal")]
    public void EquivalentAliasAddressesAreConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var alias1 = new Address(new Alias(shardNum, realmNum, publicKey));
        var alias2 = new Address(new Alias(shardNum, realmNum, publicKey));
        Assert.Equal(alias1, alias2);
        Assert.True(alias1 == alias2);
        Assert.False(alias1 != alias2);
        Assert.True(alias1.Equals(alias2));
        Assert.True(alias2.Equals(alias1));
        Assert.True(null as Alias == null as Alias);
    }
    [Fact(DisplayName = "Address: Disimilar Alias Addresses are not considered Equal")]
    public void DisimilarAliasAddressessAreNotConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey1, _) = Generator.KeyPair();
        var (publicKey2, _) = Generator.KeyPair();
        var alias1 = new Address( new Alias(shardNum, realmNum, publicKey1));
        Assert.NotEqual(alias1, new Address(new Alias(shardNum, realmNum + 1, publicKey1)));
        Assert.NotEqual(alias1, new Address(new Alias(shardNum + 1, realmNum, publicKey1)));
        Assert.NotEqual(alias1, new Address(new Alias(shardNum, realmNum, publicKey2)));
        Assert.False(alias1 == new Address(new Alias(shardNum, realmNum, publicKey2)));
        Assert.True(alias1 != new Address(new Alias(shardNum, realmNum, publicKey2)));
        Assert.False(alias1.Equals(new Address(new Alias(shardNum + 1, realmNum, publicKey1))));
        Assert.False(alias1.Equals(new Address(new Alias(shardNum, realmNum + 1, publicKey1))));
        Assert.False(alias1.Equals(new Address(new Alias(shardNum, realmNum, publicKey2))));
    }
    [Fact(DisplayName = "Address: Comparing Alias Addresses with null are not considered equal.")]
    public void NullAliasAddressesAreNotConsideredEqual()
    {
        object asNull = null;
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var alias = new Address(new Alias(shardNum, realmNum, publicKey));
        Assert.False(alias == null);
        Assert.False(null == alias);
        Assert.True(alias != null);
        Assert.False(alias.Equals(null));
        Assert.False(alias.Equals(asNull));
    }
    [Fact(DisplayName = "Address: Alias Address cast as object still considered equivalent.")]
    public void AliasCastAsObjectIsconsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var alias = new Address(new Alias(shardNum, realmNum, publicKey));
        object equivalent = new Address(new Alias(shardNum, realmNum, publicKey));
        Assert.True(alias.Equals(equivalent));
        Assert.True(equivalent.Equals(alias));
    }
    [Fact(DisplayName = "Address: Alia Addresses as objects but reference equal are same.")]
    public void ReferenceEqualIsconsideredEqualAsAlias()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var alias = new Address(new Alias(shardNum, realmNum, publicKey));
        object reference = alias;
        Assert.True(alias.Equals(reference));
        Assert.True(reference.Equals(alias));
    }
    [Fact(DisplayName = "Address: Can Create Equivalent Alias with Different Constructors")]
    public void CanCreateEquivalentAliasAddressesWithDifferentConstructors()
    {
        var (publicKey, _) = Generator.KeyPair();
        var endorsement = new Endorsement(publicKey);
        var alias1 = new Address( new Alias(publicKey));
        var alias2 = new Address( new Alias(endorsement));
        var alias3 = new Address( new Alias(0, 0, publicKey));
        var alias4 = new Address(new Alias(0, 0, endorsement));
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
    [Fact(DisplayName = "Address: Equivalent Moniker Addresses are considered Equal")]
    public void EquivalentMonikerAddressesAreConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var bytes = Generator.KeyPair().publicKey[^20..];
        var moniker1 = new Address(new Moniker(shardNum, realmNum, bytes));
        var moniker2 = new Address(new Moniker(shardNum, realmNum, bytes));
        Assert.Equal(moniker1, moniker2);
        Assert.True(moniker1 == moniker2);
        Assert.False(moniker1 != moniker2);
        Assert.True(moniker1.Equals(moniker2));
        Assert.True(moniker2.Equals(moniker1));
        Assert.True(null as Moniker == null as Moniker);
        Assert.True(Moniker.None.Equals(Moniker.None));
    }
    [Fact(DisplayName = "Address: Disimilar Moniker Addresses are not considered Equal")]
    public void DisimilarMonikerAddressesAreNotConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var bytes1 = Generator.KeyPair().publicKey[^20..];
        var bytes2 = Generator.KeyPair().publicKey[^20..];
        var moniker1 = new Address(new Moniker(shardNum, realmNum, bytes1));
        Assert.NotEqual(moniker1, new Address(new Moniker(shardNum, realmNum + 1, bytes1)));
        Assert.NotEqual(moniker1, new Address(new Moniker(shardNum + 1, realmNum, bytes1)));
        Assert.NotEqual(moniker1, new Address(new Moniker(shardNum, realmNum, bytes2)));
        Assert.False(moniker1 == new Address(new Moniker(shardNum, realmNum, bytes2)));
        Assert.True(moniker1 != new Address(new Moniker(shardNum, realmNum, bytes2)));
        Assert.False(moniker1.Equals(new Address(new Moniker(shardNum + 1, realmNum, bytes1))));
        Assert.False(moniker1.Equals(new Address(new Moniker(shardNum, realmNum + 1, bytes1))));
        Assert.False(moniker1.Equals(new Address(new Moniker(shardNum, realmNum, bytes2))));
    }
    [Fact(DisplayName = "Address: Comparing Moniker Addresses with null are not considered equal.")]
    public void NullMonikerAddressesAreNotConsideredEqual()
    {
        object asNull = null;
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var bytes = Generator.KeyPair().publicKey[^20..];
        var moniker = new Address(new Moniker(shardNum, realmNum, bytes));
        Assert.False(moniker == null);
        Assert.False(null == moniker);
        Assert.True(moniker != null);
        Assert.False(moniker.Equals(null));
        Assert.False(moniker.Equals(asNull));
        Assert.False(moniker.Equals(Moniker.None));
    }
    [Fact(DisplayName = "Address: Moniker Address cast as object still considered equivalent.")]
    public void MonikerCastAsObjectIsconsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var bytes = Generator.KeyPair().publicKey[^20..];
        var moniker = new Address( new Moniker(shardNum, realmNum, bytes));
        object equivalent = new Address( new Moniker(shardNum, realmNum, bytes));
        Assert.True(moniker.Equals(equivalent));
        Assert.True(equivalent.Equals(moniker));
    }
    [Fact(DisplayName = "Address: Moniker Addresses as objects but reference equal are same.")]
    public void ReferenceEqualIsconsideredEqualWithAddressMonikers()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var bytes = Generator.KeyPair().publicKey[^20..];
        var moniker = new Address(new Moniker(shardNum, realmNum, bytes));
        object reference = moniker;
        Assert.True(moniker.Equals(reference));
        Assert.True(reference.Equals(moniker));
    }
    [Fact(DisplayName = "Address: Can Create Equivalent Moniker Address with Different Constructors")]
    public void CanCreateEquivalentMonikerWithDifferentConstructors()
    {
        var bytes = Generator.KeyPair().publicKey[^20..];
        var moniker1 = new Address( new Moniker(bytes));
        var moniker2 = new Address(new Moniker(0, 0, bytes));
        Assert.Equal(moniker1, moniker2);
        Assert.True(moniker1 == moniker2);
        Assert.False(moniker1 != moniker2);
        Assert.True(moniker1.Equals(moniker2));
    }
    [Fact(DisplayName = "Address: Different Address Types are not considered Equal")]
    public void DifferentAddressTypesAreNotConsideredEqual()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var address = new Address(shardNum, realmNum, accountNum);
        var moniker = new Address(new Moniker(shardNum, realmNum, Generator.KeyPair().publicKey[^20..]));
        var alias = new Address(new Alias(shardNum, realmNum, new Endorsement(Generator.KeyPair().publicKey)));

        Assert.False(address == moniker);
        Assert.False(moniker == alias);
        Assert.False(alias == address);
        Assert.True(address != moniker);
        Assert.True(moniker != alias);
        Assert.True(alias != address);

        Assert.Equal(shardNum, address.ShardNum);
        Assert.Equal(shardNum, alias.ShardNum);
        Assert.Equal(shardNum, moniker.ShardNum);
        Assert.Equal(realmNum, address.RealmNum);
        Assert.Equal(realmNum, alias.RealmNum);
        Assert.Equal(realmNum, moniker.RealmNum);
    }
    [Fact(DisplayName = "Address: Can Extract Moniker from Address")]
    public void CanExtractMonikerFromAddress()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var bytes = Generator.KeyPair().publicKey[^20..];
        var address = new Address(new Moniker(shardNum, realmNum, bytes));
        var moniker1 = new Moniker(shardNum, realmNum, bytes);
        
        Assert.True(address.TryGetMoniker(out Moniker moniker2));
        Assert.True(moniker1 == moniker2);

        Assert.False(address.TryGetAlias(out Alias alias));
        Assert.Null(alias);
    }
    [Fact(DisplayName = "Address: Can Extract Alias from Address")]
    public void CanExtractAliasFromAddress()
    {
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var (publicKey, _) = Generator.KeyPair();
        var address = new Address(new Alias(shardNum, realmNum, publicKey));
        var alias1 = new Address(new Alias(shardNum, realmNum, publicKey));

        Assert.True(address.TryGetAlias(out Alias alias2));
        Assert.True(alias1 == alias2);

        Assert.False(address.TryGetMoniker(out Moniker moniker));
        Assert.Null(moniker);
    }

}
