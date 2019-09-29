using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
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
                Gas = await _network.TinybarsFromGas(400),
                FunctionName = "get_balance"
            });
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Call Contract", record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(fx.ContractRecord.Contract, record.Contract);
            Assert.Empty(record.CallResult.Error);
            Assert.True(record.CallResult.Bloom.IsEmpty);
            Assert.InRange(record.CallResult.Gas, 0UL, 30_000UL);
            Assert.Empty(record.CallResult.Events);
            Assert.Equal(fx.ContractParams.InitialBalance, record.CallResult.Result.As<long>());
        }
        [Fact(DisplayName = "Payable Contract: Can Get Contract Balance from Call (Local Call)")]
        public async Task CanGetContractBalanceFromLocalCall()
        {
            await using var fx = await PayableContract.CreateAsync(_network);

            var result = await fx.Client.QueryContractAsync(new QueryContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = await _network.TinybarsFromGas(400),
                FunctionName = "get_balance",                
            });
            Assert.NotNull(result);
            Assert.Empty(result.Error);
            Assert.True(result.Bloom.IsEmpty);
            Assert.InRange(result.Gas, 0UL, 30_000UL);
            Assert.Empty(result.Events);
            Assert.Equal(fx.ContractParams.InitialBalance, result.Result.As<long>());
        }
        [Fact(DisplayName = "Payable Contract: Can Call Contract that Sends Funds")]
        public async Task CanCallContractMethodSendingFunds()
        {
            await using var fx = await PayableContract.CreateAsync(_network);
            await using var fx2 = await TestAccount.CreateAsync(_network);

            var infoBefore = await fx2.Client.GetAccountInfoAsync(fx2.Record.Address);
            var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = await _network.TinybarsFromGas(400),
                FunctionName = "send_to",
                FunctionArgs = new[] { fx2.Record.Address }
            });
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Call Contract", record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(fx.ContractRecord.Contract, record.Contract);
            Assert.Empty(record.CallResult.Error);
            Assert.True(record.CallResult.Bloom.IsEmpty);
            Assert.InRange(record.CallResult.Gas, 0UL, 30_000UL);
            Assert.Empty(record.CallResult.Events);

            var infoAfter = await fx2.Client.GetAccountInfoAsync(fx2.Record.Address);
            Assert.Equal((ulong)fx.ContractParams.InitialBalance, infoAfter.Balance - infoBefore.Balance);
        }

        [Fact(DisplayName = "Payable Contract: Can Send Funds to External Payable Default Function")]
        public async Task CanSendFundsToPayableContractWithExternalPayable()
        {
            await using var fx = await PayableContract.CreateAsync(_network);

            var extraFunds = Generator.Integer(500, 1000);
            var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = await _network.TinybarsFromGas(400),
                PayableAmount = extraFunds
            });
            Assert.Equal(ResponseCode.Success, record.Status);

            await using var fx2 = await TestAccount.CreateAsync(_network);
            var infoBefore = await fx2.Client.GetAccountInfoAsync(fx2.Record.Address);
            record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = await _network.TinybarsFromGas(400),
                FunctionName = "send_to",
                FunctionArgs = new[] { fx2.Record.Address }
            });
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Call Contract", record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(fx.ContractRecord.Contract, record.Contract);
            Assert.Empty(record.CallResult.Error);
            Assert.True(record.CallResult.Bloom.IsEmpty);
            Assert.InRange(record.CallResult.Gas, 0UL, 30_000UL);
            Assert.Empty(record.CallResult.Events);

            var infoAfter = await fx2.Client.GetAccountInfoAsync(fx2.Record.Address);
            Assert.Equal((ulong)(fx.ContractParams.InitialBalance + extraFunds), infoAfter.Balance - infoBefore.Balance);
        }
        [Fact(DisplayName = "Payable Contract: Can Call Contract that Sends Funds to Deleted Account does not Raise Error (IS THIS A NETWORK BUG?)")]
        public async Task SendFundsToDeletedAccountDoesNotRaiseError()
        {
            // Setup the Simple Payable Contract and An account for "send to".
            await using var fxContract = await PayableContract.CreateAsync(_network);
            await using var fxAccount = await TestAccount.CreateAsync(_network);

            // Get the Info for the account state and then delete the account.
            var infoBefore = await fxAccount.Client.GetAccountInfoAsync(fxAccount.Record.Address);
            var deleteReceipt = await fxAccount.Client.DeleteAccountAsync(new Account(fxAccount.Record.Address, fxAccount.PrivateKey), fxAccount.Network.Payer);
            Assert.Equal(ResponseCode.Success, deleteReceipt.Status);

            // Double check the balance on the contract, confirm it has hbars
            var contractBalanceBefore = await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fxContract.ContractRecord.Contract,
                Gas = await _network.TinybarsFromGas(400),
                FunctionName = "get_balance"
            });
            Assert.NotNull(contractBalanceBefore);
            Assert.InRange(fxContract.ContractParams.InitialBalance, 1, int.MaxValue);
            Assert.Equal(fxContract.ContractParams.InitialBalance, contractBalanceBefore.CallResult.Result.As<long>());

            // Call the contract, sending to the address of the now deleted account
            var sendRecord = await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fxContract.ContractRecord.Contract,
                Gas = await _network.TinybarsFromGas(400),
                FunctionName = "send_to",
                FunctionArgs = new[] { fxAccount.Record.Address }
            });
            Assert.NotNull(sendRecord);
            Assert.Equal(ResponseCode.Success, sendRecord.Status);
            Assert.False(sendRecord.Hash.IsEmpty);
            Assert.NotNull(sendRecord.Concensus);
            Assert.Equal("Call Contract", sendRecord.Memo);
            Assert.InRange(sendRecord.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(fxContract.ContractRecord.Contract, sendRecord.Contract);
            Assert.Empty(sendRecord.CallResult.Error);
            Assert.True(sendRecord.CallResult.Bloom.IsEmpty);
            Assert.InRange(sendRecord.CallResult.Gas, 0UL, 30_000UL);
            Assert.Empty(sendRecord.CallResult.Events);

            // Confirm that the balance on the contract is now zero.
            var contractBalanceAfter = await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fxContract.ContractRecord.Contract,
                Gas = await _network.TinybarsFromGas(400),
                FunctionName = "get_balance"
            });
            Assert.NotNull(contractBalanceAfter);
            Assert.Equal(0, contractBalanceAfter.CallResult.Result.As<long>());

            // Try to get info on the deleted account, but this will fail because the
            // account is already deleted.
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                // So if this throws an error, why did the above call not fail?
                await fxAccount.Client.GetAccountInfoAsync(fxAccount.Record.Address);
            });
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
                    Gas = await _network.TinybarsFromGas(1500),
                    FunctionName = "send_to",
                    FunctionArgs = new[] { new Address(0, 0, long.MaxValue) }
                });
            });
            Assert.Equal(ResponseCode.InvalidSolidityAddress, tex.Status);
            Assert.StartsWith("Contract call failed, status: InvalidSolidityAddress", tex.Message);
        }
        [Fact(DisplayName = "Payable Contract: It appears to be possible to burn hbars (IS THIS A NETWORK BUG?)")]
        public async Task ItAppearsToBePossibleToBurnHbars()
        {
            // Setup the Simple Payable Contract and An account for "send to".
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxContract = await PayableContract.CreateAsync(_network);

            // Get the Info for the account state and then delete the account.
            var info1Before = await fxAccount1.Client.GetAccountInfoAsync(fxAccount1.Record.Address);
            var info2Before = await fxAccount2.Client.GetAccountInfoAsync(fxAccount2.Record.Address);
            var delete1Receipt = await fxAccount1.Client.DeleteAccountAsync(new Account(fxAccount1.Record.Address, fxAccount1.PrivateKey), fxAccount1.Network.Payer);
            Assert.Equal(ResponseCode.Success, delete1Receipt.Status);

            // Double check the balance on the contract, confirm it has hbars
            var contractBalanceBefore = await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fxContract.ContractRecord.Contract,
                Gas = await _network.TinybarsFromGas(400),
                FunctionName = "get_balance"
            });
            Assert.NotNull(contractBalanceBefore);
            Assert.InRange(fxContract.ContractParams.InitialBalance, 1, int.MaxValue);
            Assert.Equal(fxContract.ContractParams.InitialBalance, contractBalanceBefore.CallResult.Result.As<long>());

            // Call the contract, sending to the address of the now deleted account
            var sendRecord = await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fxContract.ContractRecord.Contract,
                Gas = await _network.TinybarsFromGas(400),
                FunctionName = "send_to",
                FunctionArgs = new[] { fxAccount1.Record.Address }
            });
            Assert.NotNull(sendRecord);
            Assert.Equal(ResponseCode.Success, sendRecord.Status);
            Assert.False(sendRecord.Hash.IsEmpty);
            Assert.NotNull(sendRecord.Concensus);
            Assert.Equal("Call Contract", sendRecord.Memo);
            Assert.InRange(sendRecord.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(fxContract.ContractRecord.Contract, sendRecord.Contract);
            Assert.Empty(sendRecord.CallResult.Error);
            Assert.True(sendRecord.CallResult.Bloom.IsEmpty);
            Assert.InRange(sendRecord.CallResult.Gas, 0UL, 30_000UL);
            Assert.Empty(sendRecord.CallResult.Events);

            // Confirm that the balance on the contract is now zero.
            var contractBalanceAfter = await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fxContract.ContractRecord.Contract,
                Gas = await _network.TinybarsFromGas(400),
                FunctionName = "get_balance"
            });
            Assert.NotNull(contractBalanceAfter);
            Assert.Equal(0, contractBalanceAfter.CallResult.Result.As<long>());

            // Try to get info on the deleted account, but this will fail because the
            // account is already deleted.
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                // So if this throws an error, why did the above call not fail?
                await fxAccount1.Client.GetAccountInfoAsync(fxAccount1.Record.Address);
            });

            // Delete the Contract, returning any hidden hbars to account number 2
            var deleteContractRecord = await fxContract.Client.DeleteContractAsync(fxContract.ContractRecord.Contract, fxAccount2.Record.Address);
            Assert.Equal(ResponseCode.Success, deleteContractRecord.Status);

            // Check the balance of account number 2, did the  hbars go there?
            var info2After = await fxAccount2.Client.GetAccountInfoAsync(fxAccount2.Record.Address);
            Assert.Equal(info2Before.Balance, info2After.Balance); // NOPE!
        }
    }
}
