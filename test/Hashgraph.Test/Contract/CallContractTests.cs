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
            Assert.Empty(record.CallResult.Error);
            Assert.True(record.CallResult.Bloom.IsEmpty);
            Assert.InRange(record.CallResult.Gas, 0UL, 30_000UL);
            Assert.Empty(record.CallResult.Events);
            Assert.Equal("Hello, world!", record.CallResult.Result.As<string>()); ;
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
            Assert.Empty(record.CallResult.Error);
            Assert.True(record.CallResult.Bloom.IsEmpty);
            Assert.InRange(record.CallResult.Gas, 0UL, 30_000UL);
            Assert.Empty(record.CallResult.Events);
            Assert.Equal(fx.CreateContractParams.Arguments[0] as string, record.CallResult.Result.As<string>());
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
            Assert.Empty(setRecord.CallResult.Error);
            Assert.True(setRecord.CallResult.Bloom.IsEmpty);
            Assert.InRange(setRecord.CallResult.Gas, 0UL, 50_000UL);
            Assert.Empty(setRecord.CallResult.Events);

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
            Assert.Empty(getRecord.CallResult.Error);
            Assert.True(getRecord.CallResult.Bloom.IsEmpty);
            Assert.InRange(getRecord.CallResult.Gas, 0UL, 30_000UL);
            Assert.Empty(getRecord.CallResult.Events);
            Assert.Equal(newMessage, getRecord.CallResult.Result.As<string>());
            Assert.InRange(getRecord.CallResult.Gas, 0UL, 30_000UL);
        }
        /* Future Tests
         * 
         * Exercise Payable Contract
         */
    }
}
