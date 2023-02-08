using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract;

[Collection(nameof(NetworkCredentials))]
public class ContractInfoTests
{
    private readonly NetworkCredentials _network;
    public ContractInfoTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Contract Info: Can Get Stateless Contract Info")]
    public async Task CanGetStatelessContractInfo()
    {
        await using var fx = await GreetingContract.CreateAsync(_network);

        var info = await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
        Assert.NotNull(info);
        Assert.Equal(fx.ContractRecord.Contract, info.Contract);
        Assert.Equal(fx.ContractRecord.Contract, info.Address);  // Assume for now they are equal
        Assert.NotNull(info.SmartContractId);
        Assert.Equal(fx.ContractParams.Administrator, info.Administrator);
        Assert.InRange(info.Expiration, ConsensusTimeStamp.Now, ConsensusTimeStamp.MaxValue);
        Assert.Equal(fx.ContractParams.RenewPeriod, info.RenewPeriod);
        Assert.InRange(info.Size, 0, fx.FileParams.Contents.Length);
        Assert.Equal(fx.ContractParams.Memo, info.Memo);
        Assert.Equal((ulong)fx.ContractParams.InitialBalance, info.Balance);
        AssertHg.NotEmpty(info.Ledger);
    }
    [Fact(DisplayName = "Contract Info: Can Get Stateful Contract Info")]
    public async Task CanGetStatefulContractInfo()
    {
        await using var fx = await StatefulContract.CreateAsync(_network);

        var info = await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
        Assert.NotNull(info);
        Assert.Equal(fx.ContractRecord.Contract, info.Contract);
        Assert.Equal(fx.ContractRecord.Contract, info.Address);  // Assume for now they are equal
        Assert.NotNull(info.SmartContractId);
        Assert.Equal(fx.ContractParams.Administrator, info.Administrator);
        Assert.InRange(info.Expiration, DateTime.UtcNow, DateTime.MaxValue);
        Assert.Equal(fx.ContractParams.RenewPeriod, info.RenewPeriod);
        Assert.InRange(info.Size, 0, fx.FileParams.Contents.Length);
        Assert.Equal(fx.ContractParams.Memo, info.Memo);
        Assert.Equal((ulong)fx.ContractParams.InitialBalance, info.Balance);
        AssertHg.NotEmpty(info.Ledger);
    }
    [Fact(DisplayName = "Contract Info: Retrieving Non Existent Contract Raises Error")]
    public async Task GetNonExistantContractRaisesError()
    {
        await using var fx = await TestAccount.CreateAsync(_network);

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fx.Client.GetContractInfoAsync(fx.Record.Address);
        });
        Assert.Equal(ResponseCode.InvalidContractId, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: InvalidContractId", pex.Message);
    }

    [Fact(DisplayName = "Contract Info: Can Get Imutable Stateful Contract Info")]
    async Task CanGetImutableStatefulContractInfo()
    {
        await using var fx = await StatefulContract.CreateAsync(_network, f =>
        {
            f.ContractParams.Administrator = null;
        });

        var info = await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
        Assert.NotNull(info);
        Assert.Equal(fx.ContractRecord.Contract, info.Contract);
        Assert.Equal(fx.ContractRecord.Contract, info.Address);  // Assume for now they are equal
        Assert.NotNull(info.SmartContractId);
        // Immutable Contracts list their "contract" key as the administrator Key.
        Assert.Equal(KeyType.Contract, info.Administrator.Type);
        Assert.Equal(fx.ContractRecord.Contract, info.Administrator.Contract);
        Assert.InRange(info.Expiration, DateTime.UtcNow, DateTime.MaxValue);
        Assert.Equal(fx.ContractParams.RenewPeriod, info.RenewPeriod);
        Assert.InRange(info.Size, 0, fx.FileParams.Contents.Length);
        Assert.StartsWith(fx.ContractParams.Memo, info.Memo);
        Assert.Equal((ulong)fx.ContractParams.InitialBalance, info.Balance);
        AssertHg.NotEmpty(info.Ledger);
    }
}