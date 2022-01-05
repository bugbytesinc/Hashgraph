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
            // NETWORK V0.21.0 DEFECT vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(infoFromAccount.Ledger.ToArray());
            // NETWORK V0.21.0 DEFECT: ^^^^

            var infoFromAlias = await client.GetAccountInfoAsync(alias);
            Assert.Equal(createReceipt.Address, infoFromAlias.Address);
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
            // NETWORK V0.21.0 DEFECT vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(infoFromAlias.Ledger.ToArray());
            // NETWORK V0.21.0 DEFECT: ^^^^
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
            // NETWORK V0.21.0 DEFECT vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(infoFromAccount.Ledger.ToArray());
            // NETWORK V0.21.0 DEFECT: ^^^^

            var infoFromAlias = await client.GetAccountInfoAsync(alias);
            Assert.Equal(createReceipt.Address, infoFromAlias.Address);
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
            // NETWORK V0.21.0 DEFECT vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(infoFromAlias.Ledger.ToArray());
            // NETWORK V0.21.0 DEFECT: ^^^^
        }
        [Fact(DisplayName = "Create Account By Alias: Can Create Account and get Records")]
        public async Task CanCreateAccountAndGetRecords()
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
            var allRecords = await client.GetAllTransactionRecordsAsync(receipt.Id);
            Assert.Equal(2, allRecords.Count);
            Assert.Null(allRecords[0].ParentTransactionConcensus);

            var createRecord = allRecords[1] as CreateAccountRecord;
            Assert.NotNull(createRecord);
            Assert.NotNull(createRecord.Address);
            Assert.Equal(_network.ServerRealm, createRecord.Address.RealmNum);
            Assert.Equal(_network.ServerShard, createRecord.Address.ShardNum);
            Assert.True(createRecord.Address.AccountNum > 0);
            Assert.Equal(1, createRecord.Id.Nonce);
            // NETWORK V0.21.0 DEFECT vvvv
            // NOT IMPLEMENTED YET
            //Assert.Equal(allRecords[0].Concensus, createRecord.ParentTransactionConcensus);
            Assert.Null(createRecord.ParentTransactionConcensus);
            // NETWORK V0.21.0 DEFECT: ^^^^

            var createRecordByTx = await client.GetTransactionRecordAsync(createRecord.Id) as CreateAccountRecord;
            Assert.NotNull(createRecordByTx);
            Assert.NotNull(createRecordByTx.Address);
            Assert.Equal(_network.ServerRealm, createRecordByTx.Address.RealmNum);
            Assert.Equal(_network.ServerShard, createRecordByTx.Address.ShardNum);
            Assert.Equal(createRecord.Address, createRecordByTx.Address);
            Assert.Equal(createRecord.Id, createRecordByTx.Id);
            // NETWORK V0.21.0 DEFECT vvvv
            // NOT IMPLEMENTED YET
            //Assert.Equal(allRecords[0].Concensus, createRecordByTx.ParentTransactionConcensus);
            Assert.Null(createRecordByTx.ParentTransactionConcensus);
            // NETWORK V0.21.0 DEFECT: ^^^^            

            var balances = await client.GetAccountBalancesAsync(alias);
            Assert.NotNull(balances);
            Assert.Equal(createRecord.Address, balances.Address);
            Assert.True(balances.Crypto > 0);
            Assert.Empty(balances.Tokens);

            var infoFromAccount = await client.GetAccountInfoAsync(createRecord.Address);
            Assert.Equal(createRecord.Address, infoFromAccount.Address);
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
            // NETWORK V0.21.0 DEFECT vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(infoFromAccount.Ledger.ToArray());
            // NETWORK V0.21.0 DEFECT: ^^^^

            var infoFromAlias = await client.GetAccountInfoAsync(alias);
            Assert.Equal(createRecord.Address, infoFromAlias.Address);
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
            // NETWORK V0.21.0 DEFECT vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(infoFromAlias.Ledger.ToArray());
            // NETWORK V0.21.0 DEFECT: ^^^^
        }
    }
}