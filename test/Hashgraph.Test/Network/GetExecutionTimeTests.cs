using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentials))]
    public class GetExecutionTimeTests
    {
        private readonly NetworkCredentials _network;
        public GetExecutionTimeTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }

        [Fact(DisplayName = "NETWORK 0.19.0 DEFECT: Get Network Info: Can Get Network Exectuion Times Bug")]
        public async Task CanGetNetworkExecutionTimesNetworkBug()
        {
            // The following unit test should succeed, however there is a bug
            // that results in the COST_ANSWER always returning BUSY.
            var testFailException = (await Assert.ThrowsAsync<PrecheckException>(CanGetNetworkExecutionTimes));
            Assert.Equal(ResponseCode.Busy, testFailException.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: Busy", testFailException.Message);

            //[Fact(DisplayName = "Get Network Info: Can Get Network Exectuion Times")]
            async Task CanGetNetworkExecutionTimes()
            {
                await using var fxAccount1 = await TestAccount.CreateAsync(_network);
                await using var fxAccount2 = await TestAccount.CreateAsync(_network);
                await using var fxAccount3 = await TestAccount.CreateAsync(_network);

                var times = await fxAccount1.Client.GetExecutionTimes(new[] { fxAccount1.Record.Id, fxAccount2.Record.Id, fxAccount3.Record.Id });

                Assert.Equal(3, times.Count);
            }
        }

        [Fact(DisplayName = "NETWORK 0.19.0 DEFECT: Get Network Info: Invalid Transaction Id Results in Error Bug")]
        public async Task InvalidTransactionIdResultsInErrorBug()
        {
            // The following unit test should succeed, however there is a bug
            // that results in the COST_ANSWER always returning BUSY.
            var testFailException = (await Assert.ThrowsAsync<Xunit.Sdk.EqualException>(InvalidTransactionIdResultsInError));
            Assert.Equal("InvalidTransactionId", testFailException.Expected);
            Assert.Equal("Busy", testFailException.Actual);

            //[Fact(DisplayName = "Get Network Info: Invalid Transaction Id Results in Error")]
            async Task InvalidTransactionIdResultsInError()
            {
                await using var client = _network.NewClient();
                var transactionId = client.CreateNewTxId();

                var pex = await Assert.ThrowsAsync<PrecheckException>(async () => {
                    await client.GetExecutionTimes(new[] { transactionId });
                });
                Assert.Equal(ResponseCode.InvalidTransactionId, pex.Status);
                Assert.StartsWith("Transaction Failed Pre-Check: InvalidTransactionId", pex.Message);
            }
        }
    }
}