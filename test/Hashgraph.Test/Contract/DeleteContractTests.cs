using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentials))]
    public class DeleteContractTests
    {
        private readonly NetworkCredentials _network;
        public DeleteContractTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Contract Delete: Can Call Delete without Error.")]
        public async Task DeleteContractNotYetSupported()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var record = await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, _network.Payer);
            Assert.Equal(ResponseCode.Success, record.Status);
        }
        [Fact(DisplayName = "Contract Delete: Deleting contract removes it (get info should fail).")]
        public async Task DeleteContractRemovesContract()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var record = await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, _network.Payer);
            Assert.Equal(ResponseCode.Success, record.Status);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
            });
            Assert.Equal(ResponseCode.ContractDeleted, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: ContractDeleted", pex.Message);
        }
        [Fact(DisplayName = "Contract Delete: Deleting a deleted contract raises an error.")]
        public async Task DeleteTwiceRaisesAnError()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var record = await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, _network.Payer);
            Assert.Equal(ResponseCode.Success, record.Status);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, _network.Payer);
            });
            Assert.Equal(ResponseCode.ContractDeleted, tex.Status);
            Assert.StartsWith("Unable to delete contract, status: ContractDeleted", tex.Message);
        }
        [Fact(DisplayName = "Contract Delete: Deleting without admin key raises error.")]
        public async Task DeleteContractWithoutAdminKeyRaisesError()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var fx = await GreetingContract.SetupAsync(_network);
            fx.ContractParams.Administrator = publicKey;  // Default is to use Payor's
            fx.Client.Configure(ctx => ctx.Payer = _network.PayerWithKeys(privateKey));
            await fx.CompleteCreateAsync();

            await using (var client = _network.NewClient())  // Will not have private key
            {
                var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
                {
                    await client.DeleteContractAsync(fx.ContractRecord.Contract, _network.Payer);
                });
                Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
                Assert.StartsWith("Unable to delete contract, status: InvalidSignature", tex.Message);
            }
        }
        [Fact(DisplayName = "Contract Delete: Deleting with an invalid contract ID raises Error.")]
        public async Task DeleteContractWithInvalidContractIDRaisesError()
        {
            await using var fx = await TestAccount.CreateAsync(_network);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.DeleteContractAsync(fx.Record.Address, _network.Payer);
            });
            Assert.Equal(ResponseCode.InvalidContractId, tex.Status);
            Assert.StartsWith("Unable to delete contract, status: InvalidContractId", tex.Message);
        }
        [Fact(DisplayName = "Contract Delete: Deleting with missing contract ID raises Error.")]
        public async Task DeleteContractWithMissingIDRaisesError()
        {
            await using var client = _network.NewClient();
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.DeleteContractAsync(null, _network.Payer);
            });
            Assert.Equal("contractToDelete", ane.ParamName);
            Assert.StartsWith("Contract to Delete is missing. Please check that it is not null.", ane.Message);
        }
        [Fact(DisplayName = "Contract Delete: Deleting with missing return crypto address raises Error.")]
        public async Task DeleteContractWithMissingReturnToAddressRaisesError()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, null);
            });
            Assert.Equal("transferToAddress", ane.ParamName);
            Assert.StartsWith("Transfer address is missing. Please check that it is not null.", ane.Message);
        }
        [Fact(DisplayName = "Contract Delete: Deleting with invalid return crypto address succeeds (IS THIS A NETWORK BUG?)")]
        public async Task DeleteContractWithInvalidReturnToAddressRaisesError()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            await using var fx2 = await TestAccount.CreateAsync(_network);
            var deleteAccountRecord = await fx2.Client.DeleteAccountAsync(new Account(fx2.Record.Address, fx2.PrivateKey), _network.Payer);
            Assert.Equal(ResponseCode.Success, deleteAccountRecord.Status);

            var deleteContractReceipt = await fx.Client.DeleteContractAsync(fx.ContractRecord.Contract, fx2.Record.Address);
            Assert.Equal(ResponseCode.Success, deleteContractReceipt.Status);
        }
    }
}
