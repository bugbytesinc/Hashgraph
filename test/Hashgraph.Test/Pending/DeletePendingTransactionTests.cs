using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token
{
    [Collection(nameof(NetworkCredentials))]
    public class DeletePendingTransactionTests
    {
        private readonly NetworkCredentials _network;
        public DeletePendingTransactionTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Pending Transaction Delete: Can Delete Pending Transaction")]
        public async Task CanGetTokenInfo()
        {
            await using var fx = await TestPendingTransfer.CreateAsync(_network);
            Assert.Equal(ResponseCode.Success, fx.Record.Status);

            var receipt = await fx.Client.DeletePendingTransactionAsync(fx.Record.Pending.Id, ctx => ctx.Signatory = new Signatory(fx.PrivateKey, ctx.Signatory));
            Assert.Equal(ResponseCode.Success, receipt.Status);
        }
        [Fact(DisplayName = "NETWORK V0.21.0 DEFECT: Pending Transaction Delete: Deleting Pending Transaction Does Not Remove Pending Info")]
        public async Task DeletingPendingTransactionDoesNotRemovePendingInfoDefect()
        {
            // The network has a defect that does not serialize keys properly with get scheduled transaction info.
            var testFailException = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(DeletingPendingTransactionDoesNotRemovePendingInfo);
            Assert.StartsWith("The public key does not appear to be encoded in a recognizable", testFailException.Message);


            //[Fact(DisplayName = "Pending Transaction Delete: Deleting Pending Transaction Does Not Remove Pending Info")]
            async Task DeletingPendingTransactionDoesNotRemovePendingInfo()
            {
                await using var fx = await TestPendingTransfer.CreateAsync(_network);
                var receipt = await fx.Client.DeletePendingTransactionAsync(fx.Record.Pending.Id, ctx => ctx.Signatory = new Signatory(fx.PrivateKey, ctx.Signatory));
                var record = await fx.Client.GetTransactionRecordAsync(receipt.Id);

                var info = await fx.PayingAccount.Client.GetPendingTransactionInfoAsync(fx.Record.Pending.Id);
                Assert.Equal(fx.Record.Pending.Id, info.Id);
                Assert.Equal(fx.Record.Pending.TxId, info.TxId);
                Assert.Equal(_network.Payer, info.Creator);
                Assert.Equal(fx.PayingAccount, info.Payer);
                Assert.Single(info.Endorsements);
                Assert.Equal(new Endorsement(fx.PayingAccount.PublicKey), info.Endorsements[0]);
                Assert.Equal(new Endorsement(fx.PublicKey), info.Administrator);
                Assert.Equal(fx.Memo, info.Memo);
                Assert.True(info.Expiration > DateTime.MinValue);
                Assert.Null(info.Executed);
                Assert.Equal(record.Concensus, info.Deleted);
                Assert.False(info.PendingTransactionBody.IsEmpty);
                // NETWORK V0.21.0 DEFECT vvvv
                // NOT IMPLEMENTED YET
                Assert.Empty(info.Ledger.ToArray());
                // NETWORK V0.21.0 DEFECT: ^^^^
            }
        }
        [Fact(DisplayName = "Pending Transaction Delete: Can Delete Pending Transaction With Signatory Version")]
        public async Task CanGetTokenInfoWithSignatoryVersion()
        {
            await using var fx = await TestPendingTransfer.CreateAsync(_network);
            Assert.Equal(ResponseCode.Success, fx.Record.Status);

            var receipt = await fx.Client.DeletePendingTransactionAsync(fx.Record.Pending.Id, new Signatory(fx.PrivateKey));
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fx.PayingAccount.Client.GetPendingTransactionInfoAsync(fx.Record.Pending.Id);
            Assert.Equal(fx.Record.Pending.Id, info.Id);
            Assert.Equal(fx.Record.Pending.TxId, info.TxId);
            Assert.Equal(_network.Payer, info.Creator);
            Assert.Equal(fx.PayingAccount, info.Payer);
            Assert.Single(info.Endorsements);
            Assert.Equal(new Endorsement(fx.PayingAccount.PublicKey), info.Endorsements[0]);
            Assert.Equal(new Endorsement(fx.PublicKey), info.Administrator);
            Assert.Equal(fx.Memo, info.Memo);
            Assert.True(info.Expiration > DateTime.MinValue);
            Assert.Null(info.Executed);
            Assert.False(info.PendingTransactionBody.IsEmpty);
        }
    }
}
