using Hashgraph.Test.Fixtures;
using System;
using System.Security.Cryptography;
using Xunit;

namespace Hashgraph.Tests
{
    public class ClaimTests
    {
        [Fact(DisplayName = "Claim: Equivalent Claim are considered Equal")]
        public void EquivalentClaimAreConsideredEqual()
        {
            var (publicKey1, _) = Generator.KeyPair();
            var (publicKey2, _) = Generator.KeyPair();
            var hash = new SHA384Managed().ComputeHash(Generator.KeyPair().publicKey.ToArray());
            var endorsements = new Endorsement[] { publicKey1, publicKey2 };
            var duration = TimeSpan.FromDays(Generator.Integer(10, 20));
            var address = new Address(0, 0, Generator.Integer(1000, 2000));
            var claim1 = new Claim
            {
                Address = address,
                Hash = hash,
                Endorsements = endorsements,
                ClaimDuration = duration
            };
            var claim2 = new Claim
            {
                Address = address,
                Hash = hash,
                Endorsements = endorsements,
                ClaimDuration = duration
            };
            Assert.Equal(claim1, claim1);

            Assert.Equal(claim1, claim2);
            Assert.True(claim1 == claim2);
            Assert.False(claim1 != claim2);
            Assert.True(claim1.Equals(claim2));

            Assert.Equal(claim2, claim1);
            Assert.True(claim2 == claim1);
            Assert.False(claim2 != claim1);
            Assert.True(claim2.Equals(claim1));

            Assert.Equal(claim2, claim2);
        }
        [Fact(DisplayName = "Claim: Disimilar Address are not considered Equal")]
        public void DisimilarClaimAddressesAreNotConsideredEqual()
        {
            var (publicKey1, _) = Generator.KeyPair();
            var (publicKey2, _) = Generator.KeyPair();
            var hash = new SHA384Managed().ComputeHash(Generator.KeyPair().publicKey.ToArray());
            var endorsements = new Endorsement[] { publicKey1, publicKey2 };
            var duration = TimeSpan.FromDays(Generator.Integer(10, 20));
            var address1 = new Address(0, 0, Generator.Integer(1000, 2000));
            var address2 = new Address(0, 0, Generator.Integer(2001, 4000));
            var claim1 = new Claim
            {
                Address = address1,
                Hash = hash,
                Endorsements = endorsements,
                ClaimDuration = duration
            };
            var claim2 = new Claim
            {
                Address = address2,
                Hash = hash,
                Endorsements = endorsements,
                ClaimDuration = duration
            };
            Assert.NotEqual(claim1, claim2);
            Assert.False(claim1 == claim2);
            Assert.True(claim1 != claim2);
            Assert.False(claim1.Equals(claim2));

            Assert.NotEqual(claim2, claim1);
            Assert.False(claim2 == claim1);
            Assert.True(claim2 != claim1);
            Assert.False(claim2.Equals(claim1));
        }
        [Fact(DisplayName = "Claim: Disimilar Hashes are not considered Equal")]
        public void DisimilarClaimHashesAreNotConsideredEqual()
        {
            var (publicKey1, _) = Generator.KeyPair();
            var (publicKey2, _) = Generator.KeyPair();
            var hash1 = new SHA384Managed().ComputeHash(Generator.KeyPair().publicKey.ToArray());
            var hash2 = new SHA384Managed().ComputeHash(Generator.KeyPair().publicKey.ToArray());
            var endorsements = new Endorsement[] { publicKey1, publicKey2 };
            var duration = TimeSpan.FromDays(Generator.Integer(10, 20));
            var address = new Address(0, 0, Generator.Integer(1000, 2000));
            var claim1 = new Claim
            {
                Address = address,
                Hash = hash1,
                Endorsements = endorsements,
                ClaimDuration = duration
            };
            var claim2 = new Claim
            {
                Address = address,
                Hash = hash2,
                Endorsements = endorsements,
                ClaimDuration = duration
            };
            Assert.NotEqual(claim1, claim2);
            Assert.False(claim1 == claim2);
            Assert.True(claim1 != claim2);
            Assert.False(claim1.Equals(claim2));

            Assert.NotEqual(claim2, claim1);
            Assert.False(claim2 == claim1);
            Assert.True(claim2 != claim1);
            Assert.False(claim2.Equals(claim1));
        }
        [Fact(DisplayName = "Claim: Disimilar Endorsements are not considered Equal")]
        public void DisimilarClaimEndorsementsAreNotConsideredEqual()
        {
            var (publicKey1, _) = Generator.KeyPair();
            var (publicKey2, _) = Generator.KeyPair();
            var hash = new SHA384Managed().ComputeHash(Generator.KeyPair().publicKey.ToArray());
            var endorsements1 = new Endorsement[] { publicKey1 };
            var endorsements2 = new Endorsement[] { publicKey2 };
            var duration = TimeSpan.FromDays(Generator.Integer(10, 20));
            var address = new Address(0, 0, Generator.Integer(1000, 2000));
            var claim1 = new Claim
            {
                Address = address,
                Hash = hash,
                Endorsements = endorsements1,
                ClaimDuration = duration
            };
            var claim2 = new Claim
            {
                Address = address,
                Hash = hash,
                Endorsements = endorsements2,
                ClaimDuration = duration
            };
            Assert.NotEqual(claim1, claim2);
            Assert.False(claim1 == claim2);
            Assert.True(claim1 != claim2);
            Assert.False(claim1.Equals(claim2));

            Assert.NotEqual(claim2, claim1);
            Assert.False(claim2 == claim1);
            Assert.True(claim2 != claim1);
            Assert.False(claim2.Equals(claim1));
        }
        [Fact(DisplayName = "Claim: Disimilar Durations are not considered Equal")]
        public void DisimilarClaimDurationsAreNotConsideredEqual()
        {
            var (publicKey1, _) = Generator.KeyPair();
            var (publicKey2, _) = Generator.KeyPair();
            var hash = new SHA384Managed().ComputeHash(Generator.KeyPair().publicKey.ToArray());
            var endorsements = new Endorsement[] { publicKey1 };
            var duration1 = TimeSpan.FromDays(Generator.Integer(10, 20));
            var duration2 = TimeSpan.FromDays(Generator.Integer(40, 50));
            var address = new Address(0, 0, Generator.Integer(1000, 2000));
            var claim1 = new Claim
            {
                Address = address,
                Hash = hash,
                Endorsements = endorsements,
                ClaimDuration = duration1
            };
            var claim2 = new Claim
            {
                Address = address,
                Hash = hash,
                Endorsements = endorsements,
                ClaimDuration = duration2
            };
            Assert.NotEqual(claim1, claim2);
            Assert.False(claim1 == claim2);
            Assert.True(claim1 != claim2);
            Assert.False(claim1.Equals(claim2));

            Assert.NotEqual(claim2, claim1);
            Assert.False(claim2 == claim1);
            Assert.True(claim2 != claim1);
            Assert.False(claim2.Equals(claim1));
        }
    }
}
