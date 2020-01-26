using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentials))]
    public class TransferTests
    {
        private readonly NetworkCredentials _network;
        public TransferTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Transfer: Can Send to Gateway Node")]
        public async Task CanTransferCryptoToGatewayNode()
        {
            long fee = 0;
            long transferAmount = 10;
            await using var client = _network.NewClient();
            client.Configure(ctx => fee = ctx.FeeLimit);
            var fromAccount = _network.Payer;
            var toAddress = _network.Gateway;
            var balanceBefore = await client.GetAccountBalanceAsync(fromAccount);
            var receipt = await client.TransferAsync(fromAccount, toAddress, transferAmount);
            var balanceAfter = await client.GetAccountBalanceAsync(fromAccount);
            var maxFee = (ulong)(3 * fee);
            Assert.InRange(balanceAfter, balanceBefore - (ulong)transferAmount - maxFee, balanceBefore - (ulong)transferAmount);
        }
        [Fact(DisplayName = "Transfer: Can Send to New Account")]
        public async Task CanTransferCryptoToNewAccount()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var transferAmount = (long)Generator.Integer(10, 100);
            var newBalance = await fx.Client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance, newBalance);

            var receipt = await fx.Client.TransferAsync(_network.Payer, fx.Record.Address, transferAmount);
            var newBalanceAfterTransfer = await fx.Client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance + (ulong)transferAmount, newBalanceAfterTransfer);
        }
        [Fact(DisplayName = "Transfer: Can Send to multitransfer to New Account")]
        public async Task CanMultiTransferCryptoToNewAccount()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var transferAmount = (long)Generator.Integer(10, 100);
            var newBalance = await fx.Client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance, newBalance);

            var transfers = new Dictionary<Address, long> { { _network.Payer, -transferAmount }, { fx.Record.Address, transferAmount } };
            var receipt = await fx.Client.TransferAsync(transfers);
            var newBalanceAfterTransfer = await fx.Client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance + (ulong)transferAmount, newBalanceAfterTransfer);
        }
        [Fact(DisplayName = "Transfer: Can Get Transfer Record Showing Transfers")]
        public async Task CanGetTransferRecordShowingTransfers()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var transferAmount = (long)Generator.Integer(10, 100);
            var newBalance = await fx.Client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance, newBalance);

            var record = await fx.Client.TransferWithRecordAsync(_network.Payer, fx.Record.Address, transferAmount);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.Equal(4, record.Transfers.Count);
            Assert.Equal(-transferAmount - (long)record.Fee, record.Transfers[_network.Payer]);
            Assert.Equal(transferAmount, record.Transfers[fx.Record.Address]);

            var newBalanceAfterTransfer = await fx.Client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance + (ulong)transferAmount, newBalanceAfterTransfer);
        }
        [Fact(DisplayName = "Transfer: Can Send from New Account")]
        public async Task CanTransferCryptoFromNewAccount()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var transferAmount = fx.CreateParams.InitialBalance / 2;
            await using var client = _network.NewClient();
            var info = await client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance, info.Balance);
            Assert.Equal(new Endorsement(fx.PublicKey), info.Endorsement);

            var receipt = await client.TransferAsync(fx.Record.Address, _network.Payer, (long)transferAmount, fx.PrivateKey);
            var newBalanceAfterTransfer = await client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance - (ulong)transferAmount, newBalanceAfterTransfer);
        }
        [Fact(DisplayName = "Transfer: Can Send from New Account via Transfers Map")]
        public async Task CanTransferCryptoFromNewAccountViaDictionary()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var transferAmount = (long)(fx.CreateParams.InitialBalance / 2);
            await using var client = _network.NewClient();
            var info = await client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance, info.Balance);
            Assert.Equal(new Endorsement(fx.PublicKey), info.Endorsement);
            var transfers = new Dictionary<Address, long> { { fx.Record.Address, -transferAmount }, { _network.Payer, transferAmount } };
            var receipt = await client.TransferAsync(transfers, fx.PrivateKey);
            var newBalanceAfterTransfer = await client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance - (ulong)transferAmount, newBalanceAfterTransfer);
        }
        [Fact(DisplayName = "Transfer: Can Drain All Crypto from New Account")]
        public async Task CanTransferAllCryptoFromNewAccount()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var info = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance, info.Balance);
            Assert.Equal(new Endorsement(fx.PublicKey), info.Endorsement);

            var receipt = await fx.Client.TransferAsync(fx.Record.Address, _network.Payer, (long)fx.CreateParams.InitialBalance, fx.PrivateKey, ctx => ctx.FeeLimit = 1000000);
            var newBalanceAfterTransfer = await fx.Client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(0UL, newBalanceAfterTransfer);
        }
        [Fact(DisplayName = "Transfer: Insufficient Funds Throws Error")]
        public async Task InsufficientFundsThrowsError()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var transferAmount = (long)(fx.CreateParams.InitialBalance * 2);
            var exception = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.TransferAsync(fx.Record.Address, _network.Payer, transferAmount, fx.PrivateKey);
            });
            Assert.StartsWith("Unable to execute crypto transfer, status: InsufficientAccountBalance", exception.Message);
            Assert.NotNull(exception.TxId);
            Assert.Equal(ResponseCode.InsufficientAccountBalance, exception.Status);
        }
        [Fact(DisplayName = "Transfer: Insufficient Fee Throws Error")]
        public async Task InsufficientFeeThrowsError()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var transferAmount = (long)(fx.CreateParams.InitialBalance / 2);
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.TransferAsync(fx.Record.Address, _network.Payer, transferAmount, fx.PrivateKey, ctx =>
                {
                    ctx.FeeLimit = 1;
                });
            });
            Assert.StartsWith("Transaction Failed Pre-Check: InsufficientTxFee", pex.Message);
            Assert.Equal(ResponseCode.InsufficientTxFee, pex.Status);
        }
        [Fact(DisplayName = "Transfer: Can Send and Receive multiple accounts in single transaction.")]
        public async Task CanSendAndReceiveMultipleAccounts()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var account1 = fx1.Record.Address;
            var account2 = fx2.Record.Address;
            var sig1 = new Signatory(fx1.PrivateKey);
            var sig2 = new Signatory(fx2.PrivateKey);
            var transferAmount = (long)Generator.Integer(100, 200);
            var transfers = new Dictionary<Address, long>
            {
                { payer, -2 * transferAmount },
                { account1, transferAmount },
                { account2, transferAmount }
            };
            var sendRecord = await fx1.Client.TransferWithRecordAsync(transfers);
            Assert.Equal(ResponseCode.Success, sendRecord.Status);

            Assert.Equal((ulong)transferAmount + fx1.CreateParams.InitialBalance, await fx1.Client.GetAccountBalanceAsync(account1));
            Assert.Equal((ulong)transferAmount + fx2.CreateParams.InitialBalance, await fx2.Client.GetAccountBalanceAsync(account2));
            transfers = new Dictionary<Address, long>
            {
                { account1, -transferAmount },
                { account2, -transferAmount },
                { payer, 2 * transferAmount }
            };
            var returnRecord = await fx1.Client.TransferWithRecordAsync(transfers, new Signatory(sig1, sig2), ctx => ctx.FeeLimit = 1_000_000);
            Assert.Equal(ResponseCode.Success, returnRecord.Status);

            Assert.Equal(fx1.CreateParams.InitialBalance, await fx1.Client.GetAccountBalanceAsync(account1));
            Assert.Equal(fx2.CreateParams.InitialBalance, await fx2.Client.GetAccountBalanceAsync(account2));
        }
        [Fact(DisplayName = "Transfer: Multi-Account Transfer Transactions must add up to net zero.")]
        public async Task UnblancedMultiTransferRequestsRaiseError()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var account1 = fx1.Record.Address;
            var account2 = fx2.Record.Address;
            var sig1 = new Signatory(fx1.PrivateKey);
            var sig2 = new Signatory(fx2.PrivateKey);
            var transferAmount = (long)Generator.Integer(100, 200);
            var transfers = new Dictionary<Address, long>
            {
                { payer, -transferAmount },
                { account1, transferAmount },
                { account2, transferAmount }
            };
            var aor = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await fx1.Client.TransferWithRecordAsync(transfers);
            });
            Assert.Equal("transfers", aor.ParamName);
            Assert.StartsWith("The sum of sends and receives does not balance.", aor.Message);
        }
        [Fact(DisplayName = "Transfer: net values of aero are not allowed in transfers.")]
        public async Task NetZeroTransactionIsAllowed()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var account1 = fx1.Record.Address;
            var account2 = fx2.Record.Address;
            var sig1 = new Signatory(fx1.PrivateKey);
            var sig2 = new Signatory(fx2.PrivateKey);
            var transferAmount = (long)Generator.Integer(100, 200);
            var transfers = new Dictionary<Address, long>
            {
                { account1, 0 },
                { account2, 0 },
            };
            var aor = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await fx1.Client.TransferWithRecordAsync(transfers, sig1);
            });
            Assert.Equal("transfers", aor.ParamName);
            Assert.StartsWith($"The amount to transfer to/from 0.0.{account1.AccountNum} must be a value, negative for transfers out, and positive for transfers in. A value of zero is not allowed.", aor.Message);

            Assert.Equal(fx1.CreateParams.InitialBalance, await fx1.Client.GetAccountBalanceAsync(account1));
            Assert.Equal(fx2.CreateParams.InitialBalance, await fx2.Client.GetAccountBalanceAsync(account2));
        }
        [Fact(DisplayName = "Transfer: Null Transfers Dictionary Raises Error.")]
        public async Task NullSendDictionaryRaisesError()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var transferAmount = (long)Generator.Integer(100, 200);
            var and = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx1.Client.TransferWithRecordAsync(null);
            });
            Assert.Equal("transfers", and.ParamName);
            Assert.StartsWith("The dictionary of transfers can not be null", and.Message);
        }
        [Fact(DisplayName = "Transfer: Empty Transfers Dictionary Raises Error.")]
        public async Task MissingSendDictionaryRaisesError()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var transferAmount = (long)Generator.Integer(100, 200);

            var transfers = new Dictionary<Address, long> { };
            var aor = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await fx1.Client.TransferWithRecordAsync(transfers);
            });
            Assert.Equal("transfers", aor.ParamName);
            Assert.StartsWith("The dictionary of transfers can not be empty. (Parameter 'tran", aor.Message);
        }
        [Fact(DisplayName = "Transfer: Transaction ID Information Makes Sense for Receipt")]
        public async Task TransactionIdMakesSenseForReceipt()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var lowerBound = Epoch.UniqueClockNanosAfterDrift();
            var transferAmount = (long)Generator.Integer(10, 100);
            var receipt = await fx.Client.TransferAsync(_network.Payer, fx.Record.Address, transferAmount);
            var upperBound = Epoch.UniqueClockNanosAfterDrift();
            var txId = receipt.Id;
            Assert.NotNull(txId);
            Assert.Equal(_network.Payer, txId.Address);
            Assert.InRange(txId.ValidStartSeconds, lowerBound / 1_000_000_000, upperBound / 1_000_000_000);
            Assert.InRange(txId.ValidStartNanos, 0, 1_000_000_000);
        }
        [Fact(DisplayName = "Transfer: Transaction ID Information Makes Sense for Record")]
        public async Task TransactionIdMakesSenseForRecord()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var lowerBound = Epoch.UniqueClockNanosAfterDrift();
            var transferAmount = (long)Generator.Integer(10, 100);
            var record = await fx.Client.TransferWithRecordAsync(_network.Payer, fx.Record.Address, transferAmount);
            var upperBound = Epoch.UniqueClockNanosAfterDrift();
            var txId = record.Id;
            Assert.NotNull(txId);
            Assert.Equal(_network.Payer, txId.Address);
            Assert.InRange(txId.ValidStartSeconds, lowerBound / 1_000_000_000, upperBound / 1_000_000_000);
            Assert.InRange(txId.ValidStartNanos, 0, 1_000_000_000);
        }
        [Fact(DisplayName = "Transfer: Receipt Contains Exchange Information")]
        public async Task TransferReceiptContainsExchangeInformation()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var transferAmount = (long)Generator.Integer(10, 100);
            var receipt = await fx.Client.TransferAsync(_network.Payer, fx.Record.Address, transferAmount);
            Assert.NotNull(receipt.CurrentExchangeRate);
            // Well, testnet doesn't actually have good data here
            Assert.InRange(receipt.CurrentExchangeRate.Expiration, DateTime.MinValue, DateTime.MaxValue);
            Assert.NotNull(receipt.NextExchangeRate);
            Assert.InRange(receipt.NextExchangeRate.Expiration, DateTime.MinValue, DateTime.MaxValue);
        }
        [Fact(DisplayName = "Transfer: Receipt Contains Exchange Information")]
        public async Task TransferRecordContainsExchangeInformation()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var transferAmount = (long)Generator.Integer(10, 100);
            var record = await fx.Client.TransferWithRecordAsync(_network.Payer, fx.Record.Address, transferAmount);
            Assert.NotNull(record.CurrentExchangeRate);
            // Well, testnet doesn't actually have good data here
            Assert.InRange(record.CurrentExchangeRate.Expiration, DateTime.MinValue, DateTime.MaxValue);
            Assert.NotNull(record.NextExchangeRate);
            Assert.InRange(record.NextExchangeRate.Expiration, DateTime.MinValue, DateTime.MaxValue);
        }
        [Fact(DisplayName = "Transfer: Transfer to a Topic Raises Error.")]
        public async Task TransferToATopicRaisesError()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestTopic.CreateAsync(_network);
            var payer = _network.Payer;
            var transferAmount = (long)Generator.Integer(1, (int)fx1.CreateParams.InitialBalance);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx1.Client.TransferAsync(fx1.Record.Address, fx2.Record.Topic, transferAmount);
            });
            Assert.Equal(ResponseCode.AccountIdDoesNotExist, tex.Status);
            Assert.StartsWith("Unable to execute crypto transfer, status: AccountIdDoesNotExist", tex.Message);
        }
    }
}
