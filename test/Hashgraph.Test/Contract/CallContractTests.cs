using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract;

[Collection(nameof(NetworkCredentials))]
public class CallContractTests
{
    private readonly NetworkCredentials _network;
    public CallContractTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Call Contract: Can Call Contract with No Arguments")]
    public async Task CanCreateAContractAsync()
    {
        await using var fx = await GreetingContract.CreateAsync(_network);

        var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "greet"
        }, ctx => ctx.Memo = "Call Contract");
        Assert.NotNull(record);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.Equal("Call Contract", record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Empty(record.CallResult.Error);
        Assert.False(record.CallResult.Bloom.IsEmpty);
        Assert.InRange(record.CallResult.GasUsed, 0UL, 30_000UL);
        Assert.Empty(record.CallResult.Events);
        /**
         * HEDERA CHURN: THE FOLLOWING WILL BE ADDED BACK IF/WHEN HAPI SUPPORTS IT.
         * 
         *  Assert.Empty(record.CallResult.StateChanges);
         */
        Assert.Equal(Moniker.None, record.CallResult.EncodedAddress);
        Assert.Equal("Hello, world!", record.CallResult.Result.As<string>()); ;
    }
    [Fact(DisplayName = "Call Contract: Can Call Contract that keeps State")]
    public async Task CanCreateAContractWithStateAsync()
    {
        await using var fx = await StatefulContract.CreateAsync(_network);

        var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_message"
        }, ctx => ctx.Memo = "Call Contract");
        Assert.NotNull(record);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.Equal("Call Contract", record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Empty(record.CallResult.Error);
        Assert.False(record.CallResult.Bloom.IsEmpty);
        Assert.InRange(record.CallResult.GasUsed, 0UL, 30_000UL);
        // NETWORK DEFECT: NOT IMPLEMENED
        Assert.Equal(0, record.CallResult.GasLimit);
        Assert.Equal(0, record.CallResult.PayableAmount);
        Assert.Equal(Address.None, record.CallResult.MessageSender);
        Assert.Empty(record.CallResult.Events);
        Assert.Equal(fx.ContractParams.Arguments[0] as string, record.CallResult.Result.As<string>());
    }
    [Fact(DisplayName = "Call Contract: Can Call Contract that keeps State alternate Signatory")]
    public async Task CanCreateAContractWithStateAlternateSignatoryAsync()
    {
        await using var fx = await StatefulContract.CreateAsync(_network);

        var receipt = await fx.Client.CallContractAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_message",
            Signatory = _network.PrivateKey
        }, ctx => ctx.Signatory = null);
        Assert.NotNull(receipt);
        Assert.Equal(ResponseCode.Success, receipt.Status);
    }
    [Fact(DisplayName = "Call Contract: Can Call Contract that sets State")]
    public async Task CanCreateAContractAndSetStateAsync()
    {
        await using var fx = await StatefulContract.CreateAsync(_network);

        var newMessage = Generator.Code(50);
        var setRecord = await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 40000,
            FunctionName = "set_message",
            FunctionArgs = new object[] { newMessage }
        }, ctx => ctx.Memo = "Call Contract");
        Assert.NotNull(setRecord);
        Assert.Equal(ResponseCode.Success, setRecord.Status);
        Assert.False(setRecord.Hash.IsEmpty);
        Assert.NotNull(setRecord.Concensus);
        Assert.Equal("Call Contract", setRecord.Memo);
        Assert.InRange(setRecord.Fee, 0UL, ulong.MaxValue);
        Assert.Empty(setRecord.CallResult.Error);
        Assert.False(setRecord.CallResult.Bloom.IsEmpty);
        Assert.InRange(setRecord.CallResult.GasUsed, 0UL, 50_000UL);
        // NETWORK DEFECT: NOT IMPLEMENED
        Assert.Equal(0, setRecord.CallResult.GasLimit);
        Assert.Equal(0, setRecord.CallResult.PayableAmount);
        Assert.Equal(Address.None, setRecord.CallResult.MessageSender);
        Assert.Empty(setRecord.CallResult.Events);
        /**
         * HEDERA CHURN: THE FOLLOWING WILL BE ADDED BACK IF/WHEN HAPI SUPPORTS IT.
         * 
         *  Assert.Empty(setRecord.CallResult.StateChanges);
         */
        Assert.Equal(Moniker.None, setRecord.CallResult.EncodedAddress);

        var getRecord = await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_message"
        });
        Assert.NotNull(getRecord);
        Assert.Equal(ResponseCode.Success, getRecord.Status);
        Assert.False(getRecord.Hash.IsEmpty);
        Assert.NotNull(getRecord.Concensus);
        Assert.Empty(getRecord.Memo);
        Assert.InRange(getRecord.Fee, 0UL, ulong.MaxValue);
        Assert.Empty(getRecord.CallResult.Error);
        Assert.False(getRecord.CallResult.Bloom.IsEmpty);
        Assert.Empty(getRecord.CallResult.Events);
        /**
         * HEDERA CHURN: THE FOLLOWING WILL BE ADDED BACK IF/WHEN HAPI SUPPORTS IT.
         * 
         *  Assert.Empty(getRecord.CallResult.StateChanges);
         */
        Assert.Equal(Moniker.None, getRecord.CallResult.EncodedAddress);
        Assert.Equal(newMessage, getRecord.CallResult.Result.As<string>());
        Assert.InRange(getRecord.CallResult.GasUsed, 0UL, 30_000UL);
        // NETWORK DEFECT: NOT IMPLEMENED
        Assert.Equal(0, getRecord.CallResult.GasLimit);
        Assert.Equal(0, getRecord.CallResult.PayableAmount);
        Assert.Equal(Address.None, getRecord.CallResult.MessageSender);
    }
    [Fact(DisplayName = "Call Contract: Can Call Contract that sets State (receipt version)")]
    public async Task CanCreateAContractAndSetStateWithoutRecordAsync()
    {
        await using var fx = await StatefulContract.CreateAsync(_network);

        var newMessage = Generator.Code(50);
        var setRecord = await fx.Client.CallContractAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 40000,
            FunctionName = "set_message",
            FunctionArgs = new object[] { newMessage }
        });
        Assert.NotNull(setRecord);
        Assert.Equal(ResponseCode.Success, setRecord.Status);

        var getRecord = await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_message"
        }, ctx => ctx.Memo = "Call Contract");
        Assert.NotNull(getRecord);
        Assert.Equal(ResponseCode.Success, getRecord.Status);
        Assert.False(getRecord.Hash.IsEmpty);
        Assert.NotNull(getRecord.Concensus);
        Assert.Equal("Call Contract", getRecord.Memo);
        Assert.InRange(getRecord.Fee, 0UL, ulong.MaxValue);
        Assert.Empty(getRecord.CallResult.Error);
        Assert.False(getRecord.CallResult.Bloom.IsEmpty);
        Assert.Empty(getRecord.CallResult.Events);
        /**
         * HEDERA CHURN: THE FOLLOWING WILL BE ADDED BACK IF/WHEN HAPI SUPPORTS IT.
         * 
         *  Assert.Empty(getRecord.CallResult.StateChanges);
         */
        Assert.Equal(Moniker.None, getRecord.CallResult.EncodedAddress);
        Assert.Equal(newMessage, getRecord.CallResult.Result.As<string>());
        Assert.InRange(getRecord.CallResult.GasUsed, 0UL, 30_000UL);
        Assert.Equal(0, getRecord.CallResult.PayableAmount);
        Assert.Equal(Address.None, getRecord.CallResult.MessageSender);
    }
    [Fact(DisplayName = "Call Contract: Calling Deleted Contract Raises Error")]
    public async Task CallingDeletedContractRaisesError()
    {
        await using var fx = await GreetingContract.CreateAsync(_network);
        var deleteReceipt = await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, _network.Payer, fx.PrivateKey);
        Assert.Equal(ResponseCode.Success, deleteReceipt.Status);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 8000,
                FunctionName = "greet",
            });
        });
        Assert.Equal(ResponseCode.ContractDeleted, tex.Status);
        Assert.StartsWith("Contract call failed, status: ContractDeleted", tex.Message);
    }
    [Fact(DisplayName = "Call Contract: Can Not Schedule Call Contract")]
    public async Task CanNotScheduleCallContract()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxContract = await StatefulContract.CreateAsync(_network);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fxContract.ContractRecord.Contract,
                Gas = 4000,
                FunctionName = "get_message",
                Signatory = new PendingParams { PendingPayer = fxPayer }
            });
        });
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }
}