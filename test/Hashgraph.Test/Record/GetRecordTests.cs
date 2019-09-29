using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Record
{
    [Collection(nameof(NetworkCredentials))]
    public class GetRecordTests
    {
        private readonly NetworkCredentials _network;
        public GetRecordTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Get Record: Can get Transaction Record")]
        public async Task CanGetTransactionRecord()
        {
            await using var fx = await TestAccount.CreateAsync(_network);

            var amount = Generator.Integer(20, 30);
            var receipt = await fx.Client.TransferAsync(_network.Payer, fx.Record.Address, amount);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            var record = await fx.Client.GetTransactionRecordAsync(receipt.Id);
            Assert.NotNull(record);
            Assert.Equal(receipt.Id, record.Id);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Transfer Crypto", record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
        }
        [Fact(DisplayName = "Get Record: Empty Transaction ID throws error.")]
        public async Task EmptyTransactionIdThrowsError()
        {
            await using var client = _network.NewClient();
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.GetTransactionRecordAsync(null);
            });
            Assert.Equal("transaction", ane.ParamName);
            Assert.StartsWith("Transaction is missing. Please check that it is not null.", ane.Message);
        }
        [Fact(DisplayName = "Get Record: Invalid Transaction ID throws error.")]
        public async Task InvalidTransactionIdThrowsError()
        {
            await using var client = _network.NewClient();
            var txId = Protobuf.FromTransactionId(new Proto.TransactionID { AccountID = Protobuf.ToAccountID(_network.Payer), TransactionValidStart = new Proto.Timestamp { Seconds = 500, Nanos = 100 } });
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.GetTransactionRecordAsync(txId);
            });
            Assert.Equal(ResponseCode.RecordNotFound, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: RecordNotFound", pex.Message);
        }
        [Fact(DisplayName = "Get Record: Can Get Record for Existing but Failed Transaction")]
        public async Task CanGetRecordForFailedTransaction()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.CallContractWithRecordAsync(new CallContractParams
                {
                    Contract = fx.ContractRecord.Contract,
                    Gas = await _network.TinybarsFromGas(400),
                    FunctionName = "not_a_real_method",
                });
            });
            var record = await fx.Client.GetTransactionRecordAsync(tex.TxId);
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.ContractRevertExecuted, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Call Contract", record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        }
    }
}
