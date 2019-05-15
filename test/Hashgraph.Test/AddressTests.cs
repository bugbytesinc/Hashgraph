using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests
{
    public class AddressTests
    {
        [Fact(DisplayName = "Address: Equivalent Addresses are considered Equal")]
        public void EquivalentAddressAreConsideredEqual()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(0, 200);
            var address1 = new Address(realmNum, shardNum, accountNum);
            var address2 = new Address(realmNum, shardNum, accountNum);
            Assert.Equal(address1, address2);
            Assert.True(address1 == address2);
            Assert.False(address1 != address2);
        }
        [Fact(DisplayName = "Address: Disimilar Addresses are not considered Equal")]
        public void DisimilarAddressesAreNotConsideredEqual()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(0, 200);
            var address1 = new Address(realmNum, shardNum, accountNum);
            Assert.NotEqual(address1, new Address(realmNum + 1, shardNum, accountNum));
            Assert.NotEqual(address1, new Address(realmNum, shardNum + 1, accountNum));
            Assert.NotEqual(address1, new Address(realmNum, shardNum, accountNum + 1));
            Assert.False(address1 == new Address(realmNum, shardNum, accountNum + 1));
            Assert.True(address1 != new Address(realmNum, shardNum, accountNum + 1));
        }
    }
}
