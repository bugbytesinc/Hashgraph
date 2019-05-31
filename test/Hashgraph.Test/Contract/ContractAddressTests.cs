using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentials))]
    public class ContractAddressTests
    {
        private readonly NetworkCredentials _network;
        public ContractAddressTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Contract Address: Can Get Stateless Contract Address from Smart Contract ID")]
        public async Task CanGetStatelessContractAddressFromSmartContractAddress()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var info = await fx.Client.GetContractInfoAsync(fx.ContractCreateRecord.Contract);

            var address = await fx.Client.GetAddressFromSmartContractId(info.SmartContractId);

            Assert.Equal(fx.ContractCreateRecord.Contract, address);
        }
        [Fact(DisplayName = "Contract Address: Can Get Stateful Contract Address from Smart Contract ID")]
        public async Task CanGetStatefulContractAddressFromSmartContractAddress()
        {
            await using var fx = await StatefulContract.CreateAsync(_network);

            var info = await fx.Client.GetContractInfoAsync(fx.ContractCreateRecord.Contract);

            var address = await fx.Client.GetAddressFromSmartContractId(info.SmartContractId);

            Assert.Equal(fx.ContractCreateRecord.Contract, address);
        }
        [Fact(DisplayName = "Contract Address: Can Get Account Address from Smart Contract ID")]
        public async Task CanGetAccountAddressFromSmartContractAddress()
        {
            await using var fx = await TestAccount.CreateAsync(_network);

            var info = await fx.Client.GetAccountInfoAsync(fx.AccountRecord.Address);

            var address = await fx.Client.GetAddressFromSmartContractId(info.SmartContractId);

            Assert.Equal(fx.AccountRecord.Address, address);
        }
        [Fact(DisplayName = "Contract Address: Retrieving Deleted Address from Smart Contract ID Succeeds")]
        public async Task GetNonExistantContractRaisesError()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var info = await fx.Client.GetAccountInfoAsync(fx.AccountRecord.Address);
            await fx.Client.DeleteAccountAsync(new Account(fx.AccountRecord.Address, fx.PrivateKey), _network.Payer);

            var address = await fx.Client.GetAddressFromSmartContractId(info.SmartContractId);

            Assert.Equal(fx.AccountRecord.Address, address);
        }
        [Fact(DisplayName = "Contract Address: Invalid Smart Contract ID raises Error (BUT THE WRONG TYPE)")]
        public async Task InvalidSmartContractIDRaisesError()
        {
            await using var client = _network.NewClient();

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.GetAddressFromSmartContractId("00000000000BOGUS000000000000000000000018c7");
            });
            Assert.StartsWith("Unable to communicate with network: Status(StatusCode=Unknown", pex.Message);
            Assert.NotNull(pex.InnerException);
        }
    }
}
