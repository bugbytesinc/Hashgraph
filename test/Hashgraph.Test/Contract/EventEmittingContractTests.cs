using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
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
        [Fact(DisplayName = "Event Emitting Contract: Can Get Contract Balance from Call (Local Call)")]
        public async Task CanGetContractBalanceFromLocalCall()
        {
            await using var fx = await EventEmittingContract.CreateAsync(_network);

            var result = await fx.Client.QueryContractAsync(new QueryContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = await _network.TinybarsFromGas(400),
                FunctionName = "get_balance"                
            });

            Assert.NotNull(result);
            Assert.Empty(result.Error);
            Assert.True(result.Bloom.IsEmpty);
            Assert.InRange(result.Gas, 0UL, 400UL);
            Assert.Empty(result.Events);
            Assert.Equal(fx.ContractParams.InitialBalance, result.Result.As<long>());
        }
        [Fact(DisplayName = "Event Emitting Contract: Can Call Contract that Sends Funds, Emitting Event")]
        public async Task CanCallContractMethodSendingFunds()
        {
            await using var fx = await EventEmittingContract.CreateAsync(_network);
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
            Assert.InRange(record.CallResult.Gas, 0UL, 300_000UL);
            Assert.Single(record.CallResult.Events);

            // Now check the emitted Event
            var result = record.CallResult.Events[0];
            Assert.Equal(fx.ContractRecord.Contract, result.Contract);
            Assert.False(result.Bloom.IsEmpty);
            Assert.Single(result.Topic);
            Assert.Equal("9277a4302be4a765ae8585e09a9306bd55da10e20e59ed4f611a04ba606fece8", Hex.FromBytes(result.Topic[0]));

            var (address, amount) = result.Data.As<Address, long>();
            Assert.Equal(fx2.Record.Address, address);
            Assert.Equal(fx.ContractParams.InitialBalance, amount);

            var infoAfter = await fx2.Client.GetAccountInfoAsync(fx2.Record.Address);
            Assert.Equal((ulong)fx.ContractParams.InitialBalance, infoAfter.Balance - infoBefore.Balance);
        }

        [Fact(DisplayName = "Event Emitting Contract: It appears to be possible to burn hbars (IS THIS A NETWORK BUG?)")]
        public async Task ItAppearsToBePossibleToBurnHbars()
        {
            // Setup the Simple Event Emitting Contract and An account for "send to".
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxContract = await EventEmittingContract.CreateAsync(_network);

            // Get the Info for the account state and then delete the account.
            var info1Before = await fxAccount1.Client.GetAccountInfoAsync(fxAccount1.Record.Address);
            var info2Before = await fxAccount2.Client.GetAccountInfoAsync(fxAccount2.Record.Address);
            var delete1Receipt = await fxAccount1.Client.DeleteAccountAsync(new Account(fxAccount1.Record.Address, fxAccount1.PrivateKey), fxAccount1.Network.Payer);
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
            Assert.InRange(sendRecord.CallResult.Gas, 0UL, 300_000UL);
            Assert.Single(sendRecord.CallResult.Events);

            // Now check the emitted Event, Confirm that the contract thought it sent hbars
            var emittedEvent = sendRecord.CallResult.Events[0];
            Assert.Equal(fxContract.ContractRecord.Contract, emittedEvent.Contract);
            Assert.False(emittedEvent.Bloom.IsEmpty);
            Assert.Single(emittedEvent.Topic);
            Assert.Equal("9277a4302be4a765ae8585e09a9306bd55da10e20e59ed4f611a04ba606fece8", Hex.FromBytes(emittedEvent.Topic[0]));
            var (address, amount) = emittedEvent.Data.As<Address, long>();
            Assert.Equal(fxAccount1.Record.Address, address);  // Account matches deleted account
            Assert.Equal(fxContract.ContractParams.InitialBalance, amount);  // Amount matches previous balance in contract.

            // Confirm that the balance on the contract is now zero.
            var contractBalanceAfter = await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fxContract.ContractRecord.Contract,
                Gas = await _network.TinybarsFromGas(400),
                FunctionName = "get_balance"
            });
            Assert.NotNull(contractBalanceAfter);
            Assert.Equal(0, contractBalanceAfter.CallResult.Result.As<long>());

            // Double Check: try to get info on the deleted account, 
            // but this will fail because the account is already deleted.
            pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                // So if this throws an error, why did the above transfer not fail?
                await fxAccount1.Client.GetAccountInfoAsync(fxAccount1.Record.Address);
            });
            Assert.Equal(ResponseCode.AccountDeleted, pex.Status);

            // Delete the Contract, returning any hidden hbars to account number 2
            var deleteContractRecord = await fxContract.Client.DeleteContractAsync(fxContract.ContractRecord.Contract, fxAccount2.Record.Address);
            Assert.Equal(ResponseCode.Success, deleteContractRecord.Status);

            // Check the balance of account number 2, did the hbars go there?
            var info2After = await fxAccount2.Client.GetAccountInfoAsync(fxAccount2.Record.Address);
            Assert.Equal(info2Before.Balance, info2After.Balance); // NOPE, where did they go!
        }
    }
}
