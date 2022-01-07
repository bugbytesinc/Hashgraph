using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract;

[Collection(nameof(NetworkCredentials))]
public class SystemRestoreContractTests
{
    private readonly NetworkCredentials _network;
    public SystemRestoreContractTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "System Contract Restore: Restore Contract is Broken")]
    public async Task SystemRestoreContractIsBroken()
    {
        var systemAddress = await _network.GetSystemUndeleteAdminAddress();
        if (systemAddress is null)
        {
            _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
            return;
        }

        await using var fx = await GreetingContract.CreateAsync(_network);
        await fx.Client.DeleteContractAsync(fx, _network.Payer, fx.PrivateKey);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fx.Client.SystemRestoreContractAsync(fx.ContractRecord.Contract, ctx => ctx.Payer = systemAddress);
        });
        Assert.Equal(ResponseCode.InvalidFileId, tex.Status);
        Assert.StartsWith("Unable to restore contract, status: InvalidFileId", tex.Message);
    }
    [Fact(DisplayName = "System Contract Restore: Can Not Schedule Restore.")]
    public async Task CanNotScheduleRestore()
    {
        var systemAddress = await _network.GetSystemDeleteAdminAddress();
        if (systemAddress is null)
        {
            _network.Output?.WriteLine("TEST SKIPPED: No access to System Delete Administrator Account.");
            return;
        }

        await using var fxContract = await GreetingContract.CreateAsync(_network);
        await fxContract.Client.DeleteContractAsync(fxContract, _network.Payer, fxContract.PrivateKey);

        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxContract.Client.SystemRestoreContractAsync(
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