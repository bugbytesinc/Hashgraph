namespace Hashgraph.Test.Contract;

[Collection(nameof(NetworkCredentials))]
public class EventEmittingContractTests
{
    private readonly NetworkCredentials _network;
    public EventEmittingContractTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Event Emitting Contract: Can Get Contract Balance from Call")]
    public async Task CanGetContractBalanceFromCall()
    {
        await using var fx = await EventEmittingContract.CreateAsync(_network);

        var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 40000,
            FunctionName = "get_balance"
        });
        Assert.NotNull(record);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(fx.ContractRecord.Contract, record.CallResult.Contract);
        Assert.Empty(record.CallResult.Error);
        Assert.False(record.CallResult.Bloom.IsEmpty);
        Assert.InRange(record.CallResult.GasUsed, 0UL, 40_000UL);
        // NETWORK DEFECT: NOT IMPLEMENED
        Assert.Equal(0, record.CallResult.GasLimit);
        Assert.Equal(0, record.CallResult.PayableAmount);
        Assert.Equal(Address.None, record.CallResult.MessageSender);
        Assert.Empty(record.CallResult.Events);
        Assert.NotEmpty(fx.ContractRecord.CallResult.Nonces);
        /**
         * This looks like a bug in the hedera EVM implementation?
         */
        Assert.Empty(record.CallResult.Nonces);
        /**
         * HEDERA CHURN: THE FOLLOWING WILL BE ADDED BACK IF/WHEN HAPI SUPPORTS IT.
         * 
         *  Assert.Empty(record.CallResult.StateChanges);
         */
        Assert.Equal(Moniker.None, record.CallResult.EncodedAddress);
        Assert.Equal(fx.ContractParams.InitialBalance, record.CallResult.Result.As<long>());
        Assert.Equal(0, record.CallResult.FunctionArgs.Size);
    }
    [Fact(DisplayName = "Event Emitting Contract: Can Get Contract Balance from Call (Local Call)")]
    public async Task CanGetContractBalanceFromLocalCall()
    {
        await using var fx = await EventEmittingContract.CreateAsync(_network);

        var result = await fx.Client.QueryContractAsync(new QueryContractParams
        {
            Contract = fx.ContractRecord.Contract,
            Gas = 40000,
            FunctionName = "get_balance"
        });

        Assert.NotNull(result);
        Assert.Empty(result.Error);
        Assert.False(result.Bloom.IsEmpty);
        Assert.InRange(result.GasUsed, 0UL, 40000UL);
        // NETWORK DEFECT: NOT IMPLEMENED
        Assert.Equal(0, result.GasLimit);
        Assert.Equal(0, result.PayableAmount);
        Assert.Equal(Address.None, result.MessageSender);
        Assert.Empty(result.Events);
        /**
         * HEDERA CHURN: THE FOLLOWING WILL BE ADDED BACK IF/WHEN HAPI SUPPORTS IT.
         * 
         *  Assert.Empty(result.StateChanges);
         */
        Assert.Equal(Moniker.None, result.EncodedAddress);
        Assert.Equal(fx.ContractParams.InitialBalance, result.Result.As<long>());
        Assert.Equal(0, result.FunctionArgs.Size);
        Assert.Empty(result.Nonces);
    }
    [Fact(DisplayName = "Event Emitting Contract: Can Call Contract that Sends Funds, Emitting Event")]
    async Task CanCallContractMethodSendingFunds()
    {
        await using var fx = await EventEmittingContract.CreateAsync(_network);
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
        Assert.InRange(record.CallResult.GasUsed, 0UL, 300_000UL);
        // NETWORK DEFECT: NOT IMPLEMENED
        Assert.Equal(0, record.CallResult.GasLimit);
        Assert.Equal(0, record.CallResult.PayableAmount);
        Assert.Equal(Address.None, record.CallResult.MessageSender);
        Assert.Single(record.CallResult.Events);
        /**
         * HEDERA CHURN: THE FOLLOWING WILL BE ADDED BACK IF/WHEN HAPI SUPPORTS IT.
         * 
         *  Assert.Empty(record.CallResult.StateChanges);
         */
        Assert.Equal(Moniker.None, record.CallResult.EncodedAddress);

        // Now check the emitted Event
        var result = record.CallResult.Events[0];
        Assert.Equal(fx.ContractRecord.Contract, result.Contract);
        Assert.False(result.Bloom.IsEmpty);
        Assert.Single(result.Topic);
        Assert.Equal("9277a4302be4a765ae8585e09a9306bd55da10e20e59ed4f611a04ba606fece8", Hex.FromBytes(result.Topic[0]));

        /**
         * HEDERA CHURN: THE FOLLOWING WILL BE ADDED BACK IF/WHEN HAPI SUPPORTS IT.
         * 
         *  Assert.Empty(record.CallResult.StateChanges);
         */
        Assert.Equal(Moniker.None, record.CallResult.EncodedAddress);

        var (address, amount) = result.Data.As<Address, long>();
        Assert.Equal(fx2.Record.Address, address);
        Assert.Equal(fx.ContractParams.InitialBalance, amount);

        // Alternate Way
        var objects = result.Data.GetAll(typeof(Address), typeof(long));
        Assert.Equal(fx2.Record.Address, objects[0]);
        Assert.Equal(fx.ContractParams.InitialBalance, objects[1]);

        var infoAfter = await fx2.Client.GetAccountInfoAsync(fx2.Record.Address);
        Assert.Equal((ulong)fx.ContractParams.InitialBalance, infoAfter.Balance - infoBefore.Balance);
    }
    [Fact(DisplayName = "Event Emitting Contract: Attempts to Misplace Hbars Fails")]
    public async Task AttemptToSendHbarsToDeletedAccountFails()
    {
        // Setup the Simple Event Emitting Contract and An account for "send to".
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxContract = await EventEmittingContract.CreateAsync(_network);

        // Get the Info for the account state and then delete the account.
        var info1Before = await fxAccount1.Client.GetAccountInfoAsync(fxAccount1.Record.Address);
        var info2Before = await fxAccount2.Client.GetAccountInfoAsync(fxAccount2.Record.Address);
        var delete1Receipt = await fxAccount1.Client.DeleteAccountAsync(fxAccount1.Record.Address, fxAccount1.Network.Payer, fxAccount1.PrivateKey);
        Assert.Equal(ResponseCode.Success, delete1Receipt.Status);

        // Confirm deleted account by trying to get info on the deleted account, 
        // this will throw an exception.
        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fxAccount1.Client.GetAccountInfoAsync(fxAccount1.Record.Address);
        });
        Assert.Equal(ResponseCode.AccountDeleted, pex.Status);

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

        // Confirm that the balance on the contract has not changed.
        var contractBalanceAfter = await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
        {
            Contract = fxContract.ContractRecord.Contract,
            Gas = 30000,
            FunctionName = "get_balance"
        });
        Assert.NotNull(contractBalanceAfter);
        Assert.Equal(fxContract.ContractParams.InitialBalance, contractBalanceAfter.CallResult.Result.As<long>());

        // Double Check: try to get info on the deleted account, 
        // but this will fail because the account is already deleted.
        pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            // So if this throws an error, why did the above transfer not fail?
            await fxAccount1.Client.GetAccountInfoAsync(fxAccount1.Record.Address);
        });
        Assert.Equal(ResponseCode.AccountDeleted, pex.Status);

        // Delete the Contract, returning any hidden hbars to account number 2
        var deleteContractRecord = await fxContract.Client.DeleteContractAsync(fxContract.ContractRecord.Contract, fxAccount2.Record.Address, fxContract.PrivateKey);
        Assert.Equal(ResponseCode.Success, deleteContractRecord.Status);

        // Check the balance of account number 2, the hBars should be there.
        var info2After = await fxAccount2.Client.GetAccountInfoAsync(fxAccount2.Record.Address);
        Assert.Equal((ulong)fxContract.ContractParams.InitialBalance + info2Before.Balance, info2After.Balance);
    }
}