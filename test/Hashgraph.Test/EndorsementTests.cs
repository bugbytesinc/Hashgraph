using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using Xunit;

namespace Hashgraph.Tests
{
    public class EndorsementsTests
    {
        [Fact(DisplayName = "Endorsements: Can Create Valid Endorsements Object")]
        public void CreateValidEndorsementsObject()
        {
            var (publicKey1, _) = Generator.KeyPair();
            var (publicKey2, _) = Generator.KeyPair();

            new Endorsement(publicKey1);
            new Endorsement(1, publicKey1);
            new Endorsement(publicKey1, publicKey2);
            new Endorsement(1, new Endorsement(1, publicKey1, publicKey2), new Endorsement(2, publicKey1, publicKey2));
        }
        [Fact(DisplayName = "Endorsements: Too large of a requried count throws error.")]
        public void TooLargeRequiredCountThrowsError()
        {

            var (publicKey, _) = Generator.KeyPair();
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Endorsement((uint)Generator.Integer(2, 4), publicKey);
            });
            Assert.Equal("requiredCount", exception.ParamName);
            Assert.StartsWith("The required number of keys for a valid signature cannot exceed the number of public keys provided.", exception.Message);
        }
        [Fact(DisplayName = "Endorsements: Empty Private key throws Exception")]
        public void EmptyValueForKeyThrowsError()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Endorsement();
            });
            Assert.Equal("endorsements", exception.ParamName);
            Assert.StartsWith("At least one endorsement in a list is required.", exception.Message);
        }
        [Fact(DisplayName = "Endorsements: Invalid Bytes in Private key throws Exception")]
        public void InvalidBytesForValueForKeyThrowsError()
        {
            var (originalKey, _) = Generator.KeyPair();
            var invalidKey = originalKey.ToArray();
            invalidKey[0] = 0;
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Endorsement(Endorsement.Type.Ed25519, invalidKey);
            });
            Assert.StartsWith("The public key was not provided in a recognizable Ed25519 format.", exception.Message);
        }
        [Fact(DisplayName = "Endorsements: Invalid Byte Length in Private key throws Exception")]
        public void InvalidByteLengthForValueForKeyThrowsError()
        {
            var (originalKey, _) = Generator.KeyPair();
            var invalidKey = originalKey.ToArray().Take(32).ToArray();
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Endorsement(Endorsement.Type.Ed25519, invalidKey);
            });
            Assert.StartsWith("The public key was not provided in a recognizable Ed25519 format.", exception.Message);
        }
        [Fact(DisplayName = "Endorsements: Equivalent Endorsements are considered Equal")]
        public void EquivalentEndorsementsAreConsideredEqual()
        {
            var (publicKey1, _) = Generator.KeyPair();
            var (publicKey2, _) = Generator.KeyPair();
            var endorsement1 = new Endorsement(publicKey1);
            var endorsement2 = new Endorsement(publicKey1);
            Assert.Equal(endorsement1, endorsement2);
            Assert.True(endorsement1 == endorsement2);
            Assert.False(endorsement1 != endorsement2);

            endorsement1 = new Endorsement(publicKey1, publicKey2);
            endorsement2 = new Endorsement(publicKey1, publicKey2);
            Assert.Equal(endorsement1, endorsement2);
            Assert.True(endorsement1 == endorsement2);
            Assert.False(endorsement1 != endorsement2);
        }
        [Fact(DisplayName = "Endorsements: Disimilar Endorsements are not considered Equal")]
        public void DisimilarEndorsementsAreNotConsideredEqual()
        {
            var (publicKey1, _) = Generator.KeyPair();
            var (publicKey2, _) = Generator.KeyPair();
            var endorsements1 = new Endorsement(publicKey1);
            var endorsements2 = new Endorsement(publicKey2);
            Assert.NotEqual(endorsements1, endorsements2);
            Assert.False(endorsements1 == endorsements2);
            Assert.True(endorsements1 != endorsements2);

            endorsements1 = new Endorsement(publicKey1);
            endorsements2 = new Endorsement(publicKey1, publicKey2);
            Assert.NotEqual(endorsements1, endorsements2);
            Assert.False(endorsements1 == endorsements2);
            Assert.True(endorsements1 != endorsements2);

            endorsements1 = new Endorsement(publicKey1, publicKey2);
            endorsements2 = new Endorsement(1, publicKey1, publicKey2);
            Assert.NotEqual(endorsements1, endorsements2);
            Assert.False(endorsements1 == endorsements2);
            Assert.True(endorsements1 != endorsements2);
        }
    }
}
