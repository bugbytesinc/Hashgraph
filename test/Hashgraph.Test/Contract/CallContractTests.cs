using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class CallContractTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public CallContractTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "Call Contract: Can Call Contract with No Arguments")]
        public async Task CanCreateAContractAsync()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);

            var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractCreateRecord.Contract,
                Gas = 30_000,
                FunctionName = "greet"
            });
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Call Contract", record.Memo);
            Assert.InRange(record.Fee, 0UL, 100_000UL);
            Assert.Equal(fx.ContractCreateRecord.Contract, record.Contract);
            Assert.Empty(record.Error);
            Assert.True(record.Bloom.IsEmpty);
            Assert.InRange(record.Gas, 0UL, 30_000UL);
            Assert.Empty(record.Events);
            Assert.Equal("Hello, world!", record.Result.As<string>()); ;
        }
        [Fact(DisplayName = "Call Contract: Can Call Contract that keeps State")]
        public async Task CanCreateAContractWithStateAsync()
        {
            await using var fx = await StatefulContractInstance.CreateAsync(_networkCredentials);

            var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractCreateRecord.Contract,
                Gas = 30_000,
                FunctionName = "get_message"
            });
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Call Contract", record.Memo);
            Assert.InRange(record.Fee, 0UL, 100_000UL);
            Assert.Equal(fx.ContractCreateRecord.Contract, record.Contract);
            Assert.Empty(record.Error);
            Assert.True(record.Bloom.IsEmpty);
            Assert.InRange(record.Gas, 0UL, 30_000UL);
            Assert.Empty(record.Events);
            Assert.Equal(fx.CreateContractParams.Arguments[0] as string, record.Result.As<string>());
        }
        [Fact(DisplayName = "Call Contract: Can Call Contract that sets State")]
        public async Task CanCreateAContractAndSetStateAsync()
        {
            await using var fx = await StatefulContractInstance.CreateAsync(_networkCredentials);

            var newMessage = Generator.Code(50);
            var setRecord = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractCreateRecord.Contract,
                Gas = 50_000,
                FunctionName = "set_message",
                FunctionArgs = new object[] { newMessage }
            });
            Assert.NotNull(setRecord);
            Assert.Equal(ResponseCode.Success, setRecord.Status);
            Assert.False(setRecord.Hash.IsEmpty);
            Assert.NotNull(setRecord.Concensus);
            Assert.Equal("Call Contract", setRecord.Memo);
            Assert.InRange(setRecord.Fee, 0UL, 100_000UL);
            Assert.Equal(fx.ContractCreateRecord.Contract, setRecord.Contract);
            Assert.Empty(setRecord.Error);
            Assert.True(setRecord.Bloom.IsEmpty);
            Assert.InRange(setRecord.Gas, 0UL, 50_000UL);
            Assert.Empty(setRecord.Events);

            var getRecord = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractCreateRecord.Contract,
                Gas = 30_000,
                FunctionName = "get_message"
            });
            Assert.NotNull(getRecord);
            Assert.Equal(ResponseCode.Success, getRecord.Status);
            Assert.False(getRecord.Hash.IsEmpty);
            Assert.NotNull(getRecord.Concensus);
            Assert.Equal("Call Contract", getRecord.Memo);
            Assert.InRange(getRecord.Fee, 0UL, 100_000UL);
            Assert.Equal(fx.ContractCreateRecord.Contract, getRecord.Contract);
            Assert.Empty(getRecord.Error);
            Assert.True(getRecord.Bloom.IsEmpty);
            Assert.InRange(getRecord.Gas, 0UL, 30_000UL);
            Assert.Empty(getRecord.Events);
            Assert.Equal(newMessage, getRecord.Result.As<string>());
            Assert.InRange(getRecord.Gas, 0UL, 30_000UL);
        }
    }
}
