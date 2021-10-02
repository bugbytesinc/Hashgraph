using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Test.Fixtures
{
    public class TestPendingTransfer : IAsyncDisposable
    {
        public TestAccount SendingAccount;
        public TestAccount ReceivingAccount;
        public TestAccount PayingAccount;
        public Client Client;
        public TransferParams TransferParams;
        public TransactionRecord Record;
        public ReadOnlyMemory<byte> PublicKey;
        public ReadOnlyMemory<byte> PrivateKey;
        public string Memo;

        public NetworkCredentials Network;

        public static async Task<TestPendingTransfer> CreateAsync(NetworkCredentials networkCredentials, Action<TestPendingTransfer> customize = null)
        {
            var fx = new TestPendingTransfer();
            networkCredentials.Output?.WriteLine("STARTING SETUP: Scheduled Transfer Crypto Transaction");
            (fx.PublicKey, fx.PrivateKey) = Generator.KeyPair();
            fx.Network = networkCredentials;
            fx.Client = networkCredentials.NewClient();
            fx.SendingAccount = await TestAccount.CreateAsync(networkCredentials);
            fx.ReceivingAccount = await TestAccount.CreateAsync(networkCredentials);
            fx.PayingAccount = await TestAccount.CreateAsync(networkCredentials);
            fx.Memo = Generator.Code(10);
            var xferAmount = (long)fx.SendingAccount.CreateParams.InitialBalance / 2;
            fx.TransferParams = new TransferParams
            {
                CryptoTransfers = new[] {
                    KeyValuePair.Create(fx.SendingAccount.Record.Address, -xferAmount),
                    KeyValuePair.Create(fx.ReceivingAccount.Record.Address, xferAmount)
                },
                Signatory = new Signatory(
                    fx.PayingAccount.PrivateKey,
                    fx.PrivateKey,
                    new PendingParams
                    {
                        PendingPayer = fx.PayingAccount,
                        Administrator = fx.PublicKey,
                        Memo = fx.Memo
                    })
            };
            customize?.Invoke(fx);
            try
            {
                fx.Record = await fx.Client.TransferWithRecordAsync(fx.TransferParams);
            }
            catch (TransactionException ex) when (ex.Message?.StartsWith("The Network Changed the price of Retrieving a Record while attempting to retrieve this record") == true)
            {
                var record = await fx.Client.GetTransactionRecordAsync(ex.TxId);
                if (record is not null)
                {
                    fx.Record = record;
                }
                else
                {
                    throw;
                }
            }
            Assert.Equal(ResponseCode.Success, fx.Record.Status);
            networkCredentials.Output?.WriteLine("SETUP COMPLETED: Scheduled Transfer Crypto Transaction");
            return fx;
        }

        public async ValueTask DisposeAsync()
        {
            Network.Output?.WriteLine("STARTING TEARDOWN: Scheduled Transfer Crypto Transaction");
            try
            {
                if (!PrivateKey.IsEmpty)
                {
                    await Client.DeletePendingTransactionAsync(Record.Pending.Id, PrivateKey, ctx =>
                    {
                        ctx.Memo = "TestPendingTransfer Teardown: Delete Pending TX (may already be deleted)";
                    });
                }
            }
            catch
            {
                //noop
            }
            await ReceivingAccount.DisposeAsync();
            await SendingAccount.DisposeAsync();
            await PayingAccount.DisposeAsync();
            await Client.DisposeAsync();
            Network.Output?.WriteLine("TEARDOWN COMPLETED: Scheduled Transfer Crypto Transaction");
        }
    }
}