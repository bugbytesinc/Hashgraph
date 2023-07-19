using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract;

[Collection(nameof(NetworkCredentials))]
public class PayableContractTests
{
    private readonly NetworkCredentials _network;
    public PayableContractTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Payable Contract: Can Get Contract Balance from Call")]
    public async Task CanGetContractBalanceFromCall()
    {
        await using var fx = await PayableContract.CreateAsync(_network);

        var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_balance"
        }, ctx => ctx.Memo = ".NET SDK Test");
        Assert.NotNull(record);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.Equal(".NET SDK Test", record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(fx.ContractRecord.Contract, record.CallResult.Contract);
        Assert.Empty(record.CallResult.Error);
        Assert.False(record.CallResult.Bloom.IsEmpty);
        Assert.InRange(record.CallResult.GasUsed, 0UL, 30_000UL);
        // NETWORK DEFECT: NOT IMPLEMENED
        Assert.Equal(0, record.CallResult.GasLimit);
        Assert.Equal(0, record.CallResult.PayableAmount);
        Assert.Equal(Address.None, record.CallResult.MessageSender);
        Assert.Empty(record.CallResult.Events);
        /**
         * HEDERA CHURN: THE FOLLOWING WILL BE ADDED BACK IF/WHEN HAPI SUPPORTS IT.
         * 
         *  Assert.Empty(record.CallResult.StateChanges);
         */
        Assert.Equal(Moniker.None, record.CallResult.EncodedAddress);
        Assert.Equal(fx.ContractParams.InitialBalance, record.CallResult.Result.As<long>());
        Assert.Equal(0, record.CallResult.FunctionArgs.Size);
        /// UM, is this correct?
        Assert.Empty(record.CallResult.Nonces);

        // Ensure matches API vesion.
        var apiBalance = await fx.Client.GetContractBalanceAsync(fx.ContractRecord.Contract);
        Assert.Equal((ulong)fx.ContractParams.InitialBalance, apiBalance);
    }
    [Fact(DisplayName = "Payable Contract: Can Get Contract Balance from Call (Local Call)")]
    public async Task CanGetContractBalanceFromLocalCall()
    {
        await using var fx = await PayableContract.CreateAsync(_network);

        var result = await fx.Client.QueryContractAsync(new QueryContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_balance",
        });
        Assert.NotNull(result);
        Assert.Empty(result.Error);
        Assert.False(result.Bloom.IsEmpty);
        Assert.InRange(result.GasUsed, 0UL, 30_000UL);
        // NETWORK DEFECT: NOT IMPLEMENED
        Assert.Equal(0, result.GasLimit);
        Assert.Equal(0, result.PayableAmount);
        Assert.Equal(Address.None, result.MessageSender);
        Assert.Empty(result.Events);
        Assert.Equal(fx.ContractParams.InitialBalance, result.Result.As<long>());
        Assert.Equal(0, result.FunctionArgs.Size);
        Assert.Empty(result.Nonces);
        /**
         * HEDERA CHURN: THE FOLLOWING WILL BE ADDED BACK IF/WHEN HAPI SUPPORTS IT.
         * 
         *  Assert.Empty(result.StateChanges);
         */
        Assert.Equal(Moniker.None, result.EncodedAddress);

        // Ensure matches API vesion.
        var apiBalance = await fx.Client.GetContractBalanceAsync(fx.ContractRecord.Contract);
        Assert.Equal((ulong)fx.ContractParams.InitialBalance, apiBalance);

        // Ensure matches Info version
        var info = await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
        Assert.Equal((ulong)fx.ContractParams.InitialBalance, info.Balance);
    }
    [Fact(DisplayName = "Payable Contract: Can Call Contract that Sends Funds")]
    async Task CanCallContractMethodSendingFunds()
    {
        await using var fx = await PayableContract.CreateAsync(_network);
        await using var fx2 = await TestAccount.CreateAsync(_network);

        var infoBefore = await fx2.Client.GetAccountInfoAsync(fx2.Record.Address);
        var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 40000,
            FunctionName = "send_to",
            FunctionArgs = new[] { fx2.Record.Address }
        });
        Assert.NotNull(record);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Empty(record.CallResult.Error);
        Assert.False(record.CallResult.Bloom.IsEmpty);
        Assert.InRange(record.CallResult.GasUsed, 0UL, 40_000UL);
        // NETWORK DEFECT: NOT IMPLEMENED
        Assert.Equal(0, record.CallResult.GasLimit);
        Assert.Empty(record.CallResult.Events);
        /**
         * HEDERA CHURN: THE FOLLOWING WILL BE ADDED BACK IF/WHEN HAPI SUPPORTS IT.
         * 
         *  Assert.Empty(record.CallResult.StateChanges);
         */
        Assert.Equal(Moniker.None, record.CallResult.EncodedAddress);

        var infoAfter = await fx2.Client.GetAccountInfoAsync(fx2.Record.Address);
        Assert.Equal((ulong)fx.ContractParams.InitialBalance, infoAfter.Balance - infoBefore.Balance);
    }
    [Fact(DisplayName = "Payable Contract: Can Send Funds to External Payable Default Function")]
    async Task CanSendFundsToPayableContractWithExternalPayable()
    {
        await using var fx = await PayableContract.CreateAsync(_network);

        var extraFunds = Generator.Integer(500, 1000);
        var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 30000,
            PayableAmount = extraFunds
        });
        Assert.Equal(ResponseCode.Success, record.Status);

        await using var fx2 = await TestAccount.CreateAsync(_network);
        var infoBefore = await fx2.Client.GetAccountInfoAsync(fx2.Record.Address);
        record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 40000,
            FunctionName = "send_to",
            FunctionArgs = new[] { fx2.Record.Address }
        }, ctx => ctx.Memo = ".NET SDK Test");
        Assert.NotNull(record);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.Equal(".NET SDK Test", record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Empty(record.CallResult.Error);
        Assert.False(record.CallResult.Bloom.IsEmpty);
        Assert.InRange(record.CallResult.GasUsed, 0UL, 40_000UL);
        // NETWORK DEFECT: NOT IMPLEMENED
        Assert.Equal(0, record.CallResult.GasLimit);
        Assert.Equal(0, record.CallResult.PayableAmount);
        Assert.Equal(Address.None, record.CallResult.MessageSender);
        Assert.Empty(record.CallResult.Events);
        /**
         * HEDERA CHURN: THE FOLLOWING WILL BE ADDED BACK IF/WHEN HAPI SUPPORTS IT.
         * 
         *  Assert.Empty(record.CallResult.StateChanges);
         */
        Assert.Equal(Moniker.None, record.CallResult.EncodedAddress);

        var infoAfter = await fx2.Client.GetAccountInfoAsync(fx2.Record.Address);
        Assert.Equal((ulong)(fx.ContractParams.InitialBalance + extraFunds), infoAfter.Balance - infoBefore.Balance);
    }
    [Fact(DisplayName = "Payable Contract: Can Call Contract that Sends Funds to Deleted Account Raises Error")]
    public async Task SendFundsToDeletedAccountRaisesError()
    {
        // Setup the Simple Payable Contract and An account for "send to".
        await using var fxContract = await PayableContract.CreateAsync(_network);
        await using var fxAccount = await TestAccount.CreateAsync(_network);

        // Get the Info for the account state and then delete the account.
        var infoBefore = await fxAccount.Client.GetAccountInfoAsync(fxAccount.Record.Address);
        var deleteReceipt = await fxAccount.Client.DeleteAccountAsync(fxAccount.Record.Address, fxAccount.Network.Payer, fxAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, deleteReceipt.Status);

        // Double check the balance on the contract, confirm it has hbars
        var contractBalanceBefore = await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fxContract.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_balance"
        });
        Assert.NotNull(contractBalanceBefore);
        Assert.InRange(fxContract.ContractParams.InitialBalance, 1, int.MaxValue);
        Assert.Equal(fxContract.ContractParams.InitialBalance, contractBalanceBefore.CallResult.Result.As<long>());

        // Ensure matches API vesion.
        var apiBalance = await fxContract.Client.GetContractBalanceAsync(fxContract.ContractRecord.Contract);
        Assert.Equal((ulong)fxContract.ContractParams.InitialBalance, apiBalance);

        // Ensure matches Info version
        var info = await fxContract.Client.GetContractInfoAsync(fxContract.ContractRecord.Contract);
        Assert.Equal((ulong)fxContract.ContractParams.InitialBalance, info.Balance);

        // Call the contract, sending to the address of the now deleted account
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fxContract.ContractRecord.Contract,
                Gas = 30000,
                FunctionName = "send_to",
                FunctionArgs = new[] { fxAccount.Record.Address }
            });
        });
        Assert.Equal(ResponseCode.InvalidSolidityAddress, tex.Status);
        Assert.StartsWith("Contract call failed, status: InvalidSolidityAddress", tex.Message);

        // Confirm that the balance on the contract remained unchanged.
        var contractBalanceAfter = await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fxContract.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_balance"
        });
        Assert.NotNull(contractBalanceAfter);
        Assert.Equal(fxContract.ContractParams.InitialBalance, contractBalanceAfter.CallResult.Result.As<long>());

        // Ensure matches API vesion.
        apiBalance = await fxContract.Client.GetContractBalanceAsync(fxContract.ContractRecord.Contract);
        Assert.Equal((ulong)fxContract.ContractParams.InitialBalance, apiBalance);

        // Ensure matches Info version
        info = await fxContract.Client.GetContractInfoAsync(fxContract.ContractRecord.Contract);
        Assert.Equal((ulong)fxContract.ContractParams.InitialBalance, info.Balance);

    }
    [Fact(DisplayName = "Payable Contract: Can Call Contract that Sends Funds to Non Existent Account Raises Error")]
    public async Task SendFundsToInvalidAccountRaisesError()
    {
        await using var fx = await PayableContract.CreateAsync(_network);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 40000,
                FunctionName = "send_to",
                FunctionArgs = new[] { new Address(0, 0, long.MaxValue) }
            });
        });
        Assert.Equal(ResponseCode.InvalidSolidityAddress, tex.Status);
        Assert.StartsWith("Contract call failed, status: InvalidSolidityAddress", tex.Message);
    }
    [Fact(DisplayName = "Payable Contract: Attempts to Misplace hBars Should Fail")]
    public async Task AttemptsToMisplaceHbarsThruPayableContractShouldFail()
    {
        // Setup the Simple Payable Contract and An account for "send to".
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxContract = await PayableContract.CreateAsync(_network);

        // Get the Info for the account state and then delete the account.
        var info1Before = await fxAccount1.Client.GetAccountInfoAsync(fxAccount1.Record.Address);
        var info2Before = await fxAccount2.Client.GetAccountInfoAsync(fxAccount2.Record.Address);
        var delete1Receipt = await fxAccount1.Client.DeleteAccountAsync(fxAccount1.Record.Address, fxAccount1.Network.Payer, fxAccount1.PrivateKey);
        Assert.Equal(ResponseCode.Success, delete1Receipt.Status);

        // Double check the balance on the contract, confirm it has hbars
        var contractBalanceBefore = await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fxContract.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_balance"
        });
        Assert.NotNull(contractBalanceBefore);
        Assert.InRange(fxContract.ContractParams.InitialBalance, 1, int.MaxValue);
        Assert.Equal(fxContract.ContractParams.InitialBalance, contractBalanceBefore.CallResult.Result.As<long>());

        // Ensure matches API vesion.
        var apiBalance = await fxContract.Client.GetContractBalanceAsync(fxContract.ContractRecord.Contract);
        Assert.Equal((ulong)fxContract.ContractParams.InitialBalance, apiBalance);

        // Call the contract, sending to the address of the now deleted account
        // Call the contract, sending to the address of the now deleted account
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fxContract.ContractRecord.Contract,
                Gas = 30000,
                FunctionName = "send_to",
                FunctionArgs = new[] { fxAccount1.Record.Address }
            });
        });
        Assert.Equal(ResponseCode.InvalidSolidityAddress, tex.Status);
        Assert.StartsWith("Contract call failed, status: InvalidSolidityAddress", tex.Message);

        // Confirm that the balance on the contract did not change.
        var contractBalanceAfter = await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fxContract.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_balance"
        });
        Assert.NotNull(contractBalanceAfter);
        Assert.Equal(fxContract.ContractParams.InitialBalance, contractBalanceAfter.CallResult.Result.As<long>());

        // Ensure matches API vesion.
        apiBalance = await fxContract.Client.GetContractBalanceAsync(fxContract.ContractRecord.Contract);
        Assert.Equal((ulong)fxContract.ContractParams.InitialBalance, apiBalance);

        // Try to get info on the deleted account, but this will fail because the
        // account is already deleted.
        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            // So if this throws an error, why did the above call not fail?
            await fxAccount1.Client.GetAccountInfoAsync(fxAccount1.Record.Address);
        });

        // Delete the Contract, returning any hidden hbars to account number 2
        var deleteContractRecord = await fxContract.Client.DeleteContractAsync(fxContract.ContractRecord.Contract, fxAccount2.Record.Address, fxContract.PrivateKey);
        Assert.Equal(ResponseCode.Success, deleteContractRecord.Status);

        // Check the balance of account number 2, the hBars should be there?
        var info2After = await fxAccount2.Client.GetAccountInfoAsync(fxAccount2.Record.Address);
        Assert.Equal((ulong)fxContract.ContractParams.InitialBalance + info2Before.Balance, info2After.Balance); // NOPE!
    }
    [Fact(DisplayName = "Payable Contract: Can Send Funds to External Payable Default Function Raises Contract's Account Balance")]
    public async Task SendFundsToPayableContractWithExternalPayableRaisesContractBalance()
    {
        await using var fx = await PayableContract.CreateAsync(_network);

        ulong initialBalance = (ulong)fx.ContractParams.InitialBalance;
        var apiBalanceBefore = await fx.Client.GetContractBalanceAsync(fx.ContractRecord.Contract);
        var infoBalanceBefore = (await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract)).Balance;
        var callBalanceBefore = (ulong)(await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_balance"
        })).CallResult.Result.As<long>();
        Assert.Equal(initialBalance, apiBalanceBefore);
        Assert.Equal(initialBalance, infoBalanceBefore);
        Assert.Equal(initialBalance, callBalanceBefore);

        var extraFunds = Generator.Integer(500, 1000);
        var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 30000,
            PayableAmount = extraFunds
        });
        Assert.Equal(ResponseCode.Success, record.Status);

        ulong finalBalance = (ulong)fx.ContractParams.InitialBalance + (ulong)extraFunds;
        var apiBalanceAfter = await fx.Client.GetContractBalanceAsync(fx.ContractRecord.Contract);
        var infoBalanceAfter = (await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract)).Balance;
        var callBalanceAfter = (ulong)(await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_balance"
        })).CallResult.Result.As<long>();
        Assert.Equal(finalBalance, apiBalanceAfter);
        Assert.Equal(finalBalance, infoBalanceAfter);
        Assert.Equal(finalBalance, callBalanceAfter);
    }

    [Fact(DisplayName = "Payable Contract: Transfer Funds to External Payable Default Function Raises Contract's Account Balance")]
    async Task TransferFundsToPayableContractWithExternalPayableRaisesContractBalance()
    {
        await using var fx = await PayableContract.CreateAsync(_network);

        ulong initialBalance = (ulong)fx.ContractParams.InitialBalance;
        var apiBalanceBefore = await fx.Client.GetContractBalanceAsync(fx.ContractRecord.Contract);
        var infoBalanceBefore = (await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract)).Balance;
        var callBalanceBefore = (ulong)(await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_balance"
        })).CallResult.Result.As<long>();
        Assert.Equal(initialBalance, apiBalanceBefore);
        Assert.Equal(initialBalance, infoBalanceBefore);
        Assert.Equal(initialBalance, callBalanceBefore);

        var extraFunds = Generator.Integer(500, 1000);
        var record = await fx.Client.TransferAsync(_network.Payer, fx.ContractRecord.Contract, extraFunds);
        Assert.Equal(ResponseCode.Success, record.Status);

        ulong finalBalance = (ulong)fx.ContractParams.InitialBalance + (ulong)extraFunds;
        var apiBalanceAfter = await fx.Client.GetContractBalanceAsync(fx.ContractRecord.Contract);
        var infoBalanceAfter = (await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract)).Balance;
        var callBalanceAfter = (ulong)(await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_balance"
        })).CallResult.Result.As<long>();
        Assert.Equal(finalBalance, apiBalanceAfter);
        Assert.Equal(finalBalance, infoBalanceAfter);
        Assert.Equal(finalBalance, callBalanceAfter);
    }
}