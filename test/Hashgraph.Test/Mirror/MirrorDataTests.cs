using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class MirrorDataTests
{
    private readonly NetworkCredentials _network;
    public MirrorDataTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Mirror Data: Can Get Account Data")]
    public async Task CanGetAccountData()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        await _network.WaitForMirrorNodeConsensusTimestamp(fxAccount.Record.Concensus.Value);
        var data = await _network.MirrorRestClient.GetAccountDataAsync(fxAccount);

        Assert.Equal(info.Address, data.Account);
        Assert.Null(data.Alias);
        Assert.Equal(info.AutoRenewPeriod.TotalSeconds, data.AutoRenewPeriod);
        Assert.Equal(info.Balance, (ulong)data.Balances.Balance);
        Assert.Equal(fxAccount.Record.Concensus, data.Created);
        Assert.Equal(info.StakingInfo.Declined, data.DeclineReward);
        Assert.Equal(info.Deleted, data.Deleted);
        Assert.Equal(info.ContractNonce, data.Nonce);
        // Network Bug: Really?  Like this isn't inconsistent.
        Assert.NotEqual(info.ContractId, data.ContractAddress);
        Assert.Equal("0x" + Hex.FromBytes(Abi.EncodeArguments(new[] { info.Address })[^20..]), data.ContractAddress);
        // End Bug
        // Another network issue, inconsistent precision
        Assert.NotEqual(info.Expiration, data.Expires);
        Assert.Equal((long)info.Expiration.Seconds, (long)data.Expires.Seconds);
        // End Bug
        Assert.Equal(info.Endorsement, data.Endorsement);
        Assert.Equal(info.AutoAssociationLimit, data.Associations);
        Assert.Equal(info.Memo, data.Memo);
        Assert.Equal(info.StakingInfo.PendingReward, data.PendingReward);
        Assert.Equal(info.ReceiveSignatureRequired, data.ReceiverSignatureRequired);
        Assert.Null(data.StakedAccount);
        Assert.Null(data.StakedNode);
        Assert.Equal(info.StakingInfo.PeriodStart, data.StakePeriodStart);
    }
}