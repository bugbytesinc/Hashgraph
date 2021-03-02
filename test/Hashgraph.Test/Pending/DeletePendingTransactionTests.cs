using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token
{
    [Collection(nameof(NetworkCredentials))]
    public class DeletePendingTransactionTests
    {
        private readonly NetworkCredentials _network;
        public DeletePendingTransactionTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Pending Transaction Delete: Can Delete Pending Transaction")]
        public async Task CanGetTokenInfo()
        {
            await using var fx = await TestPendingTransfer.CreateAsync(_network);
            Assert.Equal(ResponseCode.Success, fx.Record.Status);

            var receipt = await fx.Client.DeletePendingTransactionAsync(fx.Record.Pending.Pending, ctx => ctx.Signatory = new Signatory(fx.PrivateKey, ctx.Signatory));
            Assert.Equal(ResponseCode.Success, receipt.Status);
        }

        [Fact(DisplayName = "Pending Transaction Delete: Deleting Pending Transaction Removes Pending Info")]
        public async Task DeletingPendingTransactionRemovesPendingInfo()
        {
            await using var fx = await TestPendingTransfer.CreateAsync(_network);
            await fx.Client.DeletePendingTransactionAsync(fx.Record.Pending.Pending, ctx => ctx.Signatory = new Signatory(fx.PrivateKey, ctx.Signatory));

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.GetPendingTransactionInfo(fx.Record.Pending.Pending);
            });
            Assert.Equal(ResponseCode.InvalidScheduleId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidScheduleId", pex.Message);
        }

        // todo, permutations on signatory
        // todo, check the info
    }
}
