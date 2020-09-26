using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentials))]
    public class SystemRestoreContractTests
    {
        private readonly NetworkCredentials _network;
        public SystemRestoreContractTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "System Contract Restore: Restore Contract is Broken")]
        public async Task SystemRestoreContractIsBroken()
        {
            var systemAddress = await _network.GetSystemUndeleteAdminAddress();
            if (systemAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
                return;
            }

            await using var fx = await GreetingContract.CreateAsync(_network);
            await fx.Client.DeleteContractAsync(fx, _network.Payer);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.SystemRestoreContractAsync(fx.ContractRecord.Contract, ctx => ctx.Payer = systemAddress);
            });
            Assert.Equal(ResponseCode.InvalidFileId, tex.Status);
            Assert.StartsWith("Unable to restore contract, status: InvalidFileId", tex.Message);
        }
    }
}


