using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Schedule
{
    [Collection(nameof(NetworkCredentials))]
    public class SignPendingTransactionTests
    {
        private readonly NetworkCredentials _network;
        public SignPendingTransactionTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Pending Transaction Sign: Can Sign a Pending Transfer Transaction")]
        public async Task CanSignAPendingTransferTransaction()
        {
            await using var pendingFx = await TestPendingTransfer.CreateAsync(_network);
            Assert.Equal(ResponseCode.Success, pendingFx.Record.Status);

            var receipt = await pendingFx.Client.SignPendingTransactionAsync(new SignPendingTransactionParams
            {
                Pending = pendingFx.Record.Pending.Pending,
                TransactionBody = pendingFx.Record.Pending.TransactionBody,
                Signatory = pendingFx.SendingAccount.PrivateKey
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.NotEqual(TxId.None, receipt.Id);
            Assert.Null(receipt.Pending);
            Assert.NotNull(receipt.CurrentExchangeRate);
            Assert.InRange(receipt.CurrentExchangeRate.Expiration, DateTime.MinValue, DateTime.MaxValue);
            Assert.NotNull(receipt.NextExchangeRate);
            Assert.InRange(receipt.NextExchangeRate.Expiration, DateTime.MinValue, DateTime.MaxValue);
        }
        [Fact(DisplayName = "Pending Transaction Sign: Can Sign a Pending Transfer Transaction and get Record")]
        public async Task CanSignAPendingTransferTransactionAndGetRecord()
        {
            await using var pendingFx = await TestPendingTransfer.CreateAsync(_network);
            Assert.Equal(ResponseCode.Success, pendingFx.Record.Status);

            var record = await pendingFx.Client.SignPendingTransactionWithRecordAsync(new SignPendingTransactionParams
            {
                Pending = pendingFx.Record.Pending.Pending,
                TransactionBody = pendingFx.Record.Pending.TransactionBody,
                Signatory = pendingFx.SendingAccount.PrivateKey
            });
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.NotEqual(TxId.None, record.Id);
            Assert.Null(record.Pending);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.InRange(record.CurrentExchangeRate.Expiration, DateTime.MinValue, DateTime.MaxValue);
            Assert.NotNull(record.NextExchangeRate);
            Assert.InRange(record.NextExchangeRate.Expiration, DateTime.MinValue, DateTime.MaxValue);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        }
        [Fact(DisplayName = "Pending Transaction Sign: Signing a Transaction alters the Endorsements List")]
        public async Task SigningATransactionAltersTheEndorsementsList()
        {
            await using var pendingFx = await TestPendingTransfer.CreateAsync(_network, fx =>
            {
                fx.TransferParams.Signatory = new ScheduleParams
                {
                    PendingPayer = fx.PayingAccount,
                    Administrator = fx.PublicKey,
                    Signatory = fx.PrivateKey,
                    Memo = fx.Memo
                };
            });
            var info = await pendingFx.PayingAccount.Client.GetPendingTransactionInfoAsync(pendingFx.Record.Pending.Pending);
            Assert.Empty(info.Endorsements);

            await pendingFx.Client.SignPendingTransactionWithRecordAsync(new SignPendingTransactionParams
            {
                Pending = pendingFx.Record.Pending.Pending,
                TransactionBody = pendingFx.Record.Pending.TransactionBody,
                Signatory = pendingFx.SendingAccount.PrivateKey
            });
            info = await pendingFx.PayingAccount.Client.GetPendingTransactionInfoAsync(pendingFx.Record.Pending.Pending);
            Assert.Single(info.Endorsements);

            await pendingFx.Client.SignPendingTransactionWithRecordAsync(new SignPendingTransactionParams
            {
                Pending = pendingFx.Record.Pending.Pending,
                TransactionBody = pendingFx.Record.Pending.TransactionBody,
                Signatory = pendingFx.PayingAccount.PrivateKey
            });

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await pendingFx.PayingAccount.Client.GetPendingTransactionInfoAsync(pendingFx.Record.Pending.Pending);
            });
            Assert.Equal(ResponseCode.InvalidScheduleId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidScheduleId", pex.Message);
        }

        // todo add the delete to the cleanup
    }
}