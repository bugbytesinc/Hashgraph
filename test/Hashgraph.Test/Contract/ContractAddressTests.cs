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
        [Fact(DisplayName = "NOT SUPPORTED: Contract Address: Can Get Stateless Contract Address from Smart Contract ID")]
        public async Task CanGetStatelessContractAddressFromSmartContractAddressNotSupported()
        {
            Assert.Equal(ResponseCode.NotSupported, (await Assert.ThrowsAsync<PrecheckException>(CanGetStatelessContractAddressFromSmartContractAddress)).Status);

            //[Fact(DisplayName = "Contract Address: Can Get Stateless Contract Address from Smart Contract ID")]
            async Task CanGetStatelessContractAddressFromSmartContractAddress()
            {
                await using var fx = await GreetingContract.CreateAsync(_network);

                var info = await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);

                var address = await fx.Client.GetAddressFromSmartContractIdAsync(info.SmartContractId);

                Assert.Equal(fx.ContractRecord.Contract, address);
            }
        }    
        [Fact(DisplayName = "NOT SUPPORTED: Contract Address: Can Get Stateful Contract Address from Smart Contract ID")]
        public async Task CanGetStatefulContractAddressFromSmartContractAddressNotSupported()
        {
            Assert.Equal(ResponseCode.NotSupported, (await Assert.ThrowsAsync<PrecheckException>(CanGetStatefulContractAddressFromSmartContractAddress)).Status);

            //[Fact(DisplayName = "Contract Address: Can Get Stateful Contract Address from Smart Contract ID")]
            async Task CanGetStatefulContractAddressFromSmartContractAddress()
            {
                await using var fx = await StatefulContract.CreateAsync(_network);

                var info = await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);

                var address = await fx.Client.GetAddressFromSmartContractIdAsync(info.SmartContractId);

                Assert.Equal(fx.ContractRecord.Contract, address);
            }
        }
        [Fact(DisplayName = "NOT SUPPORTED: Contract Address: Can Get Account Address from Smart Contract ID")]
        public async Task CanGetAccountAddressFromSmartContractAddressNotSupported()
        {
            Assert.Equal(ResponseCode.NotSupported, (await Assert.ThrowsAsync<PrecheckException>(CanGetAccountAddressFromSmartContractAddress)).Status);
            //[Fact(DisplayName = "Contract Address: Can Get Account Address from Smart Contract ID")]
            async Task CanGetAccountAddressFromSmartContractAddress()
            {
                await using var fx = await TestAccount.CreateAsync(_network);

                var info = await fx.Client.GetAccountInfoAsync(fx.Record.Address);

                var address = await fx.Client.GetAddressFromSmartContractIdAsync(info.SmartContractId);

                Assert.Equal(fx.Record.Address, address);
            }
        }
        [Fact(DisplayName = "NOT SUPPORTED: Contract Address: Retrieving Deleted Address from Smart Contract ID Succeeds")]
        public async Task RetrieveDeletedAddressFromSmartContractIDNotSupported()
        {
            Assert.Equal(ResponseCode.NotSupported, (await Assert.ThrowsAsync<PrecheckException>(RetrieveDeletedAddressFromSmartContractID)).Status);

            //[Fact(DisplayName = "Contract Address: Retrieving Deleted Address from Smart Contract ID Succeeds")]
            async Task RetrieveDeletedAddressFromSmartContractID()
            {
                await using var fx = await TestAccount.CreateAsync(_network);
                var info = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
                await fx.Client.DeleteAccountAsync(new Account(fx.Record.Address, fx.PrivateKey), _network.Payer);

                var address = await fx.Client.GetAddressFromSmartContractIdAsync(info.SmartContractId);

                Assert.Equal(fx.Record.Address, address);
            }
        }
        [Fact(DisplayName = "NOT SUPPORTED: Contract Address: Invalid Smart Contract ID raises Error")]
        public async Task InvalidSmartContractIDRaisesErrorNotSupported()
        {
            Assert.StartsWith("Assert.StartsWith() Failure", (await Assert.ThrowsAsync<Xunit.Sdk.StartsWithException>(InvalidSmartContractIDRaisesError)).Message);

            //[Fact(DisplayName = "Contract Address: Invalid Smart Contract ID raises Error")]
            async Task InvalidSmartContractIDRaisesError()
            {
                await using var client = _network.NewClient();

                var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
                {
                    await client.GetAddressFromSmartContractIdAsync("00000000000BOGUS000000000000000000000018c7");
                });
                Assert.StartsWith("Unable to communicate with network: Status(StatusCode=Unknown", pex.Message);
                Assert.NotNull(pex.InnerException);
            }
        }
    }
}
