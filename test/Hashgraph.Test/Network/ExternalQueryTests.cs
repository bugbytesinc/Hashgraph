using Google.Protobuf;
using Hashgraph.Test.Fixtures;
using Proto;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.System;

[Collection(nameof(NetworkCredentials))]
public class ExternalQueryTests
{
    private readonly NetworkCredentials _network;
    public ExternalQueryTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "External Query: Can Get Receipt Via External Query")]
    public async Task CanGetReceiptViaExternalQuery()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);

        var query = new Query
        {
            TransactionGetReceipt = new TransactionGetReceiptQuery
            {
                TransactionID = new TransactionID(fxAccount.Record.Id)
            }
        };

        var result = await fxAccount.Client.QueryExternalAsync(query.ToByteArray());
        Assert.False(result.IsEmpty);

        var response = Response.Parser.ParseFrom(result.Span);
        Assert.NotNull(response);

        var receipt = response.TransactionGetReceipt.Receipt;
        Assert.NotNull(receipt);

        Assert.Equal(fxAccount.Record.Address, receipt.AccountID.AsAddress());
    }
    [Fact(DisplayName = "External Query: Can Get Account Info Via External Query")]
    public async Task CanGetAccountInfoViaExternalQuery()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);

        var query = new Query
        {
            CryptoGetInfo = new CryptoGetInfoQuery
            {
                AccountID = new AccountID(fxAccount.Record.Address)
            }
        };

        var result = await fxAccount.Client.QueryExternalAsync(query.ToByteArray());
        Assert.False(result.IsEmpty);

        var response = Response.Parser.ParseFrom(result.Span);
        Assert.NotNull(response);

        var info = response.CryptoGetInfo.AccountInfo;
        Assert.NotNull(info);

        Assert.Equal(fxAccount.Record.Address, info.AccountID.AsAddress());
    }
    [Fact(DisplayName = "External Query: Invalid Query Still Produces Result")]
    public async Task InvalidQueryStillProducesResult()
    {
        await using var client = _network.NewClient();

        var query = new Query
        {
            CryptoGetInfo = new CryptoGetInfoQuery
            {
                AccountID = new AccountID(Address.None)
            }
        };

        var result = await client.QueryExternalAsync(query.ToByteArray());
        Assert.False(result.IsEmpty);

        var response = Response.Parser.ParseFrom(result.Span);
        Assert.NotNull(response);
        Assert.Equal(ResponseCodeEnum.InvalidAccountId, response.CryptoGetInfo.Header.NodeTransactionPrecheckCode);
        Assert.Null(response.CryptoGetInfo.AccountInfo);        
    }
    [Fact(DisplayName = "External Query: Invalid Receipt Request Still Produces Result")]
    public async Task InvalidReceiptRequestStillProducesResult()
    {
        await using var client = _network.NewClient();

        var query = new Query
        {
            TransactionGetReceipt = new TransactionGetReceiptQuery()
        };

        var result = await client.QueryExternalAsync(query.ToByteArray());
        Assert.False(result.IsEmpty);

        var response = Response.Parser.ParseFrom(result.Span);
        Assert.NotNull(response);
        Assert.Null(response.TransactionGetReceipt.Receipt);   
        Assert.Equal(ResponseCodeEnum.InvalidTransactionId,response.ResponseHeader.NodeTransactionPrecheckCode);
    }
}