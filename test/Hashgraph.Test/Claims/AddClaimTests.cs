using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Claims
{
    [Collection(nameof(NetworkCredentials))]
    public class AddClaimTests
    {
        private readonly NetworkCredentials _network;
        public AddClaimTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Add Claim: Can Add a Claim")]
        public async Task CanCreateAClaimAsync()
        {
            await using var test = await TestAccount.CreateAsync(_network);

            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var claim = new Claim
            {
                Address = test.AccountRecord.Address,
                Hash = Generator.SHA384Hash(),
                Endorsements = new Endorsement[] { publicKey1, publicKey2 },
                ClaimDuration = TimeSpan.FromDays(Generator.Integer(10, 20))
            };

            var receipt = await test.Client.AddClaimAsync(claim, ctx =>
            {
                ctx.Payer = _network.PayerWithKeys(privateKey1, privateKey2);
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);
        }
        [Fact(DisplayName = "Add Claim: Adding claim without address raises error")]
        public async Task AddingClaimWithoutAddressThrowsError()
        {
            await using var client = _network.NewClient();

            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var claim = new Claim
            {
                Hash = Generator.SHA384Hash(),
                Endorsements = new Endorsement[] { publicKey1, publicKey2 },
                ClaimDuration = TimeSpan.FromDays(Generator.Integer(10, 20))
            };

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await client.AddClaimAsync(claim, ctx =>
                {
                    ctx.Payer = new Account(ctx.Payer, _network.PrivateKey, privateKey1, privateKey2);
                });
            });
            Assert.Equal("Address", exception.ParamName);
            Assert.StartsWith("The address to attach the claim to is missing", exception.Message);
        }
        [Fact(DisplayName = "Add Claim: Adding claim without hash throws error")]
        public async Task AddingClaimWithoutHashThrowsError()
        {
            await using var test = await TestAccount.CreateAsync(_network);

            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var claim = new Claim
            {
                Address = test.AccountRecord.Address,
                Endorsements = new Endorsement[] { publicKey1, publicKey2 },
                ClaimDuration = TimeSpan.FromDays(Generator.Integer(10, 20))
            };

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await test.Client.AddClaimAsync(claim, ctx =>
                {
                    ctx.Payer = new Account(ctx.Payer, _network.PrivateKey, privateKey1);
                });
            });
            Assert.Equal("Hash", exception.ParamName);
            Assert.StartsWith("The claim hash is missing. Please check that it ", exception.Message);
        }
        [Fact(DisplayName = "Add Claim: Adding claim with invalid hash throws error")]
        public async Task AddingClaimWitInvalidHashThrowsError()
        {
            await using var test = await TestAccount.CreateAsync(_network);

            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var claim = new Claim
            {
                Address = test.AccountRecord.Address,
                Hash = System.Text.Encoding.ASCII.GetBytes("Bad Hash"),
                Endorsements = new Endorsement[] { publicKey1, publicKey2 },
                ClaimDuration = TimeSpan.FromDays(Generator.Integer(10, 20))
            };

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await test.Client.AddClaimAsync(claim, ctx =>
                {
                    ctx.Payer = new Account(ctx.Payer, _network.PrivateKey, privateKey1);
                });
            });
            Assert.Equal("Hash", exception.ParamName);
            Assert.StartsWith("The claim hash is expected to be 48 bytes in length.", exception.Message);
        }
        [Fact(DisplayName = "Add Claim: Adding claim without endorsements throws error")]
        public async Task AddingClaimWithoutEndorsementsThrowsError()
        {
            await using var test = await TestAccount.CreateAsync(_network);

            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var claim = new Claim
            {
                Address = test.AccountRecord.Address,
                Hash = Generator.SHA384Hash(),
                ClaimDuration = TimeSpan.FromDays(Generator.Integer(10, 20))
            };

            var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await test.Client.AddClaimAsync(claim, ctx =>
                {
                    ctx.Payer = new Account(ctx.Payer, _network.PrivateKey, privateKey1);
                });
            });
            Assert.Equal("Endorsements", exception.ParamName);
            Assert.StartsWith("The endorsements property is missing. Please check that it is not null.", exception.Message);
        }
        [Fact(DisplayName = "Add Claim: Adding claim with empty endorsements throws error")]
        public async Task AddingClaimWithEmptyEndorsementsThrowsError()
        {
            await using var test = await TestAccount.CreateAsync(_network);

            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var claim = new Claim
            {
                Address = test.AccountRecord.Address,
                Hash = Generator.SHA384Hash(),
                Endorsements = new Endorsement[0],
                ClaimDuration = TimeSpan.FromDays(Generator.Integer(10, 20))
            };

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await test.Client.AddClaimAsync(claim, ctx =>
                {
                    ctx.Payer = new Account(ctx.Payer, _network.PrivateKey, privateKey1);
                });
            });
            Assert.Equal("Endorsements", exception.ParamName);
            Assert.StartsWith("The endorsements array is empty. Please must include at least one endorsement.", exception.Message);
        }
        [Fact(DisplayName = "Add Claim: Adding claim without duration throws error")]
        public async Task AddingClaimWithoutDurationThrowsError()
        {
            await using var test = await TestAccount.CreateAsync(_network);

            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var claim = new Claim
            {
                Address = test.AccountRecord.Address,
                Hash = Generator.SHA384Hash(),
                Endorsements = new Endorsement[] { publicKey1, publicKey2 }
            };

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await test.Client.AddClaimAsync(claim, ctx =>
                {
                    ctx.Payer = new Account(ctx.Payer, _network.PrivateKey, privateKey1, privateKey2);
                });
            });
            Assert.Equal("ClaimDuration", exception.ParamName);
            Assert.StartsWith("Claim Duration must have some length.", exception.Message);
        }
        [Fact(DisplayName = "Add Claim: Adding claim without all signatures throws error")]
        public async Task AddingClaimWithoutAllSignaturesThrowsError()
        {
            await using var test = await TestAccount.CreateAsync(_network);

            var (publicKey1, privateKey1) = Generator.KeyPair();
            var (publicKey2, privateKey2) = Generator.KeyPair();
            var claim = new Claim
            {
                Address = test.AccountRecord.Address,
                Hash = Generator.SHA384Hash(),
                Endorsements = new Endorsement[] { publicKey1, publicKey2 },
                ClaimDuration = TimeSpan.FromDays(Generator.Integer(10, 20))
            };

            var exception = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await test.Client.AddClaimAsync(claim, ctx =>
                {
                    ctx.Payer = new Account(ctx.Payer, _network.PrivateKey, privateKey1);
                });
            });
            Assert.Equal(ResponseCode.InvalidSignature, exception.Status);
            Assert.StartsWith("Unable to attach claim, status: InvalidSignature", exception.Message);
        }
    }
}
