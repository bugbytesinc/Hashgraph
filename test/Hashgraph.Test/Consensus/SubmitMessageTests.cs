using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Topic;

[Collection(nameof(NetworkCredentials))]
public class SubmitMessageTests
{
    private readonly NetworkCredentials _network;
    public SubmitMessageTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Submit Message: Can Submit Message")]
    public async Task CanSubmitMessage()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));

        var receipt = await fx.Client.SubmitMessageAsync(fx.Record.Topic, message, fx.ParticipantPrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(1ul, receipt.SequenceNumber);
        Assert.False(receipt.RunningHash.IsEmpty);
        Assert.Equal(3ul, receipt.RunningHashVersion);

        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal(fx.Memo, info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(1UL, info.SequenceNumber);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
        Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);
        AssertHg.NotEmpty(info.Ledger);
    }
    [Fact(DisplayName = "Submit Message: Can Submit Message to Open Topic")]
    public async Task CanSubmitMessageToOpenTopic()
    {
        await using var fx = await TestTopic.CreateAsync(_network, fx =>
        {
            fx.Params.Participant = null;
        });
        var message = Encoding.ASCII.GetBytes(Generator.Memo(20, 100));

        var receipt = await fx.Client.SubmitMessageAsync(fx.Record.Topic, message);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(1ul, receipt.SequenceNumber);
        Assert.False(receipt.RunningHash.IsEmpty);
        Assert.Equal(3ul, receipt.RunningHashVersion);

        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal(fx.Memo, info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(1UL, info.SequenceNumber);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
        Assert.Null(info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);
        AssertHg.NotEmpty(info.Ledger);
    }
    [Fact(DisplayName = "Submit Message: Submit Without Participant Key Raises Error")]
    public async Task SubmitMessageWithoutKeyRaisesError()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.Memo(20, 100));

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fx.Client.SubmitMessageAsync(fx.Record.Topic, message);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
        Assert.StartsWith("Submit Message failed, status: InvalidSignature", tex.Message);
    }
    [Fact(DisplayName = "Submit Message: Submit Without Topic Raises Error")]
    public async Task SubmitMessageWithoutTopicRaisesError()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.Memo(20, 100));

        var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await fx.Client.SubmitMessageAsync(null, message);
        });
        Assert.Equal("topic", ane.ParamName);
        Assert.StartsWith("Topic Address is missing. Please check that it is not null.", ane.Message);
    }
    [Fact(DisplayName = "Submit Message: Submit With Invalid Topic Raises Error")]
    public async Task SubmitMessageWithInvalidTopicRaisesError()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.Memo(20, 100));

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fx.Client.SubmitMessageAsync(Address.None, message);
        });
        Assert.Equal(ResponseCode.InvalidTopicId, tex.Status);
        Assert.Equal(ResponseCode.InvalidTopicId, tex.Receipt.Status);
        Assert.StartsWith("Submit Message failed, status: InvalidTopicId", tex.Message);
    }
    [Fact(DisplayName = "Submit Message: Submit Without Message Raises Error")]
    public async Task SubmitMessageWithoutMessageRaisesError()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));

        var aore = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await fx.Client.SubmitMessageAsync(fx.Record.Topic, null, fx.ParticipantPrivateKey);
        });
        Assert.Equal("message", aore.ParamName);
        Assert.StartsWith("Topic Message can not be empty.", aore.Message);
    }
    [Fact(DisplayName = "Submit Message: Submit To Deleted Topic Raises Error")]
    public async Task SubmitMessageToDeletedTopicRaisesError()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var receipt = await fx.Client.DeleteTopicAsync(fx.Record.Topic, fx.AdminPrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var message = Encoding.ASCII.GetBytes(Generator.Memo(10, 100));

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fx.Client.SubmitMessageAsync(fx.Record.Topic, message);
        });
        Assert.Equal(ResponseCode.InvalidTopicId, tex.Status);
        Assert.Equal(ResponseCode.InvalidTopicId, tex.Receipt.Status);
        Assert.StartsWith("Submit Message failed, status: InvalidTopicId", tex.Message);
    }
    [Fact(DisplayName = "Submit Message: Submitting Messages Increments Sequence Number")]
    public async Task CanIncrementSequenceNumber()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var expectedSequenceNumber = Generator.Integer(10, 20);

        for (int i = 0; i < expectedSequenceNumber; i++)
        {
            var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
            var receipt = await fx.Client.SubmitMessageAsync(fx.Record.Topic, message, fx.ParticipantPrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal((ulong)i + 1, receipt.SequenceNumber);
            Assert.False(receipt.RunningHash.IsEmpty);
            Assert.Equal(3ul, receipt.RunningHashVersion);
        }

        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal((ulong)expectedSequenceNumber, info.SequenceNumber);
    }
    [Fact(DisplayName = "Submit Message: Submitting Messages Can Retrieve Records")]
    public async Task CanCallWithRecord()
    {
        SubmitMessageRecord record = null;
        await using var fx = await TestTopic.CreateAsync(_network);
        var expectedSequenceNumber = Generator.Integer(10, 20);

        for (int i = 0; i < expectedSequenceNumber; i++)
        {
            var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
            record = await fx.Client.SubmitMessageWithRecordAsync(fx.Record.Topic, message, fx.ParticipantPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.Equal((ulong)i + 1, record.SequenceNumber);
            Assert.False(record.RunningHash.IsEmpty);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
        }

        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal((ulong)expectedSequenceNumber, info.SequenceNumber);
        Assert.Equal((ulong)expectedSequenceNumber, record.SequenceNumber);
        Assert.Equal(info.RunningHash.ToArray(), record.RunningHash.ToArray());
        AssertHg.NotEmpty(info.Ledger);
    }
    [Fact(DisplayName = "Submit Message: Submitting Messages Can Retrieve Records (without extra Signatory)")]
    public async Task CanCallWithRecordPayerSignatory()
    {
        SubmitMessageRecord record = null;
        await using var fx = await TestTopic.CreateAsync(_network);
        var expectedSequenceNumber = Generator.Integer(10, 20);

        for (int i = 0; i < expectedSequenceNumber; i++)
        {
            var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
            record = await fx.Client.SubmitMessageWithRecordAsync(fx.Record.Topic, message, ctx => ctx.Signatory = new Signatory(fx.ParticipantPrivateKey, _network.PrivateKey));
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.Equal((ulong)i + 1, record.SequenceNumber);
            Assert.False(record.RunningHash.IsEmpty);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
        }

        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal((ulong)expectedSequenceNumber, info.SequenceNumber);
        Assert.Equal((ulong)expectedSequenceNumber, record.SequenceNumber);
        Assert.Equal(info.RunningHash.ToArray(), record.RunningHash.ToArray());
        AssertHg.NotEmpty(info.Ledger);
    }
    [Fact(DisplayName = "Submit Message: Can Schedule Submit Message")]
    public async Task CanScheduleSubmitMessage()
    {
        await using var fxTopic = await TestTopic.CreateAsync(_network);
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);

        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
        var schedulingReceipt = await fxTopic.Client.SubmitMessageAsync(
            fxTopic.Record.Topic,
            message,
            new Signatory(
                fxTopic.ParticipantPrivateKey,
                new PendingParams
                {
                    PendingPayer = fxPayer
                }));
        Assert.Equal(ResponseCode.Success, schedulingReceipt.Status);
        Assert.Equal(0ul, schedulingReceipt.SequenceNumber);
        Assert.True(schedulingReceipt.RunningHash.IsEmpty);
        Assert.Equal(0ul, schedulingReceipt.RunningHashVersion);

        var counterReceipt = await fxPayer.Client.SignPendingTransactionAsync(schedulingReceipt.Pending.Id, fxPayer);

        var pendingReceipt = await fxPayer.Client.GetReceiptAsync(schedulingReceipt.Pending.TxId);
        Assert.Equal(ResponseCode.Success, pendingReceipt.Status);

        var messageReceipt = Assert.IsType<SubmitMessageReceipt>(pendingReceipt);
        Assert.Equal(1ul, messageReceipt.SequenceNumber);
        Assert.False(messageReceipt.RunningHash.IsEmpty);
        Assert.Equal(3ul, messageReceipt.RunningHashVersion);

        var info = await fxTopic.Client.GetTopicInfoAsync(fxTopic.Record.Topic);
        Assert.Equal(fxTopic.Memo, info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(1UL, info.SequenceNumber);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Equal(new Endorsement(fxTopic.AdminPublicKey), info.Administrator);
        Assert.Equal(new Endorsement(fxTopic.ParticipantPublicKey), info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Equal(fxTopic.TestAccount.Record.Address, info.RenewAccount);
        AssertHg.NotEmpty(info.Ledger);
    }

    [Fact(DisplayName = "Submit Message: Can Submit a Birst of Messages")]
    public async Task CanSubmitABirstOfMessages()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var client = fxPayer.Client.Clone(ctx => { ctx.Payer = fxPayer; ctx.Signatory = fxPayer.PrivateKey; });
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
        var tasks = Enumerable.Range(1, 4000).Select(_ => Task.Run(async () =>
        {
            try
            {
                return await client.SubmitMessageAsync(fx.Record.Topic, message, fx.ParticipantPrivateKey);
            }
            catch (PrecheckException pex) when (pex.Status == ResponseCode.DuplicateTransaction)
            {
                // This is not a case we should be dealing with
                // at the lower level as it is a problem with the
                // hedera network itself, but under load duplicate
                // transaction errors are returned by the proxy instead
                // of OKs, it is as if the proxy is double submitting
                // a transaction.
                return await client.GetReceiptAsync(pex.TxId) as SubmitMessageReceipt;
            }
            catch (PrecheckException pex) when (pex.Status == ResponseCode.TransactionExpired)
            {
                // This is not a case we should be dealing with
                // at the lower level as it is a problem with the
                // hedera network itself, under load the transaction
                // can expire.  Applications will need to be programmed
                // to handle such situations.
                return null;
            }
        }));
        var receipts = (await Task.WhenAll(tasks)).Where(r => r != null).ToArray();
        foreach (var receipt in receipts)
        {
            Assert.NotNull(receipt);
            Assert.Equal(ResponseCode.Success, receipt.Status);
        }

        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal((ulong)receipts.Length, info.SequenceNumber);
    }

}