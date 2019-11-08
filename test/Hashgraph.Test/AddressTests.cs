using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests
{
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
            Assert.True(null as Address== null as Address);
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
        public void OtherObjectsAreNotConsideredEqual()
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
    }
}
