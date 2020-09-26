using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentials))]
    public class SystemDeleteContractTests
    {
        private readonly NetworkCredentials _network;
        public SystemDeleteContractTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "System Contract Delete: Can Delete Contract is Broken")]
        public async Task CanDeleteContractIsBroken()
        {
            var systemAddress = await _network.GetSystemDeleteAdminAddress();
            if (systemAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
                return;
            }

            await using var fx = await GreetingContract.CreateAsync(_network);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.SystemDeleteContractAsync(fx.ContractRecord.Contract, ctx => ctx.Payer = systemAddress);
            });
            Assert.Equal(ResponseCode.FileSystemException, tex.Status);
            Assert.StartsWith("Unable to delete contract, status: FileSystemException", tex.Message);
        }
    }
}


