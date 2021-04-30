using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentials))]
    public class SystemDeleteContractTests
    {
        private readonly NetworkCredentials _network;
        public SystemDeleteContractTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "System Contract Delete: Can Delete Contract")]
        public async Task CanDeleteContract()
        {
            var systemAddress = await _network.GetSystemDeleteAdminAddress();
            if (systemAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
                return;
            }

            await using var fx = await GreetingContract.CreateAsync(_network);

            var receipt = await fx.Client.SystemDeleteContractAsync(fx.ContractRecord.Contract, ctx => ctx.Payer = systemAddress);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            // But you still can't get the info
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
            });
            Assert.Equal(ResponseCode.ContractDeleted, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: ContractDeleted", pex.Message);
        }
        [Fact(DisplayName = "System Contract Delete: Can Not Schedule Delete.")]
        public async Task CanNotScheduleDelete()
        {
            var systemAddress = await _network.GetSystemDeleteAdminAddress();
            if (systemAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
                return;
            }

            await using var fxContract = await GreetingContract.CreateAsync(_network);
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxContract.Client.SystemDeleteContractAsync(
                    fxContract.ContractRecord.Contract,
                    ctx =>
                    {
                        ctx.Payer = systemAddress;
                        ctx.Signatory = new Signatory(
                            _network.PrivateKey,
                            new PendingParams { PendingPayer = fxPayer }
                        );
                    });
            });
            Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
            Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
        }
    }
}


