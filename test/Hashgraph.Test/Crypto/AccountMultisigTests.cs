using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentials))]
    public class AccountMultisigTests
    {
        private readonly NetworkCredentials _network;
        public AccountMultisigTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Multi-Sig Account: Can Create Account")]
        public async Task CanCreateAccountAsync()
        {
            var initialBalance = (ulong)Generator.Integer(10, 200);
            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var endorsement = new Endorsement(publicKey1, publicKey2);
            var signatory = new Signatory(privateKey1, privateKey2);
            var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = initialBalance,
                Endorsement = endorsement
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
            Assert.Equal(endorsement, info.Endorsement);

            Assert.Equal(new Address(0, 0, 0), info.Proxy);
            Assert.False(info.Deleted);

            // Move remaining funds back to primary account.
            var from = createResult.Address;
            await client.TransferAsync(from, _network.Payer, (long)initialBalance, signatory);

            var receipt = await client.DeleteAccountAsync(createResult.Address, _network.Payer, signatory);
            Assert.NotNull(receipt);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var exception = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.GetAccountInfoAsync(createResult.Address);
            });
            Assert.StartsWith("Transaction Failed Pre-Check: AccountDeleted", exception.Message);
        }
        [Fact(DisplayName = "Multi-Sig Account: 2 of 2 requires both signatures to transfer")]
        public async Task RequiresAllSignaturesToTransferOut()
        {
            var initialBalance = (ulong)Generator.Integer(10, 200);
            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var endorsement = new Endorsement(publicKey1, publicKey2);
            var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = initialBalance,
                Endorsement = endorsement
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            // Move funds back to primary account (still use Payer to pay TX Fee)
            var from = createResult.Address;
            var record = await client.TransferWithRecordAsync(from, _network.Payer, (long)initialBalance, new Signatory(privateKey1, privateKey2));

            var balance = await client.GetAccountBalanceAsync(createResult.Address);
            Assert.Equal(0UL, balance);
        }
        [Fact(DisplayName = "Multi-Sig Account: 1 of 2 requires only one signature to transfer")]
        public async Task RequiresOneOfTwoSignaturesToTransferOut()
        {
            var initialBalance = (ulong)Generator.Integer(10, 200);
            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var endorsement = new Endorsement(1, publicKey1, publicKey2);
            var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = initialBalance,
                Endorsement = endorsement
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            // Move funds back to primary account (still use Payer to pay TX Fee)
            var from = createResult.Address;
            var record = await client.TransferWithRecordAsync(from, _network.Payer, (long)initialBalance, privateKey1);

            var balance = await client.GetAccountBalanceAsync(createResult.Address);
            Assert.Equal(0UL, balance);
        }
        [Fact(DisplayName = "Multi-Sig Account: 2 sets of 1 of 2 requires only one signature from each compound key to transfer")]
        public async Task RequiresTwoSetsOfOneOfTwoSignaturesToTransferOut()
        {
            var initialBalance = (ulong)Generator.Integer(10, 200);
            var (publicKey1a, privateKey1a) = Generator.KeyPair();
            var (publicKey1b, privateKey1b) = Generator.KeyPair();
            var (publicKey2a, privateKey2a) = Generator.KeyPair();
            var (publicKey2b, privateKey2b) = Generator.KeyPair();
            var endorsement = new Endorsement(new Endorsement(1, publicKey1a, publicKey1b), new Endorsement(1, publicKey2a, publicKey2b));
            var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = initialBalance,
                Endorsement = endorsement
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            // Fail by not providing all necessary keys (note only one of the root keys here)
            var from = createResult.Address;
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await client.TransferWithRecordAsync(from, _network.Payer, (long)initialBalance, privateKey1a);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);

            // Now try with proper set of signatures
            from = createResult.Address;
            var record = await client.TransferWithRecordAsync(from, _network.Payer, (long)initialBalance, new Signatory(privateKey1b, privateKey2a));

            var balance = await client.GetAccountBalanceAsync(createResult.Address);
            Assert.Equal(0UL, balance);
        }
        [Fact(DisplayName = "Multi-Sig Account: Can Change signing key to 2 sets of 1 of 2 requires only one signature from each compound key to transfer")]
        public async Task ChangeKeyToRequiresTwoSetsOfOneOfTwoSignaturesToTransferOut()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var (publicKey1a, privateKey1a) = Generator.KeyPair();
            var (publicKey1b, privateKey1b) = Generator.KeyPair();
            var (publicKey2a, privateKey2a) = Generator.KeyPair();
            var (publicKey2b, privateKey2b) = Generator.KeyPair();
            var endorsement = new Endorsement(new Endorsement(1, publicKey1a, publicKey1b), new Endorsement(1, publicKey2a, publicKey2b));
            var receipt = await fx.Client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = fx.Record.Address,
                Endorsement = endorsement,
                Signatory = new Signatory(fx.PrivateKey, privateKey1a, privateKey1b, privateKey2a, privateKey2b)
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);

            // Fail by not providing all necessary keys (note only one of the root keys here)
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.TransferWithRecordAsync(fx.Record.Address, _network.Payer, (long)fx.CreateParams.InitialBalance, fx.PrivateKey);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);

            // Now try with proper set of signatures
            var record = await fx.Client.TransferWithRecordAsync(fx.Record.Address, _network.Payer, (long)fx.CreateParams.InitialBalance, new Signatory(privateKey1a, privateKey2b));

            var balance = await fx.Client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(0UL, balance);
        }
        [Fact(DisplayName = "Multi-Sig Account: Can change to simple signature.")]
        public async Task CanChangeToSimpleSignature()
        {
            var initialBalance = (ulong)Generator.Integer(10, 200);
            var (publicKey1a, privateKey1a) = Generator.KeyPair();
            var (publicKey1b, privateKey1b) = Generator.KeyPair();
            var (publicKey2a, privateKey2a) = Generator.KeyPair();
            var (publicKey2b, privateKey2b) = Generator.KeyPair();
            var (publicKey3, privateKey3) = Generator.KeyPair();
            var endorsement = new Endorsement(new Endorsement(1, publicKey1a, publicKey1b), new Endorsement(1, publicKey2a, publicKey2b));
            var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = initialBalance,
                Endorsement = endorsement
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            // Fail by not providing all necessary keys (note only one of the root keys here)
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await client.TransferWithRecordAsync(createResult.Address, _network.Payer, (long)initialBalance, privateKey3);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);

            await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = createResult.Address,
                Endorsement = publicKey3,
                Signatory = privateKey3,
            }, ctx =>
            {
                ctx.Signatory = new Signatory(_network.Signatory, privateKey1a, privateKey2a);
            });

            // Now try with proper set of signatures
            var record = await client.TransferWithRecordAsync(createResult.Address, _network.Payer, (long)initialBalance, privateKey3);

            var balance = await client.GetAccountBalanceAsync(createResult.Address);
            Assert.Equal(0UL, balance);
        }
        [Fact(DisplayName = "Multi-Sig Account: Can rotate to complex signature.")]
        public async Task CanRotateToComplexSignature()
        {
            var initialBalance = (ulong)Generator.Integer(10, 200);
            var (publicKey1a, privateKey1a) = Generator.KeyPair();
            var (publicKey1b, privateKey1b) = Generator.KeyPair();
            var (publicKey2a, privateKey2a) = Generator.KeyPair();
            var (publicKey2b, privateKey2b) = Generator.KeyPair();
            var (publicKey3a, privateKey3a) = Generator.KeyPair();
            var (publicKey3b, privateKey3b) = Generator.KeyPair();
            var (publicKey3c, privateKey3c) = Generator.KeyPair();
            var endorsement = new Endorsement(new Endorsement(1, publicKey1a, publicKey1b), new Endorsement(1, publicKey2a, publicKey2b));
            var client = _network.NewClient();
            var createResult = await client.CreateAccountAsync(new CreateAccountParams
            {
                InitialBalance = initialBalance,
                Endorsement = endorsement
            });
            Assert.Equal(ResponseCode.Success, createResult.Status);

            // Fail by not providing all necessary keys (note only one of the root keys here)
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await client.TransferWithRecordAsync(createResult.Address, _network.Payer, (long)initialBalance, privateKey3a);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);

            await client.UpdateAccountAsync(new UpdateAccountParams
            {
                Address = createResult.Address,
                Endorsement = new Endorsement(1, publicKey3a, publicKey3b, publicKey3c),
                Signatory = privateKey3a,
            }, ctx =>
            {
                ctx.Signatory = new Signatory(_network.Signatory, privateKey1a, privateKey2a);
            });

            // Now try with proper set of signatures
            var record = await client.TransferWithRecordAsync(createResult.Address, _network.Payer, (long)initialBalance, privateKey3c);

            var balance = await client.GetAccountBalanceAsync(createResult.Address);
            Assert.Equal(0UL, balance);
        }
    }
}