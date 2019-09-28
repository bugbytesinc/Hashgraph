using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.QueryFee
{
    [Collection(nameof(NetworkCredentials))]
    public class QueryFeeTests
    {
        private readonly NetworkCredentials _network;
        public QueryFeeTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Query Fee: Can Get Query Fees of Query Fees")]
        public async Task CheckQueryFeesOfQueryFees()
        {
            // Note: We have to make TXs manually because the API does not 
            // at this time expose the TX used to pay for a query.

            await using var fx = await TestAccount.CreateAsync(_network);
            var account = new Account(fx.Record.Address, fx.PrivateKey);
            await fx.Client.TransferAsync(_network.Payer, account, 10_000_000);
            await using var client = fx.Client.Clone(ctx => {
                ctx.Payer = account;
                ctx.FeeLimit = 1_000_000;
            });

            var balanceBeforeQuery = (long)await fx.Client.GetAccountBalanceAsync(account);

            var txGetAccountInfo = client.CreateNewTxId();
            var accountInfo = await client.GetAccountInfoAsync(_network.Payer, ctx => ctx.Transaction = txGetAccountInfo);

            // Since accountInfo is returned before the transaction to pay for it settles, 
            // wait for the transaction to settle then get target balance.
            await fx.Client.GetReceiptAsync(txGetAccountInfo);
            var balanceAfterQuery = (long)await fx.Client.GetAccountBalanceAsync(account);

            // Get the the transaction record with target account for real
            var txQueryRecord = client.CreateNewTxId();
            var queryRecord = await client.GetTransactionRecordAsync(txGetAccountInfo, ctx => ctx.Transaction = txQueryRecord);

            // Once again wait for transaction to settle using payer account, then get target balance
            await fx.Client.GetReceiptAsync(txQueryRecord);
            var balanceAfterQueryRecord = (long)await fx.Client.GetAccountBalanceAsync(account);

            // Get the transaction record one more time with target account
            var txQueryRecordRecord = client.CreateNewTxId();
            var queryRecordRecord = await client.GetTransactionRecordAsync(txQueryRecord, ctx => ctx.Transaction = txQueryRecordRecord);

            // Once again wait for transaction to settle using payer account, then get target balance
            await fx.Client.GetReceiptAsync(txQueryRecordRecord);
            var balanceAfterQueryRecordRecord = (long)await fx.Client.GetAccountBalanceAsync(account);

            _network.Output?.WriteLine($"Initial Balance:              {balanceBeforeQuery:n0}");
            _network.Output?.WriteLine($"After Get Account Info Query: {balanceAfterQuery:n0}");
            _network.Output?.WriteLine($"After Get Record:             {balanceAfterQueryRecord:n0}");
            _network.Output?.WriteLine($"After Record Record:          {balanceAfterQueryRecordRecord:n0}");

            Assert.Equal(balanceBeforeQuery + queryRecord.Transfers[account], balanceAfterQuery);
            Assert.Equal(balanceAfterQuery + queryRecordRecord.Transfers[account], balanceAfterQueryRecord);
            Assert.True(balanceAfterQueryRecord > balanceAfterQueryRecordRecord);
        }
    }
}
