using Hashgraph.Test.Fixtures;
using System;
using System.Security.Cryptography;
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
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Freeze Administrator Account.");
                return;
            }
            await using var client = _network.NewClient();
            var suspendParameters = new SuspendNetworkParams
            {
                Starting = TimeSpan.FromSeconds(5),
                Duration = TimeSpan.FromSeconds(60)
            };
            var receipt = await client.SuspendNetworkAsync(suspendParameters, ctx => ctx.Payer = systemAddress);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            await Task.Delay(TimeSpan.FromSeconds(30));

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.GetAccountInfoAsync(_network.Payer);
            });
            Assert.Equal(ResponseCode.PlatformNotActive, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: PlatformNotActive", pex.Message);

            await Task.Delay(TimeSpan.FromSeconds(50));

            var info = await client.GetAccountInfoAsync(_network.Payer);
            Assert.Equal(_network.Payer, info.Address);
        }
        [Fact(DisplayName = "Suspend Network: Can Suspend Network with Update File")]
        public async Task CanSuspendNetworkWithUpdateFile()
        {
            await using var client = _network.NewClient();
            using var sha384 = new SHA384Managed();
            var systemAddress = await _network.GetSystemFreezeAdminAddress();
            if (systemAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Freeze Administrator Account.");
                return;
            }

            var specialFileAddress = new Address(0, 0, 150);
            var contents = await client.GetFileContentAsync(specialFileAddress);
            var contentHash = sha384.ComputeHash(contents.ToArray());

            var suspendParameters = new SuspendNetworkParams
            {
                Starting = TimeSpan.FromSeconds(5),
                Duration = TimeSpan.FromSeconds(60),
                UpdateFile = specialFileAddress,
                UpdateFileHash = contentHash
            };

            var receipt = await client.SuspendNetworkAsync(suspendParameters, ctx => ctx.Payer = systemAddress);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            await Task.Delay(20000);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.GetAccountInfoAsync(_network.Payer);
            });
            Assert.Equal(ResponseCode.PlatformNotActive, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: PlatformNotActive", pex.Message);

            await Task.Delay(50000);

            var info = await client.GetAccountInfoAsync(_network.Payer);
            Assert.Equal(_network.Payer, info.Address);
        }
    }
}

