﻿namespace Hashgraph.Test.Topic;

[Collection(nameof(NetworkCredentials))]
public class CreateTopicTests
{
    private readonly NetworkCredentials _network;
    public CreateTopicTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Create Topic: Can Create")]
    public async Task CanCreateATopicAsync()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        Assert.NotNull(fx.Record);
        Assert.NotNull(fx.Record.Topic);
        Assert.True(fx.Record.Topic.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, fx.Record.Status);

        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal(fx.Memo, info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(0UL, info.SequenceNumber);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
        Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);
        AssertHg.NotEmpty(info.Ledger);
    }
    [Fact(DisplayName = "Create Topic: Can Create (Receipt Version)")]
    public async Task CanCreateATopicWithReceiptAsync()
    {
        await using var client = _network.NewClient();
        var receipt = await client.CreateTopicAsync(new CreateTopicParams
        {
            Memo = "Receipt Version"
        });
        Assert.NotNull(receipt);
        Assert.NotNull(receipt.Topic);
        Assert.True(receipt.Topic.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await client.GetTopicInfoAsync(receipt.Topic);
        Assert.Equal("Receipt Version", info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(0UL, info.SequenceNumber);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Null(info.Administrator);
        Assert.Null(info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Null(info.RenewAccount);
        AssertHg.NotEmpty(info.Ledger);
    }
    [Fact(DisplayName = "Create Topic: Can Create Topic with Null Memo raises error.")]
    public async Task CreateWithNullMemoRaisesError()
    {
        var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await TestTopic.CreateAsync(_network, fx =>
            {
                fx.Params.Memo = null;
            });
        });
        Assert.Equal("Memo", ane.ParamName);
        Assert.StartsWith("Memo can not be null.", ane.Message);
    }
    [Fact(DisplayName = "Create Topic: Can Create Topic with empty Memo")]
    public async Task CanCreateATopicWithEmptyMemoAsync()
    {

        await using var fx = await TestTopic.CreateAsync(_network, fx =>
        {
            fx.Params.Memo = string.Empty;
        });
        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Empty(info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(0UL, info.SequenceNumber);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
        Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);
        AssertHg.NotEmpty(info.Ledger);
    }
    [Fact(DisplayName = "Create Topic: Can Create Topic with no Administrator and Auto Renew Raises Error")]
    public async Task CanCreateATopicWithNoAdministratorAndAutoRenewAccountRaisesError()
    {
        var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await TestTopic.CreateAsync(_network, fx =>
            {
                fx.Params.Administrator = null;
            });
        });
        Assert.Equal("Administrator", ane.ParamName);
        Assert.StartsWith("The Administrator endorssement must not be null if RenewAccount is specified.", ane.Message);
    }
    [Fact(DisplayName = "Create Topic: Can Create Topic with no Administrator")]
    public async Task CanCreateATopicWithNoAdministrator()
    {
        await using var fx = await TestTopic.CreateAsync(_network, fx =>
        {
            fx.Params.Administrator = null;
            fx.Params.RenewAccount = null;
        });
        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal(fx.Memo, info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(0UL, info.SequenceNumber);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Null(info.Administrator);
        Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Null(info.RenewAccount);
        AssertHg.NotEmpty(info.Ledger);
    }
    [Fact(DisplayName = "Create Topic: Can Create Topic with no Participant Requirement")]
    public async Task CanCreateATopicWithNoParticipant()
    {
        await using var fx = await TestTopic.CreateAsync(_network, fx =>
        {
            fx.Params.Participant = null;
        });
        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal(fx.Memo, info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(0UL, info.SequenceNumber);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
        Assert.Null(info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);
        AssertHg.NotEmpty(info.Ledger);
    }
    [Fact(DisplayName = "Create Topic: Create Topic with no invalid renew period raises error.")]
    public async Task CanCreateATopicWithInvalidRenewPeriodRaisesError()
    {
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await TestTopic.CreateAsync(_network, fx =>
            {
                fx.Params.RenewPeriod = TimeSpan.FromDays(1);
            });
        });
        Assert.Equal(ResponseCode.AutorenewDurationNotInRange, tex.Status);
        Assert.Equal(ResponseCode.AutorenewDurationNotInRange, tex.Receipt.Status);
        Assert.StartsWith("Unable to create Consensus Topic, status: AutorenewDurationNotInRange", tex.Message);
    }
    [Fact(DisplayName = "Create Topic: Can Create Topic with no Renew Account")]
    public async Task CanCreateATopicWithNoRenewAccount()
    {
        await using var fx = await TestTopic.CreateAsync(_network, fx =>
        {
            fx.Params.RenewAccount = null;
        });
        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal(fx.Memo, info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(0UL, info.SequenceNumber);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
        Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Null(info.RenewAccount);
        AssertHg.NotEmpty(info.Ledger);
    }
    [Fact(DisplayName = "NETWORK V0.21.0 UNSUPPORTED: Create Topic: Can Create Topic with Alias Renew Account")]
    public async Task CanCreateATopicWithAliasRenewAccountDefect()
    {
        // Creating a topic with a renewal account using its alias address has not yet been
        // implemented by the network, although it will accept the transaction.
        var testFailException = (await Assert.ThrowsAsync<TransactionException>(CanCreateATopicWithAliasRenewAccount));
        Assert.StartsWith("Unable to create Consensus Topic, status: InvalidAutorenewAccount", testFailException.Message);

        //[Fact(DisplayName = "Create Topic: Can Create Topic with Alias Renew Account")]
        async Task CanCreateATopicWithAliasRenewAccount()
        {
            await using var fxRenew = await TestAliasAccount.CreateAsync(_network);
            await using var fx = await TestTopic.CreateAsync(_network, fx =>
            {
                fx.Params.RenewAccount = fxRenew.Alias;
                fx.Signatory = new Signatory(fx.AdminPrivateKey, fx.ParticipantPrivateKey, fxRenew.PrivateKey);
            });
            var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
            Assert.Equal(fx.Memo, info.Memo);
            Assert.NotEmpty(info.RunningHash.ToArray());
            Assert.Equal(0UL, info.SequenceNumber);
            Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
            Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
            Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
            Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
            Assert.Equal(fxRenew.CreateRecord.Address, info.RenewAccount);
            AssertHg.Equal(_network.Ledger, info.Ledger);
        }
    }
    [Fact(DisplayName = "Create Topic: Create Topic with missing signatures raises error.")]
    public async Task CanCreateATopicWithMissingSignaturesRaisesError()
    {
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await TestTopic.CreateAsync(_network, fx =>
            {
                fx.Params.Signatory = null;
            });
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
        Assert.StartsWith("Unable to create Consensus Topic, status: InvalidSignature", tex.Message);
    }

    [Fact(DisplayName = "Create Topic: Can Not Schedule a Create Topic")]
    public async Task CanNotScheduleACreateTopic()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await TestTopic.CreateAsync(_network, fx =>
            {
                fx.Params.Signatory = new Signatory(fx.Signatory, new PendingParams
                {
                    PendingPayer = fxPayer,
                });
            });
        });
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Receipt.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }
    [Fact(DisplayName = "Create Topic: Can Create Minimal Topic with Renew Account")]
    public async Task CanCreateMinimalTopicWithRenewAccount()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
        var (newPublicKey, newPrivateKey) = Generator.KeyPair();

        await using var client = fxAccount.Client.Clone(ctx =>
        {
            ctx.Payer = fxAccount;
            ctx.Signatory = fxAccount;
        });

        var receipt = await client.CreateTopicAsync(new CreateTopicParams
        {
            RenewAccount = fxAccount,
            Memo = "TEST",
            Administrator = newPublicKey,
            Signatory = newPrivateKey
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxAccount.Client.GetTopicInfoAsync(receipt.Topic);
        Assert.Equal("TEST", info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(0UL, info.SequenceNumber);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(new Endorsement(newPublicKey), info.Administrator);
        Assert.Null(info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Equal(fxAccount.Record.Address, info.RenewAccount);
        AssertHg.NotEmpty(info.Ledger);
    }
}