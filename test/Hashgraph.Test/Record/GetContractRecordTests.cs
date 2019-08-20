using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
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
        [Fact(DisplayName = "Contract Records: Creating a Contract leaves a record that can be retrieved.")]
        public async Task CanRetrieveRecordFromContractCreate()
        {
            await using var fxContract = await StatefulContract.CreateAsync(_network);
            var records = await fxContract.Client.GetContractRecordsAsync(fxContract.ContractRecord.Contract);
            Assert.NotNull(records);
            Assert.Single(records);
            var record = records[0];
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.Equal(fxContract.ContractRecord.Id, record.Id);
            Assert.InRange(record.Fee, 0UL, 833_333_334UL);
            Assert.StartsWith("Stateful Contract Create: Instantiating Stateful Instance", record.Memo);
            Assert.NotNull(record.Concensus);
            Assert.False(record.Hash.IsEmpty);
        }
        [Fact(DisplayName = "Contract Records: Calling Contract Method creates record that can be retrieved.")]
        public async Task CanRetrieveRecordsFromContractMethodCalls()
        {
            await using var fxContract = await StatefulContract.CreateAsync(_network);

            var transactionCount = Generator.Integer(2, 3);
            for (int i = 0; i < transactionCount; i++)
            {
                await fxContract.Client.CallContractWithRecordAsync(new CallContractParams
                {
                    Contract = fxContract.ContractRecord.Contract,
                    Gas = 30_000,
                    FunctionName = "get_message"
                });
            }
            var records = await fxContract.Client.GetContractRecordsAsync(fxContract.ContractRecord.Contract);
            Assert.NotNull(records);
            Assert.Equal(transactionCount + 1, records.Length);
            foreach (var record in records.Skip(1))
            {
                Assert.Equal(ResponseCode.Success, record.Status);
                Assert.InRange(record.Fee, 0UL, 41_666_667UL);
                Assert.Equal("Call Contract", record.Memo);
                Assert.NotNull(record.Concensus);
                Assert.False(record.Hash.IsEmpty);
            }
        }
        [Fact(DisplayName = "Contract Records: Casting Account Address as Contract Raises an Error")]
        public async Task GetTransactionRecordsForAccountViaGetContractRecordsRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);

            var cryptoTransferFee = 83_333_334;
            var transferAmount = Generator.Integer(200, 500);
            var transactionCount = Generator.Integer(2, 5);
            var childAccount = new Account(fxAccount.Record.Address, fxAccount.PrivateKey);
            var parentAccount = _network.Payer;
            await fxAccount.Client.TransferAsync(parentAccount, childAccount, transactionCount * (cryptoTransferFee + transferAmount));
            await using (var client = fxAccount.Client.Clone(ctx => ctx.Payer = childAccount))
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
