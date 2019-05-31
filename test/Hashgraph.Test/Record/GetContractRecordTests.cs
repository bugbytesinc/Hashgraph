using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Record
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class GetContractRecordTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public GetContractRecordTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "Contract Records: Contract with no records returns no records.")]
        public async Task ContractWithNoRecords()
        {
            await using var fx = await StatefulContractInstance.CreateAsync(_networkCredentials);

            var transactionCount = Generator.Integer(2, 5);
            for (int i = 0; i < transactionCount; i++)
            {
                await fx.Client.CallContractWithRecordAsync(new CallContractParams
                {
                    Contract = fx.ContractCreateRecord.Contract,
                    Gas = 30_000,
                    FunctionName = "get_message"
                });
            }
            var records = await fx.Client.GetContractRecordsAsync(fx.ContractCreateRecord.Contract);
            Assert.NotNull(records);
            Assert.Empty(records);
        }
        [Fact(DisplayName = "Contract Records: Casting Account Address as Contract returns no records (BUT DOES NOT FAIL)")]
        public async Task CanGetTransactionRecordsForAccount()
        {
            await using var fx = await TestAccountInstance.CreateAsync(_networkCredentials);

            var transactionCount = Generator.Integer(2, 5);
            var childAccount = new Account(fx.AccountRecord.Address, fx.PrivateKey);
            var parentAccount = _networkCredentials.CreateDefaultAccount();
            await fx.Client.TransferAsync(parentAccount, childAccount, transactionCount * 100001);
            await using (var client = fx.Client.Clone(ctx => ctx.Payer = childAccount))
            {
                for (int i = 0; i < transactionCount; i++)
                {
                    await client.TransferAsync(childAccount, parentAccount, 1);
                }
            }
            var records = await fx.Client.GetContractRecordsAsync(childAccount);
            Assert.NotNull(records);
            Assert.Empty(records);
        }
        [Fact(DisplayName = "Contract Records: Get with Empty Contract address raises Error.")]
        public async Task EmptyAccountRaisesError()
        {
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.GetContractRecordsAsync(null);
            });
            Assert.Equal("contract", ane.ParamName);
            Assert.StartsWith("Contract Address is missing. Please check that it is not null.", ane.Message);
        }
    }
}
