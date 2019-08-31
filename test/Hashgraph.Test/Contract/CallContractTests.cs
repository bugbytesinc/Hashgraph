using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentials))]
    public class CallContractTests
    {
        private readonly NetworkCredentials _network;
        public CallContractTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Call Contract: Can Call Contract with No Arguments")]
        public async Task CanCreateAContractAsync()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 30_000,
                FunctionName = "greet"
            });
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Call Contract", record.Memo);
            Assert.InRange(record.Fee, 0UL, 30_000_000UL);
            Assert.Equal(fx.ContractRecord.Contract, record.Contract);
            Assert.Empty(record.CallResult.Error);
            Assert.True(record.CallResult.Bloom.IsEmpty);
            Assert.InRange(record.CallResult.Gas, 0UL, 30_000UL);
            Assert.Empty(record.CallResult.Events);
            Assert.Equal("Hello, world!", record.CallResult.Result.As<string>()); ;
        }
        [Fact(DisplayName = "Call Contract: Can Call Contract that keeps State")]
        public async Task CanCreateAContractWithStateAsync()
        {
            await using var fx = await StatefulContract.CreateAsync(_network);

            var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 30_000,
                FunctionName = "get_message"
            });
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Call Contract", record.Memo);
            Assert.InRange(record.Fee, 0UL, 30_000_000UL);
            Assert.Equal(fx.ContractRecord.Contract, record.Contract);
            Assert.Empty(record.CallResult.Error);
            Assert.True(record.CallResult.Bloom.IsEmpty);
            Assert.InRange(record.CallResult.Gas, 0UL, 30_000UL);
            Assert.Empty(record.CallResult.Events);
            Assert.Equal(fx.ContractParams.Arguments[0] as string, record.CallResult.Result.As<string>());
        }
        [Fact(DisplayName = "Call Contract: Can Call Contract that sets State")]
        public async Task CanCreateAContractAndSetStateAsync()
        {
            await using var fx = await StatefulContract.CreateAsync(_network);

            var newMessage = Generator.Code(50);
            var setRecord = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 50_000,
                FunctionName = "set_message",
                FunctionArgs = new object[] { newMessage }
            });
            Assert.NotNull(setRecord);
            Assert.Equal(ResponseCode.Success, setRecord.Status);
            Assert.False(setRecord.Hash.IsEmpty);
            Assert.NotNull(setRecord.Concensus);
            Assert.Equal("Call Contract", setRecord.Memo);
            Assert.InRange(setRecord.Fee, 0UL, 31_000_000UL);
            Assert.Equal(fx.ContractRecord.Contract, setRecord.Contract);
            Assert.Empty(setRecord.CallResult.Error);
            Assert.True(setRecord.CallResult.Bloom.IsEmpty);
            Assert.InRange(setRecord.CallResult.Gas, 0UL, 50_000UL);
            Assert.Empty(setRecord.CallResult.Events);

            var getRecord = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 30_000,
                FunctionName = "get_message"
            });
            Assert.NotNull(getRecord);
            Assert.Equal(ResponseCode.Success, getRecord.Status);
            Assert.False(getRecord.Hash.IsEmpty);
            Assert.NotNull(getRecord.Concensus);
            Assert.Equal("Call Contract", getRecord.Memo);
            Assert.InRange(getRecord.Fee, 0UL, 30_000_000UL);
            Assert.Equal(fx.ContractRecord.Contract, getRecord.Contract);
            Assert.Empty(getRecord.CallResult.Error);
            Assert.True(getRecord.CallResult.Bloom.IsEmpty);
            Assert.InRange(getRecord.CallResult.Gas, 0UL, 30_000UL);
            Assert.Empty(getRecord.CallResult.Events);
            Assert.Equal(newMessage, getRecord.CallResult.Result.As<string>());
            Assert.InRange(getRecord.CallResult.Gas, 0UL, 30_000UL);
        }
        [Fact(DisplayName = "Call Contract: Calling Deleted Contract Raises Error")]
        public async Task CallingDeletedContractRaisesError()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var deleteReceipt = await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, _network.Payer);
            Assert.Equal(ResponseCode.Success, deleteReceipt.Status);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.CallContractWithRecordAsync(new CallContractParams
                {
                    Contract = fx.ContractRecord.Contract,
                    Gas = 30_000,
                    FunctionName = "greet",
                });
            });
            Assert.Equal(ResponseCode.ContractDeleted, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: ContractDeleted", pex.Message);
        }
    }
}
