using Hashgraph.Test.Fixtures;
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
        //[Fact(DisplayName = "NOT SUPPORTED: Get Claim: Can Reterieve a Claim, but admin keys are wrong (IS THIS A NETWORK BUG?)")]
        //public async Task CanRetriveAClaimAsyncNotSupported()
        //{
        //    Assert.Equal(ResponseCode.NotSupported, (await Assert.ThrowsAsync<PrecheckException>(CanRetriveAClaimAsync)).Status);

        //    //[Fact(DisplayName = "Get Claim: Can Reterieve a Claim, but admin keys are wrong (IS THIS A NETWORK BUG?): NOT SUPPORTED")]
        //    async Task CanRetriveAClaimAsync()
        //    {
        //        await using var accountFx = await TestAccount.CreateAsync(_network);

        //        var (publicKey1, privateKey1) = Generator.KeyPair();
        //        var (publicKey2, privateKey2) = Generator.KeyPair();
        //        var claim = new Claim
        //        {
        //            Address = accountFx.Record.Address,
        //            Hash = new SHA384Managed().ComputeHash(Generator.KeyPair().publicKey.ToArray()),
        //            Endorsements = new Endorsement[] { publicKey1, publicKey2 },
        //            ClaimDuration = TimeSpan.FromDays(Generator.Integer(100, 200))
        //        };

        //        accountFx.Client.Configure(ctx =>
        //        {
        //            ctx.Payer = _network.PayerWithKeys(privateKey1, privateKey2);
        //        });

        //        var receipt = await accountFx.Client.AddClaimAsync(claim);
        //        Assert.Equal(ResponseCode.Success, receipt.Status);

        //        var copy = await accountFx.Client.GetClaimAsync(claim.Address, claim.Hash);
        //        Assert.NotNull(copy);
        //        // We cannot do this because of a
        //        // network bug
        //        //Assert.Equal(claim, copy);
        //        // instead we must do this:
        //        Assert.Equal(claim.Address, copy.Address);
        //        Assert.Equal(claim.Hash.ToArray(), copy.Hash.ToArray());
        //        Assert.Equal(new Endorsement(claim.Endorsements), copy.Endorsements[0]); // Note: this is a bug in the network.
        //        Assert.Equal(claim.ClaimDuration, copy.ClaimDuration);
        //    }
        //}
        //[Fact(DisplayName = "NOT SUPPORTED: Get Claim: Retrieving Non-Existent Claim throws Error")]
        //public async Task RetriveAClaimThatDoesNotExistThrowsExcepitonNotSupported()
        //{
        //    Assert.StartsWith("Assert.Equal() Failure", (await Assert.ThrowsAsync<Xunit.Sdk.EqualException>(RetriveAClaimThatDoesNotExistThrowsExcepiton)).Message);

        //    //[Fact(DisplayName = "Get Claim: Retrieving Non-Existent Claim throws Error")]
        //    async Task RetriveAClaimThatDoesNotExistThrowsExcepiton()
        //    {
        //        await using var accountFx = await TestAccount.CreateAsync(_network);

        //        var (publicKey1, privateKey1) = Generator.KeyPair();
        //        var (publicKey2, privateKey2) = Generator.KeyPair();
        //        var claim = new Claim
        //        {
        //            Address = accountFx.Record.Address,
        //            Hash = Generator.SHA384Hash(),
        //            Endorsements = new Endorsement[] { publicKey1, publicKey2 },
        //            ClaimDuration = TimeSpan.FromDays(Generator.Integer(100, 200))
        //        };

        //        accountFx.Client.Configure(ctx =>
        //        {
        //            ctx.Payer = new Account(ctx.Payer, _network.PrivateKey, privateKey1, privateKey2);
        //        });

        //        var exception = await Assert.ThrowsAsync<PrecheckException>(async () =>
        //        {
        //            await accountFx.Client.GetClaimAsync(claim.Address, claim.Hash);
        //        });
        //        Assert.Equal(ResponseCode.ClaimNotFound, exception.Status);
        //        Assert.StartsWith("Transaction Failed Pre-Check: ClaimNotFound", exception.Message);
        //    }
        //}
    }
}
