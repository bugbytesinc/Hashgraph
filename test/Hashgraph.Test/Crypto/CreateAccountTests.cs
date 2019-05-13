﻿using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class CreateAccountTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public CreateAccountTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "Create Account: Can Create Account")]
        public async Task CanCreateAccountAsync()
        {
            var initialBalance = (ulong)Generator.Integer(10, 200);
            var (publicKey, privateKey) = Generator.KeyPair();
            var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = initialBalance,
                PublicKey = publicKey
            });
            Assert.NotNull(createResult);
            Assert.NotNull(createResult.Address);
            Assert.Equal(_networkCredentials.ServerRealm, createResult.Address.RealmNum);
            Assert.Equal(_networkCredentials.ServerShard, createResult.Address.ShardNum);
            Assert.True(createResult.Address.AccountNum > 0);

            var info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(initialBalance, info.Balance);
            Assert.Equal(createResult.Address.RealmNum, info.Address.RealmNum);
            Assert.Equal(createResult.Address.ShardNum, info.Address.ShardNum);
            Assert.Equal(createResult.Address.AccountNum, info.Address.AccountNum);
            Assert.False(info.Deleted);

            // Move remaining funds back to primary account.
            var from = new Account(createResult.Address.RealmNum, createResult.Address.ShardNum, createResult.Address.AccountNum, privateKey);
            await client.TransferAsync(from, _networkCredentials.CreateDefaultAccount(), (long)initialBalance);

            // We should do the following when deleting an account is testable on the network.
            //await client.DeleteAccountAsync(newAccount, _networkCredentials.CreateDefaultAccount());
            //var deletedAccount = await client.DeleteAccountAsync(createdAccount, _networkCredentials.CreateDefaultAccount());
            //Assert.NotNull(deletedAccount);
            //Assert.Equal(_networkCredentials.ServerRealm, deletedAccount.RealmNum);
            //Assert.Equal(_networkCredentials.ServerShard, deletedAccount.ShardNum);
            //Assert.True(deletedAccount.AccountNum > 0);

            info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(0UL, info.Balance);
            Assert.Equal(createResult.Address.RealmNum, info.Address.RealmNum);
            Assert.Equal(createResult.Address.ShardNum, info.Address.ShardNum);
            Assert.Equal(createResult.Address.AccountNum, info.Address.AccountNum);
            //Assert.True(info.Deleted);
        }
        [Fact(DisplayName = "Create Account: Set Send Threshold")]
        public async Task CanSetSendTreshold()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var expectedValue = (ulong)Generator.Integer(500, 1000);
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                PublicKey = publicKey,
                SendThresholdCreateRecord = expectedValue
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(expectedValue, info.SendThresholdCreateRecord);
        }
        [Fact(DisplayName = "Create Account: Set Receive Threshold")]
        public async Task CanSetReceiveTreshold()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var expectedValue = (ulong)Generator.Integer(500, 1000);
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                PublicKey = publicKey,
                ReceiveThresholdCreateRecord = expectedValue
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(expectedValue, info.ReceiveThresholdCreateRecord);
        }
        [Fact(DisplayName = "Create Account: Set Signature Required True")]
        public async Task CanSetSignatureRequiredTrue()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                PublicKey = publicKey,
                RequireReceiveSignature = true
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.True(info.ReceiveSignatureRequired);
        }
        [Fact(DisplayName = "Create Account: Set Signature Required False")]
        public async Task CanSetSignatureRequiredFalse()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                PublicKey = publicKey,
                RequireReceiveSignature = false
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.False(info.ReceiveSignatureRequired);
        }
        [Fact(DisplayName = "Create Account: Set Auto Renew Period")]
        public async Task CanSetAutoRenewPeriod()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var expectedValue = TimeSpan.FromDays(Generator.Integer(20, 60));
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                PublicKey = publicKey,
                AutoRenewPeriod = expectedValue
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(expectedValue, info.AutoRenewPeriod);
        }
    }
}