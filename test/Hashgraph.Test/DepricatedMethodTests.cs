#pragma warning disable CS0618
using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using NSec.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto
{
    [Collection(nameof(NetworkCredentials))]
    public class DepricatedMethodTests
    {
        private readonly NetworkCredentials _network;
        public DepricatedMethodTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Depricated Transfer: Can Send to multitransfer to New Account")]
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
        [Fact(DisplayName = "Depricated Transfer: Can Send from New Account via Transfers Map")]
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
        [Fact(DisplayName = "Depricated Transfer: Can Send and Receive multiple accounts in single transaction.")]
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
        [Fact(DisplayName = "Depricated Transfer: Multi-Account Transfer Transactions must add up to net zero.")]
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
            Assert.Equal("cryptoTransfers", aor.ParamName);
            Assert.StartsWith("The sum of crypto sends and receives does not balance.", aor.Message);
        }
        [Fact(DisplayName = "Depricated Transfer: Null Transfers Dictionary Raises Error.")]
        public async Task NullSendDictionaryRaisesError()
        {
            var fx1 = await TestAccount.CreateAsync(_network);
            var fx2 = await TestAccount.CreateAsync(_network);
            var payer = _network.Payer;
            var transferAmount = (long)Generator.Integer(100, 200);
            var and = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx1.Client.TransferWithRecordAsync((IEnumerable<KeyValuePair<Address, long>>)null);
            });
            Assert.Equal("cryptoTransfers", and.ParamName);
            Assert.StartsWith("The dictionary of crypto transfers can not be null", and.Message);
        }
        [Fact(DisplayName = "Depricated Transfer: Empty Transfers Dictionary Raises Error.")]
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
            Assert.Equal("cryptoTransfers", aor.ParamName);
            Assert.StartsWith("The dictionary of crypto transfers can not be empty", aor.Message);
        }
        [Fact(DisplayName = "Depricated Transfer: net values of aero are not allowed in transfers.")]
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
            Assert.Equal("cryptoTransfers", aor.ParamName);
            Assert.StartsWith($"The amount to transfer crypto to/from 0.0.{account1.AccountNum} must be a value, negative for transfers out, and positive for transfers in. A value of zero is not allowed.", aor.Message);

            Assert.Equal(fx1.CreateParams.InitialBalance, await fx1.Client.GetAccountBalanceAsync(account1));
            Assert.Equal(fx2.CreateParams.InitialBalance, await fx2.Client.GetAccountBalanceAsync(account2));
        }
    }
}
