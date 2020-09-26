using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests
{
    public class TokenAddressTransferTests
    {
        [Fact(DisplayName = "TokenAddressTransfer: Can Create Token Transfer Object")]
        public void CanCreateTokenAddressTransferObject()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt = new TokenAddressTransfer(token, address, amount);
            Assert.Equal(token, tt.Token);
            Assert.Equal(address, tt.Address);
            Assert.Equal(amount, tt.Amount);
        }
        [Fact(DisplayName = "TokenAddressTransfer: Equivalent TokenAddressTransfers are considered Equal")]
        public void EquivalentTokenAddressTransferAreConsideredEqual()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt1 = new TokenAddressTransfer(token, address, amount);
            var tt2 = new TokenAddressTransfer(token, address, amount);
            Assert.Equal(tt1, tt2);
            Assert.True(tt1 == tt2);
            Assert.False(tt1 != tt2);
            Assert.True(tt1.Equals(tt2));
            Assert.True(tt2.Equals(tt1));
            Assert.True(null as TokenAddressTransfer == null as TokenAddressTransfer);
        }
        [Fact(DisplayName = "TokenAddressTransfer: Disimilar TokenAddressTransfers are not considered Equal")]
        public void DisimilarTokenAddressTransfersAreNotConsideredEqual()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            var other = new Address(0, 0, Generator.Integer(500, 600));
            long amount = Generator.Integer(500, 600);
            var tt = new TokenAddressTransfer(token, address, amount);
            Assert.NotEqual(tt, new TokenAddressTransfer(token, other, amount));
            Assert.NotEqual(tt, new TokenAddressTransfer(other, address, amount));
            Assert.NotEqual(tt, new TokenAddressTransfer(token, address, amount + 1));
            Assert.False(tt == new TokenAddressTransfer(token, address, amount + 1));
            Assert.True(tt != new TokenAddressTransfer(token, address, amount + 1));
            Assert.False(tt.Equals(new TokenAddressTransfer(other, address, amount)));
            Assert.False(tt.Equals(new TokenAddressTransfer(token, other, amount)));
            Assert.False(tt.Equals(new TokenAddressTransfer(token, address, amount + 1)));
        }
        [Fact(DisplayName = "TokenAddressTransfer: Comparing with null are not considered equal.")]
        public void NullTokenAddressTransfersAreNotConsideredEqual()
        {
            object asNull = null;
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt = new TokenAddressTransfer(token, address, amount);
            Assert.False(tt == null);
            Assert.False(null == tt);
            Assert.True(tt != null);
            Assert.False(tt.Equals(null));
            Assert.False(tt.Equals(asNull));
        }
        [Fact(DisplayName = "TokenAddressTransfer: Comparing with other objects are not considered equal.")]
        public void OtherObjectsAreNotConsideredEqual()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt = new TokenAddressTransfer(token, address, amount);
            Assert.False(tt.Equals("Something that is not an TokenAddressTransfer"));
        }
        [Fact(DisplayName = "TokenAddressTransfer: TokenAddressTransfer cast as object still considered equivalent.")]
        public void TokenAddressTransferCastAsObjectIsconsideredEqual()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt = new TokenAddressTransfer(token, address, amount);
            object equivalent = new TokenAddressTransfer(token, address, amount);
            Assert.True(tt.Equals(equivalent));
            Assert.True(equivalent.Equals(tt));
        }
        [Fact(DisplayName = "TokenAddressTransfer: TokenAddressTransfer as objects but reference equal are same.")]
        public void ReferenceEqualIsconsideredEqual()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt = new TokenAddressTransfer(token, address, amount);
            object reference = tt;
            Assert.True(tt.Equals(reference));
            Assert.True(reference.Equals(tt));
        }
        [Fact(DisplayName = "TokenAddressTransfer: Can Create New Token Transfer Records with the Add Method")]
        public void CombinAmountsForNewTokenAddressTransferRecord()
        {
            var token = new Address(0, 0, Generator.Integer(0, 200));
            var address = new Address(0, 0, Generator.Integer(200, 400));
            long amount = Generator.Integer(500, 600);
            var tt1 = new TokenAddressTransfer(token, address, amount);
            var tt2 = tt1.Add(amount);
            var tt3 = tt2.Add(-amount);
            Assert.Equal(amount, tt1.Amount);
            Assert.Equal(amount * 2, tt2.Amount);
            Assert.Equal(amount, tt3.Amount);
            Assert.True(tt1.Equals(tt3));
            Assert.True(tt3.Equals(tt1));
        }
    }
}
