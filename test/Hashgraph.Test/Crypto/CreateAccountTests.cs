using Hashgraph.Test.Fixtures;
using NSec.Cryptography;
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
                    var createResult = await client.CreateAccountAsync(publicKey, initialBalance);
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
            }
        }
    }
}
