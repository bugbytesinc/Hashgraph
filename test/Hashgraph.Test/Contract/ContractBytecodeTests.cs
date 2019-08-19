using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentials))]
    public class ContractBytecodeTests
    {
        private readonly NetworkCredentials _network;
        public ContractBytecodeTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Contract Bytecode: Can Get Stateless Contract Bytecode")]
        public async Task CanGetStatelessContractBytecode()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var bytecode = await fx.Client.GetContractBytecodeAsync(fx.ContractRecord.Contract);
            _network.Output.WriteLine($"Size {fx.FileParams.Contents.Length}");
            Assert.False(bytecode.IsEmpty);
        }
        [Fact(DisplayName = "Contract Bytecode: Can Get Stateful Contract Bytecode")]
        public async Task CanGetStatefulContractBytecode()
        {
            await using var fx = await StatefulContract.CreateAsync(_network);

            var bytecode = await fx.Client.GetContractBytecodeAsync(fx.ContractRecord.Contract);

            Assert.False(bytecode.IsEmpty);
        }
        [Fact(DisplayName = "Contract Bytecode: Can Get Event Emitting Contract Bytecode")]
        public async Task CanGetEventEmittingContractBytecode()
        {
            await using var fx = await EventEmittingContract.CreateAsync(_network);

            var bytecode = await fx.Client.GetContractBytecodeAsync(fx.ContractRecord.Contract);

            Assert.False(bytecode.IsEmpty);
        }
        [Fact(DisplayName = "Contract Bytecode: Can Get Event Payable Contract Bytecode")]
        public async Task CanGetEventPayableContractBytecode()
        {
            await using var fx = await PayableContract.CreateAsync(_network);

            var bytecode = await fx.Client.GetContractBytecodeAsync(fx.ContractRecord.Contract);

            Assert.False(bytecode.IsEmpty);
        }
        [Fact(DisplayName = "Contract Bytecode: Retrieving Non Existent Contract Bytecode Raises Error")]
        public async Task GetNonExistantContractRaisesError()
        {
            await using var fx = await TestAccount.CreateAsync(_network);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.GetContractBytecodeAsync(fx.Record.Address);
            });
            Assert.Equal(ResponseCode.InvalidContractId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidContractId", pex.Message);
        }
    }
}
