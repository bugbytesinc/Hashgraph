using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using Proto;
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

        [Fact(DisplayName = "Pending Transaction Create: Can Schedule a Transfer Transaction With Nonce")]
        public async Task CanScheduleATransferTransactionWithNonce()
        {
            var (nonce, _) = Generator.KeyPair();
            await using var pendingFx = await TestPendingTransfer.CreateAsync(_network, fx =>
            {
                fx.TransferParams.Signatory = new Signatory(
                        fx.PayingAccount.PrivateKey,
                        new ScheduleParams
                        {
                            Nonce = nonce,
                            PendingPayer = fx.PayingAccount,
                            Administrator = fx.PublicKey,
                            Signatory = fx.PrivateKey,
                            Memo = fx.Memo
                        });
            });
            Assert.Equal(ResponseCode.Success, pendingFx.Record.Status);
            Assert.NotNull(pendingFx.Record.Pending);

            var info = await pendingFx.Client.GetPendingTransactionInfoAsync(pendingFx.Record.Pending.Pending);
            var scheduledTx = TransactionBody.Parser.ParseFrom(info.TransactionBody.ToByteString());
            Assert.Equal(nonce.ToArray(), scheduledTx.TransactionID.Nonce.ToByteArray());
        }

        // Check various forms of signing
        // Ensure bad signatures are ignored.
        // Schedule a transaction, then drain the payer's funds
        /// schedule a transaction, then delete the payer's account.
    }
}