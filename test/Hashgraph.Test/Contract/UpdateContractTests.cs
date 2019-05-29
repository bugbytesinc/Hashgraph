using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Contract
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class UpdateContractTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public UpdateContractTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [Fact(DisplayName = "Contract Update: Can Update Multiple Properties of Contract.")]
        public async Task CanUpdateMultiplePropertiesInOneCall()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);
            var (newPublicKey, newPrivateKey) = Generator.KeyPair();
            var newExpiration = Generator.TruncatedFutureDate(2400, 4800);
            var newEndorsement = new Endorsement(newPublicKey);
            var newRenewal = TimeSpan.FromDays(Generator.Integer(60, 90));
            var updatedPayer = new Account(fx.Payer, _networkCredentials.AccountPrivateKey, newPrivateKey);
            var newMemo = Generator.Code(50);
            fx.Client.Configure(ctx => ctx.Payer = updatedPayer);
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractCreateRecord.Contract,
                Expiration = newExpiration,
                Administrator = newEndorsement,
                RenewPeriod = newRenewal,
                Memo = newMemo
            });
            var info = await fx.Client.GetContractInfoAsync(fx.ContractCreateRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Address);
            //Assert.Equal(newExpiration, info.Expiration);
            Assert.Equal(newEndorsement, info.Administrator);
            Assert.Equal(newRenewal, info.RenewPeriod);
            Assert.Equal(newMemo, info.Memo);
        }

        [Fact(DisplayName = "Contract Update: Update Expiration Fails, but returns Success (network bug)")]
        public async Task UpdateContractExpirationDate()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);

            var oldExpiration = (await fx.Client.GetContractInfoAsync(fx.ContractCreateRecord.Contract)).Expiration;
            var newExpiration = oldExpiration.AddMonths(12);
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractCreateRecord.Contract,
                Expiration = newExpiration
            });
            var info = await fx.Client.GetContractInfoAsync(fx.ContractCreateRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Address);
            // This is what it should be.
            //Assert.Equal(newExpiration, info.Expiration);
            // This is what it is
            Assert.Equal(oldExpiration, info.Expiration);
            Assert.Equal(_networkCredentials.AccountPublicKey, info.Administrator);
            Assert.Equal(fx.CreateContractParams.RenewPeriod, info.RenewPeriod);
            Assert.Equal(fx.Memo, info.Memo);
        }

        [Fact(DisplayName = "Contract Update: Can Update Admin Key.")]
        public async Task CanUpdateAdminKey()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);
            var (newPublicKey, newPrivateKey) = Generator.KeyPair();
            var newEndorsement = new Endorsement(newPublicKey);
            var updatedPayer = new Account(fx.Payer, _networkCredentials.AccountPrivateKey, newPrivateKey);
            fx.Client.Configure(ctx => ctx.Payer = updatedPayer);
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractCreateRecord.Contract,
                Administrator = newEndorsement,
            });
            var info = await fx.Client.GetContractInfoAsync(fx.ContractCreateRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Address);
            Assert.Equal(newEndorsement, info.Administrator);
            Assert.Equal(fx.CreateContractParams.RenewPeriod, info.RenewPeriod);
            Assert.Equal(fx.Memo, info.Memo);
        }

        [Fact(DisplayName = "Contract Update: Can Update Renew Period.")]
        public async Task CanUpdateRenewPeriod()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);
            var newRenewal = TimeSpan.FromDays(Generator.Integer(60, 90));
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractCreateRecord.Contract,
                RenewPeriod = newRenewal,
            });
            var info = await fx.Client.GetContractInfoAsync(fx.ContractCreateRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Address);
            Assert.Equal(fx.CreateContractParams.Administrator, info.Administrator);
            Assert.Equal(newRenewal, info.RenewPeriod);
            Assert.Equal(fx.Memo, info.Memo);
        }
        [Fact(DisplayName = "Contract Update: Updating Contract Bytecode Silently Fails (network bug)")]
        public async Task CanUpdateContractBytecode()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);
            await using var fx2 = await StatefulContractInstance.SetupAsync(_networkCredentials);
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractCreateRecord.Contract,
                File = fx2.FileCreateRecord.File
            });
            var info = await fx.Client.GetContractInfoAsync(fx.ContractCreateRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Address);
            Assert.Equal(fx.CreateContractParams.Administrator, info.Administrator);
            Assert.Equal(fx.CreateContractParams.RenewPeriod, info.RenewPeriod);
            Assert.Equal(fx.Memo, info.Memo);

            // Call a method that was not part of the original contract to check update.
            // Note, this is a bug with the network, the call returned success but we are still 
            // able to call the old contract method which should no longer exist.
            var callRecord = await fx.Client.CallContractWithRecordAsync(new CallContractParams
            {
                Contract = fx.ContractCreateRecord.Contract,
                Gas = 30_000,
                FunctionName = "greet",
            });
            Assert.NotNull(callRecord);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.Empty(callRecord.Error);
            Assert.Equal("Hello, world!", callRecord.Result.As<string>());
        }
        [Fact(DisplayName = "Contract Update: Can Update Memo.")]
        public async Task CanUpdateMemo()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);
            var newMemo = Generator.Code(50);
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractCreateRecord.Contract,
                Memo = newMemo
            });
            var info = await fx.Client.GetContractInfoAsync(fx.ContractCreateRecord.Contract);
            Assert.NotNull(info);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Contract);
            Assert.Equal(fx.ContractCreateRecord.Contract, info.Address);
            Assert.Equal(fx.CreateContractParams.Administrator, info.Administrator);
            Assert.Equal(fx.CreateContractParams.RenewPeriod, info.RenewPeriod);
            Assert.Equal(newMemo, info.Memo);
        }
        [Fact(DisplayName = "Contract Update: Updating an immutable contract raises error.")]
        public async Task UpdatingImmutableContractRaisesError()
        {
            await using var fx = await GreetingContractInstance.SetupAsync(_networkCredentials);
            fx.CreateContractParams.Administrator = null;
            await fx.CompleteCreateAsync();
            var tex = await Assert.ThrowsAsync<TransactionException>(async () => {
                await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
                {
                    Contract = fx.ContractCreateRecord.Contract,
                    Memo = Generator.Code(50)
                });
            });
            Assert.Equal(ResponseCode.ModifyingImmutableContract, tex.Status);
            Assert.StartsWith("Unable to update Contract, status: ModifyingImmutableContract",tex.Message);
        }
        [Fact(DisplayName = "Contract Update: Updating an contract without admin key raises error.")]
        public async Task UpdatingContractWithoutAdminKeyRaisesError()
        {
            var (publicKey, privateKey) = Generator.KeyPair();
            await using var fx = await GreetingContractInstance.SetupAsync(_networkCredentials);
            fx.CreateContractParams.Administrator = publicKey;
            fx.Client.Configure(ctx => ctx.Payer = new Account(ctx.Payer, _networkCredentials.AccountPrivateKey, privateKey));
            await fx.CompleteCreateAsync();
            // First, Remove admin key from Payer's Account
            var tex = await Assert.ThrowsAsync<TransactionException>(async () => {
                fx.Client.Configure(ctx => ctx.Payer = new Account(ctx.Payer, _networkCredentials.AccountPrivateKey));
                await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
                {
                    Contract = fx.ContractCreateRecord.Contract,
                    Memo = Generator.Code(50)
                });
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to update Contract, status: InvalidSignature", tex.Message);

            // Try again with the admin key to prove we have the keys right
            fx.Client.Configure(ctx => ctx.Payer = new Account(ctx.Payer, _networkCredentials.AccountPrivateKey, privateKey));
            var record = await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
            {
                Contract = fx.ContractCreateRecord.Contract,
                Memo = Generator.Code(50)
            });
            Assert.Equal(ResponseCode.Success, record.Status);
        }
        [Fact(DisplayName = "Contract Update: Updating with empty contract address raises error.")]
        public async Task UpdateWithMissingContractRaisesError()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);
            var newMemo = Generator.Code(50);
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>{
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
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);            
            var ae = await Assert.ThrowsAsync<ArgumentException>(async () => {
                await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
                {
                    Contract = fx.ContractCreateRecord.Contract
                });
            });
            Assert.Equal("updateParameters", ae.ParamName);
            Assert.StartsWith("The Contract Updates contains no update properties, it is blank.", ae.Message);
        }
        [Fact(DisplayName = "Contract Update: Updating non-existant contract raises error.")]
        public async Task UpdateWithNonExistantContractRaisesError()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);
            var newMemo = Generator.Code(50);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () => {
                await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
                {
                    Contract = new Address(0,0,2),
                    Memo = newMemo
                });
            });
            Assert.Equal(ResponseCode.InvalidContractId, tex.Status);
            Assert.StartsWith("Unable to update Contract, status: InvalidContractId", tex.Message);
        }
        [Fact(DisplayName = "Contract Update: Updating invalid duration raises error.")]
        public async Task UpdateWithInvalidDurationRaisesError()
        {
            await using var fx = await GreetingContractInstance.CreateAsync(_networkCredentials);
            var newMemo = Generator.Code(50);
            var tex = await Assert.ThrowsAsync<PrecheckException>(async () => {
                await fx.Client.UpdateContractWithRecordAsync(new UpdateContractParams
                {
                    Contract = fx.ContractCreateRecord.Contract,
                    RenewPeriod = TimeSpan.FromDays(Generator.Integer(-90, -60))
                }); ;
            });
            Assert.Equal(ResponseCode.AutorenewDurationNotInRange, tex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: AutorenewDurationNotInRange", tex.Message);
        }
    }
}
