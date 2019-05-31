using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class StakerTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public StakerTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "Get Stakers: Is Not Implemented")]
        public async Task GetStakersIsNotImplemented()
        {
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.GetStakers(_networkCredentials.CreateDefaultAccount());
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