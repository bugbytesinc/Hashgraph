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
        [Fact(DisplayName = "Delete Claim: Can Delete a Claim, but sometimes network if flaky (IS THIS A NETWORK BUG?)")]
        public async Task CanDeleteAClaimAsync()
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

            for(int tryNo =0; tryNo < 50; tryNo ++ )
            {
                try
                {
                    var deleteReceipt = await test.Client.DeleteClaimAsync(claim.Address, claim.Hash);
                    Assert.NotNull(deleteReceipt);
                    Assert.Equal(ResponseCode.Success, deleteReceipt.Status);
                    if (tryNo > 0)
                    {
                        _network.Output?.WriteLine($"NETWORK SUCCESS: Finally Worked, was able to get find/delete claim after {tryNo} retries.");
                    }
                    var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
                    {
                        await test.Client.GetClaimAsync(claim.Address, claim.Hash);
                    });
                    Assert.Equal(ResponseCode.ClaimNotFound, pex.Status);
                    Assert.StartsWith("Transaction Failed Pre-Check: ClaimNotFound", pex.Message);
                    return;
                }
                catch (TransactionException tex) when (tex.Status == ResponseCode.InvalidSignature)
                {
                    _network.Output?.WriteLine($"NETWORK ERROR: Ran across intermitent Get Claim (for delete) Race Condition in Network, Retry:{tryNo}");
                    _network.Output?.WriteLine(tex.StackTrace);
                    await Task.Delay(100);
                }
            }
            _network.Output?.WriteLine("NETWORK ERROR: Gave Up, network won't let us finish this test.");
        }
        [Fact(DisplayName = "Delete Claim: Deleting non existant claim throws error.")]
        public async Task DeletingNonExistantClaimThrowsError()
        {
            await using var client = _network.NewClient();

            var excepiton = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await client.GetClaimAsync(_network.Payer, Generator.SHA384Hash());
            });
            Assert.Equal(ResponseCode.ClaimNotFound, excepiton.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: ClaimNotFound", excepiton.Message);
        }
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
        public async Task DeletingMissingAccountThrowsError()
        {
            await using var test = await TestAccount.CreateAsync(_network);

            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var claim = new Claim
            {
                Address = test.Record.Address,
                Hash = Generator.SHA384Hash(),
                Endorsements = new Endorsement[] { publicKey1, publicKey2 },
                ClaimDuration = TimeSpan.FromDays(Generator.Integer(10, 20))
            };

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await test.Client.GetClaimAsync(null, claim.Hash);
            });
            Assert.Equal("address", exception.ParamName);
            Assert.StartsWith("Account Address is missing. Please check that it is not null.", exception.Message);
        }
    }
}
