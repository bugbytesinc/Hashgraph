using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Record
{
    [Collection(nameof(NetworkCredentials))]
    public class GetContractRecordTests
    {
        private readonly NetworkCredentials _network;
        public GetContractRecordTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Contract Records: Creating a Contract no longer leaves a record that can be retrieved.")]
        public async Task CanRetrieveRecordFromContractCreateRemoved()
        {
            await using var fxContract = await StatefulContract.CreateAsync(_network);
            var records = await fxContract.Client.GetContractRecordsAsync(fxContract.ContractRecord.Contract);
            Assert.NotNull(records);
            Assert.Empty(records);
        }
        [Fact(DisplayName = "Contract Records: Calling Contract Method no longer creates record that can be retrieved.")]
        public async Task CanRetrieveRecordsFromContractMethodCalls()
        {
            await using var fxContract = await StatefulContract.CreateAsync(_network);

            var transactionCount = Generator.Integer(2, 3);
            for (int i = 0; i < transactionCount; i++)
            {
                await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
                {
                    Contract = fxContract.ContractRecord.Contract,
                    Gas = await _network.TinybarsFromGas(400),
                    FunctionName = "get_message"
                });
            }
            var records = await fxContract.Client.GetContractRecordsAsync(fxContract.ContractRecord.Contract);
            Assert.NotNull(records);
            Assert.Empty(records);
        }
        [Fact(DisplayName = "Contract Records: Casting Account Address as Contract Raises an Error")]
        public async Task GetTransactionRecordsForAccountViaGetContractRecordsRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);

            var cryptoTransferFee = 83_333_334;
            var transferAmount = Generator.Integer(200, 500);
            var transactionCount = Generator.Integer(2, 5);
            var childAccount = fxAccount.Record.Address;
            var parentAccount = _network.Payer;
            await fxAccount.Client.TransferAsync(parentAccount, childAccount, transactionCount * (cryptoTransferFee + transferAmount));
            await using (var client = fxAccount.Client.Clone(ctx => { ctx.Payer = childAccount; ctx.Signatory = fxAccount.PrivateKey; }))
            {
                for (int i = 0; i < transactionCount; i++)
                {
                    await client.TransferAsync(childAccount, parentAccount, transferAmount, ctx => { ctx.FeeLimit = cryptoTransferFee; });
                }
            }
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fxAccount.Client.GetContractRecordsAsync(childAccount);
            });
            Assert.Equal(ResponseCode.InvalidContractId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidContractId", pex.Message);
        }
        [Fact(DisplayName = "Contract Records: Get with Empty Contract address raises Error.")]
        public async Task EmptyAccountRaisesError()
        {
            await using var client = _network.NewClient();
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.GetContractRecordsAsync(null);
            });
            Assert.Equal("contract", ane.ParamName);
            Assert.StartsWith("Contract Address is missing. Please check that it is not null.", ane.Message);
        }
    }
}
