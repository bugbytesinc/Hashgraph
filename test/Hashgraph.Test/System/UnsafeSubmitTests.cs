using Google.Protobuf;
using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.System
{
    [Collection(nameof(NetworkCredentials))]
    public class UnsafeSubmitTests
    {
        private readonly NetworkCredentials _network;
        public UnsafeSubmitTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Unsafe Submit: Submit Unsafe Transaction")]
        public async Task SubmitUnsafeTransaction()
        {
            await using var client = _network.NewClient();
            var systemAddress = await _network.GetSystemAccountAddress();
            if (systemAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Administrator Account.");
                return;
            }
            // Ok, lets build a TX from Scratch, Including a Signature
            var txid = client.CreateNewTxId();
            var transfers = new Proto.TransferList();
            transfers.AccountAmounts.Add(new Proto.AccountAmount
            {
                AccountID = new Proto.AccountID(_network.Payer),
                Amount = -1
            });
            transfers.AccountAmounts.Add(new Proto.AccountAmount
            {
                AccountID = new Proto.AccountID(_network.Gateway),
                Amount = 1
            });
            var body = new Proto.TransactionBody
            {
                TransactionID = new Proto.TransactionID(txid),
                NodeAccountID = new Proto.AccountID(_network.Gateway),
                TransactionFee = 30_00_000_000,
                TransactionValidDuration = new Proto.Duration { Seconds = 180 },
                Memo = "Unsafe Test",
                CryptoTransfer = new Proto.CryptoTransferTransactionBody { Transfers = transfers }
            };
            var invoice = new Invoice(body);
            await (_network.Signatory as ISignatory).SignAsync(invoice);
            var transaction = new Proto.Transaction
            {
                SignedTransactionBytes = invoice.GetSignedTransaction(6).ToByteString()
            };

            var receipt = await client.SubmitUnsafeTransactionAsync(transaction.ToByteArray(), ctx => ctx.Payer = systemAddress);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(txid, receipt.Id);
        }
        [Fact(DisplayName = "Unsafe Submit: Submit Unsafe Transaction With Record")]
        public async Task SubmitUnsafeTransactionWithRecord()
        {
            await using var client = _network.NewClient();
            var systemAddress = await _network.GetSystemAccountAddress();
            if (systemAddress is null)
            {
                _network.Output?.WriteLine("TEST SKIPPED: No access to System Administrator Account.");
                return;
            }
            // Ok, lets build a TX from Scratch, Including a Signature
            var txid = client.CreateNewTxId();
            var transfers = new Proto.TransferList();
            transfers.AccountAmounts.Add(new Proto.AccountAmount
            {
                AccountID = new Proto.AccountID(_network.Payer),
                Amount = -1
            });
            transfers.AccountAmounts.Add(new Proto.AccountAmount
            {
                AccountID = new Proto.AccountID(_network.Gateway),
                Amount = 1
            });
            var body = new Proto.TransactionBody
            {
                TransactionID = new Proto.TransactionID(txid),
                NodeAccountID = new Proto.AccountID(_network.Gateway),
                TransactionFee = 30_00_000_000,
                TransactionValidDuration = new Proto.Duration { Seconds = 180 },
                Memo = "Unsafe Test",
                CryptoTransfer = new Proto.CryptoTransferTransactionBody { Transfers = transfers }
            };
            var invoice = new Invoice(body);
            await (_network.Signatory as ISignatory).SignAsync(invoice);
            var transaction = new Proto.Transaction
            {
                SignedTransactionBytes = invoice.GetSignedTransaction(6).ToByteString()
            };

            var record = await client.SubmitUnsafeTransactionWithRecordAsync(transaction.ToByteArray(), ctx => ctx.Payer = systemAddress);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.Equal(txid, record.Id);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Equal("Unsafe Test", record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Equal(3, record.Transfers.Count);
            Assert.Equal(-1 - (long)record.Fee, record.Transfers[_network.Payer]);
        }
    }
}
