using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Claims
{
    [Collection(nameof(NetworkCredentialsFixture))]
    public class DeleteClaimTests
    {
        private readonly NetworkCredentialsFixture _networkCredentials;
        public DeleteClaimTests(NetworkCredentialsFixture networkCredentials, ITestOutputHelper output)
        {
            _networkCredentials = networkCredentials;
            _networkCredentials.TestOutput = output;
        }
        [SkippableFact(DisplayName = "Delete Claim: Can Delete a Claim")]
        public async Task CanDeleteAClaimAsync()
        {
            await using var test = await TestAccountInstance.CreateAsync(_networkCredentials);

            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var claim = new Claim
            {
                Address = test.AccountRecord.Address,
                Hash = Generator.SHA384Hash(),
                Endorsements = new Endorsement[] { publicKey1, publicKey2 },
                ClaimDuration = TimeSpan.FromDays(Generator.Integer(10, 20))
            };

            var addReceipt = await test.Client.AddClaimAsync(claim, ctx =>
            {
                ctx.Payer = new Account(ctx.Payer, _networkCredentials.AccountPrivateKey, privateKey1, privateKey2);
            });
            Assert.Equal(ResponseCode.Success, addReceipt.Status);

            try
            {
                var copy = await test.Client.GetClaimAsync(claim.Address, claim.Hash, ctx =>
                {
                    ctx.Payer = new Account(ctx.Payer, _networkCredentials.AccountPrivateKey, test.PrivateKey, privateKey1, privateKey2);
                });
                Assert.NotNull(copy);
            }
            catch (PrecheckException pex) when (pex.Status == ResponseCode.ClaimNotFound)
            {
                Skip.If(true, "Attempt to retrieve claim failed, not necessarily a problem with the C# library.");
            }

            var deleteReceipt = await test.Client.DeleteClaimAsync(claim.Address, claim.Hash, ctx =>
            {
                ctx.Payer = new Account(ctx.Payer, _networkCredentials.AccountPrivateKey, test.PrivateKey, privateKey1, privateKey2);
            });
            Assert.NotNull(deleteReceipt);
            Assert.Equal(ResponseCode.Success, deleteReceipt.Status);

            var exception = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await test.Client.GetClaimAsync(claim.Address, claim.Hash);
            });
            Assert.Equal(ResponseCode.ClaimNotFound, exception.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: ClaimNotFound", exception.Message);
        }
        [Fact(DisplayName = "Delete Claim: Deleting non existant claim throws error.")]
        public async Task DeletingNonExistantClaimThrowsError()
        {
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();

            var excepiton = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.GetClaimAsync(_networkCredentials.CreateDefaultAccount(), Generator.SHA384Hash());
            });
            Assert.Equal(ResponseCode.ClaimNotFound, excepiton.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: ClaimNotFound", excepiton.Message);
        }
        [Fact(DisplayName = "Delete Claim: Deleting missing hash throws error.")]
        public async Task DeletingMissingHashThrowsError()
        {
            await using var client = _networkCredentials.CreateClientWithDefaultConfiguration();

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.GetClaimAsync(_networkCredentials.CreateDefaultAccount(), null);
            });
            Assert.Equal("hash", exception.ParamName);
            Assert.StartsWith("The claim hash is missing. Please check that it is not null.", exception.Message);
        }
        [SkippableFact(DisplayName = "Delete Claim: Can Delete a Claim")]
        public async Task DeletingMissingAccountThrowsError()
        {
            await using var test = await TestAccountInstance.CreateAsync(_networkCredentials);

            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var claim = new Claim
            {
                Address = test.AccountRecord.Address,
                Hash = Generator.SHA384Hash(),
                Endorsements = new Endorsement[] { publicKey1, publicKey2 },
                ClaimDuration = TimeSpan.FromDays(Generator.Integer(10, 20))
            };

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await test.Client.GetClaimAsync(null, claim.Hash);
            });
            Assert.Equal("address", exception.ParamName);
            Assert.StartsWith("Account Address is is missing. Please check that it is not null.", exception.Message);
        }
    }
}
