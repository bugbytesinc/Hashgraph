using Hashgraph.Test.Fixtures;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentials))]
    public class NetworkAdministrationTests
    {
        private readonly NetworkCredentials _network;
        public NetworkAdministrationTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }

        [Fact(DisplayName = "Network Administration: Can Schedule and CancelSuspend Network")]
        public async Task CanScheduleAndCancelSuspendNetwork()
        {
            var systemAddress = await _network.GetSystemFreezeAdminAddress();
            if (systemAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Freeze Administrator Account.");
                return;
            }
            await using var client = _network.NewClient();
            var receipt = await client.SuspendNetworkAsync(DateTime.UtcNow.AddSeconds(20), ctx => ctx.Payer = systemAddress);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            receipt = await client.AbortNetworkUpgrade(ctx => ctx.Payer = systemAddress);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            await Task.Delay(TimeSpan.FromSeconds(30));

            // Confirm network is still up and running.
            var info = await client.GetAccountInfoAsync(_network.Payer);
            Assert.Equal(_network.Payer, info.Address);
        }
        [Fact(DisplayName = "Network Administration: Can Upload Update File")]
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

            var receipt = await client.PrepareNetworkUpgrade(specialFileAddress, contentHash, ctx => ctx.Payer = systemAddress);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            await Task.Delay(TimeSpan.FromSeconds(10));

            receipt = await client.AbortNetworkUpgrade(ctx => ctx.Payer = systemAddress);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await client.GetAccountInfoAsync(_network.Payer);
            Assert.Equal(_network.Payer, info.Address);
        }
    }
}

