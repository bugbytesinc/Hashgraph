using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class GetPsudoRandomNumberTests
{
    private readonly NetworkCredentials _network;
    public GetPsudoRandomNumberTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }

    [Fact(DisplayName = "Psudo Random Number: Can Get Bounded Number")]
    public async Task CanGetBoundedNumber()
    {
        await using var client = _network.NewClient();
        var maxValue = Generator.Integer(1, 20);
        var receipt = await client.GetPsudoRandomNumberAsync(maxValue);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var record = await client.GetTransactionRecordAsync(receipt.Id);
        var psudo = record as RangedPsudoRandomNumberRecord;
        Assert.NotNull(psudo);
        Assert.True(maxValue >= psudo.Number);
    }

    [Fact(DisplayName = "Psudo Random Number: Can Get Bounded Number With Record")]
    public async Task CanGetBoundedNumberWithRecord()
    {
        await using var client = _network.NewClient();
        var maxValue = Generator.Integer(1, 20);
        var record = await client.GetPsudoRandomNumberWithRecordAsync(maxValue);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.True(maxValue >= record.Number);
    }

    [Fact(DisplayName = "Psudo Random Number: Can Get Unbounded Number")]
    public async Task CanGetUnboundedNumber()
    {
        await using var client = _network.NewClient();
        var receipt = await client.GetPsudoRandomNumberAsync();
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var record = await client.GetTransactionRecordAsync(receipt.Id);
        var psudo = record as BytesPsudoRandomNumberRecord;
        Assert.NotNull(psudo);
        Assert.False(psudo.Bytes.IsEmpty);
    }

    [Fact(DisplayName = "Psudo Random Number: Can Get Unbounded Number With Record")]
    public async Task CanGetUnboundedNumberWithRecord()
    {
        await using var client = _network.NewClient();
        var record = await client.GetPsudoRandomNumberWithRecordAsync();
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Bytes.IsEmpty);
    }

    [Fact(DisplayName = "Psudo Random Number: Invalid Bounds Raises Error")]
    public async Task InvalidBoundsRaisesError()
    {
        await using var client = _network.NewClient();
        var maxValue = -Generator.Integer(1, 20);
        var ae = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await client.GetPsudoRandomNumberAsync(maxValue);
        });
        Assert.Equal("maxValue", ae.ParamName);
        Assert.StartsWith("If specified, the maximum random value must be greater than zero.", ae.Message);
    }
}