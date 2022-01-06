using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Schedule
{
    [Collection(nameof(NetworkCredentials))]
    public class CreatePendingTransactionTests
    {
        private readonly NetworkCredentials _network;
        public CreatePendingTransactionTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Pending Transaction Create: Can Schedule a Transfer Transaction")]
        public async Task CanScheduleATransferTransaction()
        {
            await using var pendingFx = await TestPendingTransfer.CreateAsync(_network);
            Assert.Equal(ResponseCode.Success, pendingFx.Record.Status);
            Assert.NotNull(pendingFx.Record.Pending);
        }

        [Fact(DisplayName = "NETWORK V0.21.0 UNSUPPORTED: Pending Transaction Create: Can Schedule a Transfer Transaction with Alias Payer")]
        public async Task CanScheduleATransferTransactionWithAliasPayerDefect()
        {
            // Associating an asset with an account using its alias address has not yet been
            // implemented by the network, although it will accept the transaction.
            var testFailException = (await Assert.ThrowsAsync<TransactionException>(CanScheduleATransferTransactionWithAliasPayer));
            Assert.StartsWith("Unable to schedule transaction, status: InvalidAccountId", testFailException.Message);

            //[Fact(DisplayName = "Pending Transaction Create: Can Schedule a Transfer Transaction with Alias Payer")]
            async Task CanScheduleATransferTransactionWithAliasPayer()
            {
                await using var fxPayer = await TestAliasAccount.CreateAsync(_network);
                await using var pendingFx = await TestPendingTransfer.CreateAsync(_network, fx =>
                {
                    fx.TransferParams.Signatory = new Signatory(
                        fx.PayingAccount.PrivateKey,
                        fx.PrivateKey,
                        new PendingParams
                        {
                            PendingPayer = fxPayer.Alias,
                            Administrator = fx.PublicKey,
                            Memo = fx.Memo
                        });
                });
                Assert.Equal(ResponseCode.Success, pendingFx.Record.Status);
                Assert.NotNull(pendingFx.Record.Pending);
            }
        }

        // Check various forms of signing
        // Ensure bad signatures are ignored.
        // Schedule a transaction, then drain the payer's funds
        // schedule a transaction, then delete the payer's account.
    }
}