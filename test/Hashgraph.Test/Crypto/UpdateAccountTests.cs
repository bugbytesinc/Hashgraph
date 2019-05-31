using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentials))]
    public class UpdateAccountTests
    {
        private readonly NetworkCredentials _network;
        public UpdateAccountTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Update Account: Can Update Key")]
        public async Task CanUpdateKey()
        {
            var originalKeyPair = Generator.KeyPair();
            var updatedKeyPair = Generator.KeyPair();
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                PublicKey = originalKeyPair.publicKey
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(new Endorsement(originalKeyPair.publicKey), originalInfo.Endorsement);

            var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Account = new Account(createResult.Address, originalKeyPair.privateKey, updatedKeyPair.privateKey),
                Endorsement = new Endorsement(updatedKeyPair.publicKey)
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(new Endorsement(updatedKeyPair.publicKey), updatedInfo.Endorsement);
        }
        [Fact(DisplayName = "Update Account: Can Update Send Threshold")]
        public async Task CanUpdateSendTreshold()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var originalValue = (ulong)Generator.Integer(500, 1000);
            await using var client = _network.NewClient();
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
            await using var client = _network.NewClient();
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
            await using var client = _network.NewClient();
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
    }
}
