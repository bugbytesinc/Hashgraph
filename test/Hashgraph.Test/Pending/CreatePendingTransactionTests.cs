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
        [Fact(DisplayName = "Pending Transaction Create: Can Schedule an Identical Transfer Transaction After Execution")]
        public async Task CanScheduleAnIdenticalTransferTransactionAfterExecution()
        {
            await using var pendingFx = await TestPendingTransfer.CreateAsync(_network);
            // Make Sure Pending Payer has enough money to execute pending transactions
            await pendingFx.Client.TransferAsync(_network.Payer, pendingFx.PayingAccount, 20_00_000_000);
            await pendingFx.Client.SignPendingTransactionAsync(new SignPendingTransactionParams { 
                Pending = pendingFx.Record.Pending.Pending,
                TransactionBody = pendingFx.Record.Pending.TransactionBody,
                Signatory = pendingFx.SendingAccount
            });
            var firstPendingReceipt = await pendingFx.Client.GetReceiptAsync(pendingFx.Record.Id.AsPending());
            var xferAmount = pendingFx.SendingAccount.CreateParams.InitialBalance / 2;
            await AssertHg.CryptoBalanceAsync(pendingFx.SendingAccount, pendingFx.SendingAccount.CreateParams.InitialBalance - xferAmount);
            await AssertHg.CryptoBalanceAsync(pendingFx.ReceivingAccount, pendingFx.ReceivingAccount.CreateParams.InitialBalance + xferAmount);

            // Second Try, first reset account balances
            await pendingFx.Client.TransferAsync(pendingFx.ReceivingAccount, pendingFx.SendingAccount, (long)xferAmount, pendingFx.ReceivingAccount);
            await AssertHg.CryptoBalanceAsync(pendingFx.SendingAccount, pendingFx.SendingAccount.CreateParams.InitialBalance);
            await AssertHg.CryptoBalanceAsync(pendingFx.ReceivingAccount, pendingFx.ReceivingAccount.CreateParams.InitialBalance);

            var secondSchedulingRecord = await pendingFx.Client.TransferWithRecordAsync(pendingFx.TransferParams);
            Assert.Equal(ResponseCode.Success, secondSchedulingRecord.Status);
            Assert.NotEqual(pendingFx.Record.Pending.Pending, secondSchedulingRecord.Pending.Pending);
            Assert.Equal(pendingFx.Record.Pending.TransactionBody.ToArray(), secondSchedulingRecord.Pending.TransactionBody.ToArray());

            await pendingFx.Client.SignPendingTransactionAsync(new SignPendingTransactionParams
            {
                Pending = secondSchedulingRecord.Pending.Pending,
                TransactionBody = secondSchedulingRecord.Pending.TransactionBody,
                Signatory = pendingFx.SendingAccount
            });
            var secondPendingReceipt = await pendingFx.Client.GetReceiptAsync(pendingFx.Record.Id.AsPending());
            await AssertHg.CryptoBalanceAsync(pendingFx.SendingAccount, pendingFx.SendingAccount.CreateParams.InitialBalance - xferAmount);
            await AssertHg.CryptoBalanceAsync(pendingFx.ReceivingAccount, pendingFx.ReceivingAccount.CreateParams.InitialBalance + xferAmount);
        }

        // Check various forms of signing
        // Ensure bad signatures are ignored.
        // Schedule a transaction, then drain the payer's funds
        /// schedule a transaction, then delete the payer's account.
    }
}