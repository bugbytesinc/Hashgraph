using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using Xunit;

namespace Hashgraph.Tests
{
    public class AccountTests
    {
        [Fact(DisplayName = "Can Create Valid Account Object")]
        public void CreateValidAccountObject()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(0, 200);
            var (_, privateKey) = Generator.KeyPair();

            using var account = new Account(realmNum, shardNum, accountNum, privateKey);
            Assert.Equal(realmNum, account.RealmNum);
            Assert.Equal(shardNum, account.ShardNum);
            Assert.Equal(accountNum, account.AccountNum);
        }
        [Fact(DisplayName = "Negative Realm Number throws Exception")]
        public void NegativeValueForRealmThrowsError()
        {
            var realmNum = Generator.Integer(-200, -1);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(0, 200);
            var (_, privateKey) = Generator.KeyPair();
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, privateKey);
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
            var (_, privateKey) = Generator.KeyPair();
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, privateKey);
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
            var (_, privateKey) = Generator.KeyPair();
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, privateKey);
            });
            Assert.Equal("accountNum", exception.ParamName);
            Assert.StartsWith("Account Number cannot be negative.", exception.Message);
        }
        [Fact(DisplayName = "Empty Private key throws Exception")]
        public void EmptyValueForKeyThrowsError()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(3, 200);
            var privateKey = ReadOnlyMemory<byte>.Empty;
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, privateKey);
            });
            Assert.Equal("privateKey", exception.ParamName);
            Assert.StartsWith("Private Key cannot be empty.", exception.Message);
        }
        [Fact(DisplayName = "Invalid Bytes in Private key throws Exception")]
        public void InvalidBytesForValueForKeyThrowsError()
        {
            var (_, originalKey) = Generator.KeyPair();
            var invalidKey = originalKey.ToArray();
            invalidKey[0] = 0;
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(3, 200);
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, invalidKey);
            });
            Assert.StartsWith("The private key was not provided in a recognizable Ed25519 format.", exception.Message);
        }
        [Fact(DisplayName = "Invalid Byte Length in Private key throws Exception")]
        public void InvalidByteLengthForValueForKeyThrowsError()
        {
            var (_, originalKey) = Generator.KeyPair();
            var invalidKey = originalKey.ToArray().Take(36).ToArray();
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(3, 200);
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                new Account(realmNum, shardNum, accountNum, invalidKey);
            });
            Assert.StartsWith("The private key was not provided in a recognizable Ed25519 format.", exception.Message);
        }
        [Fact(DisplayName = "Equivalent Accounts are considered Equal")]
        public void EquivalentAccountsAreConsideredEqual()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(0, 200);
            var (_, privateKey) = Generator.KeyPair();
            var account1 = new Account(realmNum, shardNum, accountNum, privateKey);
            var account2 = new Account(realmNum, shardNum, accountNum, privateKey);
            Assert.Equal(account1, account2);
            Assert.True(account1 == account2);
            Assert.False(account1 != account2);
        }
        [Fact(DisplayName = "Disimilar Accounts are not considered Equal")]
        public void DisimilarAccountsAreNotConsideredEqual()
        {
            var (_, privateKey1) = Generator.KeyPair();
            var (_, privateKey2) = Generator.KeyPair();
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(0, 200);
            var account1 = new Account(realmNum, shardNum, accountNum, privateKey1);
            var account2 = new Account(realmNum, shardNum, accountNum, privateKey2);
            Assert.NotEqual(account1, account2);
            Assert.False(account1 == account2);
            Assert.True(account1 != account2);
        }
    }
}
