using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Topic
{
    [Collection(nameof(NetworkCredentials))]
    public class SubmiLargeMessageTests
    {
        private readonly NetworkCredentials _network;
        public SubmiLargeMessageTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Submit Large Message: Can Submit Large Segmented Message")]
        public async Task CanSubmitLargeSegmentedMessage()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var message = Encoding.ASCII.GetBytes(Generator.String(1200, 1990));
            var segmentSize = Generator.Integer(100, 200);
            var expectedCount = message.Length / segmentSize + 1;
            var receipts = await fx.Client.SubmitLargeMessageAsync(fx.Record.Topic, message, segmentSize, fx.ParticipantPrivateKey);
            Assert.Equal(expectedCount, receipts.Length);
            for (int i = 0; i < expectedCount; i++)
            {
                var receipt = receipts[i];
                Assert.Equal(ResponseCode.Success, receipt.Status);
                Assert.Equal((ulong)(i + 1), receipt.SequenceNumber);
                Assert.False(receipt.RunningHash.IsEmpty);
                Assert.Equal(2ul, receipt.RunningHashVersion);
            }

            var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
            Assert.Equal(fx.Memo, info.Memo);
            Assert.NotEqual(ReadOnlyMemory<byte>.Empty, info.RunningHash);
            Assert.Equal((ulong)expectedCount, info.SequenceNumber);
            Assert.True(info.Expiration > DateTime.MinValue);
            Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
            Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
            Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
            Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);

            await Task.Delay(7000); // give the beta net time to sync

            try
            {
                await using var mirror = _network.NewMirror();
                var topicMessages = await TopicMessageCapture.CaptureOrTimeoutAsync(mirror, fx.Record.Topic, expectedCount, 7000);
                if (topicMessages.Length == 0)
                {
                    _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RETURN TOPIC IN ALLOWED TIME");
                }
                else
                {
                    var pointerIndex = 0;
                    var reconstitutedMessage = new byte[message.Length];
                    Assert.Equal(expectedCount, topicMessages.Length);
                    for (int i = 0; i < topicMessages.Length; i++)
                    {
                        var topicMessage = topicMessages[i];
                        Assert.Equal(fx.Record.Topic, topicMessage.Topic);
                        Assert.Equal((ulong)i + 1, topicMessage.SequenceNumber);
                        Assert.Equal(receipts[i].RunningHash.ToArray(), topicMessage.RunningHash.ToArray());
                        Assert.NotNull(topicMessage.SegmentInfo);
                        Assert.Equal(receipts[0].Id, topicMessage.SegmentInfo.ParentTxId);
                        Assert.Equal(i + 1, topicMessage.SegmentInfo.Index);
                        Assert.Equal(expectedCount, topicMessage.SegmentInfo.TotalSegmentCount);
                        topicMessage.Messsage.ToArray().CopyTo(reconstitutedMessage, pointerIndex);
                        pointerIndex += topicMessage.Messsage.Length;
                    }
                    Assert.Equal(message.ToArray(), reconstitutedMessage);
                }
            }
            catch (MirrorException mex) when (mex.Code == MirrorExceptionCode.TopicNotFound)
            {
                _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RECEIVE TOPIC CREATE IN ALLOWED TIME");
                return;
            }
        }
        [Fact(DisplayName = "Submit Large Message: Submit Without Participant Key Raises Error")]
        public async Task SubmitMessageWithoutKeyRaisesError()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var message = Encoding.ASCII.GetBytes(Generator.String(1200, 1990));
            var segmentSize = Generator.Integer(100, 200);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.SubmitLargeMessageAsync(fx.Record.Topic, message, segmentSize);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Submit Message failed, status: InvalidSignature", tex.Message);
        }
        [Fact(DisplayName = "Submit Large Message: Submit Without Topic Raises Error")]
        public async Task SubmitMessageWithoutTopicRaisesError()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var message = Encoding.ASCII.GetBytes(Generator.String(1200, 1990));
            var segmentSize = Generator.Integer(100, 200);

            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx.Client.SubmitLargeMessageAsync(null, message, segmentSize);
            });
            Assert.Equal("topic", ane.ParamName);
            Assert.StartsWith("Topic Address is missing. Please check that it is not null.", ane.Message);
        }
        [Fact(DisplayName = "Submit Large Message: Submit With Invalid Topic Raises Error")]
        public async Task SubmitMessageWithInvalidTopicRaisesError()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var message = Encoding.ASCII.GetBytes(Generator.String(1200, 1990));
            var segmentSize = Generator.Integer(100, 200);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.SubmitLargeMessageAsync(Address.None, message, segmentSize, fx.ParticipantPrivateKey);
            });
            Assert.Equal(ResponseCode.InvalidTopicId, tex.Status);
            Assert.StartsWith("Submit Message failed, status: InvalidTopicId", tex.Message);
        }
        [Fact(DisplayName = "Submit Large Message: Submit Without Message Raises Error")]
        public async Task SubmitMessageWithoutMessageRaisesError()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var message = Encoding.ASCII.GetBytes(Generator.String(1200, 1990));
            var segmentSize = Generator.Integer(100, 200);

            var aore = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await fx.Client.SubmitLargeMessageAsync(fx.Record.Topic, null, segmentSize, fx.ParticipantPrivateKey);
            });
            Assert.Equal("message", aore.ParamName);
            Assert.StartsWith("Topic Message can not be empty.", aore.Message);
        }
        [Fact(DisplayName = "Submit Large Message: Submit To Deleted Topic Raises Error")]
        public async Task SubmitMessageToDeletedTopicRaisesError()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var receipt = await fx.Client.DeleteTopicAsync(fx.Record.Topic, fx.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var message = Encoding.ASCII.GetBytes(Generator.String(1200, 1990));
            var segmentSize = Generator.Integer(100, 200);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.SubmitLargeMessageAsync(fx.Record.Topic, message, segmentSize, fx.ParticipantPrivateKey);
            });
            Assert.Equal(ResponseCode.InvalidTopicId, tex.Status);
            Assert.StartsWith("Submit Message failed, status: InvalidTopicId", tex.Message);
        }
        [Fact(DisplayName = "Submit Large Message: Submitting Messages Can Retrieve Record")]
        public async Task CanCallGetRecord()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var message = Encoding.ASCII.GetBytes(Generator.String(1200, 1990));
            var segmentSize = Generator.Integer(100, 200);
            var expectedCount = message.Length / segmentSize + 1;
            var receipts = await fx.Client.SubmitLargeMessageAsync(fx.Record.Topic, message, segmentSize, fx.ParticipantPrivateKey);
            Assert.Equal(expectedCount, receipts.Length);
            for (int i = 0; i < expectedCount; i++)
            {
                var receipt = receipts[i];
                Assert.Equal(ResponseCode.Success, receipt.Status);
                Assert.Equal((ulong)(i + 1), receipt.SequenceNumber);
                Assert.False(receipt.RunningHash.IsEmpty);
                Assert.Equal(2ul, receipt.RunningHashVersion);

                var genericRecord = await fx.Client.GetTransactionRecordAsync(receipt.Id);
                var messageRecord = Assert.IsType<SubmitMessageRecord>(genericRecord);
                Assert.Equal(ResponseCode.Success, messageRecord.Status);
                Assert.Equal((ulong)(i + 1), messageRecord.SequenceNumber);
                Assert.Equal(2ul, messageRecord.RunningHashVersion);
                Assert.Equal(receipt.Id, messageRecord.Id);
                Assert.Equal(receipt.RunningHash.ToArray(), messageRecord.RunningHash.ToArray());
                Assert.False(messageRecord.Hash.IsEmpty);
                Assert.NotNull(messageRecord.Concensus);
                Assert.Empty(messageRecord.Memo);
                Assert.InRange(messageRecord.Fee, 0UL, ulong.MaxValue);
            }
            var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
            Assert.Equal((ulong)expectedCount, info.SequenceNumber);
        }
        [Fact(DisplayName = "Submit Large Message: Throws Error if Run out of Crypto")]
        public async Task ThrowsErrorIfRunOutOfCrypto()
        {
            await using var fxTopic = await TestTopic.CreateAsync(_network);
            await using var fxAccount = await TestAccount.CreateAsync(_network, a => a.CreateParams.InitialBalance = 1_200_000);
            var message = Encoding.ASCII.GetBytes(Generator.String(1200, 1990));
            var segmentSize = Generator.Integer(100, 200);
            var expectedCount = message.Length / segmentSize + 1;
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fxTopic.Client.SubmitLargeMessageAsync(fxTopic.Record.Topic, message, segmentSize, fxTopic.ParticipantPrivateKey, ctx => {
                    ctx.Payer = fxAccount.Record.Address;
                    ctx.Signatory = fxAccount.PrivateKey;
                });
            });
            Assert.Equal(ResponseCode.InsufficientPayerBalance, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InsufficientPayerBalance", pex.Message);
        }
        [Fact(DisplayName = "Submit Large Message: Can Submit Large Segmented Message with Even Boundary")]
        public async Task CanSubmitLargeSegmentedMessageWithEvenBoundary()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var segmentSize = Generator.Integer(100, 200);
            var expectedCount = Generator.Integer(3, 10);            
            var message = Encoding.ASCII.GetBytes(Generator.Code(segmentSize * expectedCount));
            var receipts = await fx.Client.SubmitLargeMessageAsync(fx.Record.Topic, message, segmentSize, fx.ParticipantPrivateKey);
            Assert.Equal(expectedCount, receipts.Length);
            for (int i = 0; i < expectedCount; i++)
            {
                var receipt = receipts[i];
                Assert.Equal(ResponseCode.Success, receipt.Status);
                Assert.Equal((ulong)(i + 1), receipt.SequenceNumber);
                Assert.False(receipt.RunningHash.IsEmpty);
                Assert.Equal(2ul, receipt.RunningHashVersion);
            }

            var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
            Assert.Equal(fx.Memo, info.Memo);
            Assert.NotEqual(ReadOnlyMemory<byte>.Empty, info.RunningHash);
            Assert.Equal((ulong)expectedCount, info.SequenceNumber);
            Assert.True(info.Expiration > DateTime.MinValue);
            Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
            Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
            Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
            Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);

            await Task.Delay(7000); // give the beta net time to sync

            try
            {
                await using var mirror = _network.NewMirror();
                var topicMessages = await TopicMessageCapture.CaptureOrTimeoutAsync(mirror, fx.Record.Topic, expectedCount, 7000);
                if (topicMessages.Length == 0)
                {
                    _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RETURN TOPIC IN ALLOWED TIME");
                }
                else
                {
                    var pointerIndex = 0;
                    var reconstitutedMessage = new byte[message.Length];
                    Assert.Equal(expectedCount, topicMessages.Length);
                    for (int i = 0; i < topicMessages.Length; i++)
                    {
                        var topicMessage = topicMessages[i];
                        Assert.Equal(fx.Record.Topic, topicMessage.Topic);
                        Assert.Equal((ulong)i + 1, topicMessage.SequenceNumber);
                        Assert.Equal(receipts[i].RunningHash.ToArray(), topicMessage.RunningHash.ToArray());
                        Assert.NotNull(topicMessage.SegmentInfo);
                        Assert.Equal(receipts[0].Id, topicMessage.SegmentInfo.ParentTxId);
                        Assert.Equal(i + 1, topicMessage.SegmentInfo.Index);
                        Assert.Equal(expectedCount, topicMessage.SegmentInfo.TotalSegmentCount);
                        topicMessage.Messsage.ToArray().CopyTo(reconstitutedMessage, pointerIndex);
                        pointerIndex += topicMessage.Messsage.Length;
                    }
                    Assert.Equal(message.ToArray(), reconstitutedMessage);
                }
            }
            catch (MirrorException mex) when (mex.Code == MirrorExceptionCode.TopicNotFound)
            {
                _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RECEIVE TOPIC CREATE IN ALLOWED TIME");
                return;
            }
        }
        [Fact(DisplayName = "Submit Large Message: Can Submit Large Segmented Message with Even Boundary")]
        public async Task CanSubmitLargeSegmentedMessageSmallerThanSegment()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var segmentSize = Generator.Integer(100, 200);
            var expectedCount = 1;
            var message = Encoding.ASCII.GetBytes(Generator.Code(segmentSize / 2));
            var receipts = await fx.Client.SubmitLargeMessageAsync(fx.Record.Topic, message, segmentSize, fx.ParticipantPrivateKey);
            Assert.Equal(expectedCount, receipts.Length);
            for (int i = 0; i < expectedCount; i++)
            {
                var receipt = receipts[i];
                Assert.Equal(ResponseCode.Success, receipt.Status);
                Assert.Equal((ulong)(i + 1), receipt.SequenceNumber);
                Assert.False(receipt.RunningHash.IsEmpty);
                Assert.Equal(2ul, receipt.RunningHashVersion);
            }

            var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
            Assert.Equal(fx.Memo, info.Memo);
            Assert.NotEqual(ReadOnlyMemory<byte>.Empty, info.RunningHash);
            Assert.Equal((ulong)expectedCount, info.SequenceNumber);
            Assert.True(info.Expiration > DateTime.MinValue);
            Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
            Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
            Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
            Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);

            await Task.Delay(7000); // give the beta net time to sync

            try
            {
                await using var mirror = _network.NewMirror();
                var topicMessages = await TopicMessageCapture.CaptureOrTimeoutAsync(mirror, fx.Record.Topic, expectedCount, 7000);
                if (topicMessages.Length == 0)
                {
                    _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RETURN TOPIC IN ALLOWED TIME");
                }
                else
                {
                    var pointerIndex = 0;
                    var reconstitutedMessage = new byte[message.Length];
                    Assert.Equal(expectedCount, topicMessages.Length);
                    for (int i = 0; i < topicMessages.Length; i++)
                    {
                        var topicMessage = topicMessages[i];
                        Assert.Equal(fx.Record.Topic, topicMessage.Topic);
                        Assert.Equal((ulong)i + 1, topicMessage.SequenceNumber);
                        Assert.Equal(receipts[i].RunningHash.ToArray(), topicMessage.RunningHash.ToArray());
                        Assert.NotNull(topicMessage.SegmentInfo);
                        Assert.Equal(receipts[0].Id, topicMessage.SegmentInfo.ParentTxId);
                        Assert.Equal(i + 1, topicMessage.SegmentInfo.Index);
                        Assert.Equal(expectedCount, topicMessage.SegmentInfo.TotalSegmentCount);
                        topicMessage.Messsage.ToArray().CopyTo(reconstitutedMessage, pointerIndex);
                        pointerIndex += topicMessage.Messsage.Length;
                    }
                    Assert.Equal(message.ToArray(), reconstitutedMessage);
                }
            }
            catch (MirrorException mex) when (mex.Code == MirrorExceptionCode.TopicNotFound)
            {
                _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RECEIVE TOPIC CREATE IN ALLOWED TIME");
                return;
            }
        }
        [Fact(DisplayName = "Submit Large Message: Can Submit Large Segmented Message with Two Segments")]
        public async Task CanSubmitLargeSegmentedMessageWithTwoSegments()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var segmentSize = Generator.Integer(100, 200);
            var expectedCount = 2;
            var message = Encoding.ASCII.GetBytes(Generator.Code(3 * segmentSize / 2));
            var receipts = await fx.Client.SubmitLargeMessageAsync(fx.Record.Topic, message, segmentSize, fx.ParticipantPrivateKey);
            Assert.Equal(expectedCount, receipts.Length);
            for (int i = 0; i < expectedCount; i++)
            {
                var receipt = receipts[i];
                Assert.Equal(ResponseCode.Success, receipt.Status);
                Assert.Equal((ulong)(i + 1), receipt.SequenceNumber);
                Assert.False(receipt.RunningHash.IsEmpty);
                Assert.Equal(2ul, receipt.RunningHashVersion);
            }

            var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
            Assert.Equal(fx.Memo, info.Memo);
            Assert.NotEqual(ReadOnlyMemory<byte>.Empty, info.RunningHash);
            Assert.Equal((ulong)expectedCount, info.SequenceNumber);
            Assert.True(info.Expiration > DateTime.MinValue);
            Assert.Equal(new Endorsement(fx.AdminPublicKey), info.Administrator);
            Assert.Equal(new Endorsement(fx.ParticipantPublicKey), info.Participant);
            Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
            Assert.Equal(fx.TestAccount.Record.Address, info.RenewAccount);

            await Task.Delay(7000); // give the beta net time to sync

            try
            {
                await using var mirror = _network.NewMirror();
                var topicMessages = await TopicMessageCapture.CaptureOrTimeoutAsync(mirror, fx.Record.Topic, expectedCount, 7000);
                if (topicMessages.Length == 0)
                {
                    _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RETURN TOPIC IN ALLOWED TIME");
                }
                else
                {
                    var pointerIndex = 0;
                    var reconstitutedMessage = new byte[message.Length];
                    Assert.Equal(expectedCount, topicMessages.Length);
                    for (int i = 0; i < topicMessages.Length; i++)
                    {
                        var topicMessage = topicMessages[i];
                        Assert.Equal(fx.Record.Topic, topicMessage.Topic);
                        Assert.Equal((ulong)i + 1, topicMessage.SequenceNumber);
                        Assert.Equal(receipts[i].RunningHash.ToArray(), topicMessage.RunningHash.ToArray());
                        Assert.NotNull(topicMessage.SegmentInfo);
                        Assert.Equal(receipts[0].Id, topicMessage.SegmentInfo.ParentTxId);
                        Assert.Equal(i + 1, topicMessage.SegmentInfo.Index);
                        Assert.Equal(expectedCount, topicMessage.SegmentInfo.TotalSegmentCount);
                        topicMessage.Messsage.ToArray().CopyTo(reconstitutedMessage, pointerIndex);
                        pointerIndex += topicMessage.Messsage.Length;
                    }
                    Assert.Equal(message.ToArray(), reconstitutedMessage);
                }
            }
            catch (MirrorException mex) when (mex.Code == MirrorExceptionCode.TopicNotFound)
            {
                _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RECEIVE TOPIC CREATE IN ALLOWED TIME");
                return;
            }
        }
    }
}
