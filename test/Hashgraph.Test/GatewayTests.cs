using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests
{
    public class GatewayTests
    {
        [Fact(DisplayName = "Gateway: Equivalent Gateways are considered Equal")]
        public void EquivalentGatewaysAreConsideredEqual()
        {
            var url = "testnet.hedera.com:123";
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(0, 200);
            var gateway1 = new Gateway(url, realmNum, shardNum, accountNum);
            var gateway2 = new Gateway(url, realmNum, shardNum, accountNum);
            Assert.Equal(gateway1, gateway2);
            Assert.True(gateway1 == gateway2);
            Assert.False(gateway1 != gateway2);
        }
        [Fact(DisplayName = "Gateway: Disimilar Gateways are not considered Equal")]
        public void DisimilarGatewaysAreNotConsideredEqual()
        {
            var url = "testnet.hedera.com:123";
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(0, 200);
            var gateway1 = new Gateway(url, realmNum, shardNum, accountNum);
            Assert.NotEqual(gateway1, new Gateway(url + "1", realmNum, shardNum, accountNum));
            Assert.NotEqual(gateway1, new Gateway(url, realmNum + 1, shardNum, accountNum));
            Assert.NotEqual(gateway1, new Gateway(url, realmNum, shardNum + 1, accountNum));
            Assert.NotEqual(gateway1, new Gateway(url, realmNum, shardNum, accountNum + 1));
            Assert.False(gateway1 == new Gateway(url, realmNum + 1, shardNum, accountNum));
            Assert.True(gateway1 != new Gateway(url, realmNum + 1, shardNum, accountNum));
        }
    }
}
