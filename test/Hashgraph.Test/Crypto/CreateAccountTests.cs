using Hashgraph.Test.Fixtures;
using NSec.Cryptography;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class CreateAccountTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public CreateAccountTests(NetworkCredentialsFixture networkCredentials)
        {
            _networkCredentials = networkCredentials;
        }
        [Fact(DisplayName = "Create Account: Can Create Account")]
        public async Task CanCreateAccountAsync()
        {
            using (var key = Key.Create(SignatureAlgorithm.Ed25519, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport }))
            {
                var initialBalance = (ulong)Generator.Integer(10, 200);
                var (publicKey, privateKey) = Generator.KeyPair();
                await using (var client = _networkCredentials.CreateClientWithDefaultConfiguration())
                {
                    var newAddress = await client.CreateAccountAsync(publicKey, initialBalance);
                    Assert.NotNull(newAddress);
                    Assert.Equal(_networkCredentials.ServerRealm, newAddress.RealmNum);
                    Assert.Equal(_networkCredentials.ServerShard, newAddress.ShardNum);
                    Assert.True(newAddress.AccountNum > 0);

                    var info = await client.GetAccountInfoAsync(newAddress);
                    Assert.Equal(initialBalance, info.Balance);
                    Assert.Equal(newAddress.RealmNum, info.Address.RealmNum);
                    Assert.Equal(newAddress.ShardNum, info.Address.ShardNum);
                    Assert.Equal(newAddress.AccountNum, info.Address.AccountNum);
                    Assert.False(info.Deleted);

                    // Move remaining funds back to primary account.
                    var from = new Account(newAddress.RealmNum, newAddress.ShardNum, newAddress.AccountNum, privateKey);
                    await client.TransferAsync(from, _networkCredentials.CreateDefaultAccount(), (long)initialBalance);

                    // We should do the following when deleting an account is testable on the network.
                    //await client.DeleteAccountAsync(newAccount, _networkCredentials.CreateDefaultAccount());
                    //var deletedAccount = await client.DeleteAccountAsync(createdAccount, _networkCredentials.CreateDefaultAccount());
                    //Assert.NotNull(deletedAccount);
                    //Assert.Equal(_networkCredentials.ServerRealm, deletedAccount.RealmNum);
                    //Assert.Equal(_networkCredentials.ServerShard, deletedAccount.ShardNum);
                    //Assert.True(deletedAccount.AccountNum > 0);

                    info = await client.GetAccountInfoAsync(newAddress);
                    Assert.Equal(0UL, info.Balance);
                    Assert.Equal(newAddress.RealmNum, info.Address.RealmNum);
                    Assert.Equal(newAddress.ShardNum, info.Address.ShardNum);
                    Assert.Equal(newAddress.AccountNum, info.Address.AccountNum);
                    //Assert.True(info.Deleted);
                }
            }
        }
    }
}
