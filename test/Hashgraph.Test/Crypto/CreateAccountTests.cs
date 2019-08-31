using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentials))]
    public class CreateAccountTests
    {
        private readonly NetworkCredentials _network;
        public CreateAccountTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Create Account: Can Create Account")]
        public async Task CanCreateAccountAsync()
        {
            var initialBalance = (ulong)Generator.Integer(10, 200);
            var (publicKey, privateKey) = Generator.KeyPair();
            var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = initialBalance,
                PublicKey = publicKey
            });
            Assert.NotNull(createResult);
            Assert.NotNull(createResult.Address);
            Assert.Equal(_network.ServerRealm, createResult.Address.RealmNum);
            Assert.Equal(_network.ServerShard, createResult.Address.ShardNum);
            Assert.True(createResult.Address.AccountNum > 0);

            var info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(initialBalance, info.Balance);
            Assert.Equal(createResult.Address.RealmNum, info.Address.RealmNum);
            Assert.Equal(createResult.Address.ShardNum, info.Address.ShardNum);
            Assert.Equal(createResult.Address.AccountNum, info.Address.AccountNum);
            Assert.Equal(new Address(0,0,0), info.Proxy);
            Assert.False(info.Deleted);

            // Move remaining funds back to primary account.
            var from = new Account(createResult.Address, privateKey);
            await client.TransferAsync(from, _network.Payer, (long)initialBalance, ctx => ctx.FeeLimit = 140_000);

            var receipt = await client.DeleteAccountAsync(new Account(createResult.Address, privateKey), _network.Payer, ctx => ctx.FeeLimit = 6922777);
            Assert.NotNull(receipt);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var exception = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.GetAccountInfoAsync(createResult.Address);
            });
            Assert.StartsWith("Transaction Failed Pre-Check: AccountDeleted", exception.Message);
        }
        [Fact(DisplayName = "Create Account: Set Send Threshold")]
        public async Task CanSetSendTreshold()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var expectedValue = (ulong)Generator.Integer(500, 1000);
            await using var client = _network.NewClient();
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
            await using var client = _network.NewClient();
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
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountWithRecordAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                PublicKey = publicKey,
                RequireReceiveSignature = true
            }, ctx =>
            {
                ctx.Payer = new Account(ctx.Payer, _network.PrivateKey, privateKey);
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.True(info.ReceiveSignatureRequired);
        }
        [Fact(DisplayName = "Create Account: Set Signature Required False")]
        public async Task CanSetSignatureRequiredFalse()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _network.NewClient();
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
            await using var client = _network.NewClient();
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
        [Fact(DisplayName = "Create Account: Set Account Proxy Stake")]
        public async Task CanSetProxyStakeAccount()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                PublicKey = publicKey,
                Proxy = _network.Gateway,
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(_network.Gateway,info.Proxy);
        }
    }
}