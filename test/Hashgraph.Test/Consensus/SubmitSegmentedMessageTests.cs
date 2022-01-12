using Hashgraph.Test.Fixtures;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Topic;

[Collection(nameof(NetworkCredentials))]
public class SubmitSegmentedMessageTests
{
    private readonly NetworkCredentials _network;
    public SubmitSegmentedMessageTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Submit Segmented Message: Can Submit Single Segmented Message")]
    public async Task CanSubmitSingleSegmentedMessage()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var submitParams = new SubmitMessageParams
        {
            Topic = fx.Record.Topic,
            Segment = Encoding.ASCII.GetBytes(Generator.String(120, 199)),
            Index = 1,
            TotalSegmentCount = 1,
            Signatory = fx.ParticipantPrivateKey
        };
        var receipt = await fx.Client.SubmitMessageAsync(submitParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(1ul, receipt.SequenceNumber);
        Assert.False(receipt.RunningHash.IsEmpty);
        Assert.Equal(3ul, receipt.RunningHashVersion);
        var txId = receipt.Id;

        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal(fx.Memo, info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(1UL, info.SequenceNumber);
        Assert.True(info.Expiration > DateTime.MinValue);
        Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
        Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        await Task.Delay(7000); // give the beta net time to sync

        TopicMessage topicMessage = null;
        using var ctx = new CancellationTokenSource();
        await using var mirror = _network.NewMirror();
        try
        {
            var subscribeTask = mirror.SubscribeTopicAsync(new SubscribeTopicParams
            {
                Topic = fx.Record.Topic,
                Starting = DateTime.UtcNow.AddHours(-1),
                MessageWriter = new TopicMessageWriterAdapter(m =>
                {
                    topicMessage = m;
                    ctx.Cancel();
                }),
                CancellationToken = ctx.Token
            });

            ctx.CancelAfter(5000);

            await subscribeTask;

            if (topicMessage == null)
            {
                _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RETURN TOPIC IN ALLOWED TIME");
            }
            else
            {
                Assert.Equal(submitParams.Topic, topicMessage.Topic);
                Assert.Equal(1ul, topicMessage.SequenceNumber);
                Assert.Equal(receipt.RunningHash.ToArray(), topicMessage.RunningHash.ToArray());
                Assert.Equal(submitParams.Segment.ToArray(), topicMessage.Messsage.ToArray());
                Assert.NotNull(topicMessage.SegmentInfo);
                Assert.Equal(txId, topicMessage.SegmentInfo.ParentTxId);
                Assert.Equal(1, topicMessage.SegmentInfo.Index);
                Assert.Equal(1, topicMessage.SegmentInfo.TotalSegmentCount);
            }
        }
        catch (MirrorException mex) when (mex.Code == MirrorExceptionCode.TopicNotFound)
        {
            _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RECEIVE TOPIC CREATE IN ALLOWED TIME");
            return;
        }
    }
    [Fact(DisplayName = "Submit Segmented Message: Can Submit Double Segmented Message")]
    public async Task CanSubmitTwoSegmentedMessage()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var submitParams = new SubmitMessageParams[2];
        var receipt = new SubmitMessageReceipt[2];
        submitParams[0] = new SubmitMessageParams
        {
            Topic = fx.Record.Topic,
            Segment = Encoding.ASCII.GetBytes(Generator.String(120, 199)),
            Index = 1,
            TotalSegmentCount = 2,
            Signatory = fx.ParticipantPrivateKey
        };
        receipt[0] = await fx.Client.SubmitMessageAsync(submitParams[0]);
        Assert.Equal(ResponseCode.Success, receipt[0].Status);
        Assert.Equal(1ul, receipt[0].SequenceNumber);
        Assert.False(receipt[0].RunningHash.IsEmpty);
        Assert.Equal(3ul, receipt[0].RunningHashVersion);
        var txId = receipt[0].Id;

        submitParams[1] = new SubmitMessageParams
        {
            Topic = fx.Record.Topic,
            Segment = Encoding.ASCII.GetBytes(Generator.String(120, 199)),
            ParentTxId = txId,
            Index = 2,
            TotalSegmentCount = 2,
            Signatory = fx.ParticipantPrivateKey
        };
        receipt[1] = await fx.Client.SubmitMessageAsync(submitParams[1]);
        Assert.Equal(ResponseCode.Success, receipt[1].Status);
        Assert.Equal(2ul, receipt[1].SequenceNumber);
        Assert.False(receipt[1].RunningHash.IsEmpty);
        Assert.Equal(3ul, receipt[1].RunningHashVersion);

        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal(fx.Memo, info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(2UL, info.SequenceNumber);
        Assert.True(info.Expiration > DateTime.MinValue);
        Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
        Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        await Task.Delay(7000); // give the beta net time to sync

        try
        {
            await using var mirror = _network.NewMirror();
            var topicMessages = await TopicMessageCapture.CaptureOrTimeoutAsync(mirror, fx.Record.Topic, submitParams.Length, 7000);
            if (topicMessages.Length == 0)
            {
                _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RETURN TOPIC IN ALLOWED TIME");
            }
            else
            {
                Assert.Equal(2, topicMessages.Length);
                for (int i = 0; i < topicMessages.Length; i++)
                {
                    var topicMessage = topicMessages[i];
                    Assert.Equal(fx.Record.Topic, topicMessage.Topic);
                    Assert.Equal((ulong)i + 1, topicMessage.SequenceNumber);
                    Assert.Equal(receipt[i].RunningHash.ToArray(), topicMessage.RunningHash.ToArray());
                    Assert.Equal(submitParams[i].Segment.ToArray(), topicMessage.Messsage.ToArray());
                    Assert.NotNull(topicMessage.SegmentInfo);
                    Assert.Equal(txId, topicMessage.SegmentInfo.ParentTxId);
                    Assert.Equal(i + 1, topicMessage.SegmentInfo.Index);
                    Assert.Equal(submitParams.Length, topicMessage.SegmentInfo.TotalSegmentCount);
                }
            }
        }
        catch (MirrorException mex) when (mex.Code == MirrorExceptionCode.TopicNotFound)
        {
            _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RECEIVE TOPIC CREATE IN ALLOWED TIME");
            return;
        }
    }
    [Fact(DisplayName = "Submit Segmented Message: Can Submit Bogus Segmented Message Metadata")]
    public async Task CanSubmitBogusSegmentedMessageMetadata()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var parentTx = fx.Client.CreateNewTxId();
        var submitParams = new SubmitMessageParams
        {
            Topic = fx.Record.Topic,
            Segment = Encoding.ASCII.GetBytes(Generator.String(120, 199)),
            ParentTxId = parentTx,
            Index = 100,
            TotalSegmentCount = 200,
            Signatory = fx.ParticipantPrivateKey
        };
        var receipt = await fx.Client.SubmitMessageAsync(submitParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(1ul, receipt.SequenceNumber);
        Assert.False(receipt.RunningHash.IsEmpty);
        Assert.Equal(3ul, receipt.RunningHashVersion);

        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal(fx.Memo, info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(1UL, info.SequenceNumber);
        Assert.True(info.Expiration > DateTime.MinValue);
        Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
        Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        await Task.Delay(7000); // give the beta net time to sync

        try
        {
            await using var mirror = _network.NewMirror();
            var topicMessages = await TopicMessageCapture.CaptureOrTimeoutAsync(mirror, fx.Record.Topic, 1, 7000);
            if (topicMessages.Length == 0)
            {
                _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RETURN TOPIC IN ALLOWED TIME");
            }
            else
            {
                var topicMessage = topicMessages[0];
                Assert.Equal(submitParams.Topic, topicMessage.Topic);
                Assert.Equal(1ul, topicMessage.SequenceNumber);
                Assert.Equal(receipt.RunningHash.ToArray(), topicMessage.RunningHash.ToArray());
                Assert.Equal(submitParams.Segment.ToArray(), topicMessage.Messsage.ToArray());
                Assert.NotNull(topicMessage.SegmentInfo);
                Assert.Equal(parentTx, topicMessage.SegmentInfo.ParentTxId);
                Assert.Equal(100, topicMessage.SegmentInfo.Index);
                Assert.Equal(200, topicMessage.SegmentInfo.TotalSegmentCount);
            }
        }
        catch (MirrorException mex) when (mex.Code == MirrorExceptionCode.TopicNotFound)
        {
            _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RECEIVE TOPIC CREATE IN ALLOWED TIME");
            return;
        }
    }
    [Fact(DisplayName = "Submit Segmented Message: Can Submit Message to Open Topic")]
    public async Task CanSubmitMessageToOpenTopic()
    {
        await using var fx = await TestTopic.CreateAsync(_network, fx =>
        {
            fx.Params.Participant = null;
        });
        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
        var submitParams = new SubmitMessageParams
        {
            Topic = fx.Record.Topic,
            Segment = Encoding.ASCII.GetBytes(Generator.String(120, 199)),
            Index = 1,
            TotalSegmentCount = 1,
            Signatory = fx.ParticipantPrivateKey
        };
        var receipt = await fx.Client.SubmitMessageAsync(submitParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(1ul, receipt.SequenceNumber);
        Assert.False(receipt.RunningHash.IsEmpty);
        Assert.Equal(3ul, receipt.RunningHashVersion);

        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal(fx.Memo, info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(1UL, info.SequenceNumber);
        Assert.True(info.Expiration > DateTime.MinValue);
        Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
        Assert.Null(info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^
    }
    [Fact(DisplayName = "Submit Segmented Message: Submit Without Topic Raises Error")]
    public async Task SubmitMessageWithoutTopicRaisesError()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));

        var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            var submitParams = new SubmitMessageParams
            {
                Topic = null,
                Segment = Encoding.ASCII.GetBytes(Generator.String(120, 199)),
                Index = 1,
                TotalSegmentCount = 1,
                Signatory = null
            };
            await fx.Client.SubmitMessageAsync(submitParams);
        });
        Assert.Equal("topic", ane.ParamName);
        Assert.StartsWith("Topic Address is missing. Please check that it is not null.", ane.Message);
    }
    [Fact(DisplayName = "Submit Segmented Message: Submit With Invalid Topic Raises Error")]
    public async Task SubmitMessageWithInvalidTopicRaisesError()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));

        var aore = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            var txId = fx.Client.CreateNewTxId();
            var submitParams = new SubmitMessageParams
            {
                Topic = Address.None,
                Segment = Encoding.ASCII.GetBytes(Generator.String(120, 199)),
                ParentTxId = txId,
                Index = 1,
                TotalSegmentCount = 1,
                Signatory = null
            };
            await fx.Client.SubmitMessageAsync(submitParams, ctx => ctx.Transaction = txId);
        });
        Assert.Equal("ParentTxId", aore.ParamName);
        Assert.StartsWith("The Parent Transaction cannot be specified (must be null) when the segment index is one. (Parameter 'ParentTxId')", aore.Message);
    }
    [Fact(DisplayName = "Submit Segmented Message: Submit Without Message Raises Error")]
    public async Task SubmitMessageWithoutMessageRaisesError()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));

        var aore = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            var txId = fx.Client.CreateNewTxId();
            var submitParams = new SubmitMessageParams
            {
                Topic = fx.Record.Topic,
                Segment = null,
                ParentTxId = txId,
                Index = 1,
                TotalSegmentCount = 1,
                Signatory = null
            };
            await fx.Client.SubmitMessageAsync(submitParams, ctx => ctx.Transaction = txId);
        });
        Assert.Equal("message", aore.ParamName);
        Assert.StartsWith("Topic Message can not be empty.", aore.Message);
    }
    [Fact(DisplayName = "Submit Segmented Message: Submit To Deleted Topic Raises Error")]
    public async Task SubmitMessageToDeletedTopicRaisesError()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var receipt = await fx.Client.DeleteTopicAsync(fx.Record.Topic, fx.AdminPrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            var submitParams = new SubmitMessageParams
            {
                Topic = fx.Record.Topic,
                Segment = Encoding.ASCII.GetBytes(Generator.String(120, 199)),
                Index = 1,
                TotalSegmentCount = 1,
                Signatory = fx.ParticipantPrivateKey
            };
            await fx.Client.SubmitMessageAsync(submitParams);
        });
        Assert.Equal(ResponseCode.InvalidTopicId, tex.Status);
        Assert.Equal(ResponseCode.InvalidTopicId, tex.Receipt.Status);
        Assert.StartsWith("Submit Message failed, status: InvalidTopicId", tex.Message);
    }
    [Fact(DisplayName = "Submit Segmented Message: Submitting Messages Can Retrieve Record")]
    public async Task CanCallGetRecord()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var expectedSequenceNumber = Generator.Integer(10, 20);
        for (int i = 0; i < expectedSequenceNumber; i++)
        {
            var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
            var submitParams = new SubmitMessageParams
            {
                Topic = fx.Record.Topic,
                Segment = Encoding.ASCII.GetBytes(Generator.String(120, 199)),
                Index = 1,
                TotalSegmentCount = 1,
                Signatory = fx.ParticipantPrivateKey
            };

            var receipt = await fx.Client.SubmitMessageAsync(submitParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal((ulong)(i + 1), receipt.SequenceNumber);
            Assert.False(receipt.RunningHash.IsEmpty);
            Assert.Equal(3ul, receipt.RunningHashVersion);
            Assert.Equal(_network.Payer, receipt.Id.Address);

            var genericRecord = await fx.Client.GetTransactionRecordAsync(receipt.Id);
            var messageRecord = Assert.IsType<SubmitMessageRecord>(genericRecord);
            Assert.Equal(ResponseCode.Success, messageRecord.Status);
            Assert.Equal((ulong)(i + 1), messageRecord.SequenceNumber);
            Assert.Equal(3ul, messageRecord.RunningHashVersion);
            Assert.Equal(receipt.Id, messageRecord.Id);
            Assert.Equal(receipt.RunningHash.ToArray(), messageRecord.RunningHash.ToArray());
            Assert.False(messageRecord.Hash.IsEmpty);
            Assert.NotNull(messageRecord.Concensus);
            Assert.Empty(messageRecord.Memo);
            Assert.InRange(messageRecord.Fee, 0UL, ulong.MaxValue);
        }
        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal((ulong)expectedSequenceNumber, info.SequenceNumber);
    }
    [Fact(DisplayName = "Submit Segmented Message: Parent Transaction is Enforced for First Segment")]
    public async Task ParentTransactionIsEnforcedForFirstSegment()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));

        var aore = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            var submitParams = new SubmitMessageParams
            {
                Topic = fx.Record.Topic,
                Segment = message,
                ParentTxId = fx.Client.CreateNewTxId(),
                Index = 1,
                TotalSegmentCount = 2,
                Signatory = fx.ParticipantPrivateKey
            };
            await fx.Client.SubmitMessageAsync(submitParams);
        });
        Assert.StartsWith("The Parent Transaction cannot be specified (must be null) when the segment index is one. (Parameter 'ParentTxId')", aore.Message);
    }
    [Fact(DisplayName = "Submit Segmented Message: Parent Transaction is Not Enforced for Second Segment")]
    public async Task ParentTransactionIsNOtEnforcedForSecondSegment()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
        var receipt1 = await fx.Client.SubmitMessageAsync(new SubmitMessageParams
        {
            Topic = fx.Record.Topic,
            Segment = message,
            Index = 1,
            TotalSegmentCount = 2,
            Signatory = fx.ParticipantPrivateKey
        });
        Assert.Equal(ResponseCode.Success, receipt1.Status);

        var receipt2 = await fx.Client.SubmitMessageAsync(new SubmitMessageParams
        {
            Topic = fx.Record.Topic,
            Segment = message,
            ParentTxId = fx.Client.CreateNewTxId(),
            Index = 2,
            TotalSegmentCount = 2,
            Signatory = fx.ParticipantPrivateKey
        });
        Assert.Equal(ResponseCode.Success, receipt2.Status);
    }
    [Fact(DisplayName = "Submit Segmented Message: Negative Segment Index Raises Error")]
    public async Task SubmitMessageWithNegativeSgmentIndexRaisesError()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));

        var aore = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            var txId = fx.Client.CreateNewTxId();
            var submitParams = new SubmitMessageParams
            {
                Topic = fx.Record.Topic,
                Segment = message,
                ParentTxId = txId,
                Index = -5,
                TotalSegmentCount = 1,
                Signatory = null
            };
            await fx.Client.SubmitMessageAsync(submitParams);
        });
        Assert.Equal("Index", aore.ParamName);
        Assert.StartsWith("Segment index must be between one and the total segment count inclusively.", aore.Message);
    }
    [Fact(DisplayName = "Submit Segmented Message: Index Too Large Raises Error")]
    public async Task SubmitMessageIndexTooLargeRaisesError()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));

        var aore = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            var txId = fx.Client.CreateNewTxId();
            var submitParams = new SubmitMessageParams
            {
                Topic = fx.Record.Topic,
                Segment = message,
                ParentTxId = txId,
                Index = 5,
                TotalSegmentCount = 2,
                Signatory = null
            };
            await fx.Client.SubmitMessageAsync(submitParams);
        });
        Assert.Equal("Index", aore.ParamName);
        Assert.StartsWith("Segment index must be between one and the total segment count inclusively.", aore.Message);
    }
    [Fact(DisplayName = "Submit Segmented Message: Negative Total Segment Count Raises Error")]
    public async Task SubmitMessageNegativeTotalSegmentCountRaisesError()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));

        var aore = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            var txId = fx.Client.CreateNewTxId();
            var submitParams = new SubmitMessageParams
            {
                Topic = fx.Record.Topic,
                Segment = message,
                ParentTxId = txId,
                Index = 2,
                TotalSegmentCount = -2,
                Signatory = null
            };
            await fx.Client.SubmitMessageAsync(submitParams);
        });
        Assert.Equal("TotalSegmentCount", aore.ParamName);
        Assert.StartsWith("Total Segment Count must be a positive number.", aore.Message);
    }
    [Fact(DisplayName = "Submit Segmented Message: Network Allows Duplicate Segments")]
    public async Task NetworkAllowsDuplicateSegments()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var parentTx = fx.Client.CreateNewTxId();
        var copies = Generator.Integer(10, 20);
        for (int i = 0; i < copies; i++)
        {
            var receipt = await fx.Client.SubmitMessageAsync(new SubmitMessageParams
            {
                Topic = fx.Record.Topic,
                Segment = Encoding.ASCII.GetBytes(Generator.String(120, 199)),
                ParentTxId = parentTx,
                Index = 2,
                TotalSegmentCount = 3,
                Signatory = fx.ParticipantPrivateKey
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal((ulong)(i + 1), receipt.SequenceNumber);
            Assert.False(receipt.RunningHash.IsEmpty);
            Assert.Equal(3ul, receipt.RunningHashVersion);
        }

        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal(fx.Memo, info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal((ulong)copies, info.SequenceNumber);
        Assert.True(info.Expiration > DateTime.MinValue);
        Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
        Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^
    }
    [Fact(DisplayName = "Submit Segmented Message: Submitting Messages Can Return Record")]
    public async Task SubmittingMessagesCanReturnRecord()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var expectedSequenceNumber = Generator.Integer(10, 20);
        for (int i = 0; i < expectedSequenceNumber; i++)
        {
            var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
            var submitParams = new SubmitMessageParams
            {
                Topic = fx.Record.Topic,
                Segment = Encoding.ASCII.GetBytes(Generator.String(120, 199)),
                Index = 1,
                TotalSegmentCount = 1,
                Signatory = fx.ParticipantPrivateKey
            };

            var messageRecord = await fx.Client.SubmitMessageWithRecordAsync(submitParams);
            Assert.Equal(ResponseCode.Success, messageRecord.Status);
            Assert.Equal((ulong)(i + 1), messageRecord.SequenceNumber);
            Assert.Equal(3ul, messageRecord.RunningHashVersion);
            Assert.False(messageRecord.RunningHash.IsEmpty);
            Assert.False(messageRecord.Hash.IsEmpty);
            Assert.NotNull(messageRecord.Concensus);
            Assert.Empty(messageRecord.Memo);
            Assert.InRange(messageRecord.Fee, 0UL, ulong.MaxValue);
        }
        var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
        Assert.Equal((ulong)expectedSequenceNumber, info.SequenceNumber);
    }

    [Fact(DisplayName = "Submit Segmented Message: Can Schedule Submit Single Segmented Message")]
    public async Task CanScheduleSubmitSingleSegmentedMessage()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxTopic = await TestTopic.CreateAsync(_network);
        var submitParams = new SubmitMessageParams
        {
            Topic = fxTopic.Record.Topic,
            Segment = Encoding.ASCII.GetBytes(Generator.String(120, 199)),
            Index = 1,
            TotalSegmentCount = 1,
            Signatory = new Signatory(
                fxTopic.ParticipantPrivateKey,
                new PendingParams
                {
                    PendingPayer = fxPayer
                })
        };
        var schedulingReceipt = await fxTopic.Client.SubmitMessageAsync(submitParams);
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
        Assert.True(info.Expiration > DateTime.MinValue);
        Assert.Equal(new Endorsement(fxTopic.AdminPublicKey), info.Administrator);
        Assert.Equal(new Endorsement(fxTopic.ParticipantPublicKey), info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Equal(fxTopic.TestAccount.Record.Address, info.RenewAccount);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^
    }
}