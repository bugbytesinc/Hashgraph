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
                Endorsement = publicKey
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
            Assert.Equal(new Address(0, 0, 0), info.Proxy);
            Assert.Empty(info.Tokens);
            Assert.False(info.Deleted);

            // Move remaining funds back to primary account.
            var from = createResult.Address;
            await client.TransferAsync(from, _network.Payer, (long)initialBalance, privateKey);

            var receipt = await client.DeleteAccountAsync(createResult.Address, _network.Payer, privateKey);
            Assert.NotNull(receipt);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var exception = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.GetAccountInfoAsync(createResult.Address);
            });
            Assert.StartsWith("Transaction Failed Pre-Check: AccountDeleted", exception.Message);
        }
        [Fact(DisplayName = "Create Account: Set Signature Required True")]
        public async Task CanSetSignatureRequiredTrue()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountWithRecordAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                Endorsement = publicKey,
                Signatory = privateKey,
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
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                Endorsement = publicKey,
                RequireReceiveSignature = false
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.False(info.ReceiveSignatureRequired);
        }
        [Fact(DisplayName = "Create Account: Can't Set Auto Renew Period other than 7890000 seconds")]
        public async Task CanSetAutoRenewPeriod()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            var expectedValue = TimeSpan.FromDays(Generator.Integer(20, 60));
            await using var client = _network.NewClient();
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                var createResult = await client.CreateAccountAsync(new CreateAccountParams
                {
                    InitialBalance = 1,
                    Endorsement = publicKey,
                    AutoRenewPeriod = expectedValue
                });
            });
            Assert.Equal(ResponseCode.AutorenewDurationNotInRange, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: AutorenewDurationNotInRange", pex.Message);
        }
        [Fact(DisplayName = "Create Account: Set Account Proxy Stake")]
        public async Task CanSetProxyStakeAccount()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = 1,
                Endorsement = publicKey,
                Proxy = _network.Gateway,
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            var info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(_network.Gateway, info.Proxy);
        }
        [Fact(DisplayName = "Create Account: Empty Endorsement is Not Allowed")]
        public async Task EmptyEndorsementIsNotAllowed()
        {
            await using var client = _network.NewClient();
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.CreateAccountAsync(new CreateAccountParams
                {
                    InitialBalance = 10,
                    Endorsement = Endorsement.None
                });
            });
            Assert.Equal(ResponseCode.KeyRequired, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: KeyRequired", pex.Message);
        }

        [Fact(DisplayName = "Create Account: Can Schedule Create Account")]
        public async Task CanScheduleCreateAccount()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            var fxAccount = await TestAccount.CreateAsync(_network, fx => {
                fx.CreateParams.Signatory = new ScheduleParams
                {
                    PendingPayer = fxPayer
                };
            });

            var transactionReceipt = await fxPayer.Client.SignPendingTransactionAsync(new SignPendingTransactionParams { 
                Pending = fxAccount.Record.Pending.Pending,
                TransactionBody = fxAccount.Record.Pending.TransactionBody,
                Signatory = fxPayer
            });

            var pendingReceipt = await fxPayer.Client.GetReceiptAsync(fxAccount.Record.Id.AsPending());
            Assert.Equal(ResponseCode.Success, pendingReceipt.Status);

            var createReceipt = Assert.IsType<CreateAccountReceipt>(pendingReceipt);
            var account = createReceipt.Address;

            var info = await fxPayer.Client.GetAccountInfoAsync(account);
            Assert.Equal(account, info.Address);
            Assert.NotNull(info.SmartContractId);
            Assert.False(info.Deleted);
            Assert.NotNull(info.Proxy);
            Assert.Equal(Address.None, info.Proxy);
            Assert.Equal(0, info.ProxiedToAccount);
            Assert.Equal(fxAccount.CreateParams.Endorsement, info.Endorsement);
            Assert.Equal(fxAccount.CreateParams.InitialBalance, info.Balance);
            Assert.Equal(fxAccount.CreateParams.RequireReceiveSignature, info.ReceiveSignatureRequired);
            Assert.Equal(fxAccount.CreateParams.AutoRenewPeriod.TotalSeconds, info.AutoRenewPeriod.TotalSeconds);
            Assert.True(info.Expiration > DateTime.MinValue);
        }
    }
}