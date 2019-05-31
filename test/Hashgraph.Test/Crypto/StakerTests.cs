using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentials))]
    public class StakerTests
    {
        private readonly NetworkCredentials _network;
        public StakerTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Get Stakers: Is Not Implemented")]
        public async Task GetStakersIsNotImplemented()
        {
            await using var client = _network.NewClient();

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.GetStakers(_network.Payer);
            });
            Assert.Equal(ResponseCode.NotSupported, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: NotSupported", pex.Message);
        }
        /* Future Tests
         * 
         * Can get list of Stakers
         * Add Proxy Stake and Re-Check Stakers
         * Remove Proxy Stake and Re-Check Stakers
         * Can get Stakers of Gateway Node
         */
    }
}