﻿using Hashgraph.Test.Fixtures;
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
        [Fact(DisplayName = "Contract Delete: Not yet supported by network.")]
        public async Task DeleteContractNotYetSupported()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () => {
                await fx.Client.DeleteContractAsync(fx.ContractCreateRecord.Contract, fx.Payer);
            });
            Assert.Equal(ResponseCode.NotSupported, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: NotSupported", pex.Message);
        }
        /*
         * Tests once implemented by hedera network:
         *   Can Delete a Contract
         *   Cannot get info on a deleted contract
         *   Cannot delete immutable contract
         *   Cannot delete contract without admin key in signature
         *   Cannot delete contract when contract id is missing.
         *   Cannot delete non existent contract
         *   Cannot delete contract when transfer to address is missing 
         *   Cannot delete contract when transfer to address is invalid
         */
    }
}