using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class ContractInfoTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public ContractInfoTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "Contract Info: Can Get Stateless Contract Info")]
        public async Task CanGetStatelessContractInfo()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);

            var info = await fx.Client.GetContractInfoAsync(fx.ContractCreateRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Address);  // Assume for now they are equal
            Assert.NotNull(info.SmartContractId);
            Assert.Equal(fx.CreateContractParams.Administrator, info.Administrator);
            Assert.InRange(info.Expiration, DateTime.UtcNow, DateTime.MaxValue);
            Assert.Equal(fx.CreateContractParams.RenewPeriod, info.RenewPeriod);
            Assert.InRange(info.Size, 0, fx.CreateFileParams.Contents.Length);
            Assert.Equal("Greeting Contract Create: Instantiating Contract Instance", info.Memo);
        }
        [Fact(DisplayName = "Contract Info: Can Get Stateful Contract Info")]
        public async Task CanGetStatefulContractInfo()
        {
            await using var fx = await StatefulContractInstance.CreateAsync(_networkCredentials);

            var info = await fx.Client.GetContractInfoAsync(fx.ContractCreateRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Address);  // Assume for now they are equal
            Assert.NotNull(info.SmartContractId);
            Assert.Equal(fx.CreateContractParams.Administrator, info.Administrator);
            Assert.InRange(info.Expiration, DateTime.UtcNow, DateTime.MaxValue);
            Assert.Equal(fx.CreateContractParams.RenewPeriod, info.RenewPeriod);
            Assert.InRange(info.Size, 0, fx.CreateFileParams.Contents.Length);
            Assert.Equal("Stateful Contract Create: Instantiating Stateful Instance", info.Memo);
        }
        [Fact(DisplayName = "Contract Info: Retrieving Non Existent Contract Raises Error")]
        public async Task GetNonExistantContractRaisesError()
        {
            await using var fx = await TestAccountInstance.CreateAsync(_networkCredentials);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.GetContractInfoAsync(fx.AccountRecord.Address);
            });
            Assert.Equal(ResponseCode.InvalidContractId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidContractId", pex.Message);
        }
    }
}
