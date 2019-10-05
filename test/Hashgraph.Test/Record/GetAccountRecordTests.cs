using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Record
{
    [Collection(nameof(NetworkCredentials))]
    public class GetAccountRecordTests
    {
        private readonly NetworkCredentials _network;
        public GetAccountRecordTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Account Records: No transactions are stored when thresholds are at max values.")]
        public async Task GetTransactionRecordsForRecentTransfers()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            var childFeeLImit = 1_000_000;
            var transferAmount = Generator.Integer(200, 500);
            var transactionCount = Generator.Integer(3, 6);
            var childAccount = new Account(fxAccount.Record.Address, fxAccount.PrivateKey);
            var parentAccount = _network.Payer;
            await fxAccount.Client.TransferAsync(parentAccount, childAccount, transactionCount * (childFeeLImit + transferAmount));
            await using (var client = fxAccount.Client.Clone(ctx => { ctx.Payer = childAccount; ctx.FeeLimit = childFeeLImit; }))
            {
                for (int i = 0; i < transactionCount; i++)
                {
                    await client.TransferWithRecordAsync(childAccount, parentAccount, transferAmount);
                }
            }
            var records = await fxAccount.Client.GetAccountRecordsAsync(childAccount);
            Assert.NotNull(records);
            Assert.Empty(records);
        }
        [Fact(DisplayName = "NOT SUPPORTED: NETWORK BUG / SECURITY FLAW: Account Records: Can get inbound transfers above specified threshold.")]
        public async Task GetTransactionRecordsForRecentTransfersInAboveThresholdIsBroken()
        {
            // This fails because the first transfer does not fill the account
            // with enough crypto to pay for the record generation.  This is a 
            // vector to attack an account with this feature turned on.
            Assert.StartsWith("Assert.Equal() Failure", (await Assert.ThrowsAsync<Xunit.Sdk.EqualException>(GetTransactionRecordsForRecentTransfersInAboveThreshold)).Message);

            //[Fact(DisplayName = "Account Records: Can get inbound transfers above specified threshold.")]
            async Task GetTransactionRecordsForRecentTransfersInAboveThreshold()
            {
                await using var fx = await TestAccount.CreateAsync(_network);
                await fx.Client.UpdateAccountWithRecordAsync(new UpdateAccountParams
                {
                    Account = new Account(fx.Record.Address, fx.PrivateKey),
                    ReceiveThresholdCreateRecord = 100
                });
                var record1 = await fx.Client.TransferWithRecordAsync(_network.Payer, fx.Record.Address, Generator.Integer(10, 90));
                var record2 = await fx.Client.TransferWithRecordAsync(_network.Payer, fx.Record.Address, Generator.Integer(101, 200));
                var record3 = await fx.Client.TransferWithRecordAsync(_network.Payer, fx.Record.Address, Generator.Integer(200, 300));
                var record4 = await fx.Client.TransferWithRecordAsync(_network.Payer, fx.Record.Address, Generator.Integer(300, 400));
                var record5 = await fx.Client.TransferWithRecordAsync(_network.Payer, fx.Record.Address, Generator.Integer(400, 500));
                var records = await fx.Client.GetAccountRecordsAsync(fx.Record.Address);

                Assert.NotNull(records);
                Assert.Equal(4, records.Length);
                Assert.Null(records.FirstOrDefault(r => r.Id == record1.Id));
                var copy2 = records.FirstOrDefault(r => r.Id == record2.Id);
                Assert.NotNull(copy2);
                Assert.Equal(record2.Status, copy2.Status);
                Assert.Equal(record2.Hash.ToArray(), copy2.Hash.ToArray());
                Assert.Equal(record2.Concensus, copy2.Concensus);
                Assert.Equal(record2.Memo, copy2.Memo);
                Assert.Equal(record2.Fee, copy2.Fee);
                var copy3 = records.FirstOrDefault(r => r.Id == record3.Id);
                Assert.NotNull(copy3);
                Assert.Equal(record3.Status, copy3.Status);
                Assert.Equal(record3.Hash.ToArray(), copy3.Hash.ToArray());
                Assert.Equal(record3.Concensus, copy3.Concensus);
                Assert.Equal(record3.Memo, copy3.Memo);
                Assert.Equal(record3.Fee, copy3.Fee);
                var copy4 = records.FirstOrDefault(r => r.Id == record4.Id);
                Assert.NotNull(copy4);
                Assert.Equal(record4.Status, copy4.Status);
                Assert.Equal(record4.Hash.ToArray(), copy4.Hash.ToArray());
                Assert.Equal(record4.Concensus, copy4.Concensus);
                Assert.Equal(record4.Memo, copy4.Memo);
                Assert.Equal(record4.Fee, copy4.Fee);
                var copy5 = records.FirstOrDefault(r => r.Id == record5.Id);
                Assert.NotNull(copy5);
                Assert.Equal(record5.Status, copy5.Status);
                Assert.Equal(record5.Hash.ToArray(), copy5.Hash.ToArray());
                Assert.Equal(record5.Concensus, copy5.Concensus);
                Assert.Equal(record5.Memo, copy5.Memo);
                Assert.Equal(record5.Fee, copy5.Fee);
            }
        }
        [Fact(DisplayName = "Account Records: Can get outbound transfers above specified threshold.")]
        public async Task GetTransactionRecordsForRecentTransfersOutAboveThreshold()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var fromAccount = new Account(fx.Record.Address, fx.PrivateKey);
            await fx.Client.UpdateAccountAsync(new UpdateAccountParams
            {
                Account = fromAccount,
                SendThresholdCreateRecord = 100
            });
            await fx.Client.TransferWithRecordAsync(_network.Payer, fromAccount, 1_000_000);
            var record1 = await fx.Client.TransferWithRecordAsync(fromAccount, _network.Payer, Generator.Integer(10, 90));
            var record2 = await fx.Client.TransferWithRecordAsync(fromAccount, _network.Payer, Generator.Integer(101, 200));
            var record3 = await fx.Client.TransferWithRecordAsync(fromAccount, _network.Payer, Generator.Integer(200, 300));
            var record4 = await fx.Client.TransferWithRecordAsync(fromAccount, _network.Payer, Generator.Integer(300, 400));
            var record5 = await fx.Client.TransferWithRecordAsync(fromAccount, _network.Payer, Generator.Integer(400, 500));
            var records = await fx.Client.GetAccountRecordsAsync(fx.Record.Address);
            Assert.NotNull(records);
            Assert.Equal(4, records.Length);
            Assert.Null(records.FirstOrDefault(r => r.Id == record1.Id));
            var copy2 = records.FirstOrDefault(r => r.Id == record2.Id);
            Assert.NotNull(copy2);
            Assert.Equal(record2.Status, copy2.Status);
            Assert.Equal(record2.Hash.ToArray(), copy2.Hash.ToArray());
            Assert.Equal(record2.Concensus, copy2.Concensus);
            Assert.Equal(record2.Memo, copy2.Memo);
            Assert.Equal(record2.Fee, copy2.Fee);
            var copy3 = records.FirstOrDefault(r => r.Id == record3.Id);
            Assert.NotNull(copy3);
            Assert.Equal(record3.Status, copy3.Status);
            Assert.Equal(record3.Hash.ToArray(), copy3.Hash.ToArray());
            Assert.Equal(record3.Concensus, copy3.Concensus);
            Assert.Equal(record3.Memo, copy3.Memo);
            Assert.Equal(record3.Fee, copy3.Fee);
            var copy4 = records.FirstOrDefault(r => r.Id == record4.Id);
            Assert.NotNull(copy4);
            Assert.Equal(record4.Status, copy4.Status);
            Assert.Equal(record4.Hash.ToArray(), copy4.Hash.ToArray());
            Assert.Equal(record4.Concensus, copy4.Concensus);
            Assert.Equal(record4.Memo, copy4.Memo);
            Assert.Equal(record4.Fee, copy4.Fee);
            var copy5 = records.FirstOrDefault(r => r.Id == record5.Id);
            Assert.NotNull(copy5);
            Assert.Equal(record5.Status, copy5.Status);
            Assert.Equal(record5.Hash.ToArray(), copy5.Hash.ToArray());
            Assert.Equal(record5.Concensus, copy5.Concensus);
            Assert.Equal(record5.Memo, copy5.Memo);
            Assert.Equal(record5.Fee, copy5.Fee);
        }


        [Fact(DisplayName = "Account Records: Get with Empty Account raises Error.")]
        public async Task EmptyAccountRaisesError()
        {
            await using var client = _network.NewClient();
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.GetAccountRecordsAsync(null);
            });
            Assert.Equal("address", ane.ParamName);
            Assert.StartsWith("Account Address is missing. Please check that it is not null.", ane.Message);
        }
        [Fact(DisplayName = "Account Records: Get with Deleted Account raises Error.")]
        public async Task DeletedAccountRaisesError()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            await fx.Client.DeleteAccountAsync(new Account(fx.Record.Address, fx.PrivateKey), _network.Payer);
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.GetAccountRecordsAsync(fx.Record.Address);
            });
            Assert.Equal(ResponseCode.AccountDeleted, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: AccountDeleted", pex.Message);
        }
        [Fact(DisplayName = "Account Records: Receiving account pays for record generation.")]
        public async Task ReceivingAccountPaysForRecordGeneration()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            await fx.Client.TransferWithRecordAsync(_network.Payer, fx.Record.Address, 10_000_000);
            await fx.Client.UpdateAccountWithRecordAsync(new UpdateAccountParams
            {
                Account = new Account(fx.Record.Address, fx.PrivateKey),
                ReceiveThresholdCreateRecord = 0
            });
            var infoBeforeTransfer = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
            var payerBalanceBefore = await fx.Client.GetAccountBalanceAsync(_network.Payer);
            var transferReceipt = await fx.Client.TransferAsync(_network.Payer, fx.Record.Address, 1);
            var payerBalanceAfter = await fx.Client.GetAccountBalanceAsync(_network.Payer);
            var transferRecord = await fx.Client.GetTransactionRecordAsync(transferReceipt.Id);
            var infoAfterTransfer = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
            var records = await fx.Client.GetAccountRecordsAsync(fx.Record.Address);

            Assert.NotNull(records);
            Assert.Single(records);
            Assert.Equal(transferRecord.Status, records[0].Status);
            Assert.Equal(transferRecord.Hash.ToArray(), records[0].Hash.ToArray());
            Assert.Equal(transferRecord.Concensus, records[0].Concensus);
            Assert.Equal(transferRecord.Memo, records[0].Memo);
            Assert.Equal(transferRecord.Fee, records[0].Fee);

            // Note: Balance Goes DOWN for vicitim
            var victimLoss = infoBeforeTransfer.Balance - infoAfterTransfer.Balance;
            Assert.True(victimLoss > 1);

            var attackerCost = payerBalanceBefore - payerBalanceAfter;
            Assert.True(attackerCost > 0);
        }
    }
}
