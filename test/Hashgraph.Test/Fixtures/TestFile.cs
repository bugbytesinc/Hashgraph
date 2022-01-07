using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Fixtures;

public class TestFile : IAsyncDisposable
{
    public ReadOnlyMemory<byte> PublicKey;
    public ReadOnlyMemory<byte> PrivateKey;
    public CreateFileParams CreateParams;
    public Address Payer;
    public Client Client;
    public FileRecord Record;
    public NetworkCredentials Network;

    public static async Task<TestFile> CreateAsync(NetworkCredentials networkCredentials, Action<TestFile> customize = null)
    {
        var test = new TestFile();
        test.Network = networkCredentials;
        test.Network.Output?.WriteLine("STARTING SETUP: Test File Instance");
        (test.PublicKey, test.PrivateKey) = Generator.KeyPair();
        test.Payer = networkCredentials.Payer;
        test.Client = networkCredentials.NewClient();
        test.CreateParams = new CreateFileParams
        {
            Memo = Generator.Code(20),
            Expiration = Generator.TruncateToSeconds(DateTime.UtcNow.AddSeconds(7890000)),
            Endorsements = new Endorsement[] { test.PublicKey },
            Contents = Encoding.Unicode.GetBytes("Hello From .NET" + Generator.Code(50)).Take(48).ToArray(),
            Signatory = test.PrivateKey
        };
        customize?.Invoke(test);
        test.Record = await test.Client.RetryKnownNetworkIssues(async client =>
        {
            return await test.Client.CreateFileWithRecordAsync(test.CreateParams, ctx =>
            {
                ctx.Memo = "TestFileInstance Setup: Creating Test File on Network";
            });
        });
        Assert.Equal(ResponseCode.Success, test.Record.Status);
        networkCredentials.Output?.WriteLine("SETUP COMPLETED: Test File Instance");
        return test;
    }

    public async ValueTask DisposeAsync()
    {
        Network.Output?.WriteLine("STARTING TEARDOWN: Test File Instance");
        try
        {
            await Client.DeleteFileAsync(Record.File, ctx =>
            {
                ctx.Memo = "TestFileInstance Teardown: Attempting to delete File from Network (may already be deleted)";
            });
        }
        catch
        {
            //noop
        }
        await Client.DisposeAsync();
        Network.Output?.WriteLine("TEARDOWN COMPLETED Test File Instance");
    }

    public static implicit operator Address(TestFile fxFile)
    {
        return fxFile.Record.File;
    }
}