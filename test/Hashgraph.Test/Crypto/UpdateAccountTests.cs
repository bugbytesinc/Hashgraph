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
            var (publicKey, privateKey) = Generator.KeyPair();
            var updatedKeyPair = Generator.KeyPair();
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                Endorsement = publicKey
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(new Endorsement(publicKey), originalInfo.Endorsement);

            var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = createResult.Address,
                Endorsement = new Endorsement(updatedKeyPair.publicKey),
                Signatory = new Signatory(privateKey, updatedKeyPair.privateKey)
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(new Endorsement(updatedKeyPair.publicKey), updatedInfo.Endorsement);
        }
        [Fact(DisplayName = "Update Account: Can Update Key with Record")]
        public async Task CanUpdateKeyWithRecord()
        {
            var (originalPublicKey, originalPrivateKey) = Generator.KeyPair();
            var (updatedPublicKey, updatedPrivateKey) = Generator.KeyPair();
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountWithRecordAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                Endorsement = originalPublicKey
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(new Endorsement(originalPublicKey), originalInfo.Endorsement);

            var record = await client.UpdateAccountWithRecordAsync(new UpdateAccountParams
            {
                Address = createResult.Address,
                Endorsement = new Endorsement(updatedPublicKey),
                Signatory = new Signatory(originalPrivateKey, updatedPrivateKey)
            });
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(new Endorsement(updatedPublicKey), updatedInfo.Endorsement);
        }
        [Fact(DisplayName = "Update Account: Can Update Require Receive Signature")]
        public async Task CanUpdateRequireReceiveSignature()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var originalValue = Generator.Integer(0, 1) == 1;
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                Endorsement = publicKey,
                RequireReceiveSignature = originalValue,
                Signatory = originalValue ? new Signatory(privateKey) : null   // When True, you need to include signature on create
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(originalValue, originalInfo.ReceiveSignatureRequired);

            var newValue = !originalValue;
            var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = createResult.Address,
                Signatory = privateKey,
                RequireReceiveSignature = newValue
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(newValue, updatedInfo.ReceiveSignatureRequired);
        }
        [Fact(DisplayName = "Update Account: Can't Update Auto Renew Period to other than 7890000 seconds")]
        public async Task CanUpdateAutoRenewPeriod()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var originalValue = TimeSpan.FromSeconds(7890000);
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                Endorsement = publicKey,
                AutoRenewPeriod = originalValue
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(originalValue, originalInfo.AutoRenewPeriod);

            var newValue = originalValue.Add(TimeSpan.FromDays(Generator.Integer(10, 20)));

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
                {
                    Address = createResult.Address,
                    Signatory = privateKey,
                    AutoRenewPeriod = newValue
                });
            });
            Assert.Equal(ResponseCode.AutorenewDurationNotInRange, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: AutorenewDurationNotInRange", pex.Message);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(originalValue, updatedInfo.AutoRenewPeriod);
        }
        [Fact(DisplayName = "Update Account: Can Update Proxy Stake")]
        public async Task CanUpdateProxyStake()
        {
            var fx = await TestAccount.CreateAsync(_network);

            var originalInfo = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(new Address(0, 0, 0), originalInfo.Proxy);

            var updateResult = await fx.Client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = fx.Record.Address,
                Signatory = fx.PrivateKey,
                Proxy = _network.Gateway
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(_network.Gateway, updatedInfo.Proxy);
        }
        [Fact(DisplayName = "Update Account: Can Update Proxy Stake to Invalid Address")]
        public async Task CanUpdateProxyStakeToInvalidAddress()
        {
            var emptyAddress = new Address(0, 0, 0);
            var fx = await TestAccount.CreateAsync(_network);
            await fx.Client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = fx.Record.Address,
                Signatory = fx.PrivateKey,
                Proxy = _network.Gateway
            });

            var originalInfo = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(_network.Gateway, originalInfo.Proxy);

            var updateResult = await fx.Client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = fx.Record.Address,
                Signatory = fx.PrivateKey,
                Proxy = emptyAddress
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(emptyAddress, updatedInfo.Proxy);
        }
        [Fact(DisplayName = "Update Account: Update with Insufficient Funds Returns Required Fee")]
        public async Task UpdateWithInsufficientFundsReturnsRequiredFee()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                Endorsement = publicKey,
                Signatory = privateKey,
                RequireReceiveSignature = true
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.True(originalInfo.ReceiveSignatureRequired);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.UpdateAccountAsync(new UpdateAccountParams
                {
                    Address = createResult.Address,
                    Signatory = privateKey,
                    RequireReceiveSignature = false,
                }, ctx =>
                {
                    ctx.FeeLimit = 1;
                });
            });
            Assert.Equal(ResponseCode.InsufficientTxFee, pex.Status);
            var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = createResult.Address,
                Signatory = privateKey,
                RequireReceiveSignature = false
            }, ctx =>
            {
                ctx.FeeLimit = (long)pex.RequiredFee;
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.False(updatedInfo.ReceiveSignatureRequired);
        }
        [Fact(DisplayName = "Update Account: Empty Endorsement is Not Allowed")]
        public async Task EmptyEndorsementIsNotAllowed()
        {
            var (originalPublicKey, originalPrivateKey) = Generator.KeyPair();
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 10,
                Endorsement = originalPublicKey
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(new Endorsement(originalPublicKey), originalInfo.Endorsement);

            var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await client.UpdateAccountAsync(new UpdateAccountParams
                {
                    Address = createResult.Address,
                    Endorsement = Endorsement.None,
                    Signatory = new Signatory(originalPrivateKey)
                });
            });
            Assert.Equal("Endorsement", aoe.ParamName);
            Assert.StartsWith("Endorsement can not be 'None', it must contain at least one key requirement.", aoe.Message);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(originalInfo.Endorsement, updatedInfo.Endorsement);

            var receipt = await client.TransferAsync(createResult.Address, _network.Payer, 5, originalPrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var newBalance = await client.GetAccountBalanceAsync(createResult.Address);
            Assert.Equal(5ul, newBalance);
        }
        [Fact(DisplayName = "Update Account: Nested List of Nested List of Endorsement Allowed")]
        public async Task NestedListEndorsementsIsAllowed()
        {
            var (originalPublicKey, originalPrivateKey) = Generator.KeyPair();
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 10,
                Endorsement = originalPublicKey
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var originalInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(new Endorsement(originalPublicKey), originalInfo.Endorsement);

            var nestedEndorsement = new Endorsement(new Endorsement(new Endorsement(new Endorsement(new Endorsement(new Endorsement(originalPublicKey))))));
            var updateResult = await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = createResult.Address,
                Endorsement = nestedEndorsement,
                Signatory = new Signatory(originalPrivateKey)
            });
            Assert.Equal(ResponseCode.Success, updateResult.Status);

            var updatedInfo = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(nestedEndorsement, updatedInfo.Endorsement);

            var receipt = await client.TransferAsync(createResult.Address, _network.Payer, 5, originalPrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var newBalance = await client.GetAccountBalanceAsync(createResult.Address);
            Assert.Equal(5ul, newBalance);
        }
    }
}
