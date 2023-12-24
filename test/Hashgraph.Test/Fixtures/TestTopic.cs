namespace Hashgraph.Test.Fixtures;

public class TestTopic : IAsyncDisposable
{
    public string Memo;
    public ReadOnlyMemory<byte> AdminPublicKey;
    public ReadOnlyMemory<byte> AdminPrivateKey;
    public ReadOnlyMemory<byte> ParticipantPublicKey;
    public ReadOnlyMemory<byte> ParticipantPrivateKey;
    public TestAccount TestAccount;
    public Address Payer;
    public Signatory Signatory;
    public Client Client;
    public CreateTopicParams Params;
    public CreateTopicRecord Record;
    public NetworkCredentials Network;

    public static async Task<TestTopic> CreateAsync(NetworkCredentials networkCredentials, Action<TestTopic> customize = null)
    {
        var fx = new TestTopic();
        fx.Network = networkCredentials;
        fx.Network.Output?.WriteLine("STARTING SETUP: Test Topic Instance");
        fx.Memo = Generator.Memo(20);
        (fx.AdminPublicKey, fx.AdminPrivateKey) = Generator.KeyPair();
        (fx.ParticipantPublicKey, fx.ParticipantPrivateKey) = Generator.KeyPair();
        fx.Payer = networkCredentials.Payer;
        fx.Client = networkCredentials.NewClient();
        fx.TestAccount = await TestAccount.CreateAsync(networkCredentials);
        fx.Signatory = new Signatory(fx.AdminPrivateKey, fx.ParticipantPrivateKey, fx.TestAccount.PrivateKey);
        fx.Params = new CreateTopicParams
        {
            Memo = fx.Memo,
            Administrator = fx.AdminPublicKey,
            Participant = fx.ParticipantPublicKey,
            RenewAccount = fx.TestAccount.Record.Address,
            Signatory = fx.Signatory
        };
        customize?.Invoke(fx);
        fx.Record = await networkCredentials.RetryForKnownNetworkIssuesAsync(async () =>
        {
            return await fx.Client.CreateTopicWithRecordAsync(fx.Params, ctx =>
            {
                ctx.Memo = ".NET SDK Test: " + fx.Memo ?? "(null memo)";
            });
        });
        Assert.Equal(ResponseCode.Success, fx.Record.Status);
        networkCredentials.Output?.WriteLine("SETUP COMPLETED: Test Topic Instance");
        return fx;
    }

    public async ValueTask DisposeAsync()
    {
        Network.Output?.WriteLine("STARTING TEARDOWN: Test Topic Instance");
        try
        {
            await Client.DeleteTopicAsync(Record.Topic, AdminPrivateKey, ctx =>
            {
                ctx.Memo = ".NET SDK Test: Attempting to delete Topic from Network (may already be deleted)";
            });
        }
        catch
        {
            //noop
        }
        await Client.DisposeAsync();
        await TestAccount.DisposeAsync();
        Network.Output?.WriteLine("TEARDOWN COMPLETED Test Topic Instance");
    }

    public static implicit operator Address(TestTopic fxTopic)
    {
        return fxTopic.Record.Topic;
    }
}