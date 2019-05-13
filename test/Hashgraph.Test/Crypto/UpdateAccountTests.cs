using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class UpdateAccountTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public UpdateAccountTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "Update Account: Can Update Key")]
        public async Task CanUpdateKey()
        {
            var originalKeyPair = Generator.KeyPair();
            var updatedKeyPair = Generator.KeyPair();
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                PublicKey = originalKeyPair.publicKey
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(originalKeyPair.publicKey.ToArray().TakeLast(32).ToArray(), originalInfo.PublicKey.ToArray());

            var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams {
                Account = new Account(createResult.Address, originalKeyPair.privateKey),
                PrivateKey = updatedKeyPair.privateKey
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(updatedKeyPair.publicKey.ToArray().TakeLast(32).ToArray(), updatedInfo.PublicKey.ToArray());
        }
        [Fact(DisplayName = "Update Account: Can Update Send Threshold")]
        public async Task CanUpdateSendTreshold()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var originalValue = (ulong) Generator.Integer(500, 1000);
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                PublicKey = publicKey,
                SendThresholdCreateRecord = originalValue
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(originalValue, originalInfo.SendThresholdCreateRecord);

            var newValue = originalValue + (ulong)Generator.Integer(500, 1000);
            var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Account = new Account(createResult.Address, privateKey),
                SendThresholdCreateRecord = newValue
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(newValue, updatedInfo.SendThresholdCreateRecord);
        }
        [Fact(DisplayName = "Update Account: Can Update Receive Threshold")]
        public async Task CanUpdateReceiveTreshold()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var originalValue = (ulong)Generator.Integer(500, 1000);
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                PublicKey = publicKey,
                ReceiveThresholdCreateRecord = originalValue
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(originalValue, originalInfo.ReceiveThresholdCreateRecord);

            var newValue = originalValue + (ulong)Generator.Integer(500, 1000);
            var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Account = new Account(createResult.Address, privateKey),
                ReceiveThresholdCreateRecord = newValue
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(newValue, updatedInfo.ReceiveThresholdCreateRecord);
        }
        [Fact(DisplayName = "Update Account: Can Update Auto Renew Period")]
        public async Task CanUpdateAutoRenewPeriod()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var originalValue = TimeSpan.FromDays(Generator.Integer(10, 20));
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                PublicKey = publicKey,
                AutoRenewPeriod = originalValue
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(originalValue, originalInfo.AutoRenewPeriod);

            var newValue = originalValue.Add(TimeSpan.FromDays(Generator.Integer(10, 20)));
            var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Account = new Account(createResult.Address, privateKey),
                AutoRenewPeriod = newValue
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(newValue, updatedInfo.AutoRenewPeriod);
        }
        [Fact(DisplayName = "Update Account: Can Update Expiration")]
        public async Task CanUpdateExpiration()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                PublicKey = publicKey                
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);            

            var newValue = originalInfo.Expiration.Add(TimeSpan.FromDays(Generator.Integer(30, 60)));
            var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Account = new Account(createResult.Address, privateKey),
                Expiration = newValue
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(newValue, updatedInfo.Expiration);
        }
    }
}
