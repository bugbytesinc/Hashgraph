﻿using Hashgraph.Implementation;
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
            var newAccount = new Account(fx.Record.Address, fx.PrivateKey);
            var info = await client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance, info.Balance);
            Assert.Equal(new Endorsement(fx.PublicKey), info.Endorsement);

            var receipt = await client.TransferAsync(newAccount, _network.Payer, (long)transferAmount);
            var newBalanceAfterTransfer = await client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance - (ulong)transferAmount, newBalanceAfterTransfer);
        }
        [Fact(DisplayName = "Transfer: Can Drain All Crypto from New Account")]
        public async Task CanTransferAllCryptoFromNewAccount()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var newAccount = new Account(fx.Record.Address, fx.PrivateKey);
            var info = await fx.Client.GetAccountInfoAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance, info.Balance);
            Assert.Equal(new Endorsement(fx.PublicKey), info.Endorsement);

            var receipt = await fx.Client.TransferAsync(newAccount, _network.Payer, (long)fx.CreateParams.InitialBalance, ctx=>ctx.FeeLimit = 1000000); ;
            var newBalanceAfterTransfer = await fx.Client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(0UL, newBalanceAfterTransfer);
        }
        [Fact(DisplayName = "Transfer: Insufficient Funds Throws Error")]
        public async Task InsufficientFundsThrowsError()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var transferAmount = (long)(fx.CreateParams.InitialBalance * 2);
            var account = new Account(fx.Record.Address, fx.PrivateKey);
            var exception = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.TransferAsync(account, _network.Payer, transferAmount);
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
            var account = new Account(fx.Record.Address, fx.PrivateKey);
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.TransferAsync(account, _network.Payer, transferAmount, ctx =>
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
            var account1 = new Account(fx1.Record.Address, fx1.PrivateKey);
            var account2 = new Account(fx2.Record.Address, fx2.PrivateKey);
            var transferAmount = (long)Generator.Integer(100, 200);

            var sendAccounts = new Dictionary<Account, long> { { payer, 2 * transferAmount } };
            var receiveAddresses = new Dictionary<Address, long> { { account1, transferAmount }, { account2, transferAmount } };

            var sendRecord = await fx1.Client.TransferWithRecordAsync(sendAccounts, receiveAddresses);
            Assert.Equal(ResponseCode.Success, sendRecord.Status);

            Assert.Equal((ulong)transferAmount + fx1.CreateParams.InitialBalance, await fx1.Client.GetAccountBalanceAsync(account1));
            Assert.Equal((ulong)transferAmount + fx2.CreateParams.InitialBalance, await fx2.Client.GetAccountBalanceAsync(account2));

            sendAccounts = new Dictionary<Account, long> { { account1, transferAmount }, { account2, transferAmount } };
            receiveAddresses = new Dictionary<Address, long> { { payer, 2 * transferAmount } };
            var returnRecord = await fx1.Client.TransferWithRecordAsync(sendAccounts, receiveAddresses, ctx=>ctx.FeeLimit = 1_000_000);
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
            var account1 = new Account(fx1.Record.Address, fx1.PrivateKey);
            var account2 = new Account(fx2.Record.Address, fx2.PrivateKey);
            var transferAmount = (long)Generator.Integer(100, 200);

            var sendAccounts = new Dictionary<Account, long> { { payer, transferAmount } };
            var receiveAddresses = new Dictionary<Address, long> { { account1, transferAmount }, { account2, transferAmount } };

            var aor = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await fx1.Client.TransferWithRecordAsync(sendAccounts, receiveAddresses);
            });
            Assert.Equal("sendAccounts", aor.ParamName);
            Assert.StartsWith("The sum of sends and receives does not balance.", aor.Message);
        }
        [Fact(DisplayName = "Transfer: Overlapping transfer entries are allowed.")]
        public async Task OverlappingTransferEntiesAreAllowed()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var account1 = new Account(fx1.Record.Address, fx1.PrivateKey);
            var account2 = new Account(fx2.Record.Address, fx2.PrivateKey);
            var transferAmount = (long)Generator.Integer(100, 200);

            var sendAccounts = new Dictionary<Account, long> { { payer, 3 * transferAmount } };
            var receiveAddresses = new Dictionary<Address, long> { { account1, transferAmount }, { account2, transferAmount }, { payer, transferAmount } };

            var sendRecord = await fx1.Client.TransferWithRecordAsync(sendAccounts, receiveAddresses, ctx => ctx.FeeLimit = 1_000_000);
            Assert.Equal(ResponseCode.Success, sendRecord.Status);

            Assert.Equal((ulong)transferAmount + fx1.CreateParams.InitialBalance, await fx1.Client.GetAccountBalanceAsync(account1));
            Assert.Equal((ulong)transferAmount + fx2.CreateParams.InitialBalance, await fx2.Client.GetAccountBalanceAsync(account2));

            sendAccounts = new Dictionary<Account, long> { { account1, transferAmount }, { account2, transferAmount }, { payer, transferAmount } };
            receiveAddresses = new Dictionary<Address, long> { { payer, 3 * transferAmount } };
            var returnRecord = await fx1.Client.TransferWithRecordAsync(sendAccounts, receiveAddresses, ctx => ctx.FeeLimit = 1_000_000);
            Assert.Equal(ResponseCode.Success, returnRecord.Status);

            Assert.Equal(fx1.CreateParams.InitialBalance, await fx1.Client.GetAccountBalanceAsync(account1));
            Assert.Equal(fx2.CreateParams.InitialBalance, await fx2.Client.GetAccountBalanceAsync(account2));
        }
        [Fact(DisplayName = "Transfer: Net Zero Transacton Is Allowed")]
        public async Task NetZeroTransactionIsAllowed()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var account1 = new Account(fx1.Record.Address, fx1.PrivateKey);
            var account2 = new Account(fx2.Record.Address, fx2.PrivateKey);
            var transferAmount = (long)Generator.Integer(100, 200);

            var sendAccounts = new Dictionary<Account, long> { { account1, transferAmount }, { account2, transferAmount } };
            var receiveAddresses = new Dictionary<Address, long> { { account1, transferAmount }, { account2, transferAmount } };

            var sendRecord = await fx1.Client.TransferWithRecordAsync(sendAccounts, receiveAddresses, ctx => ctx.FeeLimit = 1_000_000);
            Assert.Equal(ResponseCode.Success, sendRecord.Status);

            Assert.Equal(fx1.CreateParams.InitialBalance, await fx1.Client.GetAccountBalanceAsync(account1));
            Assert.Equal(fx2.CreateParams.InitialBalance, await fx2.Client.GetAccountBalanceAsync(account2));
        }
        [Fact(DisplayName = "Transfer: Null Send Dictionary Raises Error.")]
        public async Task NullSendDictionaryRaisesError()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var account1 = new Account(fx1.Record.Address, fx1.PrivateKey);
            var account2 = new Account(fx2.Record.Address, fx2.PrivateKey);
            var transferAmount = (long)Generator.Integer(100, 200);

            var receiveAddresses = new Dictionary<Address, long> { { account1, transferAmount }, { account2, transferAmount } };

            var and = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx1.Client.TransferWithRecordAsync(null, receiveAddresses);
            });
            Assert.Equal("sendAccounts", and.ParamName);
            Assert.StartsWith("The send accounts parameter cannot be null.", and.Message);
        }
        [Fact(DisplayName = "Transfer: Missing Send Dictionary Raises Error.")]
        public async Task MissingSendDictionaryRaisesError()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var account1 = new Account(fx1.Record.Address, fx1.PrivateKey);
            var account2 = new Account(fx2.Record.Address, fx2.PrivateKey);
            var transferAmount = (long)Generator.Integer(100, 200);

            var sendAccounts = new Dictionary<Account, long> { };
            var receiveAddresses = new Dictionary<Address, long> { { account1, transferAmount }, { account2, transferAmount } };

            var aor = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await fx1.Client.TransferWithRecordAsync(sendAccounts, receiveAddresses);
            });
            Assert.Equal("sendAccounts", aor.ParamName);
            Assert.StartsWith("There must be at least one send account to transfer money from.", aor.Message);
        }


        [Fact(DisplayName = "Transfer: Null Receive Dictionary Raises Error.")]
        public async Task NullReceiveDictionaryRaisesError()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var account1 = new Account(fx1.Record.Address, fx1.PrivateKey);
            var account2 = new Account(fx2.Record.Address, fx2.PrivateKey);
            var transferAmount = (long)Generator.Integer(100, 200);

            var sendAaccounts = new Dictionary<Account, long> { { account1, transferAmount }, { account2, transferAmount } };

            var and = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx1.Client.TransferWithRecordAsync(sendAaccounts, null);
            });
            Assert.Equal("receiveAddresses", and.ParamName);
            Assert.StartsWith("The receive address parameter cannot be null.", and.Message);
        }
        [Fact(DisplayName = "Transfer: Missing Send Dictionary Raises Error.")]
        public async Task MissingReceiveDictionaryRaisesError()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var account1 = new Account(fx1.Record.Address, fx1.PrivateKey);
            var account2 = new Account(fx2.Record.Address, fx2.PrivateKey);
            var transferAmount = (long)Generator.Integer(100, 200);

            var sendAccounts = new Dictionary<Account, long> { { account1, transferAmount }, { account2, transferAmount } };
            var receiveAddresses = new Dictionary<Address, long> { };

            var aor = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await fx1.Client.TransferWithRecordAsync(sendAccounts, receiveAddresses);
            });
            Assert.Equal("receiveAddresses", aor.ParamName);
            Assert.StartsWith("There must be at least one receive address to transfer money to.", aor.Message);
        }
        [Fact(DisplayName = "Transfer: Negative Send Dictionary Raises Error.")]
        public async Task NegativeSendValueRaisesError()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var account1 = new Account(fx1.Record.Address, fx1.PrivateKey);
            var account2 = new Account(fx2.Record.Address, fx2.PrivateKey);
            var transferAmount = (long)Generator.Integer(100, 200);

            var sendAccounts = new Dictionary<Account, long> { { account1, -transferAmount }, { account2, 2 * transferAmount } };
            var receiveAddresses = new Dictionary<Address, long> { { payer, 2 * transferAmount } };

            var aor = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await fx1.Client.TransferWithRecordAsync(sendAccounts, receiveAddresses);
            });
            Assert.Equal("sendAccounts", aor.ParamName);
            Assert.StartsWith("All amount entries must be positive values", aor.Message);
        }
        [Fact(DisplayName = "Transfer: Negative Receive Dictionary Raises Error.")]
        public async Task NegativeReceiveValueRaisesError()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var account1 = new Account(fx1.Record.Address, fx1.PrivateKey);
            var account2 = new Account(fx2.Record.Address, fx2.PrivateKey);
            var transferAmount = (long)Generator.Integer(100, 200);

            var sendAccounts = new Dictionary<Account, long> { { account1, transferAmount }, { account2, 2 * transferAmount } };
            var receiveAddresses = new Dictionary<Address, long> { { payer, -transferAmount } };

            var aor = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await fx1.Client.TransferWithRecordAsync(sendAccounts, receiveAddresses);
            });
            Assert.Equal("receiveAddresses", aor.ParamName);
            Assert.StartsWith("All amount entries must be positive values", aor.Message);
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
    }
}
