using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Fixtures;

public class TestTopicMessage : IAsyncDisposable
{
    public NetworkCredentials Network;
    public TestTopic TestTopic;
    public SubmitMessageRecord Record;
    public ReadOnlyMemory<byte> Message;

    public static async Task<TestTopicMessage> CreateAsync(NetworkCredentials networkCredentials, Action<TestTopicMessage> customize = null)
    {
        var fx = new TestTopicMessage();
        fx.Network = networkCredentials;
        fx.Network.Output?.WriteLine("STARTING SETUP: Test Topic Message Instance");
        fx.TestTopic = await TestTopic.CreateAsync(networkCredentials);
        fx.Message = Encoding.ASCII.GetBytes(Generator.String(10, 100));
        customize?.Invoke(fx);
        fx.Record = await fx.TestTopic.Client.RetryKnownNetworkIssues(async client =>
        {
            return await fx.TestTopic.Client.SubmitMessageWithRecordAsync(fx.TestTopic.Record.Topic, fx.Message, fx.TestTopic.ParticipantPrivateKey, ctx =>
            {
                ctx.Memo = "TestTopicMessage Setup: " + fx.TestTopic.Memo ?? "(null memo)";
            });
        });
        Assert.Equal(ResponseCode.Success, fx.Record.Status);
        networkCredentials.Output?.WriteLine("SETUP COMPLETED: Test Topic Message Instance");
        return fx;
    }

    public async ValueTask DisposeAsync()
    {
        Network.Output?.WriteLine("STARTING TEARDOWN: Test Topic Message Instance");
        await TestTopic.DisposeAsync();
        Network.Output?.WriteLine("TEARDOWN COMPLETED Test Topic Message Instance");
    }
}