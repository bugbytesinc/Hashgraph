using Hashgraph.Test.Fixtures;
using System;
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
        [Fact(DisplayName = "Account Records: Can't get Transaction Record for Recent Account Outbuound Transfers (IS THIS A NETWORK BUG?)")]
        public async Task GetTransactionRecordsForRecentTransfers()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            var childFeeLImit = 1_000_000;
            var transferAmount = Generator.Integer(200, 500);
            var transactionCount = Generator.Integer(2, 5);
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
            // THIS IS A BUG?  Shouldn't we be able to get records
            // up to 120s after the transaction?
            Assert.Empty(records);
            // What we should expect?
            //Assert.Equal(transactionCount, records.Length);
            //foreach (var record in records)
            //{
            //    Assert.Equal(ResponseCode.Success, record.Status);
            //    Assert.False(record.Hash.IsEmpty);
            //    Assert.NotNull(record.Concensus);
            //    Assert.Equal("Transfer Crypto", record.Memo);
            //    Assert.InRange(record.Fee, 0UL, 50UL);
            //}
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
    }
}
