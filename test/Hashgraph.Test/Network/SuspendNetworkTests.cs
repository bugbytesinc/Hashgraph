using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentials))]
    public class SuspendNetworkTests
    {
        private readonly NetworkCredentials _network;
        public SuspendNetworkTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }

        [Fact(DisplayName = "Suspend Network: Can Suspend Network")]
        public async Task CanSuspendNetwork()
        {
            var systemAddress = await _network.GetSystemFreezeAdminAddress();
            if (systemAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
                return;
            }
            await using var fxFile = await TestFile.CreateAsync(_network);

            var start = TimeSpan.FromSeconds(5);
            var interval = TimeSpan.FromSeconds(60);

            var receipt = await fxFile.Client.SuspendNetworkAsync(start, interval, fxFile, ctx => ctx.Payer = systemAddress);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            await Task.Delay(20000);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fxFile.Client.GetAccountInfoAsync(_network.Payer);
            });
            Assert.Equal(ResponseCode.PlatformNotActive, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: PlatformNotActive", pex.Message);

            await Task.Delay(50000);

            var info = await fxFile.Client.GetAccountInfoAsync(_network.Payer);
            Assert.Equal(_network.Payer, info.Address);
        }
    }
}

