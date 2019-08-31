using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentials))]
    public class UpdateContractTests
    {
        private readonly NetworkCredentials _network;
        public UpdateContractTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Contract Update: Can Update Multiple Properties of Contract.")]
        public async Task CanUpdateMultiplePropertiesInOneCall()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var (newPublicKey, newPrivateKey) = Generator.KeyPair();
            var newExpiration = Generator.TruncatedFutureDate(2400, 4800);
            var newEndorsement = new Endorsement(newPublicKey);
            var newRenewal = TimeSpan.FromDays(Generator.Integer(60, 90));
            var updatedPayer = _network.PayerWithKeys(newPrivateKey);
            var newMemo = Generator.Code(50);
            fx.Client.Configure(ctx => ctx.Payer = updatedPayer);
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Expiration = newExpiration,
                Administrator = newEndorsement,
                RenewPeriod = newRenewal,
                Memo = newMemo
            });
            var info = await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractRecord.Contract, info.Address);
            Assert.Equal(newExpiration, info.Expiration);
            Assert.Equal(newEndorsement, info.Administrator);
            Assert.Equal(newRenewal, info.RenewPeriod);
            Assert.Equal(newMemo, info.Memo);
        }

        [Fact(DisplayName = "Contract Update: Can Update Expiration Date")]
        public async Task UpdateContractExpirationDate()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var oldExpiration = (await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract)).Expiration;
            var newExpiration = oldExpiration.AddMonths(12);
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Expiration = newExpiration
            });
            var info = await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractRecord.Contract, info.Address);
            Assert.Equal(newExpiration, info.Expiration);
            Assert.Equal(_network.PublicKey, info.Administrator);
            Assert.Equal(fx.ContractParams.RenewPeriod, info.RenewPeriod);
            Assert.Equal(fx.Memo, info.Memo);
        }

        [Fact(DisplayName = "Contract Update: Can Update Admin Key.")]
        public async Task CanUpdateAdminKey()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var (newPublicKey, newPrivateKey) = Generator.KeyPair();
            var newEndorsement = new Endorsement(newPublicKey);
            var updatedPayer = _network.PayerWithKeys(newPrivateKey);
            fx.Client.Configure(ctx => ctx.Payer = updatedPayer);
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Administrator = newEndorsement,
            });
            var info = await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractRecord.Contract, info.Address);
            Assert.Equal(newEndorsement, info.Administrator);
            Assert.Equal(fx.ContractParams.RenewPeriod, info.RenewPeriod);
            Assert.Equal(fx.Memo, info.Memo);
        }

        [Fact(DisplayName = "Contract Update: Can Update Renew Period.")]
        public async Task CanUpdateRenewPeriod()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var newRenewal = TimeSpan.FromDays(Generator.Integer(60, 90));
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractRecord.Contract,
                RenewPeriod = newRenewal,
            });
            var info = await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractRecord.Contract, info.Address);
            Assert.Equal(fx.ContractParams.Administrator, info.Administrator);
            Assert.Equal(newRenewal, info.RenewPeriod);
            Assert.Equal(fx.Memo, info.Memo);
        }
        [Fact(DisplayName = "Contract Update: Updating Contract Bytecode Silently Fails (IS THIS A NETWORK BUG?)")]
        public async Task CanUpdateContractBytecode()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            await using var fx2 = await StatefulContract.SetupAsync(_network);
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractRecord.Contract,
                File = fx2.FileRecord.File
            });
            var info = await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractRecord.Contract, info.Address);
            Assert.Equal(fx.ContractParams.Administrator, info.Administrator);
            Assert.Equal(fx.ContractParams.RenewPeriod, info.RenewPeriod);
            Assert.Equal(fx.Memo, info.Memo);

            // Call a method that was not part of the original contract to check update.
            // Note, this is a bug with the network, the call returned success but we are still 
            // able to call the old contract method which should no longer exist.
            var callRecord = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Gas = 30_000,
                FunctionName = "greet",
            });
            Assert.NotNull(callRecord);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.Empty(callRecord.CallResult.Error);
            Assert.Equal("Hello, world!", callRecord.CallResult.Result.As<string>());
        }
        [Fact(DisplayName = "Contract Update: Can Update Memo.")]
        public async Task CanUpdateMemo()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var newMemo = Generator.Code(50);
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Memo = newMemo
            });
            var info = await fx.Client.GetContractInfoAsync(fx.ContractRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractRecord.Contract, info.Address);
            Assert.Equal(fx.ContractParams.Administrator, info.Administrator);
            Assert.Equal(fx.ContractParams.RenewPeriod, info.RenewPeriod);
            Assert.Equal(newMemo, info.Memo);
        }
        [Fact(DisplayName = "Contract Update: Updating an immutable contract raises error.")]
        public async Task UpdatingImmutableContractRaisesError()
        {
            await using var fx = await GreetingContract.SetupAsync(_network);
            fx.ContractParams.Administrator = null;
            await fx.CompleteCreateAsync();
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
                {
                    Contract = fx.ContractRecord.Contract,
                    Memo = Generator.Code(50)
                });
            });
            Assert.Equal(ResponseCode.ModifyingImmutableContract, tex.Status);
            Assert.StartsWith("Unable to update Contract, status: ModifyingImmutableContract", tex.Message);
        }
        [Fact(DisplayName = "Contract Update: Updating an contract without admin key raises error.")]
        public async Task UpdatingContractWithoutAdminKeyRaisesError()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var fx = await GreetingContract.SetupAsync(_network);
            fx.ContractParams.Administrator = publicKey;
            fx.Client.Configure(ctx => ctx.Payer = new Account(ctx.Payer, _network.PrivateKey, privateKey));
            await fx.CompleteCreateAsync();
            // First, Remove admin key from Payer's Account
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                fx.Client.Configure(ctx => ctx.Payer = new Account(ctx.Payer, _network.PrivateKey));
                await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
                {
                    Contract = fx.ContractRecord.Contract,
                    Memo = Generator.Code(50)
                });
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to update Contract, status: InvalidSignature", tex.Message);

            // Try again with the admin key to prove we have the keys right
            fx.Client.Configure(ctx => ctx.Payer = new Account(ctx.Payer, _network.PrivateKey, privateKey));
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractRecord.Contract,
                Memo = Generator.Code(50)
            });
            Assert.Equal(ResponseCode.Success, record.Status);
        }
        [Fact(DisplayName = "Contract Update: Updating with empty contract address raises error.")]
        public async Task UpdateWithMissingContractRaisesError()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var newMemo = Generator.Code(50);
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
                {
                    Memo = newMemo
                });
            });
            Assert.Equal("Contract", ane.ParamName);
            Assert.StartsWith("Contract address is missing. Please check that it is not null.", ane.Message);
        }
        [Fact(DisplayName = "Contract Update: Updating with no changes raises error.")]
        public async Task UpdateWithNoChangesRaisesError()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var ae = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
                {
                    Contract = fx.ContractRecord.Contract
                });
            });
            Assert.Equal("updateParameters", ae.ParamName);
            Assert.StartsWith("The Contract Updates contains no update properties, it is blank.", ae.Message);
        }
        [Fact(DisplayName = "Contract Update: Updating non-existant contract raises error.")]
        public async Task UpdateWithNonExistantContractRaisesError()
        {
            await using var fx = await TestFile.CreateAsync(_network);
            var invalidContractAddress = fx.Record.File;
            await fx.Client.DeleteFileAsync(invalidContractAddress);

            var newMemo = Generator.Code(50);
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
                {
                    Contract = invalidContractAddress,
                    Memo = newMemo
                });
            });
            Assert.Equal(ResponseCode.InvalidContractId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidContractId", pex.Message);
        }
        [Fact(DisplayName = "Contract Update: Updating invalid duration raises error.")]
        public async Task UpdateWithInvalidDurationRaisesError()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);
            var newMemo = Generator.Code(50);
            var tex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
                {
                    Contract = fx.ContractRecord.Contract,
                    RenewPeriod = TimeSpan.FromDays(Generator.Integer(-90, -60))
                }); ;
            });
            Assert.Equal(ResponseCode.AutorenewDurationNotInRange, tex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: AutorenewDurationNotInRange", tex.Message);
        }
    }
}
