using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentials))]
    public class DeleteContractTests
    {
        private readonly NetworkCredentials _network;
        public DeleteContractTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Contract Delete: Can Call Delete without Error.")]
        public async Task CanDeleteContract()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var record = await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, _network.Payer, ctx => ctx.Signatory = new Signatory(_network.PrivateKey, fx.PrivateKey));
            Assert.Equal(ResponseCode.Success, record.Status);
        }
        [Fact(DisplayName = "Contract Delete: Can Call Delete without Error using Signatory")]
        public async Task CanDeleteContractUsingSignatory()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var record = await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, _network.Payer, fx.PrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
        }
        [Fact(DisplayName = "Contract Delete: Deleting contract removes it (get info should fail).")]
        public async Task DeleteContractRemovesContract()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var record = await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, _network.Payer, fx.PrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
            });
            Assert.Equal(ResponseCode.ContractDeleted, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: ContractDeleted", pex.Message);
        }
        [Fact(DisplayName = "Contract Delete: Deleting a deleted contract raises an error.")]
        public async Task DeleteTwiceRaisesAnError()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var record = await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, _network.Payer, fx.PrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, _network.Payer, fx.PrivateKey);
            });
            Assert.Equal(ResponseCode.ContractDeleted, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: ContractDeleted", pex.Message);
        }
        [Fact(DisplayName = "Contract Delete: Deleting without admin key raises error.")]
        public async Task DeleteContractWithoutAdminKeyRaisesError()
        {
            await using var fxContract = await GreetingContract.CreateAsync(_network);
            await using var client = _network.NewClient();
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await client.DeleteContractAsync(fxContract.ContractRecord.Contract, _network.Payer);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to delete contract, status: InvalidSignature", tex.Message);
        }
        [Fact(DisplayName = "Contract Delete: Deleting with an invalid contract ID raises Error.")]
        public async Task DeleteContractWithInvalidContractIDRaisesError()
        {
            await using var fx = await TestAccount.CreateAsync(_network);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.DeleteContractAsync(fx.Record.Address, _network.Payer);
            });
            Assert.Equal(ResponseCode.InvalidContractId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidContractId", pex.Message);
        }
        [Fact(DisplayName = "Contract Delete: Deleting with missing contract ID raises Error.")]
        public async Task DeleteContractWithMissingIDRaisesError()
        {
            await using var client = _network.NewClient();
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.DeleteContractAsync(null, _network.Payer);
            });
            Assert.Equal("contractToDelete", ane.ParamName);
            Assert.StartsWith("Contract to Delete is missing. Please check that it is not null.", ane.Message);
        }
        [Fact(DisplayName = "Contract Delete: Deleting with missing return crypto address raises Error.")]
        public async Task DeleteContractWithMissingReturnToAddressRaisesError()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, null);
            });
            Assert.Equal("transferToAddress", ane.ParamName);
            Assert.StartsWith("Transfer address is missing. Please check that it is not null.", ane.Message);
        }
        [Fact(DisplayName = "Contract Delete: Deleting with invalid return crypto fails.")]
        public async Task DeleteContractWithInvalidAddressRaisesError()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            await using var fx2 = await TestAccount.CreateAsync(_network);
            var deleteAccountRecord = await fx2.Client.DeleteAccountAsync(fx2.Record.Address, _network.Payer, fx2.PrivateKey);
            Assert.Equal(ResponseCode.Success, deleteAccountRecord.Status);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, fx2.Record.Address, fx.PrivateKey);
            });
            Assert.Equal(ResponseCode.AccountDeleted, tex.Status);
            Assert.StartsWith("Unable to delete contract, status: AccountDeleted", tex.Message);
        }
        [Fact(DisplayName = "Contract Delete: Remaining Contract Balance is Returned to Account")]
        public async Task ReturnRemainingContractBalanceUponDelete()
        {
            // Setup the Simple Event Emitting Contract and An account for "send to".
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxContract = await EventEmittingContract.CreateAsync(_network);

            // Get the Info for the Account to receive funds before any changes happen.
            var infoBefore = await fxAccount.Client.GetAccountInfoAsync(fxAccount.Record.Address);
            Assert.Equal(fxAccount.CreateParams.InitialBalance, infoBefore.Balance);

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

            // Delete the Contract, returning contract balance to Account
            var deleteContractRecord = await fxContract.Client.DeleteContractAsync(fxContract.ContractRecord.Contract, fxAccount.Record.Address, fxContract.PrivateKey);
            Assert.Equal(ResponseCode.Success, deleteContractRecord.Status);

            // Check the balance of account to see if it went up by contract's balance.
            var infoAfter = await fxAccount.Client.GetAccountInfoAsync(fxAccount.Record.Address);
            Assert.Equal(infoBefore.Balance + (ulong)fxContract.ContractParams.InitialBalance, infoAfter.Balance);
        }
        [Fact(DisplayName = "Contract Delete: Deleting an Imutable Contract Raises Error")]
        public async Task DeleteImutableContractRaisesError()
        {
            await using var fxContract = await GreetingContract.CreateAsync(_network, fx =>
            {
                fx.ContractParams.Administrator = null;
            });

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxContract.Client.DeleteContractAsync(fxContract.ContractRecord.Contract, _network.Payer);
            });
            Assert.Equal(ResponseCode.ModifyingImmutableContract, tex.Status);
            Assert.StartsWith("Unable to delete contract, status: ModifyingImmutableContract", tex.Message);

            var info = await fxContract.Client.GetContractInfoAsync(fxContract.ContractRecord.Contract);
            Assert.NotNull(info);
        }
        [Fact(DisplayName = "Contract Delete: Can Not Schedule Delete.")]
        public async Task CanNotScheduleDelete()
        {
            await using var fxContract = await GreetingContract.CreateAsync(_network);
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxContract.Client.DeleteContractAsync(
                    fxContract.ContractRecord.Contract,
                    _network.Payer,
                    ctx => ctx.Signatory = new Signatory(
                        _network.PrivateKey,
                        fxContract.PrivateKey,
                        new PendingParams
                        {
                            PendingPayer = fxPayer
                        }));
            });
            Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
            Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
        }
    }
}