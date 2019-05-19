using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class TransferTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public TransferTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "Transfer: Can Send to Gateway Node")]
        public async Task CanTransferCryptoToGatewayNode()
        {
            long fee = 0;
            long transferAmount = 10;
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            client.Configure(ctx => fee = ctx.FeeLimit);
            var fromAccount = _networkCredentials.CreateDefaultAccount();
            var toAddress = _networkCredentials.CreateDefaultGateway();
            var balanceBefore = await client.GetAccountBalanceAsync(fromAccount);
            var receipt = await client.TransferAsync(fromAccount, toAddress, transferAmount);
            var balanceAfter = await client.GetAccountBalanceAsync(fromAccount);
            var maxFee = (ulong)(3 * fee);
            Assert.InRange(balanceAfter, balanceBefore - (ulong)transferAmount - maxFee, balanceBefore - (ulong)transferAmount);
        }
        [Fact(DisplayName = "Transfer: Can Send to New Account")]
        public async Task CanTransferCryptoToNewAccount()
        {
            var transferAmount = (long)Generator.Integer(10, 100);
            var initialBalance = (ulong)Generator.Integer(10, 100);
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = initialBalance,
                PublicKey = publicKey
            });
            var newBalance = await client.GetAccountBalanceAsync(createResult.Address);
            Assert.Equal(initialBalance, newBalance);

            var receipt = await client.TransferAsync(_networkCredentials.CreateDefaultAccount(), createResult.Address, transferAmount);
            var newBalanceAfterTransfer = await client.GetAccountBalanceAsync(createResult.Address);
            Assert.Equal(initialBalance + (ulong)transferAmount, newBalanceAfterTransfer);
        }
        [Fact(DisplayName = "Transfer: Can Send from New Account")]
        public async Task CanTransferCryptoFromNewAccount()
        {
            var initialBalance = (ulong)Generator.Integer(10000, 100000);
            var transferAmount = initialBalance / 2;
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = initialBalance,
                PublicKey = publicKey
            });
            var newAccount = new Account(createResult.Address.RealmNum, createResult.Address.ShardNum, createResult.Address.AccountNum, privateKey);
            var info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(initialBalance, info.Balance);
            Assert.Equal(new Endorsement(publicKey), info.Endorsement);

            var receipt = await client.TransferAsync(newAccount, _networkCredentials.CreateDefaultAccount(), (long)transferAmount);
            var newBalanceAfterTransfer = await client.GetAccountBalanceAsync(createResult.Address);
            Assert.Equal(initialBalance - (ulong)transferAmount, newBalanceAfterTransfer);
        }
        [Fact(DisplayName = "Transfer: Can Drain All Crypto from New Account")]
        public async Task CanTransferAllCryptoFromNewAccount()
        {
            var initialBalance = (ulong)Generator.Integer(10000, 100000);
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = initialBalance,
                PublicKey = publicKey
            });
            var newAccount = new Account(createResult.Address.RealmNum, createResult.Address.ShardNum, createResult.Address.AccountNum, privateKey);
            var info = await client.GetAccountInfoAsync(createResult.Address);
            Assert.Equal(initialBalance, info.Balance);
            Assert.Equal(new Endorsement(publicKey), info.Endorsement);

            var receipt = await client.TransferAsync(newAccount, _networkCredentials.CreateDefaultAccount(), (long)initialBalance);
            var newBalanceAfterTransfer = await client.GetAccountBalanceAsync(createResult.Address);
            Assert.Equal(0UL, newBalanceAfterTransfer);
        }
        [Fact(DisplayName = "Transfer: Insufficient Funds Throws Error")]
        public async Task InsufficientFundsThrowsError()
        {
            var initialBalance = (ulong)Generator.Integer(10, 100);
            var transferAmount = (long)(initialBalance * 2);
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var address = (await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = initialBalance,
                PublicKey = publicKey
            })).Address;
            var account = new Account(address.RealmNum, address.ShardNum, address.AccountNum, privateKey);
            var exception = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await client.TransferAsync(account, _networkCredentials.CreateDefaultAccount(), transferAmount);
            });
            Assert.StartsWith("Unable to execute crypto transfer, status: InsufficientAccountBalance", exception.Message);
            Assert.NotNull(exception.TxId);
            Assert.Equal(ResponseCode.InsufficientAccountBalance, exception.Status);
        }
        [Fact(DisplayName = "Transfer: Insufficient Fee Throws Error")]
        public async Task InsufficientFeeThrowsError()
        {
            var initialBalance = (ulong)Generator.Integer(10, 100);
            var transferAmount = (long)(initialBalance / 2);
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var address = (await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = initialBalance,
                PublicKey = publicKey
            })).Address;
            var account = new Account(address.RealmNum, address.ShardNum, address.AccountNum, privateKey);
            var exception = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.TransferAsync(account, _networkCredentials.CreateDefaultAccount(), transferAmount, ctx =>
                {
                    ctx.FeeLimit = 1;
                });
            });
            Assert.StartsWith("Transaction Failed Pre-Check: InsufficientTxFee", exception.Message);
            Assert.Equal(ResponseCode.InsufficientTxFee, exception.Status);
        }
    }
}
