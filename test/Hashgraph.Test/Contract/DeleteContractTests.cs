using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class DeleteContractTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public DeleteContractTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [SkippableFact(DisplayName = "Contract Delete: Can Delete a Contract")]
        public async Task CanDeleteAContract()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);

            try
            {
                var receipt = await fx.Client.DeleteContractAsync(fx.ContractCreateRecord.Contract, fx.Payer);
                Assert.NotNull(receipt);
                Assert.Equal(ResponseCode.Success, receipt.Status);
            }
            catch (PrecheckException tex) when (tex.Status == ResponseCode.NotSupported)
            {
                Skip.If(true, "Support for deleting contracts is not implemented by the network yet.");
            }

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
                {
                    await fx.Client.GetContractInfoAsync(fx.ContractCreateRecord.Contract);
                });
            Assert.Equal(ResponseCode.InvalidContractId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidContractId", pex.Message);
        }
        /*
         * Additional Tests once implemented by hedera network:
         *   Cannot delete immutable contract
         *   Cannot delete contract without admin key in signature
         *   Cannot delete contract when contract id is missing.
         *   Cannot delete non existent contract
         *   Cannot delete contract when transfer to address is missing 
         *   Cannot delete contract when transfer to address is invalid
         */
    }
}
