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
            foreach (var feeDetail in schedule.Current.Data["CryptoCreate"])
            {
                var feeDetailData = Proto.FeeData.Parser.ParseJson(feeDetail);
                Assert.NotNull(feeDetailData);
                Assert.True(feeDetailData.Nodedata.Max > 0);
            }
        }
    }
}