using Hashgraph.Test.Fixtures;
using System;
using Xunit;

namespace Hashgraph.Tests;

public class GatewayTests
{
    [Fact(DisplayName = "Gateway: Equivalent Gateways are considered Equal")]
    public void EquivalentGatewaysAreConsideredEqual()
    {
        var uri = new Uri("http://testnet.hedera.com:123");
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var gateway1 = new Gateway(uri, shardNum, realmNum, accountNum);
        var gateway2 = new Gateway(uri, shardNum, realmNum, accountNum);
        var gateway3 = new Gateway(new Uri("http://testnet.hedera.com:123"), shardNum, realmNum, accountNum);
        Assert.Equal(gateway1, gateway2);
        Assert.Equal(gateway1, gateway3);
        Assert.Equal(gateway2, gateway3);
        Assert.True(gateway1 == gateway2);
        Assert.True(gateway2 == gateway3);
        Assert.True(gateway2 == gateway3);
        Assert.False(gateway1 != gateway2);
        Assert.False(gateway1 != gateway3);
        Assert.False(gateway2 != gateway3);
    }
    [Fact(DisplayName = "Gateway: Disimilar Gateways are not considered Equal")]
    public void DisimilarGatewaysAreNotConsideredEqual()
    {
        var uri = new Uri("http://testnet.hedera.com:123");
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        var gateway1 = new Gateway(uri, shardNum, realmNum, accountNum);
        Assert.NotEqual(gateway1, new Gateway(new Uri("http://testnet.hedera.com:1234"), shardNum, realmNum, accountNum));
        Assert.NotEqual(gateway1, new Gateway(uri, shardNum, realmNum + 1, accountNum));
        Assert.NotEqual(gateway1, new Gateway(uri, shardNum + 1, realmNum, accountNum));
        Assert.NotEqual(gateway1, new Gateway(uri, shardNum, realmNum, accountNum + 1));
        Assert.False(gateway1 == new Gateway(uri, shardNum, realmNum + 1, accountNum));
        Assert.True(gateway1 != new Gateway(uri, shardNum, realmNum + 1, accountNum));
    }
}