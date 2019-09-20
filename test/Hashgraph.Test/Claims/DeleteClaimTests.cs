using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Claims
{
    [Collection(nameof(NetworkCredentials))]
    public class DeleteClaimTests
    {
        private readonly NetworkCredentials _network;
        public DeleteClaimTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "NOT SUPPORTED: Delete Claim: Can Delete a Claim")]
        public async Task CanDeleteAClaimNotSupported()
        {
            Assert.Equal(ResponseCode.InsufficientTxFee, (await Assert.ThrowsAsync<PrecheckException>(CanDeleteAClaim)).Status);

            //[Fact(DisplayName = "Delete Claim: Can Delete a Claim")]
            async Task CanDeleteAClaim()
            {
                await using var test = await TestAccount.CreateAsync(_network);

                var claim = new Claim
                {
                    Address = test.Record.Address,
                    Hash = Generator.SHA384Hash(),
                    Endorsements = new Endorsement[] { _network.PublicKey },
                    ClaimDuration = TimeSpan.FromTicks(Generator.TruncatedFutureDate(24, 48).Ticks)
                };

                var addReceipt = await test.Client.AddClaimAsync(claim);
                Assert.Equal(ResponseCode.Success, addReceipt.Status);

                var deleteReceipt = await test.Client.DeleteClaimAsync(claim.Address, claim.Hash);
                Assert.NotNull(deleteReceipt);
                Assert.Equal(ResponseCode.Success, deleteReceipt.Status);

                var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
                {
                    await test.Client.GetClaimAsync(claim.Address, claim.Hash);
                });
                Assert.Equal(ResponseCode.ClaimNotFound, pex.Status);
                Assert.StartsWith("Transaction Failed Pre-Check: ClaimNotFound", pex.Message);
            }
        }
        //[Fact(DisplayName = "NOT SUPPORTED: Delete Claim: Deleting non existant claim throws error")]
        //public async Task DeletingNonExistantClaimThrowsErrorNotSupported()
        //{
        //    Assert.StartsWith("Assert.Equal() Failure", (await Assert.ThrowsAsync<Xunit.Sdk.EqualException>(DeletingNonExistantClaimThrowsError)).Message);

        //    //[Fact(DisplayName = "Delete Claim: Deleting non existant claim throws error")]
        //    async Task DeletingNonExistantClaimThrowsError()
        //    {
        //        await using var client = _network.NewClient();

        //        var excepiton = await Assert.ThrowsAsync<PrecheckException>(async () =>
        //        {
        //            await client.GetClaimAsync(_network.Payer, Generator.SHA384Hash());
        //        });
        //        Assert.Equal(ResponseCode.ClaimNotFound, excepiton.Status);
        //        Assert.StartsWith("Transaction Failed Pre-Check: ClaimNotFound", excepiton.Message);
        //    }
        //}
        [Fact(DisplayName = "Delete Claim: Deleting missing hash throws error.")]
        public async Task DeletingMissingHashThrowsError()
        {
            await using var client = _network.NewClient();

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.GetClaimAsync(_network.Payer, null);
            });
            Assert.Equal("hash", exception.ParamName);
            Assert.StartsWith("The claim hash is missing. Please check that it is not null.", exception.Message);
        }
        [Fact(DisplayName = "Delete Claim: Missing Address throws error.")]
        public async Task DeletingMissingAddressThrowsError()
        {
            await using var client = _network.NewClient();

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.GetClaimAsync(null, Generator.SHA384Hash());
            });
            Assert.Equal("address", exception.ParamName);
            Assert.StartsWith("Account Address is missing. Please check that it is not null.", exception.Message);
        }
    }
}
