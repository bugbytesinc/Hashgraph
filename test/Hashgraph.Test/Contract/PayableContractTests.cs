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
                Gas = 300_000,
                FunctionName = "get_balance"
            });
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Call Contract", record.Memo);
            Assert.InRange(record.Fee, 0UL, 100_000UL);
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
                Gas = 300_000,
                FunctionName = "get_balance"
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
                Gas = 300_000,
                FunctionName = "send_to",
                FunctionArgs = new[] { fx2.Record.Address }
            });
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Call Contract", record.Memo);
            Assert.InRange(record.Fee, 0UL, 100_000UL);
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
                Gas = 300_000,
                PayableAmount = extraFunds
            });
            Assert.Equal(ResponseCode.Success, record.Status);

            await using var fx2 = await TestAccount.CreateAsync(_network);
            var infoBefore = await fx2.Client.GetAccountInfoAsync(fx2.Record.Address);
            record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 300_000,
                FunctionName = "send_to",
                FunctionArgs = new[] { fx2.Record.Address }
            });
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Call Contract", record.Memo);
            Assert.InRange(record.Fee, 0UL, 100_000UL);
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
            await using var fx = await PayableContract.CreateAsync(_network);
            await using var fx2 = await TestAccount.CreateAsync(_network);

            var infoBefore = await fx2.Client.GetAccountInfoAsync(fx2.Record.Address);
            var receipt = await fx2.Client.DeleteAccountAsync(new Account(fx2.Record.Address, fx2.PrivateKey), fx2.Network.Payer);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var record = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 300_000,
                FunctionName = "send_to",
                FunctionArgs = new[] { fx2.Record.Address }
            });
            Assert.NotNull(record);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Equal("Call Contract", record.Memo);
            Assert.InRange(record.Fee, 0UL, 100_000UL);
            Assert.Equal(fx.ContractRecord.Contract, record.Contract);
            Assert.Empty(record.CallResult.Error);
            Assert.True(record.CallResult.Bloom.IsEmpty);
            Assert.InRange(record.CallResult.Gas, 0UL, 30_000UL);
            Assert.Empty(record.CallResult.Events);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                // So if this throws an error, why did the above call not fail?
                await fx2.Client.GetAccountInfoAsync(fx2.Record.Address);
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
                    Gas = 300_000,
                    FunctionName = "send_to",
                    FunctionArgs = new[] { new Address(0, 0, long.MaxValue) }
                });
            });
            Assert.Equal(ResponseCode.InvalidSolidityAddress, tex.Status);
            Assert.StartsWith("Contract call failed, status: InvalidSolidityAddress", tex.Message);
        }
    }
}
