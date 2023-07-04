using Hashgraph.Extensions;
using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
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
        Assert.NotEqual(info.Expiration, data.Expiration);
        Assert.Equal((long)info.Expiration.Seconds, (long)data.Expiration.Seconds);
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
    [Fact(DisplayName = "Mirror Data: Can Get Gossip Node List")]
    public async Task CanGetGossipNodeList()
    {
        var list = new List<Task<long>>();
        await using var grpClient = _network.NewClient();
        await foreach (var node in _network.MirrorRestClient.GetGossipNodesAsync())
        {
            Assert.False(node.Account.IsNullOrNone());
            Assert.NotEmpty(node.Endpoints);
            Assert.NotEmpty(node.Description);
            Assert.False(node.File.IsNullOrNone());
            Assert.True(node.MinimumStake > 0);
            Assert.True(node.MaximumStake > 0);
            Assert.NotNull(node.Memo);
            Assert.True(node.NodeId >= 0);
            Assert.NotEmpty(node.CertificateHash);
            Assert.NotEmpty(node.PublicKey);
            Assert.True(node.RewardRateStart >= 0);
            Assert.True(node.Stake >= 0);
            Assert.True(node.StakeNotRewarded >= 0);
            Assert.True(node.StakeRewarded >= 0);
            Assert.NotNull(node.ValidRange);
            Assert.NotNull(node.ValidRange.Starting);
            foreach (var endpoint in node.Endpoints)
            {
                Assert.NotNull(endpoint);
                Assert.NotEmpty(endpoint.Address);
                Assert.True(endpoint.Port > 0);
                if (endpoint.Port == 50211)
                {
                    list.Add(Task.Run(async () =>
                    {
                        var uri = new Uri($"http://{endpoint.Address}:{endpoint.Port}");
                        var gateway = new Gateway(uri, node.Account);
                        var task = grpClient.PingAsync(ctx => ctx.Gateway = gateway);
                        if (await Task.WhenAny(task, Task.Delay(10000)) == task)
                        {
                            return task.Result;
                        }
                        else
                        {
                            return -1;
                        }
                    }));
                }
            }
        }
        var results = await Task.WhenAll(list);
        _network.Output.WriteLine(JsonSerializer.Serialize(results));
        Assert.Equal(list.Count, results.Length);

        var activeGateways = await _network.MirrorRestClient.GetActiveGatewaysAsync(15000);
        // Yes, this is a fuzzy test in that network circumstances
        // could cause this to fail.  But most of the time since the
        // timeout is higher in the second series of pings, this value
        // should almost always be equal to or higher.
        Assert.True(results.Count(r => r > -1) <= activeGateways.Count);
    }
    [Fact(DisplayName = "Mirror Data: Can Get Token Data")]
    public async Task CanGetTokenData()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        var info = await fxToken.Client.GetTokenInfoAsync(fxToken);
        await _network.WaitForMirrorNodeConsensusTimestamp(fxToken.Record.Concensus.Value);
        var data = await _network.MirrorRestClient.GetTokenAsync(fxToken);

        Assert.NotNull(data);
        Assert.Equal(info.Token, data.Token);
        Assert.Equal(info.Symbol, data.Symbol);
        Assert.Equal(info.Name, data.Name);
        Assert.Equal(info.Memo, data.Memo);
        Assert.Equal(info.Treasury, data.Treasury);
        Assert.Equal(info.Decimals, (uint)data.Decimals);
        Assert.Equal(info.Circulation, (ulong)data.Circulation);
        Assert.Equal(info.Ceiling, data.Ceiling);
        Assert.Equal(info.Type, data.Type);
        Assert.True(data.Modified.Seconds > 0);
        Assert.Equal(info.RenewAccount, data.RenewAccount);
        Assert.Equal(info.RenewPeriod?.TotalSeconds, data.RenewPeriodInSeconds);
        Assert.Equal(info.Administrator, data.Administrator);
        Assert.Equal(info.SupplyEndorsement, data.SupplyEndorsement);
        Assert.Equal(info.RoyaltiesEndorsement, data.RoyaltiesEndorsement);
        Assert.Equal(info.SuspendEndorsement, data.SuspendEndorsement);
        Assert.Equal(info.GrantKycEndorsement, data.GrantKycEndorsement);
        Assert.Equal(info.PauseEndorsement, data.PauseEndorsement);
        Assert.Equal(info.ConfiscateEndorsement, data.ConfiscateEndorsement);
        Assert.Equal(info.TradableStatus == TokenTradableStatus.Suspended, data.SuspenedByDefault);
        Assert.Equal(info.PauseStatus, data.PauseStatus);
        Assert.Equal(info.Circulation, (ulong)data.InitialSupply);
        Assert.Equal("FINITE", data.SupplyType);
        Assert.True(data.Created.Seconds > 0);
        // Another network issue, inconsistent precision
        Assert.NotEqual(info.Expiration, data.Expiration);
        // End Bug
        Assert.Equal(info.Deleted, data.Deleted);
    }
    [Fact(DisplayName = "Mirror Data: Can Get Asset Data")]
    public async Task CanGetAssetData()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network);
        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset);
        await _network.WaitForMirrorNodeConsensusTimestamp(fxAsset.MintRecord.Concensus.Value);
        var data = await _network.MirrorRestClient.GetTokenAsync(fxAsset);

        Assert.NotNull(data);
        Assert.Equal(info.Token, data.Token);
        Assert.Equal(info.Symbol, data.Symbol);
        Assert.Equal(info.Name, data.Name);
        Assert.Equal(info.Memo, data.Memo);
        Assert.Equal(info.Treasury, data.Treasury);
        Assert.Equal(info.Decimals, (uint)data.Decimals);
        Assert.Equal(info.Circulation, (ulong)data.Circulation);
        Assert.Equal(info.Ceiling, data.Ceiling);
        Assert.Equal(info.Type, data.Type);
        Assert.True(data.Modified.Seconds > 0);
        Assert.Equal(info.RenewAccount, data.RenewAccount);
        Assert.Equal(info.RenewPeriod?.TotalSeconds, data.RenewPeriodInSeconds);
        Assert.Equal(info.Administrator, data.Administrator);
        Assert.Equal(info.SupplyEndorsement, data.SupplyEndorsement);
        Assert.Equal(info.RoyaltiesEndorsement, data.RoyaltiesEndorsement);
        Assert.Equal(info.SuspendEndorsement, data.SuspendEndorsement);
        Assert.Equal(info.GrantKycEndorsement, data.GrantKycEndorsement);
        Assert.Equal(info.PauseEndorsement, data.PauseEndorsement);
        Assert.Equal(info.ConfiscateEndorsement, data.ConfiscateEndorsement);
        Assert.Equal(info.TradableStatus == TokenTradableStatus.Suspended, data.SuspenedByDefault);
        Assert.Equal(info.PauseStatus, data.PauseStatus);
        Assert.Equal(0, data.InitialSupply);
        Assert.Equal("FINITE", data.SupplyType);
        Assert.True(data.Created.Seconds > 0);
        // Another network issue, inconsistent precision
        Assert.NotEqual(info.Expiration, data.Expiration);
        // End Bug
        Assert.Equal(info.Deleted, data.Deleted);
    }
    [Fact(DisplayName = "Mirror Data: Can Get Hcs Message")]
    public async Task CanGetHcsMessage()
    {
        await using var fxTopic = await TestTopic.CreateAsync(_network);
        for (var i = 0; i < 10; i++)
        {
            var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
            var receipt = await fxTopic.Client.SubmitMessageAsync(fxTopic.Record.Topic, message, fxTopic.ParticipantPrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);
        }
        var expectedMessage = Encoding.ASCII.GetBytes(Generator.String(10, 100));
        var record = await fxTopic.Client.SubmitMessageWithRecordAsync(fxTopic.Record.Topic, expectedMessage, fxTopic.ParticipantPrivateKey);
        await _network.WaitForMirrorNodeConsensusTimestamp(record.Concensus.Value);

        var data = await _network.MirrorRestClient.GetHcsMessageAsync(fxTopic, 11);

        Assert.NotNull(data);
        Assert.Null(data.ChunkInfo);
        Assert.Equal(record.Concensus, data.TimeStamp);
        Assert.Equal(Convert.ToBase64String(expectedMessage), data.Message);
        Assert.Equal(record.Id.Address, data.Payer);
        Assert.NotEmpty(data.Hash);
        Assert.True(data.HashVersion > 0);
        Assert.Equal(record.SequenceNumber, data.SequenceNumber);
        Assert.Equal(fxTopic.Record.Topic, data.TopicId);
    }

    [Fact(DisplayName = "Mirror Data: Can Get All Hcs Messages")]
    public async Task CanGetAllHcsMessages()
    {
        await using var fxTopic = await TestTopic.CreateAsync(_network);
        var messages = new byte[10][];
        var records = new SubmitMessageRecord[10];
        for (var i = 0; i < messages.Length; i++)
        {
            messages[i] = Encoding.ASCII.GetBytes(Generator.String(10, 100));
            records[i] = await fxTopic.Client.SubmitMessageWithRecordAsync(fxTopic.Record.Topic, messages[i], fxTopic.ParticipantPrivateKey);
        }
        await _network.WaitForMirrorNodeConsensusTimestamp(records[^1].Concensus.Value);

        var index = 0;
        await foreach (var data in _network.MirrorRestClient.GetHcsMessagesAsync(fxTopic))
        {
            Assert.NotNull(data);
            Assert.Null(data.ChunkInfo);
            Assert.Equal(records[index].Concensus, data.TimeStamp);
            Assert.Equal(Convert.ToBase64String(messages[index]), data.Message);
            Assert.Equal(records[index].Id.Address, data.Payer);
            Assert.NotEmpty(data.Hash);
            Assert.True(data.HashVersion > 0);
            Assert.Equal(records[index].SequenceNumber, data.SequenceNumber);
            Assert.Equal(fxTopic.Record.Topic, data.TopicId);
            index++;
        }
    }
    [Fact(DisplayName = "Mirror Data: Can Get Token Holdings")]
    public async Task CanGetTokenHoldings()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var fxTokens = new TestToken[10];
        var xferAmounts = new ulong[fxTokens.Length];
        var assocRecords = new TransactionRecord[fxTokens.Length];
        var xferRecords = new TransactionRecord[fxTokens.Length];
        for (int i = 0; i < xferAmounts.Length; i++)
        {
            fxTokens[i] = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
            assocRecords[i] = await fxAccount.Client.AssociateTokenWithRecordAsync(fxTokens[i], fxAccount, fxAccount.PrivateKey);
            xferAmounts[i] = 2 * fxTokens[i].Params.Circulation / 3;
            xferRecords[i] = await fxTokens[i].Client.TransferTokensWithRecordAsync(fxTokens[i].Record.Token, fxTokens[i].TreasuryAccount.Record.Address, fxAccount.Record.Address, (long)xferAmounts[i], fxTokens[i].TreasuryAccount.PrivateKey);
        }
        await _network.WaitForMirrorNodeConsensusTimestamp(xferRecords[^1].Concensus.Value);
        await foreach (var data in _network.MirrorRestClient.GetAccountTokenHoldingsAsync(fxAccount))
        {
            var index = findTokenIndex(data.Token);
            Assert.NotNull(data);
            Assert.Equal(fxTokens[index].Record.Token, data.Token);
            Assert.False(data.AutoAssociated);
            Assert.Equal(xferAmounts[index], (ulong)data.Balance);
            Assert.Equal(assocRecords[index].Concensus, data.Created);
            Assert.Equal(TokenTradableStatus.Tradable, data.FreezeStatus);
            Assert.Equal(TokenKycStatus.NotApplicable, data.KycStatus);
            index++;
        }

        for (int i = 0; i < fxTokens.Length; i++)
        {
            var balance = await _network.MirrorRestClient.GetAccountTokenBalanceAsync(fxAccount, fxTokens[i]);
            Assert.Equal(xferAmounts[i], (ulong)balance);
        }

        int findTokenIndex(Address token)
        {
            for (int i = 0; i < fxTokens.Length; i++)
            {
                if (fxTokens[i].Record.Token == token)
                {
                    return i;
                }
            }
            throw new Exception("token not found");
        }
    }
    [Fact(DisplayName = "Mirror Data: Can Get Transfer Details")]
    public async Task CanGetTransferDetails()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var fxTokens = new TestToken[3];
        var xferAmounts = new ulong[fxTokens.Length];
        var assocRecords = new TransactionRecord[fxTokens.Length];
        var xferRecords = new TransactionRecord[fxTokens.Length];
        for (int i = 0; i < xferAmounts.Length; i++)
        {
            fxTokens[i] = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
            assocRecords[i] = await fxAccount.Client.AssociateTokenWithRecordAsync(fxTokens[i], fxAccount, fxAccount.PrivateKey);
            xferAmounts[i] = 2 * fxTokens[i].Params.Circulation / 3;
            xferRecords[i] = await fxTokens[i].Client.TransferTokensWithRecordAsync(fxTokens[i].Record.Token, fxTokens[i].TreasuryAccount.Record.Address, fxAccount.Record.Address, (long)xferAmounts[i], fxTokens[i].TreasuryAccount.PrivateKey);
        }
        await _network.WaitForMirrorNodeConsensusTimestamp(xferRecords[^1].Concensus.Value);
        long feeLimit = 0;
        TimeSpan validDuration = TimeSpan.Zero;
        fxAccount.Client.Configure(ctx => { feeLimit = ctx.FeeLimit; validDuration = ctx.TransactionDuration; });
        for (int i = 0; i < xferAmounts.Length; i++)
        {
            var record = xferRecords[i];
            var dataList = await _network.MirrorRestClient.GetTransactionGroupAsync(xferRecords[i].Id);
            Assert.NotNull(dataList);
            Assert.Single(dataList);
            var data = dataList[0];
            Assert.Equal(record.Id, data.TxId);
            Assert.Equal(record.Fee, (ulong)data.Fee);
            Assert.Equal(record.Concensus, data.Consensus);
            Assert.Null(data.CreatedEntity);
            Assert.Equal(feeLimit, data.FeeLimit);
            Assert.Equal(record.Memo, Encoding.UTF8.GetString(data.Memo.Span));
            Assert.Equal("CRYPTOTRANSFER", data.TransactionType);
            Assert.Equal(_network.Gateway, data.GossipNode);
            Assert.Equal(0, data.Nonce);
            Assert.Null(data.ParentConsensus);
            Assert.Equal(ResponseCode.Success, data.Status);
            Assert.False(data.IsScheduled);
            Assert.Empty(data.StakingRewards);
            AssertHg.Equal(record.Hash, data.Hash);
            Assert.Equal(validDuration, data.ValidDuration);
            Assert.Equal(new ConsensusTimeStamp(record.Id.ValidStartSeconds, record.Id.ValidStartNanos), data.ValidStarting);
            Assert.Null(data.AssessedFees);
            Assert.Empty(data.AssetTransfers);
            Assert.Equal(2, data.TokenTransfers.Length);
            var fromXfer = data.TokenTransfers.First(x => x.Account == fxTokens[i].TreasuryAccount.Record.Address);
            Assert.NotNull(fromXfer);
            Assert.Equal(fxTokens[i].Record.Token, fromXfer.Token);
            Assert.Equal(-(long)xferAmounts[i], fromXfer.Amount);
            Assert.False(fromXfer.IsAllowance);
            var toXfer = data.TokenTransfers.First(x => x.Account == fxAccount.Record.Address);
            Assert.NotNull(toXfer);
            Assert.Equal(fxTokens[i].Record.Token, toXfer.Token);
            Assert.Equal((long)xferAmounts[i], toXfer.Amount);
            Assert.False(toXfer.IsAllowance);
            Assert.Equal(3, data.CryptoTransfers.Length);
        }
    }
}