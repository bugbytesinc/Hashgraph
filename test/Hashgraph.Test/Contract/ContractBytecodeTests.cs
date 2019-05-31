using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class ContractBytecodeTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public ContractBytecodeTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "Contract Bytecode: Can Get Stateless Contract Bytecode")]
        public async Task CanGetStatelessContractBytecode()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);

            var bytecode = await fx.Client.GetContractBytecodeAsync(fx.ContractCreateRecord.Contract);
            Assert.False(bytecode.IsEmpty);
        }
        [Fact(DisplayName = "Contract Bytecode: Can Get Stateful Contract Bytecode")]
        public async Task CanGetStatefulContractBytecode()
        {
            await using var fx = await StatefulContractInstance.CreateAsync(_networkCredentials);

            var bytecode = await fx.Client.GetContractBytecodeAsync(fx.ContractCreateRecord.Contract);
            Assert.False(bytecode.IsEmpty);
        }
        [Fact(DisplayName = "Contract Bytecode: Retrieving Non Existent Contract Bytecode Raises Error")]
        public async Task GetNonExistantContractRaisesError()
        {
            await using var fx = await TestAccountInstance.CreateAsync(_networkCredentials);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.GetContractBytecodeAsync(fx.AccountRecord.Address);
            });
            Assert.Equal(ResponseCode.InvalidContractId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidContractId", pex.Message);
        }
    }
}
