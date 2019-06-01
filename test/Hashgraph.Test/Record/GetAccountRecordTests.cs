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
        [Fact(DisplayName = "Account Records: Can get Transaction Record for Account")]
        public async Task CanGetTransactionRecords()
        {
            await using var fx = await TestAccount.CreateAsync(_network);

            var transactionCount = Generator.Integer(2, 5);
            var childAccount = new Account(fx.Record.Address, fx.PrivateKey);
            var parentAccount = _network.Payer;
            await fx.Client.TransferAsync(parentAccount, childAccount, transactionCount * 100001);
            await using (var client = fx.Client.Clone(ctx => ctx.Payer = childAccount))
            {
                for (int i = 0; i < transactionCount; i++)
                {
                    await client.TransferAsync(childAccount, parentAccount, 1);
                }
            }
            var records = await fx.Client.GetAccountRecordsAsync(childAccount);
            Assert.NotNull(records);
            Assert.Equal(transactionCount, records.Length);
            foreach (var record in records)
            {
                Assert.Equal(ResponseCode.Success, record.Status);
                Assert.False(record.Hash.IsEmpty);
                Assert.NotNull(record.Concensus);
                Assert.Equal("Transfer Crypto", record.Memo);
                Assert.InRange(record.Fee, 0UL, 100_000UL);
            }
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
