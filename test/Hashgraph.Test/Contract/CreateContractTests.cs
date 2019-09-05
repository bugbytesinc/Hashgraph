using Hashgraph.Test.Fixtures;
using System;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
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
            Assert.InRange(fx.ContractRecord.Fee, 0UL, 430_000_000UL);
        }
        [Fact(DisplayName = "Create Contract: Missing File Address Raises Error")]
        public async Task MissingFileAddressRaisesError()
        {
            await using var fx = await GreetingContract.SetupAsync(_network);
            fx.ContractParams.File = null;
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () => {
                await fx.Client.CreateContractAsync(fx.ContractParams);
            });
            Assert.StartsWith("The File Address containing the contract is missing, it cannot be null.", ex.Message);
            Assert.Equal("File", ex.ParamName);            
        }
        [Fact(DisplayName = "Create Contract: Missing Gas Raises Error")]
        public async Task MissingGasRaisesError()
        {
            await using var fx = await GreetingContract.SetupAsync(_network);
            fx.ContractParams.Gas = 0;
            var ex = await Assert.ThrowsAsync<TransactionException>(async () => {
                await fx.Client.CreateContractAsync(fx.ContractParams);
            });
            Assert.StartsWith("Unable to create contract, status: InsufficientGas", ex.Message);
            Assert.Equal(ResponseCode.InsufficientGas, ex.Status);
        }
        [Fact(DisplayName = "Create Contract: Sending crypto to non-payable contract raises error.")]
        public async Task SendingCryptoToNonPayableContractRaisesError()
        {
            await using var fx = await GreetingContract.SetupAsync(_network);
            fx.ContractParams.InitialBalance = 10;
            var ex = await Assert.ThrowsAsync<TransactionException>(async () => {
                await fx.Client.CreateContractAsync(fx.ContractParams);
            });
            Assert.StartsWith("Unable to create contract, status: ContractRevertExecuted", ex.Message);
            Assert.Equal(ResponseCode.ContractRevertExecuted, ex.Status);
        }
        [Fact(DisplayName = "Create Contract: Invalid Renew Period Raises Error")]
        public async Task InvalidRenewPeriodRaisesError()
        {
            await using var fx = await GreetingContract.SetupAsync(_network);
            fx.ContractParams.RenewPeriod = TimeSpan.FromTicks(1);
            var ex = await Assert.ThrowsAsync<PrecheckException>(async () => {
                await fx.Client.CreateContractAsync(fx.ContractParams);
            });
            Assert.StartsWith("Transaction Failed Pre-Check: AutorenewDurationNotInRange", ex.Message);
            Assert.Equal(ResponseCode.AutorenewDurationNotInRange, ex.Status);
        }
        [Fact(DisplayName = "Create Contract: Can Create Without Admin Key")]
        public async Task CanCreateContractWithoutAdminKey()
        {
            await using var fx = await GreetingContract.SetupAsync(_network);
            fx.ContractParams.Administrator = null;
            fx.ContractRecord = await fx.Client.CreateContractWithRecordAsync(fx.ContractParams);
            Assert.NotNull(fx.ContractRecord);
            Assert.NotNull(fx.ContractRecord.Contract);
            Assert.Equal(ResponseCode.Success, fx.ContractRecord.Status);
            Assert.NotEmpty(fx.ContractRecord.Hash.ToArray());
            Assert.NotNull(fx.ContractRecord.Concensus);
            Assert.NotNull(fx.ContractRecord.Memo);
            Assert.InRange(fx.ContractRecord.Fee, 0UL, 420_000_000UL);
        }
        [Fact(DisplayName = "Create Contract: Random Constructor Data when not needed is ignored.")]
        public async Task CanCreateContractWithUnneededConstructorData()
        {
            await using var fx = await GreetingContract.SetupAsync(_network);
            fx.ContractParams.Arguments = new object[] { "Random Data that Should Be Ignored." };
            fx.ContractRecord = await fx.Client.CreateContractWithRecordAsync(fx.ContractParams);
            Assert.NotNull(fx.ContractRecord);
            Assert.NotNull(fx.ContractRecord.Contract);
            Assert.Equal(ResponseCode.Success, fx.ContractRecord.Status);
            Assert.NotEmpty(fx.ContractRecord.Hash.ToArray());
            Assert.NotNull(fx.ContractRecord.Concensus);
            Assert.NotNull(fx.ContractRecord.Memo);
            Assert.InRange(fx.ContractRecord.Fee, 0UL, 430_000_000UL);
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
            Assert.InRange(fx.ContractRecord.Fee, 0UL, 440_000_000UL);
        }

        [Fact(DisplayName = "Create Contract: Missing Construction Parameters that are Required raises Error")]
        public async Task CreateWithoutRequiredContractParamsThrowsError()
        {
            await using var fx = await StatefulContract.SetupAsync(_network);
            fx.ContractParams.Arguments = null;
            var ex = await Assert.ThrowsAsync<TransactionException>(async () => {
                await fx.Client.CreateContractAsync(fx.ContractParams);
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
            Assert.InRange(fx.ContractRecord.Fee, 0UL, 430_000_000UL);
        }
    }
}
