using Hashgraph.Test.Fixtures;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class TransferTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public TransferTests(NetworkCredentialsFixture networkCredentials)
        {
            _networkCredentials = networkCredentials;
        }
        [Fact(DisplayName = "Transfer Tests: Can Send to Gateway Node")]
        public async Task CanTransferCryptoToGatewayNode()
        {
            long fee = 0;
            long transferAmount = 10;
            await using (var client = _networkCredentials.CreateClientWithDefaultConfiguration())
            {
                client.Configure(ctx => fee = ctx.FeeLimit);
                var fromAccount = _networkCredentials.CreateDefaultAccount();
                var toAddress = _networkCredentials.CreateDefaultGateway();
                var balanceBefore = await client.GetAccountBalanceAsync(fromAccount);
                var receipt = await client.TransferAsync(fromAccount, toAddress, transferAmount);
                var balanceAfter = await client.GetAccountBalanceAsync(fromAccount);
                Assert.Equal((ulong)transferAmount + (ulong)fee + (ulong)fee, balanceBefore - balanceAfter);
            }
        }
        [Fact(DisplayName = "Transfer Tests: Can Send to New Account")]
        public async Task CanTransferCryptoToNewAccount()
        {
            var transferAmount = (long)Generator.Integer(10, 100);
            var initialBalance = (ulong)Generator.Integer(10, 100);
            var (publicKey, privateKey) = Generator.KeyPair();
            await using (var client = _networkCredentials.CreateClientWithDefaultConfiguration())
            {
                var newAddress = await client.CreateAccountAsync(publicKey, initialBalance);
                var newBalance = await client.GetAccountBalanceAsync(newAddress);
                Assert.Equal(initialBalance, newBalance);

                var receipt = await client.TransferAsync(_networkCredentials.CreateDefaultAccount(), newAddress, transferAmount);
                var newBalanceAfterTransfer = await client.GetAccountBalanceAsync(newAddress);
                Assert.Equal(initialBalance + (ulong)transferAmount, newBalanceAfterTransfer);
            }
        }
        [Fact(DisplayName = "Transfer Tests: Can Send from New Account")]
        public async Task CanTransferCryptoFromNewAccount()
        {
            var initialBalance = (ulong)Generator.Integer(10000, 100000);
            var transferAmount = initialBalance / 2;
            var (publicKey, privateKey) = Generator.KeyPair();
            await using (var client = _networkCredentials.CreateClientWithDefaultConfiguration())
            {
                var newAddress = await client.CreateAccountAsync(publicKey, initialBalance);
                var newAccount = new Account(newAddress.RealmNum, newAddress.ShardNum, newAddress.AccountNum, privateKey);
                var info = await client.GetAccountInfoAsync(newAddress);
                Assert.Equal(initialBalance, info.Balance);
                Assert.Equal(publicKey.ToArray().TakeLast(32).ToArray(), info.PublicKey.ToArray());

                var receipt = await client.TransferAsync(newAccount, _networkCredentials.CreateDefaultAccount(), (long)transferAmount);
                var newBalanceAfterTransfer = await client.GetAccountBalanceAsync(newAddress);
                Assert.Equal(initialBalance - (ulong)transferAmount, newBalanceAfterTransfer);
            }
        }
        [Fact(DisplayName = "Transfer Tests: Can Drain All Crypto from New Account")]
        public async Task CanTransferAllCryptoFromNewAccount()
        {
            var initialBalance = (ulong)Generator.Integer(10000, 100000);
            var (publicKey, privateKey) = Generator.KeyPair();
            await using (var client = _networkCredentials.CreateClientWithDefaultConfiguration())
            {
                var newAddress = await client.CreateAccountAsync(publicKey, initialBalance);
                var newAccount = new Account(newAddress.RealmNum, newAddress.ShardNum, newAddress.AccountNum, privateKey);
                var info = await client.GetAccountInfoAsync(newAddress);
                Assert.Equal(initialBalance, info.Balance);
                Assert.Equal(publicKey.ToArray().TakeLast(32).ToArray(), info.PublicKey.ToArray());

                var receipt = await client.TransferAsync(newAccount, _networkCredentials.CreateDefaultAccount(), (long)initialBalance);
                var newBalanceAfterTransfer = await client.GetAccountBalanceAsync(newAddress);
                Assert.Equal(0UL, newBalanceAfterTransfer);
            }
        }
    }
}
