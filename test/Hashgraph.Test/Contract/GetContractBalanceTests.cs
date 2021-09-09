using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentials))]
    public class GetContractBalanceTests
    {
        private readonly NetworkCredentials _network;
        public GetContractBalanceTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Get Contract Balance: Can Get Balance for Contract")]
        public async Task CanGetTinybarBalanceForContractAsync()
        {
            await using var fx = await PayableContract.CreateAsync(_network);

            var balance = await fx.Client.GetContractBalanceAsync(fx.ContractRecord.Contract);
            Assert.Equal((ulong)fx.ContractParams.InitialBalance, balance);

            var info = await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
            Assert.Equal(balance, info.Balance);
        }
        [Fact(DisplayName = "Get Contract Balance: Can Get Contract Balances for Contract")]
        public async Task CanGetBalancesForContractAsync()
        {
            await using var fx = await PayableContract.CreateAsync(_network);

            var balances = await fx.Client.GetContractBalancesAsync(fx.ContractRecord.Contract);
            Assert.Equal((ulong)fx.ContractParams.InitialBalance, balances.Crypto);
            Assert.Empty(balances.Tokens);
        }
        [Fact(DisplayName = "Get Contract Balance: Missing Payer Account Does not Throw Exception (Free Query)")]
        public async Task MissingPayerAccountDosNotThrowException()
        {
            await using var fx = await PayableContract.CreateAsync(_network);
            await using var client = new Client(ctx => { ctx.Gateway = _network.Gateway; });
            var balance = await client.GetContractBalanceAsync(fx.ContractRecord.Contract);
            Assert.Equal((ulong)fx.ContractParams.InitialBalance, balance);
        }
        [Fact(DisplayName = "Get Contract Balance: Querying a non-contract address raises an error.")]
        public async Task QueryingBalanceForNonContractAddressRaisesError()
        {
            await using var client = new Client(ctx => { ctx.Gateway = _network.Gateway; });
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.GetContractBalanceAsync(_network.Payer);
            });
            Assert.Equal(ResponseCode.InvalidContractId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidContractId", pex.Message);
        }
        [Fact(DisplayName = "Get Contract Balance: Missing Contract for Balance Check Throws Exception")]
        public async Task MissingBalanceContractAccountThrowsException()
        {
            await using var client = _network.NewClient();
            var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                var balance = await client.GetContractBalanceAsync(null);
            });
            Assert.StartsWith("Contract Address is missing.", ex.Message);
        }
        [Fact(DisplayName = "Get Contract Balance: Invalid Account Address Throws Exception")]
        public async Task InvalidContractAddressThrowsException()
        {
            await using var client = _network.NewClient();
            var account = new Address(0, 0, 0);
            var ex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                var balance = await client.GetContractBalanceAsync(account);
            });
            Assert.Equal(ResponseCode.InvalidContractId, ex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidContractId", ex.Message);
        }
        [Fact(DisplayName = "Get Contract Balance: Gateway Node Address is Ignored because no Fee is Charged")]
        public async Task InvalidGatewayAddressIsIgnored()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            await using var client = _network.NewClient();
            client.Configure(cfg =>
            {
                cfg.Gateway = new Gateway($"{_network.NetworkAddress}:{_network.NetworkPort}", 0, 0, 999);
            });
            var balance = await client.GetContractBalanceAsync(fx.ContractRecord.Contract);
            Assert.Equal(0ul, balance);            
        }
        [Fact(DisplayName = "Get Contract Balance: Can Set FeeLimit to Zero")]
        public async Task GetContractBalanceRequiresNoFee()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            await using var client = _network.NewClient();
            client.Configure(cfg =>
            {
                cfg.FeeLimit = 0;
            });
            var balance = await client.GetContractBalanceAsync(fx.ContractRecord.Contract);
            Assert.Equal(0ul, balance);
        }
        [Fact(DisplayName = "Get Account Balance: No Receipt is created for a Balance Query")]
        public async Task RetrievingAccountBalanceDoesNotCreateReceipt()
        {
            await using var fx = await PayableContract.CreateAsync(_network);
            await using var client = _network.NewClient();            
            var txId = client.CreateNewTxId();
            var balance = await client.GetContractBalanceAsync(fx.ContractRecord.Contract, ctx => ctx.Transaction = txId);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                var receipt = await client.GetReceiptAsync(txId);
            });
            Assert.Equal(txId, tex.TxId);
            Assert.Equal(ResponseCode.ReceiptNotFound, tex.Status);
            Assert.StartsWith("Network failed to return a transaction receipt", tex.Message);
        }
    }
}
