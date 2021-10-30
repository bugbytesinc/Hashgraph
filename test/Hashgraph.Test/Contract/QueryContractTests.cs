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
                Gas = 4000,
                ReturnValueCharge = 4000,
                FunctionName = "greet"
            });
            Assert.NotNull(result);
            Assert.Empty(result.Error);
            Assert.False(result.Bloom.IsEmpty);
            Assert.InRange(result.Gas, 0UL, ulong.MaxValue);
            Assert.Empty(result.Events);
            Assert.Equal("Hello, world!", result.Result.As<string>());
            Assert.Empty(result.CreatedContracts);
        }
        [Fact(DisplayName = "Query Contract: Can call Contract that Keeps State")]
        public async Task CanCreateAContractWithStateAsync()
        {
            await using var fx = await StatefulContract.CreateAsync(_network);
            var result = await fx.Client.QueryContractAsync(new QueryContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 8000,
                ReturnValueCharge = 4000,
                FunctionName = "get_message"
            });
            Assert.NotNull(result);
            Assert.Empty(result.Error);
            Assert.False(result.Bloom.IsEmpty);
            Assert.InRange(result.Gas, 0UL, ulong.MaxValue);
            Assert.Empty(result.Events);
            Assert.Equal(fx.ContractParams.Arguments[0] as string, result.Result.As<string>());
            Assert.Empty(result.CreatedContracts);
        }
        [Fact(DisplayName = "Query Contract: Query Contract with Insufficent funds defaults to Throw Exception")]
        public async Task QueryContractWithInsufficientFundsThrowsErrorByDefault()
        {
            await using var fx = await StatefulContract.CreateAsync(_network);
            var qex = await Assert.ThrowsAsync<ContractException>(async () =>
            {
                await fx.Client.QueryContractAsync(new QueryContractParams
                {
                    Contract = fx.ContractRecord.Contract,
                    Gas = 1,
                    ReturnValueCharge = 4000,
                    FunctionName = "get_message"
                });
            });
            Assert.NotNull(qex);
            Assert.Equal(ResponseCode.InsufficientGas, qex.Status);
            Assert.NotNull(qex.TxId);
            Assert.Equal(0ul, qex.RequiredFee);
            Assert.NotNull(qex.CallResult);
            Assert.Equal("INSUFFICIENT_GAS", qex.CallResult.Error);
            Assert.False(qex.CallResult.Bloom.IsEmpty);
            Assert.InRange(qex.CallResult.Gas, 0UL, ulong.MaxValue);
            Assert.Empty(qex.CallResult.Events);
            Assert.Equal(0, qex.CallResult.Result.Size);
            Assert.Empty(qex.CallResult.CreatedContracts);
        }
        [Fact(DisplayName = "Query Contract: Error Query Contract without Flag Set Returns without Error")]
        public async Task ErrorQueryContractWithoutFlagReturnsWithoutError()
        {
            await using var fx = await StatefulContract.CreateAsync(_network);
            var result = await fx.Client.QueryContractAsync(new QueryContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 1,
                ReturnValueCharge = 4000,
                FunctionName = "get_message",
                ThrowOnFail = false
            });
            Assert.NotNull(result);
            Assert.Equal("INSUFFICIENT_GAS", result.Error);
            Assert.False(result.Bloom.IsEmpty);
            Assert.InRange(result.Gas, 0UL, ulong.MaxValue);
            Assert.Empty(result.Events);
            Assert.Equal(0, result.Result.Size);
            Assert.Empty(result.CreatedContracts);
        }
        [Fact(DisplayName = "Query Contract: Call Contract that sets State fails.")]
        public async Task CanCreateAContractAndSetStateAsync()
        {
            await using var fx = await StatefulContract.CreateAsync(_network);

            var newMessage = Generator.Code(50);
            var qex = await Assert.ThrowsAsync<ContractException>(async () =>
            {
                await fx.Client.QueryContractAsync(new QueryContractParams
                {
                    Contract = fx.ContractRecord.Contract,
                    Gas = 20000,
                    FunctionName = "set_message",
                    FunctionArgs = new object[] { newMessage },
                    ReturnValueCharge = 900
                });
            });
            Assert.Equal(ResponseCode.LocalCallModificationException, qex.Status);
            Assert.StartsWith("Contract Query Failed with Code: LocalCallModificationException", qex.Message);
        }
        [Fact(DisplayName = "Query Contract: Call Contract that sets State fails without ThrowOnFalse")]
        public async Task CallQueryAttemptingStateChangeFailsWithoutErrorWhenThrowOnFailFalse()
        {
            await using var fx = await StatefulContract.CreateAsync(_network);

            var newMessage = Generator.Code(50);
            var result = await fx.Client.QueryContractAsync(new QueryContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 20000,
                FunctionName = "set_message",
                FunctionArgs = new object[] { newMessage },
                ReturnValueCharge = 900,
                ThrowOnFail = false
            });
            Assert.NotNull(result);
            Assert.Equal("ILLEGAL_STATE_CHANGE", result.Error);
            Assert.False(result.Bloom.IsEmpty);
            Assert.InRange(result.Gas, 0UL, ulong.MaxValue);
            Assert.Empty(result.Events);
            Assert.Equal(0, result.Result.Size);
            Assert.Empty(result.CreatedContracts);
        }
        [Fact(DisplayName = "Query Contract: Invalid Network Call Still Raises PreCheckError when ThrowOnFail is False")]
        public async Task InvalidNetworkCallStillRaisesPreCheckErrorWhenThrowOnFailFalse()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var (_, badPrivateKey) = Generator.KeyPair();
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                var result = await fx.Client.QueryContractAsync(new QueryContractParams
                {
                    Contract = fx.ContractRecord.Contract,
                    Gas = 4000,
                    ReturnValueCharge = 4000,
                    FunctionName = "greet",
                    ThrowOnFail = false
                }, ctx => ctx.Signatory = badPrivateKey);
            });
            Assert.Equal(ResponseCode.InvalidSignature, pex.Status);
        }
    }
}
