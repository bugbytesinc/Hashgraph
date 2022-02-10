using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract;

[Collection(nameof(NetworkCredentials))]
public class CreateContractTests
{
    private readonly NetworkCredentials _network;
    public CreateContractTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Create Contract: Can Create")]
    public async Task CanCreateAContractAsync()
    {
        await using var fx = await GreetingContract.CreateAsync(_network);
        Assert.NotNull(fx.ContractRecord);
        Assert.NotNull(fx.ContractRecord.Contract);
        Assert.Equal(ResponseCode.Success, fx.ContractRecord.Status);
        Assert.NotEmpty(fx.ContractRecord.Hash.ToArray());
        Assert.NotNull(fx.ContractRecord.Concensus);
        Assert.NotNull(fx.ContractRecord.Memo);
        Assert.InRange(fx.ContractRecord.Fee, 0UL, ulong.MaxValue);
    }
    [Fact(DisplayName = "Create Contract: Can Create with Payer Signature")]
    public async Task CanCreateAContractWithSignatureAsync()
    {
        await using var fxContract = await GreetingContract.CreateAsync(_network, fx =>
        {
            fx.ContractParams.Administrator = _network.PublicKey;
            fx.ContractParams.Signatory = _network.PrivateKey;
        });
        Assert.NotNull(fxContract.ContractRecord);
        Assert.NotNull(fxContract.ContractRecord.Contract);
        Assert.Equal(ResponseCode.Success, fxContract.ContractRecord.Status);
        Assert.NotEmpty(fxContract.ContractRecord.Hash.ToArray());
        Assert.NotNull(fxContract.ContractRecord.Concensus);
        Assert.NotNull(fxContract.ContractRecord.Memo);
        Assert.InRange(fxContract.ContractRecord.Fee, 0UL, ulong.MaxValue);
        Assert.Null(fxContract.ContractRecord.ParentTransactionConcensus);
    }
    [Fact(DisplayName = "Create Contract: Missing Signatory Raises Error")]
    public async Task CreateAContractWithoutSignatoryRaisesErrorAsync()
    {
        var (publicKey, privateKey) = Generator.KeyPair();
        var ex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await GreetingContract.CreateAsync(_network, fx =>
            {
                fx.ContractParams.Administrator = publicKey;
            });
        });
        Assert.StartsWith("Unable to create contract, status: InvalidSignature", ex.Message);
        Assert.Equal(ResponseCode.InvalidSignature, ex.Status);
    }
    [Fact(DisplayName = "Create Contract: Missing File Address Raises Error")]
    public async Task MissingFileAddressRaisesError()
    {
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await GreetingContract.CreateAsync(_network, fx =>
            {
                fx.ContractParams.File = null;
            });
        });
        Assert.StartsWith("The File Address containing the contract is missing, it cannot be null.", ex.Message);
        Assert.Equal("File", ex.ParamName);
    }
    [Fact(DisplayName = "Create Contract: Missing Gas Raises Error")]
    public async Task MissingGasRaisesError()
    {
        var ex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await GreetingContract.CreateAsync(_network, fx =>
            {
                fx.ContractParams.Gas = 0;
            });
        });
        Assert.StartsWith("Unable to create contract, status: InsufficientGas", ex.Message);
        Assert.Equal(ResponseCode.InsufficientGas, ex.Status);
    }
    [Fact(DisplayName = "Create Contract: Sending crypto to non-payable contract raises error.")]
    public async Task SendingCryptoToNonPayableContractRaisesError()
    {
        var ex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await GreetingContract.CreateAsync(_network, fx =>
            {
                fx.ContractParams.InitialBalance = 10;
            });
        });
        Assert.StartsWith("Unable to create contract, status: ContractRevertExecuted", ex.Message);
        Assert.Equal(ResponseCode.ContractRevertExecuted, ex.Status);
    }
    [Fact(DisplayName = "Create Contract: Invalid Renew Period Raises Error")]
    public async Task InvalidRenewPeriodRaisesError()
    {
        var ex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await GreetingContract.CreateAsync(_network, fx =>
            {
                fx.ContractParams.RenewPeriod = TimeSpan.FromTicks(1);
            });
        });
        Assert.StartsWith("Transaction Failed Pre-Check: InvalidRenewalPeriod", ex.Message);
        Assert.Equal(ResponseCode.InvalidRenewalPeriod, ex.Status);
    }
    [Fact(DisplayName = "Create Contract: Can Create Without Admin Key")]
    public async Task CanCreateContractWithoutAdminKey()
    {
        await using var fxContract = await GreetingContract.CreateAsync(_network, fx =>
        {
            fx.ContractParams.Administrator = null;
        });
        Assert.NotNull(fxContract.ContractRecord);
        Assert.NotNull(fxContract.ContractRecord.Contract);
        Assert.Equal(ResponseCode.Success, fxContract.ContractRecord.Status);
        Assert.NotEmpty(fxContract.ContractRecord.Hash.ToArray());
        Assert.NotNull(fxContract.ContractRecord.Concensus);
        Assert.NotNull(fxContract.ContractRecord.Memo);
        Assert.InRange(fxContract.ContractRecord.Fee, 0UL, ulong.MaxValue);

        Assert.Empty(fxContract.ContractRecord.CallResult.Error);
        Assert.False(fxContract.ContractRecord.CallResult.Bloom.IsEmpty);
        Assert.InRange(fxContract.ContractRecord.CallResult.Gas, 0UL, (ulong)fxContract.ContractParams.Gas);
        Assert.Empty(fxContract.ContractRecord.CallResult.Events);
        Assert.Empty(fxContract.ContractRecord.CallResult.StateChanges);
        Assert.Equal(new Moniker(Abi.EncodeArguments(new[] { fxContract.ContractRecord.Contract }).Slice(12)), fxContract.ContractRecord.CallResult.EncodedAddress);
        Assert.NotEqual(0, fxContract.ContractRecord.CallResult.Result.Size);
        Assert.False(fxContract.ContractRecord.CallResult.Result.Data.IsEmpty);
    }
    [Fact(DisplayName = "Create Contract: Random Constructor Data when not needed is ignored.")]
    public async Task CanCreateContractWithUnneededConstructorData()
    {
        await using var fxContract = await GreetingContract.CreateAsync(_network, fx =>
        {
            fx.ContractParams.Arguments = new object[] { "Random Data that Should Be Ignored." };
        });
        Assert.NotNull(fxContract.ContractRecord);
        Assert.NotNull(fxContract.ContractRecord.Contract);
        Assert.Equal(ResponseCode.Success, fxContract.ContractRecord.Status);
        Assert.NotEmpty(fxContract.ContractRecord.Hash.ToArray());
        Assert.NotNull(fxContract.ContractRecord.Concensus);
        Assert.NotNull(fxContract.ContractRecord.Memo);
        Assert.InRange(fxContract.ContractRecord.Fee, 0UL, ulong.MaxValue);

        Assert.Empty(fxContract.ContractRecord.CallResult.Error);
        Assert.False(fxContract.ContractRecord.CallResult.Bloom.IsEmpty);
        Assert.InRange(fxContract.ContractRecord.CallResult.Gas, 0UL, (ulong)fxContract.ContractParams.Gas);
        Assert.Empty(fxContract.ContractRecord.CallResult.Events);
        Assert.Empty(fxContract.ContractRecord.CallResult.StateChanges);
        Assert.Equal(new Moniker(Abi.EncodeArguments(new[] { fxContract.ContractRecord.Contract }).Slice(12)), fxContract.ContractRecord.CallResult.EncodedAddress);
        Assert.NotEqual(0, fxContract.ContractRecord.CallResult.Result.Size);
        Assert.False(fxContract.ContractRecord.CallResult.Result.Data.IsEmpty);
    }
    [Fact(DisplayName = "Create Contract: Can create without returning record.")]
    public async Task CanCreateContractWithoutReturningRecordData()
    {
        await using var fx = await GreetingContract.CreateAsync(_network);
        var receipt = await fx.Client.CreateContractAsync(fx.ContractParams);
        Assert.NotNull(receipt);
        Assert.NotNull(receipt.Contract);
        Assert.Equal(ResponseCode.Success, receipt.Status);
    }
    [Fact(DisplayName = "Create Contract: Can Create Contract with Parameters")]
    public async Task CanCreateAContractWithParameters()
    {
        await using var fx = await StatefulContract.CreateAsync(_network);
        Assert.NotNull(fx.ContractRecord);
        Assert.NotNull(fx.ContractRecord.Contract);
        Assert.Equal(ResponseCode.Success, fx.ContractRecord.Status);
        Assert.NotEmpty(fx.ContractRecord.Hash.ToArray());
        Assert.NotNull(fx.ContractRecord.Concensus);
        Assert.NotNull(fx.ContractRecord.Memo);
        Assert.InRange(fx.ContractRecord.Fee, 0UL, ulong.MaxValue);

        Assert.Empty(fx.ContractRecord.CallResult.Error);
        Assert.False(fx.ContractRecord.CallResult.Bloom.IsEmpty);
        Assert.InRange(fx.ContractRecord.CallResult.Gas, 0UL, (ulong)fx.ContractParams.Gas);
        Assert.Empty(fx.ContractRecord.CallResult.Events);
        Assert.Empty(fx.ContractRecord.CallResult.StateChanges);
        Assert.Equal(new Moniker(Abi.EncodeArguments(new[] { fx.ContractRecord.Contract }).Slice(12)), fx.ContractRecord.CallResult.EncodedAddress);
        Assert.NotEqual(0, fx.ContractRecord.CallResult.Result.Size);
        Assert.False(fx.ContractRecord.CallResult.Result.Data.IsEmpty);
    }

    [Fact(DisplayName = "Create Contract: Missing Construction Parameters that are Required raises Error")]
    public async Task CreateWithoutRequiredContractParamsThrowsError()
    {
        var ex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await StatefulContract.CreateAsync(_network, async fx =>
            {
                fx.ContractParams.Arguments = null;
                await fx.Client.CreateContractAsync(fx.ContractParams);
            });
        });
        Assert.StartsWith("Unable to create contract, status: ContractRevertExecuted", ex.Message);
        Assert.Equal(ResponseCode.ContractRevertExecuted, ex.Status);
    }
    [Fact(DisplayName = "Create Contract: Can Create Payable Contract")]
    public async Task CanCreateAPayableContract()
    {
        await using var fx = await PayableContract.CreateAsync(_network);
        Assert.NotNull(fx.ContractRecord);
        Assert.NotNull(fx.ContractRecord.Contract);
        Assert.Equal(ResponseCode.Success, fx.ContractRecord.Status);
        Assert.NotEmpty(fx.ContractRecord.Hash.ToArray());
        Assert.NotNull(fx.ContractRecord.Concensus);
        Assert.NotNull(fx.ContractRecord.Memo);
        Assert.InRange(fx.ContractRecord.Fee, 0UL, ulong.MaxValue);
    }
    [Fact(DisplayName = "Create Contract: Can Not Schedule Create")]
    public async Task CanNotScheduleCreate()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await GreetingContract.CreateAsync(_network, fx =>
            {
                fx.ContractParams.Signatory = new Signatory(
                    fx.ContractParams.Signatory,
                    new PendingParams { PendingPayer = fxPayer }
                );
            });
        });
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }
}