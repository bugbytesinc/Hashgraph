using Hashgraph.Test.Fixtures;
using NSec.Cryptography;
using System;
using Xunit;

namespace Hashgraph.Tests
{
    public class AccountTests
    {
        private const string EXAMPLE_KEY = "302e020100300506032b657004220420e29407432f49ff5c464e3199c78982f70e7c759544da7cebc471bcc9ddd99df4";

        [Fact(DisplayName = "Can Create Valid Account Object")]
        public void CreateValidAccountObject()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(0, 200);

            using (var account = new Account(realmNum, shardNum, accountNum, EXAMPLE_KEY))
            {
                Assert.Equal(realmNum, account.RealmNum);
                Assert.Equal(shardNum, account.ShardNum);
                Assert.Equal(accountNum, account.AccountNum);
            }
        }
        [Fact(DisplayName = "Negative Realm Number throws Exception")]
        public void NegativeValueForRealmThrowsError()
        {
            var realmNum = Generator.Integer(-200, -1);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(0, 200);
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, EXAMPLE_KEY);
            });
            Assert.Equal("realmNum", exception.ParamName);
            Assert.StartsWith("Realm Number cannot be negative.", exception.Message);
        }
        [Fact(DisplayName = "Negative Shard Number throws Exception")]
        public void NegativeValueForShardThrowsError()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(-200, -1);
            var accountNum = Generator.Integer(0, 200);
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, EXAMPLE_KEY);
            });
            Assert.Equal("shardNum", exception.ParamName);
            Assert.StartsWith("Shard Number cannot be negative.", exception.Message);
        }
        [Fact(DisplayName = "Negative Account Number throws Exception")]
        public void NegativeValueForAccountNumberThrowsError()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(-200, -1);
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, EXAMPLE_KEY);
            });
            Assert.Equal("accountNum", exception.ParamName);
            Assert.StartsWith("Account Number cannot be negative.", exception.Message);
        }
        [Fact(DisplayName = "Null Private key throws Exception")]
        public void NullValueForKeyThrowsError()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(3, 200);
            string signingKey = null;
            var exception = Assert.Throws<ArgumentNullException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, signingKey);
            });
            Assert.Equal("privateKeyInHex", exception.ParamName);
            Assert.StartsWith("Private Key cannot be null.", exception.Message);
        }
        [Fact(DisplayName = "Empty Private key throws Exception")]
        public void EmptyValueForKeyThrowsError()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(3, 200);
            var signingKey = String.Empty;
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, signingKey);
            });
            Assert.Equal("privateKeyInHex", exception.ParamName);
            Assert.StartsWith("Private Key cannot be empty.", exception.Message);
        }
        [Fact(DisplayName = "Invalid Hex in Private key throws Exception")]
        public void InvalidValueForKeyThrowsError()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(3, 200);
            var signingKey = "ZZ" + Generator.Code(32) + "ZZ";
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, signingKey);
            });
            Assert.StartsWith("Key does not appear to be encoded in Hex.", exception.Message);
        }
        [Fact(DisplayName = "Truncated Hex in Private key throws Exception")]
        public void TruncatedValueForKeyThrowsErrorOddNumberOfChars()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(3, 200);
            var signingKey = EXAMPLE_KEY[0..^1];
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, signingKey);
            });
            Assert.StartsWith("Key does not appear to be properly encoded in Hex, found an odd number of characters.", exception.Message);
        }
        [Fact(DisplayName = "Invalid Bytes in Private key throws Exception")]
        public void InvalidBytesForValueForKeyThrowsError()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(3, 200);
            var signingKey = "00" + EXAMPLE_KEY[0..^2];
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, signingKey);
            });
            Assert.StartsWith("The private key was not provided in a recognizable Ed25519 format.", exception.Message);
        }
        [Fact(DisplayName = "Equivalent Accounts are considered Equal")]
        public void EquivalentAccountsAreConsideredEqual()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(0, 200);
            var account1 = new Account(realmNum, shardNum, accountNum, EXAMPLE_KEY);
            var account2 = new Account(realmNum, shardNum, accountNum, EXAMPLE_KEY);
            Assert.Equal(account1, account2);
            Assert.True(account1 == account2);
            Assert.False(account1 != account2);
        }
        [Fact(DisplayName = "Disimilar Accounts are not considered Equal")]
        public void DisimilarAccountsAreNotConsideredEqual()
        {
            using (var key = Key.Create(SignatureAlgorithm.Ed25519, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport }))
            {
                var privateKey = BitConverter.ToString(key.Export(KeyBlobFormat.PkixPrivateKey)).Replace("-", "");
                var realmNum = Generator.Integer(0, 200);
                var shardNum = Generator.Integer(0, 200);
                var accountNum = Generator.Integer(0, 200);
                var account1 = new Account(realmNum, shardNum, accountNum, EXAMPLE_KEY);
                var account2 = new Account(realmNum, shardNum, accountNum, privateKey);
                Assert.NotEqual(account1, account2);
                Assert.False(account1 == account2);
                Assert.True(account1 != account2);
            }
        }
    }
}
