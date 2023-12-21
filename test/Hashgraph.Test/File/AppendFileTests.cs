namespace Hashgraph.Test.File;

[Collection(nameof(NetworkCredentials))]
public class AppendFileContentTests
{
    private readonly NetworkCredentials _network;
    public AppendFileContentTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "File Append: Can Append File Content")]
    public async Task CanAppendToFile()
    {
        await using var test = await TestFile.CreateAsync(_network);

        var appendedContent = Encoding.Unicode.GetBytes(Generator.Code(50));
        var concatinatedContent = test.CreateParams.Contents.ToArray().Concat(appendedContent).ToArray();

        var appendRecord = await test.Client.AppendFileWithRecordAsync(new AppendFileParams
        {
            File = test.Record.File,
            Contents = appendedContent,
            Signatory = test.CreateParams.Signatory
        });
        Assert.Equal(ResponseCode.Success, appendRecord.Status);

        var newContent = await test.Client.GetFileContentAsync(test.Record.File);
        Assert.Equal(concatinatedContent.ToArray(), newContent.ToArray());
    }
    [Fact(DisplayName = "File Append: Can Append File Content with extra Signature")]
    public async Task CanAppendToFileHavingExtraSignature()
    {
        await using var test = await TestFile.CreateAsync(_network);

        var (publicKey, privateKey) = Generator.KeyPair();

        await test.Client.UpdateFileAsync(new UpdateFileParams
        {
            File = test.Record.File,
            Endorsements = new[] { new Endorsement(publicKey) },
            Signatory = new Signatory(privateKey, test.CreateParams.Signatory)
        });

        var appendedContent = Encoding.Unicode.GetBytes(Generator.Code(50));
        var concatinatedContent = test.CreateParams.Contents.ToArray().Concat(appendedContent).ToArray();

        var appendRecord = await test.Client.AppendFileWithRecordAsync(new AppendFileParams
        {
            File = test.Record.File,
            Contents = appendedContent,
            Signatory = privateKey
        });
        Assert.Equal(ResponseCode.Success, appendRecord.Status);

        var newContent = await test.Client.GetFileContentAsync(test.Record.File);
        Assert.Equal(concatinatedContent.ToArray(), newContent.ToArray());
    }
    [Fact(DisplayName = "File Append: Append to Deleted File Throws Exception")]
    public async Task AppendingToDeletedFileThrowsError()
    {
        await using var test = await TestFile.CreateAsync(_network);
        var appendedContent = Encoding.Unicode.GetBytes(Generator.Code(50));

        var deleteRecord = await test.Client.DeleteFileAsync(test.Record.File, test.CreateParams.Signatory);
        Assert.Equal(ResponseCode.Success, deleteRecord.Status);

        var exception = await Assert.ThrowsAnyAsync<TransactionException>(async () =>
        {
            await test.Client.AppendFileAsync(new AppendFileParams
            {
                File = test.Record.File,
                Contents = appendedContent,
                Signatory = test.CreateParams.Signatory
            });
        });
        Assert.StartsWith("Unable to append to file, status: FileDeleted", exception.Message);
    }
    [Fact(DisplayName = "File Append: Can Not Schedule File Append")]
    public async Task CanNotScheduleFileAppend()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxFile = await TestFile.CreateAsync(_network);
        var appendedContent = Encoding.Unicode.GetBytes(Generator.Code(50));

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxFile.Client.AppendFileAsync(new AppendFileParams
            {
                File = fxFile.Record.File,
                Contents = appendedContent,
                Signatory = new Signatory(
                    fxFile.CreateParams.Signatory,
                    new PendingParams { PendingPayer = fxPayer }
                )
            });
        });
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }
}