using Hashgraph.Test.Fixtures;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Claims
{
    [Collection(nameof(NetworkCredentials))]
    public class GetClaimTests
    {
        private readonly NetworkCredentials _network;
        public GetClaimTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Get Claim: Can Reterieve a Claim")]
        public async Task CanRetriveAClaimAsync()
        {
            await using var accountFx = await TestAccount.CreateAsync(_network);

            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var claim = new Claim
            {
                Address = accountFx.AccountRecord.Address,
                Hash = new SHA384Managed().ComputeHash(Generator.KeyPair().publicKey.ToArray()),
                Endorsements = new Endorsement[] { publicKey1, publicKey2 },
                ClaimDuration = TimeSpan.FromDays(Generator.Integer(100, 200))
            };

            accountFx.Client.Configure(ctx =>
            {
                ctx.Payer = new Account(ctx.Payer, _network.PrivateKey, privateKey1, privateKey2);
            });

            var receipt = await accountFx.Client.AddClaimAsync(claim);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            try
            {
                var copy = await accountFx.Client.GetClaimAsync(claim.Address, claim.Hash);
                Assert.NotNull(copy);
                // We cannot do this because of a
                // network bug
                //Assert.Equal(claim, copy);
                // instead we must do this:
                Assert.Equal(claim.Address, copy.Address);
                Assert.Equal(claim.Hash.ToArray(), copy.Hash.ToArray());
                Assert.Equal(new Endorsement(claim.Endorsements), copy.Endorsements[0]); // Note: this is a bug in the network.
                Assert.Equal(claim.ClaimDuration, copy.ClaimDuration);
            }
            catch (PrecheckException pex) when (pex.Status == ResponseCode.ClaimNotFound)
            {
                _network.Output.WriteLine("Attempt to retrieve claim failed, not necessarily a problem with the C# library.  Skipping unit test.");
            }
        }
        [Fact(DisplayName = "Get Claim: Retrieving Non-Existent Claim throws")]
        public async Task RetriveAClaimThatDoesNotExistThrowsExcepiton()
        {
            await using var accountFx = await TestAccount.CreateAsync(_network);

            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var claim = new Claim
            {
                Address = accountFx.AccountRecord.Address,
                Hash = Generator.SHA384Hash(),
                Endorsements = new Endorsement[] { publicKey1, publicKey2 },
                ClaimDuration = TimeSpan.FromDays(Generator.Integer(100, 200))
            };

            accountFx.Client.Configure(ctx =>
            {
                ctx.Payer = new Account(ctx.Payer, _network.PrivateKey, privateKey1, privateKey2);
            });

            var exception = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await accountFx.Client.GetClaimAsync(claim.Address, claim.Hash);
            });
            Assert.Equal(ResponseCode.ClaimNotFound, exception.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: ClaimNotFound", exception.Message);
        }
    }
}
