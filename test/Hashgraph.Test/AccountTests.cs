using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using Xunit;

namespace Hashgraph.Tests
{
    public class AccountTests
    {
        [Fact(DisplayName = "Accounts: Can Create Valid Account Object")]
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
        [Fact(DisplayName = "Accounts: Negative Realm Number throws Exception")]
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
        [Fact(DisplayName = "Accounts: Negative Shard Number throws Exception")]
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
        [Fact(DisplayName = "Accounts: Negative Account Number throws Exception")]
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
        [Fact(DisplayName = "Accounts: Empty Private key throws Exception")]
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
            Assert.Equal("privateKeys", exception.ParamName);
            Assert.StartsWith("Unable to create Account object, Private Key cannot be empty.", exception.Message);
        }
        [Fact(DisplayName = "Accounts: Empty Array of Private keys throws Exception")]
        public void EmptyArrayOfKeysSucceeds()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(3, 200);
            var privateKeys = new ReadOnlyMemory<byte>[0];
            using var account = new Account(realmNum, shardNum, accountNum, privateKeys);
            Assert.Equal(realmNum, account.RealmNum);
            Assert.Equal(shardNum, account.ShardNum);
            Assert.Equal(accountNum, account.AccountNum);
        }
        [Fact(DisplayName = "Accounts: Constructor without keys succeeds.")]
        public void ConstructorWithoutKeysSucceeds()
        {
            var realmNum = Generator.Integer(0, 200);
            var shardNum = Generator.Integer(0, 200);
            var accountNum = Generator.Integer(3, 200);
            using var account = new Account(realmNum, shardNum, accountNum);
            Assert.Equal(realmNum, account.RealmNum);
            Assert.Equal(shardNum, account.ShardNum);
            Assert.Equal(accountNum, account.AccountNum);
        }
        [Fact(DisplayName = "Accounts: Invalid Bytes in Private key throws Exception")]
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
            Assert.StartsWith("Unable to create Account object, The private key was not provided in a recognizable Ed25519 format.", exception.Message);
        }
        [Fact(DisplayName = "Accounts: Invalid Byte Length in Private key throws Exception")]
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
            Assert.StartsWith("Unable to create Account object, The private key was not provided in a recognizable Ed25519 format.", exception.Message);
        }
        [Fact(DisplayName = "Accounts: Equivalent Accounts are considered Equal")]
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
        [Fact(DisplayName = "Accounts: Disimilar Accounts are not considered Equal")]
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
