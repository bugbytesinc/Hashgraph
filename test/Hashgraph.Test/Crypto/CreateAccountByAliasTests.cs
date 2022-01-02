using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentials))]
    public class CreateAccountByAliasTests
    {
        private readonly NetworkCredentials _network;
        public CreateAccountByAliasTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Create Account By Alias: Can Create Account")]
        public async Task CanCreateAccountAsync()
        {
            var initialPayment = 1_00_000_000;
            var (publicKey, privateKey) = Generator.KeyPair();
            var alias = new Alias(publicKey);
            var client = _network.NewClient();
            var receipt = await client.TransferAsync(_network.Payer, alias, initialPayment);
            Assert.NotNull(receipt);

            // If an account was created by the alias, the receipt
            // with the address is a "child receipt" of the transfer
            // receipt and must be explictly asked for.
            var allReceipts = await client.GetAllReceiptsAsync(receipt.Id);
            Assert.Equal(2, allReceipts.Count);
            Assert.Equal(receipt, allReceipts[0]);

            var createReceipt = allReceipts[1] as CreateAccountReceipt;
            Assert.NotNull(createReceipt);
            Assert.NotNull(createReceipt.Address);
            Assert.Equal(_network.ServerRealm, createReceipt.Address.RealmNum);
            Assert.Equal(_network.ServerShard, createReceipt.Address.ShardNum);
            Assert.True(createReceipt.Address.AccountNum > 0);
            Assert.Equal(1, createReceipt.Id.Nonce);

            var createReceiptByTx = await client.GetReceiptAsync(createReceipt.Id) as CreateAccountReceipt;
            Assert.NotNull(createReceiptByTx);
            Assert.NotNull(createReceiptByTx.Address);
            Assert.Equal(_network.ServerRealm, createReceiptByTx.Address.RealmNum);
            Assert.Equal(_network.ServerShard, createReceiptByTx.Address.ShardNum);
            Assert.Equal(createReceipt.Address, createReceiptByTx.Address);
            Assert.Equal(createReceipt.Id, createReceiptByTx.Id);

            var balances = await client.GetAccountBalancesAsync(alias);
            Assert.NotNull(balances);
            Assert.Equal(createReceipt.Address, balances.Address);
            Assert.True(balances.Crypto > 0);
            Assert.Empty(balances.Tokens);

            var infoFromAccount = await client.GetAccountInfoAsync(createReceipt.Address);
            Assert.Equal(createReceipt.Address, infoFromAccount.Address);
            Assert.Equal(alias, infoFromAccount.Alias);
            Assert.NotNull(infoFromAccount.SmartContractId);
            Assert.False(infoFromAccount.Deleted);
            Assert.NotNull(infoFromAccount.Proxy);
            Assert.Equal(0, infoFromAccount.ProxiedToAccount);
            Assert.Equal(new Endorsement(publicKey), infoFromAccount.Endorsement);
            Assert.True(infoFromAccount.Balance > 0);
            Assert.False(infoFromAccount.ReceiveSignatureRequired);
            Assert.True(infoFromAccount.AutoRenewPeriod.TotalSeconds > 0);
            Assert.True(infoFromAccount.Expiration > DateTime.MinValue);
            Assert.Equal(0, infoFromAccount.AssetCount);
            Assert.Equal(0, infoFromAccount.AutoAssociationLimit);
            Assert.Equal("auto-created account", infoFromAccount.Memo);

            var infoFromAlias = await client.GetAccountInfoAsync(alias);
            // NETWORK V0.21.0 DEFECT: vvvvvvv
            // The following does would be correct behavior
            // Assert.Equal(createReceipt.Address, infoFromAlias.Address);
            // NETWORK V0.21.0 DEFECT: However this is present behavior.
            Assert.Equal(Address.None, infoFromAlias.Address);
            // NETWORK V0.21.0 DEFECT: ^^^^^^^
            Assert.Equal(alias, infoFromAlias.Alias);
            Assert.NotNull(infoFromAlias.SmartContractId);
            Assert.False(infoFromAlias.Deleted);
            Assert.NotNull(infoFromAlias.Proxy);
            Assert.Equal(0, infoFromAlias.ProxiedToAccount);
            Assert.Equal(new Endorsement(publicKey), infoFromAlias.Endorsement);
            Assert.True(infoFromAlias.Balance > 0);
            Assert.False(infoFromAlias.ReceiveSignatureRequired);
            Assert.True(infoFromAlias.AutoRenewPeriod.TotalSeconds > 0);
            Assert.True(infoFromAlias.Expiration > DateTime.MinValue);
            Assert.Equal(0, infoFromAlias.AssetCount);
            Assert.Equal(0, infoFromAlias.AutoAssociationLimit);
            Assert.Equal("auto-created account", infoFromAlias.Memo);
        }

        [Fact(DisplayName = "Create Account By Alias: Can Schedule Create Account")]
        public async Task CanScheduleCreateAccountAsync()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            var initialPayment = 1_00_000_000;
            var (publicKey, privateKey) = Generator.KeyPair();
            var alias = new Alias(publicKey);
            var client = _network.NewClient();
            var receipt = await client.TransferAsync(_network.Payer, alias, initialPayment, new PendingParams { PendingPayer = fxPayer });
            Assert.NotNull(receipt);
            Assert.NotNull(receipt.Pending);

            var signingReceipt = await client.SignPendingTransactionAsync(receipt.Pending.Id, fxPayer);
            Assert.Equal(ResponseCode.Success, signingReceipt.Status);

            // If an account was created by the alias, the receipt
            // with the address is a "child receipt" of the transfer
            // receipt and must be explictly asked for.
            var allReceipts = await client.GetAllReceiptsAsync(receipt.Pending.TxId);
            Assert.Equal(2, allReceipts.Count);
            Assert.Equal(receipt.Pending.TxId, allReceipts[0].Id);

            var createReceipt = allReceipts[1] as CreateAccountReceipt;
            Assert.NotNull(createReceipt);
            Assert.NotNull(createReceipt.Address);
            Assert.Equal(_network.ServerRealm, createReceipt.Address.RealmNum);
            Assert.Equal(_network.ServerShard, createReceipt.Address.ShardNum);
            Assert.True(createReceipt.Address.AccountNum > 0);
            Assert.Equal(1, createReceipt.Id.Nonce);

            var createReceiptByTx = await client.GetReceiptAsync(createReceipt.Id) as CreateAccountReceipt;
            Assert.NotNull(createReceiptByTx);
            Assert.NotNull(createReceiptByTx.Address);
            Assert.Equal(_network.ServerRealm, createReceiptByTx.Address.RealmNum);
            Assert.Equal(_network.ServerShard, createReceiptByTx.Address.ShardNum);
            Assert.Equal(createReceipt.Address, createReceiptByTx.Address);
            Assert.Equal(createReceipt.Id, createReceiptByTx.Id);

            var balances = await client.GetAccountBalancesAsync(alias);
            Assert.NotNull(balances);
            Assert.Equal(createReceipt.Address, balances.Address);
            Assert.True(balances.Crypto > 0);
            Assert.Empty(balances.Tokens);

            var infoFromAccount = await client.GetAccountInfoAsync(createReceipt.Address);
            Assert.Equal(createReceipt.Address, infoFromAccount.Address);
            Assert.Equal(alias, infoFromAccount.Alias);
            Assert.NotNull(infoFromAccount.SmartContractId);
            Assert.False(infoFromAccount.Deleted);
            Assert.NotNull(infoFromAccount.Proxy);
            Assert.Equal(0, infoFromAccount.ProxiedToAccount);
            Assert.Equal(new Endorsement(publicKey), infoFromAccount.Endorsement);
            Assert.True(infoFromAccount.Balance > 0);
            Assert.False(infoFromAccount.ReceiveSignatureRequired);
            Assert.True(infoFromAccount.AutoRenewPeriod.TotalSeconds > 0);
            Assert.True(infoFromAccount.Expiration > DateTime.MinValue);
            Assert.Equal(0, infoFromAccount.AssetCount);
            Assert.Equal(0, infoFromAccount.AutoAssociationLimit);
            Assert.Equal("auto-created account", infoFromAccount.Memo);

            var infoFromAlias = await client.GetAccountInfoAsync(alias);
            // NETWORK V0.21.0 DEFECT: vvvvvvv
            // The would be correct behavior, not implemented by the network
            // Assert.Equal(createReceipt.Address, infoFromAlias.Address);
            // The following is how it is returning at the moment.
            Assert.Equal(Address.None, infoFromAlias.Address);
            // NETWORK V0.21.0 DEFECT: ^^^^^^^
            Assert.Equal(alias, infoFromAlias.Alias);
            Assert.NotNull(infoFromAlias.SmartContractId);
            Assert.False(infoFromAlias.Deleted);
            Assert.NotNull(infoFromAlias.Proxy);
            Assert.Equal(0, infoFromAlias.ProxiedToAccount);
            Assert.Equal(new Endorsement(publicKey), infoFromAlias.Endorsement);
            Assert.True(infoFromAlias.Balance > 0);
            Assert.False(infoFromAlias.ReceiveSignatureRequired);
            Assert.True(infoFromAlias.AutoRenewPeriod.TotalSeconds > 0);
            Assert.True(infoFromAlias.Expiration > DateTime.MinValue);
            Assert.Equal(0, infoFromAlias.AssetCount);
            Assert.Equal(0, infoFromAlias.AutoAssociationLimit);
            Assert.Equal("auto-created account", infoFromAlias.Memo);
        }
    }
}