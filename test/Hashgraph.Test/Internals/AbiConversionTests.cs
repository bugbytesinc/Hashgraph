using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Hashgraph.Test.Internals
{
    public class AbiConversionTests
    {
        [Fact(DisplayName = "ABI: Can Pack Address to UInt160")]
        public void CanPackAddressToUint160()
        {
            var address = new Address(2, 1, 3);
            var bytes = Abi.EncodeArguments(new[] { address });
            var hex = Hex.FromBytes(bytes)[^40..^0];
            Assert.Equal("0000000200000000000000010000000000000003", hex);
        }
        [Fact(DisplayName = "ABI: Can Pack and Unpack Address")]
        public void CanPackAndUnpackAddress()
        {
            var shard = Generator.Integer(1, 50);
            var realm = Generator.Integer(1, 50);
            var num = Generator.Integer(1, 50);
            var expected = new Address(shard, realm, num);
            var bytes = Abi.EncodeArguments(new[] { expected });
            var decoded = Abi.DecodeArguments(bytes, typeof(Address));
            Assert.Single(decoded);
            var actual = decoded[0] as Address;
            Assert.NotNull(actual);
            Assert.Equal(expected, actual);
        }
    }
}
