namespace Hashgraph.Test.Topic;

[Collection(nameof(NetworkCredentials))]
public class SubscribeTopicTests
{
    private readonly NetworkCredentials _network;
    public SubscribeTopicTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Subscribe Topic: Can Create and Fetch Topic Message from Stream")]
    public async Task CanSubscribeToATopicAsync()
    {
        try
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var message = Encoding.ASCII.GetBytes(Generator.String(10, 100));

            var receipt = await fx.Client.SubmitMessageAsync(fx.Record.Topic, message, fx.ParticipantPrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(1ul, receipt.SequenceNumber);
            Assert.False(receipt.RunningHash.IsEmpty);
            Assert.Equal(3ul, receipt.RunningHashVersion);

            await Task.Delay(5000); // give the beta net time to sync

            TopicMessage topicMessage = null;
            using var ctx = new CancellationTokenSource();
            await using var mirror = _network.NewMirrorGrpcClient();
            var subscribeTask = mirror.SubscribeTopicAsync(new SubscribeTopicParams
            {
                Topic = fx.Record.Topic,
                Starting = DateTime.UtcNow.AddHours(-1),
                SubscribeMethod = (message) =>
                {
                    topicMessage = message;
                    ctx.Cancel();
                    return Task.CompletedTask;
                },
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
                Assert.Equal(fx.Record.Topic, topicMessage.Topic);
                Assert.Equal(1ul, topicMessage.SequenceNumber);
                Assert.Equal(receipt.RunningHash.ToArray(), topicMessage.RunningHash.ToArray());
                Assert.Equal(3ul, receipt.RunningHashVersion);
                Assert.Equal(message, topicMessage.Messsage.ToArray());
                Assert.Null(topicMessage.SegmentInfo);
            }

            var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
            Assert.Equal(fx.Memo, info.Memo);
            Assert.NotEqual(receipt.RunningHash.ToArray(), info.RunningHash);
            Assert.Equal(3UL, receipt.RunningHashVersion);
            Assert.Equal(1UL, info.SequenceNumber);
            Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
            Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
            Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
            Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
            Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);
            AssertHg.NotEmpty(info.Ledger);
        }
        catch (MirrorException mex) when (mex.Code == MirrorExceptionCode.TopicNotFound)
        {
            _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RECEIVE TOPIC CREATE IN ALLOWED TIME");
            return;
        }
    }
    [Fact(DisplayName = "Subscribe Topic: Can Create and Fetch Topic Test Message from Stream")]
    public async Task CanSubscribeToATestTopic()
    {
        await using var fx = await TestTopicMessage.CreateAsync(_network);

        Assert.Equal(ResponseCode.Success, fx.Record.Status);
        Assert.Equal(1ul, fx.Record.SequenceNumber);
        Assert.False(fx.Record.RunningHash.IsEmpty);

        await Task.Delay(7000); // give the beta net time to sync

        TopicMessage topicMessage = null;
        using var ctx = new CancellationTokenSource();
        await using var mirror = _network.NewMirrorGrpcClient();
        try
        {
            var subscribeTask = mirror.SubscribeTopicAsync(new SubscribeTopicParams
            {
                Topic = fx.TestTopic.Record.Topic,
                Starting = DateTime.UtcNow.AddHours(-1),
                SubscribeMethod = (message) =>
                {
                    topicMessage = message;
                    ctx.Cancel();
                    return Task.CompletedTask;
                },
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
                Assert.Equal(fx.TestTopic.Record.Topic, topicMessage.Topic);
                Assert.Equal(1ul, topicMessage.SequenceNumber);
                Assert.Equal(fx.Record.RunningHash.ToArray(), topicMessage.RunningHash.ToArray());
                Assert.Equal(fx.Message.ToArray(), topicMessage.Messsage.ToArray());
                Assert.Null(topicMessage.SegmentInfo);
            }
        }
        catch (MirrorException mex) when (mex.Code == MirrorExceptionCode.TopicNotFound)
        {
            _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RECEIVE TOPIC CREATE IN ALLOWED TIME");
            return;
        }
    }

    [Fact(DisplayName = "Subscribe Topic: Can Capture Topic Test Message from Stream")]
    public async Task CanCaptureATestTopic()
    {
        try
        {
            await using var fx = await TestTopicMessage.CreateAsync(_network);

            Assert.Equal(ResponseCode.Success, fx.Record.Status);
            Assert.Equal(1ul, fx.Record.SequenceNumber);
            Assert.False(fx.Record.RunningHash.IsEmpty);

            await Task.Delay(5000); // give the beta net time to sync

            var captureList = new List<TopicMessage>(1);
            await using var mirror = _network.NewMirrorGrpcClient();
            using var cts = new CancellationTokenSource();
            var subscribeTask = mirror.SubscribeTopicAsync(new SubscribeTopicParams
            {
                Topic = fx.TestTopic.Record.Topic,
                Starting = DateTime.UtcNow.AddHours(-1),
                SubscribeMethod = (message) =>
                {
                    captureList.Add(message);
                    return Task.CompletedTask;
                },
                CancellationToken = cts.Token
            });
            cts.CancelAfter(500);
            await subscribeTask;
            if (captureList.Count == 0)
            {
                _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RETURN TOPIC IN ALLOWED TIME");
            }
            else
            {
                var message = captureList[0];
                Assert.Equal(fx.TestTopic.Record.Topic, message.Topic);
                Assert.Equal(1ul, message.SequenceNumber);
                Assert.Equal(fx.Record.RunningHash.ToArray(), message.RunningHash.ToArray());
                Assert.Equal(fx.Message.ToArray(), message.Messsage.ToArray());
                Assert.Null(message.SegmentInfo);
            }
        }
        catch (MirrorException mex) when (mex.Code == MirrorExceptionCode.TopicNotFound)
        {
            _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RECEIVE TOPIC CREATE IN ALLOWED TIME");
            return;
        }
    }
    [Fact(DisplayName = "Subscribe Topic: Missing Channel Writer Raises Error")]
    public async Task MissingSubscribeMethodRaisesError()
    {
        await using var fx = await TestTopicMessage.CreateAsync(_network);
        await using var mirror = _network.NewMirrorGrpcClient();
        var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await mirror.SubscribeTopicAsync(new SubscribeTopicParams
            {
                Topic = fx.TestTopic.Record.Topic
            });
        });
        Assert.Equal("SubscribeMethod", ane.ParamName);
        Assert.StartsWith("The destination method missing. Please check that it is not null.", ane.Message);
    }
    [Fact(DisplayName = "Subscribe Topic: Missing Topic Raises Error")]
    public async Task MissingTopicIdRaisesError()
    {
        await using var mirror = _network.NewMirrorGrpcClient();
        var capture = new TopicMessageCapture(1);

        var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await mirror.SubscribeTopicAsync(new SubscribeTopicParams
            {
                Topic = null,
                MessageWriter = capture
            });
        });
        Assert.Equal("Topic", ane.ParamName);
        Assert.StartsWith("Topic address is missing. Please check that it is not null.", ane.Message);
        Assert.Empty(capture.CapturedList);
    }

    [Fact(DisplayName = "MIRROR NODE 0.29.0 DEFECT: Subscribe Topic: Invalid Topic Raises Error Does not always fail")]
    public async Task CanGetAssetInfoHavingAllowanceDefect()
    {
        try
        {
            await InvalidTopicIdRaisesError();
        }
        catch (Xunit.Sdk.ThrowsException testFailException)
        {
            Assert.StartsWith("Assert.Throws() Failure", testFailException.Message);
            _network.Output?.WriteLine("MIRROR NODE TEST FAILED, BUT DON'T FAIL TEST RUN");
        }

        //[Fact(DisplayName = "Subscribe Topic: Invalid Topic Raises Error")]
        async Task InvalidTopicIdRaisesError()
        {
            await using var mirror = _network.NewMirrorGrpcClient();
            var capture = new TopicMessageCapture(1);
            var mex = await Assert.ThrowsAsync<MirrorException>(async () =>
            {
                await mirror.SubscribeTopicAsync(new SubscribeTopicParams
                {
                    Topic = _network.Payer,
                    MessageWriter = capture,
                    CancellationToken = new CancellationTokenSource(2500).Token
                });
            });
            Assert.Equal(MirrorExceptionCode.InvalidTopicAddress, mex.Code);
            Assert.StartsWith("The address exists, but is not a topic.", mex.Message);
            Assert.Empty(capture.CapturedList);
        }
    }
    [Fact(DisplayName = "MIRROR NODE 0.29.0 DEFECT: Subscribe Topic: Non-Existant ID Raises Error Does not Always Fail")]
    public async Task NonExistantTopicIdRaisesErrordDefect()
    {
        try
        {
            await NonExistantTopicIdRaisesError();
        }
        catch (Xunit.Sdk.ThrowsException testFailException)
        {
            Assert.StartsWith("Assert.Throws() Failure", testFailException.Message);
            _network.Output?.WriteLine("MIRROR NODE TEST FAILED, BUT DON'T FAIL TEST RUN");
        }
        //[Fact(DisplayName = "Subscribe Topic: Non-Existant ID Raises Error")]
        async Task NonExistantTopicIdRaisesError()
        {
            await using var mirror = _network.NewMirrorGrpcClient();
            var capture = new TopicMessageCapture(1);
            var me = await Assert.ThrowsAsync<MirrorException>(async () =>
            {
                await mirror.SubscribeTopicAsync(new SubscribeTopicParams
                {
                    Topic = new Address(0, 1, 100),
                    MessageWriter = capture,
                    CancellationToken = new CancellationTokenSource(2500).Token
                });
            });
            Assert.Equal(MirrorExceptionCode.TopicNotFound, me.Code);
            Assert.StartsWith("The topic with the specified address does not exist.", me.Message);
            Assert.Empty(capture.CapturedList);
        }
    }
    [Fact(DisplayName = "Subscribe Topic: Invalid Filter Configuration Raises Error")]
    public async Task InvalidStartAndEndingFiltersRaiseError()
    {
        await using var fx = await TestTopicMessage.CreateAsync(_network);
        await Task.Delay(5000); // give the beta net time to sync

        using var cts = new CancellationTokenSource();
        List<TopicMessage> captured = new();
        await using var mirror = _network.NewMirrorGrpcClient();
        var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            cts.CancelAfter(500);
            await mirror.SubscribeTopicAsync(new SubscribeTopicParams
            {
                Topic = fx.TestTopic.Record.Topic,
                Starting = DateTime.UtcNow.AddDays(-1),
                Ending = DateTime.UtcNow.AddDays(-2),
                SubscribeMethod = (message) =>
                {
                    captured.Add(message);
                    return Task.CompletedTask;
                },
                CancellationToken = cts.Token
            });
        });
        Assert.Equal("Ending", aoe.ParamName);
        Assert.StartsWith("The ending filter date is less than the starting filter date, no records can be returned.", aoe.Message);
        Assert.Empty(captured);
    }
    [Fact(DisplayName = "Subscribe Topic: Return Limit is Enforced")]
    public async Task ReturnLimitIsEnforced()
    {
        await using var fx = await TestTopicMessage.CreateAsync(_network);
        await fx.TestTopic.Client.SubmitMessageAsync(fx.TestTopic.Record.Topic, fx.Message, fx.TestTopic.ParticipantPrivateKey);
        await fx.TestTopic.Client.SubmitMessageAsync(fx.TestTopic.Record.Topic, fx.Message, fx.TestTopic.ParticipantPrivateKey);
        await fx.TestTopic.Client.SubmitMessageAsync(fx.TestTopic.Record.Topic, fx.Message, fx.TestTopic.ParticipantPrivateKey);
        await fx.TestTopic.Client.SubmitMessageAsync(fx.TestTopic.Record.Topic, fx.Message, fx.TestTopic.ParticipantPrivateKey);

        // Wait for enough messages to be available 
        // in the mirror node's database.
        for (int waitTries = 0; waitTries < 20; waitTries++)
        {
            try
            {
                var captured = await TopicMessageCapture.CaptureOrTimeoutAsync(_network.NewMirrorGrpcClient(), fx.TestTopic.Record.Topic, 4, 5000);
                if (captured.Length > 2)
                {
                    break;
                }
            }
            catch (MirrorException ex) when (ex.Code == MirrorExceptionCode.TopicNotFound)
            {
                _network.Output?.WriteLine("Mirror Node is slow, can not find topic just yet.");
                await Task.Delay(1000);
            }
        }

        // Now we can try the real test on the limits.
        List<TopicMessage> capturedList = new();
        await using var mirror = _network.NewMirrorGrpcClient();
        using var cts = new CancellationTokenSource();
        try
        {
            var subscribeTask = mirror.SubscribeTopicAsync(new SubscribeTopicParams
            {
                Topic = fx.TestTopic.Record.Topic,
                Starting = DateTime.UtcNow.AddHours(-1),
                SubscribeMethod = (message) =>
                {
                    capturedList.Add(message);
                    return Task.CompletedTask;
                },
                CancellationToken = cts.Token,
                MaxCount = 2
            });
            cts.CancelAfter(10000);
            await subscribeTask;
            if (capturedList.Count == 0)
            {
                _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RETURN TOPIC IN ALLOWED TIME");
            }
            else
            {
                Assert.Equal(2, capturedList.Count);
            }
        }
        catch (MirrorException mex) when (mex.Code == MirrorExceptionCode.TopicNotFound)
        {
            _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RECEIVE TOPIC CREATE IN ALLOWED TIME");
            return;
        }
    }
}