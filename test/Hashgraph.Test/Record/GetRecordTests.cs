using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Record
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class GetRecordTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public GetRecordTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "Get Record: Can get Transaction Record")]
        public async Task CanGetTransactionRecord()
        {
            await using var fx = await TestAccountInstance.CreateAsync(_networkCredentials);

            var amount = Generator.Integer(20, 30);
            var receipt = await fx.Client.TransferAsync(_networkCredentials.CreateDefaultAccount(), fx.AccountRecord.Address, amount);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            var record = await fx.Client.GetTransactionRecordAsync(receipt.Id);
            Assert.NotNull(record);
            Assert.Equal(receipt.Id, record.Id);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Transfer Crypto", record.Memo);
            Assert.InRange(record.Fee, 0UL, 100_000UL);
        }
        [Fact(DisplayName = "Get Record: Empty Transaction ID throws error.")]
        public async Task EmptyTransactionIdThrowsError()
        {
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () => {
                await client.GetTransactionRecordAsync(null);
            });
            Assert.Equal("transaction", ane.ParamName);
            Assert.StartsWith("Transaction is missing. Please check that it is not null.", ane.Message);
        }
        [Fact(DisplayName = "Get Record: Invalid Transaction ID throws error.")]
        public async Task InvalidTransactionIdThrowsError()
        {
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var txId = Protobuf.FromTransactionId(Transactions.CreateNewTransactionID(_networkCredentials.CreateDefaultAccount(), DateTime.UtcNow));
            var tex = await Assert.ThrowsAsync<TransactionException>(async () => {
                await client.GetTransactionRecordAsync(txId);
            });
            Assert.Equal(ResponseCode.RecordNotFound, tex.Status);
            Assert.StartsWith("Unable to retrieve transaction record.", tex.Message);
        }
        [Fact(DisplayName = "Get Record: Can Get Record for Existing but Failed Transaction")]
        public async Task CanGetRecordForFailedTransaction()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () => {
                await fx.Client.CallContractWithRecordAsync(new CallContractParams
                {
                    Contract = fx.ContractCreateRecord.Contract,
                    Gas = 50_000,
                    FunctionName = "not_a_real_method",
                });
            });
            var record = await fx.Client.GetTransactionRecordAsync(tex.TxId);
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.ContractRevertExecuted, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Call Contract", record.Memo);
            Assert.InRange(record.Fee, 0UL, 100_000UL);
        }
    }
}
