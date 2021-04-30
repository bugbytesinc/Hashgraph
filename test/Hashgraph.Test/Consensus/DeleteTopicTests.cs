using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Topic
{
    [Collection(nameof(NetworkCredentials))]
    public class DeleteTopicTests
    {
        private readonly NetworkCredentials _network;
        public DeleteTopicTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Topic Delete: Can Delete Topic")]
        public async Task CanDeleteTopic()
        {
            await using var fx = await TestTopic.CreateAsync(_network);

            var record = await fx.Client.DeleteTopicAsync(fx.Record.Topic, fx.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
            });
            Assert.Equal(ResponseCode.InvalidTopicId, pex.Status);
        }
        [Fact(DisplayName = "Topic Delete: Calling Delete without Admin Key Raises Error")]
        public async Task CallingDeleteWithoutAdminKeyRaisesError()
        {
            await using var fx = await TestTopic.CreateAsync(_network);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.DeleteTopicAsync(fx.Record.Topic);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to Delete Topic, status: InvalidSignature", tex.Message);
        }
        [Fact(DisplayName = "Topic Delete: Calling Delete on an Imutable Topic Raises an Error")]
        public async Task CallingDeleteOnImutableTopicRaisesError()
        {
            await using var fx = await TestTopic.CreateAsync(_network, fx =>
            {
                fx.Params.Administrator = null;
                fx.Params.RenewAccount = null;
            });
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.DeleteTopicAsync(fx.Record.Topic);
            });
            Assert.Equal(ResponseCode.Unauthorized, tex.Status);
            Assert.StartsWith("Unable to Delete Topic, status: Unauthorized", tex.Message);
        }
        [Fact(DisplayName = "Topic Delete: Can Delete Topic with One of Two Mult-Sig")]
        public async Task CanDeleteTopicWithMultiSig()
        {
            var (pubAdminKey2, privateAdminKey2) = Generator.KeyPair();
            await using var fx = await TestTopic.CreateAsync(_network, fx =>
            {
                fx.Params.Administrator = new Endorsement(1, fx.AdminPublicKey, pubAdminKey2);
                fx.Params.Signatory = new Signatory(fx.Signatory, privateAdminKey2);
            });

            var record = await fx.Client.DeleteTopicAsync(fx.Record.Topic, privateAdminKey2);
            Assert.Equal(ResponseCode.Success, record.Status);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                var info = await fx.Client.GetTopicInfoAsync(fx.Record.Topic);
            });
            Assert.Equal(ResponseCode.InvalidTopicId, pex.Status);
        }
        [Fact(DisplayName = "Topic Delete: Deleting a Deleted Topic Raises Error")]
        public async Task DeletingDeletedTopicRaisesError()
        {
            await using var fx = await TestTopic.CreateAsync(_network);

            var record = await fx.Client.DeleteTopicAsync(fx.Record.Topic, fx.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var pex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.DeleteTopicAsync(fx.Record.Topic, fx.AdminPrivateKey);
            });
            Assert.Equal(ResponseCode.InvalidTopicId, pex.Status);
            Assert.StartsWith("Unable to Delete Topic, status: InvalidTopicId", pex.Message);
        }
        [Fact(DisplayName = "Topic Delete: Calling Delete with invalid ID raises Error")]
        public async Task CallingDeleteOnInvalidTopicIDRaisesError()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.DeleteTopicAsync(fx.Record.Address);
            });
            Assert.Equal(ResponseCode.InvalidTopicId, tex.Status);
            Assert.StartsWith("Unable to Delete Topic, status: InvalidTopicId", tex.Message);
        }
        [Fact(DisplayName = "Topic Delete: Calling Delete with missing ID raises Error")]
        public async Task CallingDeleteWithMissingTopicIDRaisesError()
        {
            await using var client = _network.NewClient();
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.DeleteTopicAsync(null);
            });
            Assert.Equal("topic", ane.ParamName);
            Assert.StartsWith("Topic Address is missing. Please check that it is not null.", ane.Message);
        }
        [Fact(DisplayName = "Topic Delete: Can Not Schedule a Delete Topic")]
        public async Task CanNotScheduleADeleteTopic()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxTopic = await TestTopic.CreateAsync(_network);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxTopic.Client.DeleteTopicAsync(fxTopic.Record.Topic, new Signatory(fxTopic.AdminPrivateKey, new PendingParams {
                    PendingPayer = fxPayer,
                }));
            });
            Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
            Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
        }
    }
}
