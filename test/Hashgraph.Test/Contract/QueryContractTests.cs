using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentials))]
    public class QueryContractTests
    {
        private readonly NetworkCredentials _network;
        public QueryContractTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Query Contract: Can Call Contract with No Arguments")]
        public async Task CanCreateAContractAsync()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var result = await fx.Client.QueryContractAsync(new QueryContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 30_000,
                ReturnValueCharge = 186,
                FunctionName = "greet"
            });
            Assert.NotNull(result);
            Assert.Empty(result.Error);
            Assert.True(result.Bloom.IsEmpty);
            Assert.InRange(result.Gas, 0UL, 30_000UL);
            Assert.Empty(result.Events);
            Assert.Equal("Hello, world!", result.Result.As<string>()); ;
        }
        [Fact(DisplayName = "Query Contract: Can call Contract that Keeps State")]
        public async Task CanCreateAContractWithStateAsync()
        {
            await using var fx = await StatefulContract.CreateAsync(_network);
            var result = await fx.Client.QueryContractAsync(new QueryContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 1223,
                ReturnValueCharge = 372,
                FunctionName = "get_message"
            });
            Assert.NotNull(result);
            Assert.Empty(result.Error);
            Assert.True(result.Bloom.IsEmpty);
            Assert.InRange(result.Gas, 0UL, 30_000UL);
            Assert.Empty(result.Events);
            Assert.Equal(fx.ContractParams.Arguments[0] as string, result.Result.As<string>());
        }
        [Fact(DisplayName = "Query Contract: Call Contract that sets State fails.")]
        public async Task CanCreateAContractAndSetStateAsync()
        {
            await using var fx = await StatefulContract.CreateAsync(_network);

            var newMessage = Generator.Code(50);
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.QueryContractAsync(new QueryContractParams
                {
                    Contract = fx.ContractRecord.Contract,
                    Gas = 50_000,
                    FunctionName = "set_message",
                    FunctionArgs = new object[] { newMessage }
                });
            });
            Assert.Equal(ResponseCode.LocalCallModificationException, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: LocalCallModificationException", pex.Message);
        }
        [Fact(DisplayName = "Query Contract: MaxAllowedReturnSize Is Enforced.")]
        public async Task MaxAllowedReturnSizeIsEnforced()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.QueryContractAsync(new QueryContractParams
                {
                    Contract = fx.ContractRecord.Contract,
                    Gas = 30_000,
                    FunctionName = "greet",
                    MaxAllowedReturnSize = 1
                });
            });
            Assert.Equal(ResponseCode.ResultSizeLimitExceeded, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: ResultSizeLimitExceeded", pex.Message);
        }
    }
}
