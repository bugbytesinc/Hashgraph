using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Extensions
{
    [Collection(nameof(NetworkCredentials))]
    public class GetFeeScheduleTests
    {
        private readonly NetworkCredentials _network;
        public GetFeeScheduleTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Fee Schedule: Can Retrieve Network Fee Schedule")]
        public async Task CanGetFeeSchedule()
        {
            var client = _network.NewClient();
            var schedule = await client.GetFeeScheduleAsync();
            Assert.NotNull(schedule);
            Assert.NotNull(schedule.Current);
            Assert.NotNull(schedule.Next);

            // Try to re-hydrate a known-to-exist fee schedule.
            Assert.NotNull(schedule.Current.Data["CryptoCreate"]);
            var feeData = Proto.FeeData.Parser.ParseJson(schedule.Current.Data["CryptoCreate"]);
            Assert.NotNull(feeData);
            // TODO - Revisit at some point in the future,
            // is the protobuf out-of-sync with the file.
            // Assert.True(feeData.Nodedata.Max > 0);
        }
    }
}