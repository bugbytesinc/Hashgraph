using Hashgraph.Test.Fixtures;
using Xunit;

namespace Hashgraph.Tests
{
    public class TokenIdentifierTests
    {
        [Fact(DisplayName = "TokenIdentifier: Can Create Token Transfer Object from Address")]
        public void CanCreateTokenIdentifierObjectFromAddress()
        {
            var address = new Address(0, 0, Generator.Integer(200, 400));
            var identifier = new TokenIdentifier(address);
            Assert.Equal(address, identifier.Address);
            Assert.Empty(identifier.Symbol);
        }
        [Fact(DisplayName = "TokenIdentifier: Can Create Token Transfer Object from Symbol")]
        public void CanCreateTokenIdentifierObjectFromSymbol()
        {
            var symbol = Generator.UppercaseAlphaCode(20);
            var identifier = new TokenIdentifier(symbol);
            Assert.Equal(Address.None, identifier.Address);
            Assert.Equal(symbol, identifier.Symbol);
        }
        [Fact(DisplayName = "TokenIdentifier: Can Implicitly Create Token Transfer Object from Address")]
        public void CanImplicitlyCreateTokenIdentifierObjectFromAddress()
        {
            var address = new Address(0, 0, Generator.Integer(200, 400));
            TokenIdentifier identifier = address;
            Assert.Equal(address, identifier.Address);
            Assert.Empty(identifier.Symbol);
        }
        [Fact(DisplayName = "TokenIdentifier: Can Implicitly Create Token Transfer Object from Symbol")]
        public void CanImplicitlyCreateTokenIdentifierObjectFromSymbol()
        {
            var symbol = Generator.UppercaseAlphaCode(20);
            TokenIdentifier identifier = symbol;
            Assert.Equal(Address.None, identifier.Address);
            Assert.Equal(symbol, identifier.Symbol);
        }
        [Fact(DisplayName = "TokenIdentifier: Equivalent Address TokenIdentifiers are considered Equal")]
        public void EquivalentAddressTokenIdentifierAreConsideredEqual()
        {
            var address = new Address(0, 0, Generator.Integer(200, 400));
            var id1 = new TokenIdentifier(address);
            var id2 = new TokenIdentifier(address);
            Assert.Equal(id1, id2);
            Assert.True(id1 == id2);
            Assert.False(id1 != id2);
            Assert.True(id1.Equals(id2));
            Assert.True(id2.Equals(id1));
            Assert.True(null as TokenIdentifier == null as TokenIdentifier);
        }
        [Fact(DisplayName = "TokenIdentifier: Equivalent Symbol TokenIdentifiers are considered Equal")]
        public void EquivalentSymbolTokenIdentifierAreConsideredEqual()
        {
            var symbol = Generator.UppercaseAlphaCode(20);
            var id1 = new TokenIdentifier(symbol);
            var id2 = new TokenIdentifier(symbol);
            Assert.Equal(id1, id2);
            Assert.True(id1 == id2);
            Assert.False(id1 != id2);
            Assert.True(id1.Equals(id2));
            Assert.True(id2.Equals(id1));
            Assert.True(null as TokenIdentifier == null as TokenIdentifier);
        }
        [Fact(DisplayName = "TokenIdentifier: Disimilar Address TokenIdentifiers are not considered Equal")]
        public void DisimilarAddressTokenIdentifiersAreNotConsideredEqual()
        {
            var address = new Address(0, 0, Generator.Integer(200, 400));
            var other = new Address(0, 0, Generator.Integer(500, 600));
            var identifier = new TokenIdentifier(address);
            Assert.NotEqual(identifier, new TokenIdentifier(other));
            Assert.False(identifier == new TokenIdentifier(other));
            Assert.True(identifier != new TokenIdentifier(other));
            Assert.False(identifier.Equals(new TokenIdentifier(other)));
        }
        [Fact(DisplayName = "TokenIdentifier: Disimilar Address TokenIdentifiers are not considered Equal")]
        public void DisimilarSymbolTokenIdentifiersAreNotConsideredEqual()
        {
            var symbol = Generator.UppercaseAlphaCode(20);
            var other = Generator.UppercaseAlphaCode(20);
            var identifier = new TokenIdentifier(symbol);
            Assert.NotEqual(identifier, new TokenIdentifier(other));
            Assert.False(identifier == new TokenIdentifier(other));
            Assert.True(identifier != new TokenIdentifier(other));
            Assert.False(identifier.Equals(new TokenIdentifier(other)));
        }
        [Fact(DisplayName = "TokenIdentifier: Disimilar Address TokenIdentifiers are not considered Equal")]
        public void DisimilarTokenIdentifiersAreNotConsideredEqual()
        {
            var address = new Address(0, 0, Generator.Integer(200, 400));
            var identifier1 = new TokenIdentifier(address);
            var symbol = Generator.UppercaseAlphaCode(20);
            var identifier2 = new TokenIdentifier(symbol);
            Assert.NotEqual(identifier1, identifier2);
            Assert.False(identifier1 == identifier2);
            Assert.True(identifier1 != identifier2);
            Assert.False(identifier1.Equals(identifier2));
        }
        [Fact(DisplayName = "TokenIdentifier: Comparing with null are not considered equal.")]
        public void NullTokenIdentifiersAreNotConsideredEqual()
        {
            object asNull = null;
            var address = new Address(0, 0, Generator.Integer(200, 400));
            var identifier = new TokenIdentifier(address);
            Assert.False(identifier == null);
            Assert.False(null == identifier);
            Assert.True(identifier != null);
            Assert.False(identifier.Equals(null));
            Assert.False(identifier.Equals(asNull));
        }
        [Fact(DisplayName = "TokenIdentifier: Comparing with other objects are not considered equal.")]
        public void OtherObjectsAreNotConsideredEqual()
        {
            var address = new Address(0, 0, Generator.Integer(200, 400));
            var identifier = new TokenIdentifier(address);
            Assert.False(identifier.Equals("Something that is not an TokenIdentifier"));
        }
        [Fact(DisplayName = "TokenIdentifier: TokenIdentifier cast as object still considered equivalent.")]
        public void TokenIdentifierCastAsObjectIsconsideredEqual()
        {
            var address = new Address(0, 0, Generator.Integer(200, 400));
            var identifier = new TokenIdentifier(address);
            object equivalent = new TokenIdentifier(address);
            Assert.True(identifier.Equals(equivalent));
            Assert.True(equivalent.Equals(identifier));
        }
        [Fact(DisplayName = "TokenIdentifier: TokenIdentifier as objects but reference equal are same.")]
        public void ReferenceEqualIsconsideredEqual()
        {
            var address = new Address(0, 0, Generator.Integer(200, 400));
            var identifier = new TokenIdentifier(address);
            object reference = identifier;
            Assert.True(identifier.Equals(reference));
            Assert.True(reference.Equals(identifier));
        }
        [Fact(DisplayName = "TokenIdentifier: Address Type Creates Proper Token Reference")]
        public void AddressTypeCreatesProperTokenReference()
        {
            var address = new Address(0, 0, Generator.Integer(200, 400));
            var identifier = new TokenIdentifier(address);

            var tokenRef = new Proto.TokenRef(identifier);
            Assert.Empty(tokenRef.Symbol);
            Assert.Equal(address, tokenRef.TokenId.ToAddress());
        }
        [Fact(DisplayName = "TokenIdentifier: Symbol Type Creates Proper Token Reference")]
        public void SymbolTypeCreatesProperTokenReference()
        {
            var symbol = Generator.UppercaseAlphaCode(20);
            var identifier = new TokenIdentifier(symbol);

            var tokenRef = new Proto.TokenRef(identifier);
            Assert.Equal(symbol, tokenRef.Symbol);
            Assert.Null(tokenRef.TokenId);
        }
        [Fact(DisplayName = "TokenIdentifier: Shard, Realm, Number Creates Proper Token Reference")]
        public void ShardRealmNumberTypeCreatesProperTokenReference()
        {
            var address = new Address(Generator.Integer(200, 400), Generator.Integer(500, 600), Generator.Integer(700, 900));
            var identifier = new TokenIdentifier(address.ShardNum, address.RealmNum, address.AccountNum);

            Assert.Equal(address, identifier.Address);
            Assert.Equal(address.ShardNum, identifier.Address.ShardNum);
            Assert.Equal(address.RealmNum, identifier.Address.RealmNum);
            Assert.Equal(address.AccountNum, identifier.Address.AccountNum);
        }
    }
}
