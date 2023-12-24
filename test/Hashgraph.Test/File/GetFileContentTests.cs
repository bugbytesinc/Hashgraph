namespace Hashgraph.Test.File;

[Collection(nameof(NetworkCredentials))]
public class GetFileContentTests
{
    private readonly NetworkCredentials _network;
    public GetFileContentTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "File Content: Can Get File Content")]
    public async Task CanGetFileContent()
    {
        await using var test = await TestFile.CreateAsync(_network);

        var retrievedContents = await test.Client.GetFileContentAsync(test.Record.File);
        Assert.Equal(test.CreateParams.Contents.ToArray(), retrievedContents.ToArray());
    }
    [Fact(DisplayName = "File Content: Get File Content Requires a Fee")]
    public async Task RequiresAFee()
    {
        await using var test = await TestFile.CreateAsync(_network);

        var txId = test.Client.CreateNewTxId();
        var contents = await test.Client.GetFileContentAsync(test.Record.File, ctx => ctx.Transaction = txId);
        var record = await test.Client.GetTransactionRecordAsync(txId);
        Assert.True(record.Transfers[_network.Payer] < 0);
    }
}