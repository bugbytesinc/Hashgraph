using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using NSec.Cryptography;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto;

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

        var transfers = new TransferParams
        {
            CryptoTransfers = new[] { new CryptoTransfer(_network.Payer, -transferAmount), new CryptoTransfer(fx.Record.Address, transferAmount) }
        };
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
        Assert.Empty(record.TokenTransfers);
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Royalties);
        Assert.Empty(record.Associations);

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
        var transfers = new TransferParams
        {
            CryptoTransfers = new[] { new CryptoTransfer(fx.Record.Address, -transferAmount), new CryptoTransfer(_network.Payer, transferAmount) },
            Signatory = fx.PrivateKey
        };
        var receipt = await client.TransferAsync(transfers);
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

        var receipt = await fx.Client.TransferAsync(fx.Record.Address, _network.Payer, (long)fx.CreateParams.InitialBalance, fx.PrivateKey);
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
        Assert.StartsWith("Unable to execute transfers, status: InsufficientAccountBalance", exception.Message);
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
        var transfers = new TransferParams
        {
            CryptoTransfers = new[]
                {
                    new CryptoTransfer( payer, -2 * transferAmount ),
                    new CryptoTransfer(account1, transferAmount ),
                    new CryptoTransfer(account2, transferAmount )
                }
        };
        var sendRecord = await fx1.Client.TransferWithRecordAsync(transfers);
        Assert.Equal(ResponseCode.Success, sendRecord.Status);

        Assert.Equal((ulong)transferAmount + fx1.CreateParams.InitialBalance, await fx1.Client.GetAccountBalanceAsync(account1));
        Assert.Equal((ulong)transferAmount + fx2.CreateParams.InitialBalance, await fx2.Client.GetAccountBalanceAsync(account2));
        transfers = new TransferParams
        {
            CryptoTransfers = new[]
                {
                    new CryptoTransfer( account1, -transferAmount ),
                    new CryptoTransfer( account2, -transferAmount ),
                    new CryptoTransfer( payer, 2 * transferAmount )
                },
            Signatory = new Signatory(sig1, sig2)
        };
        var returnRecord = await fx1.Client.TransferWithRecordAsync(transfers, ctx => ctx.FeeLimit = 1_000_000);
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
        var transfers = new TransferParams
        {
            CryptoTransfers = new[]
                {
                    new CryptoTransfer( payer, -transferAmount ),
                    new CryptoTransfer(account1, transferAmount ),
                    new CryptoTransfer(account2, transferAmount )
                }
        };
        var aor = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await fx1.Client.TransferWithRecordAsync(transfers);
        });
        Assert.Equal("CryptoTransfers", aor.ParamName);
        Assert.StartsWith("The sum of crypto sends and receives does not balance.", aor.Message);
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
        var transfers = new TransferParams
        {
            CryptoTransfers = new[]
                {
                    new CryptoTransfer( account1, 0 ),
                    new CryptoTransfer( account2, 0 ),
                },
            Signatory = sig1
        };
        var aor = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await fx1.Client.TransferWithRecordAsync(transfers);
        });
        Assert.Equal("CryptoTransfers", aor.ParamName);
        Assert.StartsWith($"The amount to transfer crypto to/from 0.0.{account1.AccountNum} must be a value, negative for transfers out, and positive for transfers in. A value of zero is not allowed.", aor.Message);

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
        TransferParams testParams = null;
        var and = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await fx1.Client.TransferWithRecordAsync(testParams);
        });
        Assert.Equal("transfers", and.ParamName);
        Assert.StartsWith("The transfer parametes cannot not be null.", and.Message);
    }
    [Fact(DisplayName = "Transfer: Empty Transfers Dictionary Raises Error.")]
    public async Task MissingSendDictionaryRaisesError()
    {
        var fx1 = await TestAccount.CreateAsync(_network);
        var payer = _network.Payer;
        var transferAmount = (long)Generator.Integer(100, 200);

        var transfers = new TransferParams { CryptoTransfers = new CryptoTransfer[] { } };
        var aor = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await fx1.Client.TransferWithRecordAsync(transfers);
        });
        Assert.Equal("CryptoTransfers", aor.ParamName);
        Assert.StartsWith("The list of crypto transfers can not be empty.", aor.Message);
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
        Assert.Equal(ResponseCode.InvalidAccountId, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: InvalidAccountId", tex.Message);
    }
    [Fact(DisplayName = "NETWORK V0.14.0 UNSUPPORTED: Transfer: Insufficient Fee Error Provides Sufficient Fee in Exception Test Fails")]
    public async Task InsufficientFeeExceptionIncludesRequiredFeeNetwork14Regression()
    {
        var testFailException = (await Assert.ThrowsAsync<TransactionException>(InsufficientFeeExceptionIncludesRequiredFee));
        Assert.StartsWith("Unable to execute transfers, status: InsufficientTxFee", testFailException.Message);

        //[Fact(DisplayName = "Transfer: Insufficient Fee Error Provides Sufficient Fee in Exception")]
        async Task InsufficientFeeExceptionIncludesRequiredFee()
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
            Assert.True(pex.RequiredFee > 0);

            var receipt = await fx.Client.TransferAsync(fx.Record.Address, _network.Payer, transferAmount, fx.PrivateKey, ctx =>
            {
                ctx.FeeLimit = (long)pex.RequiredFee;
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var balance = await fx.Client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance - (ulong)transferAmount, balance);
        }
    }
    [Fact(DisplayName = "NETWORK V0.14.0 UNSUPPORTED: Transfer: Insufficient Fee Error Provides Sufficient Fee in Exception Test Fails")]
    public async Task InsufficientFeeExceptionIncludesRequiredFeeForRecordNetwork14Regresssion()
    {
        var testFailException = (await Assert.ThrowsAsync<TransactionException>(InsufficientFeeExceptionIncludesRequiredFeeForRecord));
        Assert.StartsWith("Unable to execute transfers, status: InsufficientTxFee", testFailException.Message);

        //[Fact(DisplayName = "Transfer: Insufficient Fee Error Provides Sufficient Fee in Exception")]
        async Task InsufficientFeeExceptionIncludesRequiredFeeForRecord()
        {
            await using var fx = await TestAccount.CreateAsync(_network);
            var transferAmount = (long)(fx.CreateParams.InitialBalance / 2);
            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.TransferWithRecordAsync(fx.Record.Address, _network.Payer, transferAmount, fx.PrivateKey, ctx =>
                {
                    ctx.FeeLimit = 1;
                });
            });
            Assert.StartsWith("Transaction Failed Pre-Check: InsufficientTxFee", pex.Message);
            Assert.Equal(ResponseCode.InsufficientTxFee, pex.Status);
            Assert.True(pex.RequiredFee > 0);

            var record = await fx.Client.TransferWithRecordAsync(fx.Record.Address, _network.Payer, transferAmount, fx.PrivateKey, ctx =>
            {
                ctx.FeeLimit = (long)pex.RequiredFee;
            });
            Assert.Equal(ResponseCode.Success, record.Status);

            var balance = await fx.Client.GetAccountBalanceAsync(fx.Record.Address);
            Assert.Equal(fx.CreateParams.InitialBalance - (ulong)transferAmount, balance);
        }
    }
    [Fact(DisplayName = "Transfer: Consistent Duplicated Signature Succeeds")]
    public async Task AllowsDuplicateSignature()
    {
        await using var client = _network.NewClient();
        var payerKey = TestKeys.ImportPrivateEd25519KeyFromBytes(_network.PrivateKey);
        var publicPrefix = payerKey.PublicKey.Export(KeyBlobFormat.PkixPublicKey).TakeLast(32).Take(6).ToArray();

        // Define Signing Method producing a duplicate signature
        Task CustomSigner(IInvoice invoice)
        {
            var goodSignature1 = SignatureAlgorithm.Ed25519.Sign(payerKey, invoice.TxBytes.Span);
            var goodSignature2 = SignatureAlgorithm.Ed25519.Sign(payerKey, invoice.TxBytes.Span);
            invoice.AddSignature(KeyType.Ed25519, publicPrefix, goodSignature1);
            invoice.AddSignature(KeyType.Ed25519, publicPrefix, goodSignature2);
            return Task.CompletedTask;
        }
        var record = await client.TransferWithRecordAsync(_network.Payer, _network.Gateway, 100, ctx =>
        {
            ctx.Signatory = new Signatory(CustomSigner);
        });
        Assert.Equal(ResponseCode.Success, record.Status);
    }
    [Fact(DisplayName = "Transfer: Inconsistent Duplicated Signature Raises Error")]
    public async Task InconsistentDuplicateSignatureRaisesError()
    {
        await using var client = _network.NewClient();
        var fakeKey1 = Key.Create(SignatureAlgorithm.Ed25519, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
        var fakeKey2 = Key.Create(SignatureAlgorithm.Ed25519, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
        var goodKey = TestKeys.ImportPrivateEd25519KeyFromBytes(_network.PrivateKey);
        var publicPrefix = goodKey.PublicKey.Export(KeyBlobFormat.PkixPublicKey).TakeLast(32).Take(6).ToArray();

        // Define Defective Signing Method Bad Signature Last
        Task CustomSigner(IInvoice invoice)
        {
            var goodSignature = SignatureAlgorithm.Ed25519.Sign(goodKey, invoice.TxBytes.Span);
            var badSignature = SignatureAlgorithm.Ed25519.Sign(fakeKey1, invoice.TxBytes.Span);
            invoice.AddSignature(KeyType.Ed25519, publicPrefix, goodSignature);
            invoice.AddSignature(KeyType.Ed25519, publicPrefix, badSignature);
            return Task.CompletedTask;
        }
        var aex1 = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await client.TransferWithRecordAsync(_network.Payer, _network.Gateway, 100, ctx =>
            {
                ctx.Signatory = new Signatory(CustomSigner);
            });
        });
        Assert.StartsWith("Signature with Duplicate Prefix Identifier was provided, but did not have an Identical Signature.", aex1.Message);

        // Define Defective Signing Method Bad Signature First
        Task CustomSignerReverse(IInvoice invoice)
        {
            var goodSignature = SignatureAlgorithm.Ed25519.Sign(goodKey, invoice.TxBytes.Span);
            var badSignature = SignatureAlgorithm.Ed25519.Sign(fakeKey1, invoice.TxBytes.Span);
            invoice.AddSignature(KeyType.Ed25519, publicPrefix, badSignature);
            invoice.AddSignature(KeyType.Ed25519, publicPrefix, goodSignature);
            return Task.CompletedTask;
        }
        var aex2 = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await client.TransferWithRecordAsync(_network.Payer, _network.Gateway, 100, ctx =>
            {
                ctx.Signatory = new Signatory(CustomSignerReverse);
            });
        });
        Assert.StartsWith("Signature with Duplicate Prefix Identifier was provided, but did not have an Identical Signature.", aex2.Message);

        // Define Defective Signing Method Bad Two Bad Signatures
        Task CustomSignerBothBad(IInvoice invoice)
        {
            var badSignature1 = SignatureAlgorithm.Ed25519.Sign(fakeKey1, invoice.TxBytes.Span);
            var badSignature2 = SignatureAlgorithm.Ed25519.Sign(fakeKey2, invoice.TxBytes.Span);
            invoice.AddSignature(KeyType.Ed25519, publicPrefix, badSignature2);
            invoice.AddSignature(KeyType.Ed25519, publicPrefix, badSignature1);
            return Task.CompletedTask;
        }
        // Inconsistent Key state should be checked before signatures are validated,
        // expecting an Argument exception and not a PreCheck exception.
        var aex3 = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await client.TransferWithRecordAsync(_network.Payer, _network.Gateway, 100, ctx =>
            {
                ctx.Signatory = new Signatory(CustomSignerBothBad);
            });
        });
        Assert.StartsWith("Signature with Duplicate Prefix Identifier was provided, but did not have an Identical Signature.", aex3.Message);
    }
    [Fact(DisplayName = "Transfer: Can Schedule A Transfer Between Two Accounts Using Context Signatory")]
    public async Task CanScheduleATransferBetweenTwoAccountsUsingContextSignatory()
    {
        var initialBalance = (ulong)Generator.Integer(100, 1000);
        var transferAmount = initialBalance / 2;
        await using var fxSendingAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        await using var fxReceivingAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        var contextSignatory = new Signatory(_network.PrivateKey, new PendingParams());
        await fxSendingAccount.Client.TransferAsync(fxSendingAccount, fxReceivingAccount, (long)transferAmount, ctx => ctx.Signatory = contextSignatory);
    }
    [Fact(DisplayName = "Transfer: Can Schedule A Transfer Between Two Accounts Using Params Signatory")]
    public async Task CanScheduleATransferBetweenTwoAccountsUsingParamsSignatory()
    {
        var initialBalance = (ulong)Generator.Integer(100, 1000);
        var transferAmount = initialBalance / 2;
        await using var fxAccount1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        var paramsSignatory = new Signatory(new PendingParams());
        await fxAccount1.Client.TransferAsync(fxAccount1, fxAccount2, (long)transferAmount, paramsSignatory);
    }
    [Fact(DisplayName = "Transfer: Can Schedule A Transfer Between Two Accounts Using Context Signatory With Private Key")]
    public async Task CanScheduleATransferBetweenTwoAccountsUsingContextSignatoryWithPrivateKey()
    {
        var initialBalance = (ulong)Generator.Integer(100, 1000);
        var transferAmount = initialBalance / 2;
        await using var fxAccount1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);

        var contextSignatory = new Signatory(_network.PrivateKey, new PendingParams());
        await fxAccount1.Client.TransferAsync(fxAccount1, fxAccount2, (long)transferAmount, ctx => ctx.Signatory = contextSignatory);
    }
    [Fact(DisplayName = "Transfer: Can Schedule A Transfer Between Two Accounts With Third Party Payer")]
    public async Task CanScheduleATransferBetweenTwoAccountsWithThirdPartyPayer()
    {
        var initialBalance = (ulong)Generator.Integer(100, 1000);
        var transferAmount = initialBalance / 2;
        await using var fxSender = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        await using var fxReceiver = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);

        var contextSignatory = new Signatory(fxPayer.PrivateKey, _network.PrivateKey, new PendingParams { PendingPayer = _network.Payer });
        var contextPayer = fxPayer.Record.Address;

        await fxSender.Client.TransferAsync(fxSender, fxReceiver, (long)transferAmount, ctx => { ctx.Payer = contextPayer; ctx.Signatory = contextSignatory; });
    }
    [Fact(DisplayName = "Transfer: Can Schedule A Transfer That Should Immediately Execute")]
    public async Task CanScheduleATransferThatShouldImmediatelyExecute()
    {
        var initialBalance = (ulong)Generator.Integer(100, 1000);
        var transferAmount = initialBalance / 2;
        await using var fxSender = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        await using var fxReceiver = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);

        var pendingSignatory = new Signatory(fxPayer.PrivateKey, fxSender.PrivateKey, new PendingParams { PendingPayer = fxPayer });

        var record = await fxSender.Client.TransferWithRecordAsync(fxSender, fxReceiver, (long)transferAmount, pendingSignatory);

        var info = await fxPayer.Client.GetPendingTransactionInfoAsync(record.Pending.Id);
        Assert.Equal(record.Pending.Id, info.Id);
        Assert.Equal(record.Pending.TxId, info.TxId);
        Assert.Equal(_network.Payer, info.Creator);
        Assert.Equal(fxPayer.Record.Address, info.Payer);
        Assert.Equal(2, info.Endorsements.Length);
        Assert.Equal(new Endorsement(fxPayer.PublicKey), info.Endorsements[0]);
        Assert.Null(info.Administrator);
        Assert.Empty(info.Memo);
        Assert.True(info.Expiration > DateTime.MinValue);
        Assert.True(record.Concensus <= info.Executed);
        Assert.Null(info.Deleted);
        Assert.False(info.PendingTransactionBody.IsEmpty);
        AssertHg.NotEmpty(info.Ledger);

        await AssertHg.CryptoBalanceAsync(fxSender, initialBalance - transferAmount);
        await AssertHg.CryptoBalanceAsync(fxReceiver, initialBalance + transferAmount);
        Assert.True(await fxPayer.Client.GetAccountBalanceAsync(fxPayer) < fxPayer.CreateParams.InitialBalance);

        var executedReceipt = await fxPayer.Client.GetReceiptAsync(record.Pending.TxId);
        Assert.Equal(ResponseCode.Success, executedReceipt.Status);
        Assert.Equal(record.Pending.TxId, executedReceipt.Id);
        Assert.NotNull(executedReceipt.CurrentExchangeRate);
        Assert.NotNull(executedReceipt.NextExchangeRate);
        Assert.Null(executedReceipt.Pending);

        var executedRecord = await fxPayer.Client.GetTransactionRecordAsync(record.Pending.TxId);
        Assert.Equal(ResponseCode.Success, executedRecord.Status);
        Assert.Equal(record.Pending.TxId, executedRecord.Id);
        Assert.InRange(executedRecord.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(executedRecord.Transfers[fxSender], -(long)transferAmount);
        Assert.Equal(executedRecord.Transfers[fxReceiver], (long)transferAmount);
        Assert.True(executedRecord.Transfers[fxPayer] < 0);
        Assert.Empty(executedRecord.TokenTransfers);
        Assert.False(executedRecord.Hash.IsEmpty);
        Assert.NotNull(executedRecord.Concensus);
        Assert.NotNull(executedRecord.CurrentExchangeRate);
        Assert.NotNull(executedRecord.NextExchangeRate);
        Assert.Empty(executedRecord.Memo);
        Assert.Null(executedRecord.Pending);
    }
    [Fact(DisplayName = "Transfer: Scheduled Receipt Does not exist until Completely Signed")]
    public async Task ScheduledReceiptDoesNotExistUntilCompletelySigned()
    {
        var initialBalance = (ulong)Generator.Integer(100, 1000);
        var transferAmount = initialBalance / 2;
        await using var fxSender = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        await using var fxReceiver = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);

        var pendingSignatory = new Signatory(fxSender.PrivateKey, new PendingParams { PendingPayer = fxPayer });

        var record = await fxSender.Client.TransferWithRecordAsync(fxSender, fxReceiver, (long)transferAmount, pendingSignatory);

        var info = await fxPayer.Client.GetPendingTransactionInfoAsync(record.Pending.Id);
        Assert.Equal(record.Pending.Id, info.Id);
        Assert.Equal(record.Pending.TxId, info.TxId);
        Assert.Equal(_network.Payer, info.Creator);
        Assert.Equal(fxPayer.Record.Address, info.Payer);
        Assert.Single(info.Endorsements);
        Assert.Equal(new Endorsement(fxSender.PublicKey), info.Endorsements[0]);
        Assert.Null(info.Administrator);
        Assert.Empty(info.Memo);
        Assert.True(info.Expiration > DateTime.MinValue);
        Assert.Null(info.Executed);
        Assert.Null(info.Deleted);
        Assert.False(info.PendingTransactionBody.IsEmpty);
        AssertHg.NotEmpty(info.Ledger);

        await AssertHg.CryptoBalanceAsync(fxSender, initialBalance);
        await AssertHg.CryptoBalanceAsync(fxReceiver, initialBalance);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxPayer.Client.GetReceiptAsync(record.Pending.TxId);
        });
        Assert.Equal(ResponseCode.ReceiptNotFound, tex.Status);
        Assert.StartsWith("Network failed to return a transaction receipt, Status Code Returned: ReceiptNotFound", tex.Message);

        var executedReceipts = await fxPayer.Client.GetAllReceiptsAsync(record.Pending.TxId);
        Assert.Empty(executedReceipts);

        var signingReceipt = await fxPayer.Client.SignPendingTransactionAsync(record.Pending.Id, fxPayer);
        Assert.Equal(ResponseCode.Success, signingReceipt.Status);
        // Since it was not created by this TX, it won't be included
        Assert.Equal(Address.None, signingReceipt.Pending.Id);
        // But, the executed TX ID will still be returned.
        Assert.Equal(record.Pending.TxId, signingReceipt.Pending.TxId);

        var executedReceipt = await fxPayer.Client.GetReceiptAsync(record.Pending.TxId);
        Assert.Equal(ResponseCode.Success, executedReceipt.Status);
        Assert.Equal(record.Pending.TxId, executedReceipt.Id);
        Assert.NotNull(executedReceipt.CurrentExchangeRate);
        Assert.NotNull(executedReceipt.NextExchangeRate);
        Assert.Null(executedReceipt.Pending);

        await AssertHg.CryptoBalanceAsync(fxSender, initialBalance - transferAmount);
        await AssertHg.CryptoBalanceAsync(fxReceiver, initialBalance + transferAmount);
        Assert.True(await fxPayer.Client.GetAccountBalanceAsync(fxPayer) < fxPayer.CreateParams.InitialBalance);

        var executedRecord = await fxPayer.Client.GetTransactionRecordAsync(record.Pending.TxId);
        Assert.Equal(ResponseCode.Success, executedRecord.Status);
        Assert.Equal(record.Pending.TxId, executedRecord.Id);
        Assert.InRange(executedRecord.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(executedRecord.Transfers[fxSender], -(long)transferAmount);
        Assert.Equal(executedRecord.Transfers[fxReceiver], (long)transferAmount);
        Assert.True(executedRecord.Transfers[fxPayer] < 0);
        Assert.Empty(executedRecord.TokenTransfers);
        Assert.False(executedRecord.Hash.IsEmpty);
        Assert.NotNull(executedRecord.Concensus);
        Assert.NotNull(executedRecord.CurrentExchangeRate);
        Assert.NotNull(executedRecord.NextExchangeRate);
        Assert.Empty(executedRecord.Memo);
        Assert.Null(executedRecord.Pending);
    }
    [Fact(DisplayName = "Transfer: Receipt for Scheduled Execution Can Be Obtained Immediately")]
    public async Task ReceiptForScheduledExecutionCanBeObtainedImmediately()
    {
        var initialBalance = (ulong)Generator.Integer(100, 1000);
        var transferAmount = initialBalance / 2;
        await using var fxSender = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        await using var fxReceiver = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);

        var pendingSignatory = new Signatory(fxSender.PrivateKey, new PendingParams { PendingPayer = fxPayer });

        var schedulingReceipt = await fxSender.Client.TransferAsync(fxSender, fxReceiver, (long)transferAmount, pendingSignatory);
        var signingReceipt = await fxPayer.Client.SignPendingTransactionAsync(schedulingReceipt.Pending.Id, fxPayer);
        var executedReceipt = await fxPayer.Client.GetReceiptAsync(schedulingReceipt.Pending.TxId);
        Assert.Equal(ResponseCode.Success, executedReceipt.Status);
        Assert.Equal(schedulingReceipt.Pending.TxId, executedReceipt.Id);
        Assert.NotNull(executedReceipt.CurrentExchangeRate);
        Assert.NotNull(executedReceipt.NextExchangeRate);
        Assert.Null(executedReceipt.Pending);

        await AssertHg.CryptoBalanceAsync(fxSender, initialBalance - transferAmount);
        await AssertHg.CryptoBalanceAsync(fxReceiver, initialBalance + transferAmount);
        Assert.True(await fxPayer.Client.GetAccountBalanceAsync(fxPayer) < fxPayer.CreateParams.InitialBalance);
    }
    [Fact(DisplayName = "Transfer: Duplicate Scheduled Transfer Returns Pending Information in Exception")]
    public async Task DuplicateScheduledTransferReturnsPendingInformationInException()
    {
        var initialBalance = (ulong)Generator.Integer(100, 1000);
        var transferAmount = initialBalance / 2;
        await using var fxAccount1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = initialBalance);
        var paramsSignatory = new Signatory(new PendingParams());

        var schedulingReceipt = await fxAccount1.Client.TransferAsync(fxAccount1, fxAccount2, (long)transferAmount, paramsSignatory);
        Assert.Equal(ResponseCode.Success, schedulingReceipt.Status);
        Assert.NotNull(schedulingReceipt.Pending);
        Assert.NotEqual(Address.None, schedulingReceipt.Pending.Id);
        Assert.NotEqual(TxId.None, schedulingReceipt.Pending.TxId);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAccount1.Client.TransferAsync(fxAccount1, fxAccount2, (long)transferAmount, paramsSignatory);
        });
        Assert.Equal(ResponseCode.IdenticalScheduleAlreadyCreated, tex.Status);
        Assert.StartsWith("Unable to schedule transaction, status: IdenticalScheduleAlreadyCreated", tex.Message);
        Assert.Equal(ResponseCode.IdenticalScheduleAlreadyCreated, tex.Receipt.Status);
        Assert.NotNull(tex.Receipt.Pending);
        Assert.Equal(schedulingReceipt.Pending.Id, tex.Receipt.Pending.Id);
        Assert.Equal(schedulingReceipt.Pending.TxId, tex.Receipt.Pending.TxId);
    }
    [Fact(DisplayName = "Transfer: Can Send to New Alias Account")]
    public async Task CanTransferCryptoToNewAliasAccount()
    {
        await using var fx = await TestAliasAccount.CreateAsync(_network);
        var startingBalance = await fx.Client.GetAccountBalanceAsync(fx.Alias);

        var transferAmount = (long)Generator.Integer(10, 100);

        var receipt = await fx.Client.TransferAsync(_network.Payer, fx.Alias, transferAmount);

        var endingBalance = await fx.Client.GetAccountBalanceAsync(fx.Alias);
        Assert.Equal(startingBalance + (ulong)transferAmount, endingBalance);
    }
    [Fact(DisplayName = "Transfer: Can Send From Alias Account")]
    public async Task CanSendFromAliasAccount()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAlias = await TestAliasAccount.CreateAsync(_network);

        var aliasStartingBalance = await fxAlias.Client.GetAccountBalanceAsync(fxAlias.Alias);
        var transferAmount = (aliasStartingBalance) / 2 + 1;

        var accountStartingBalance = await fxAlias.Client.GetAccountBalanceAsync(fxAccount);
        var receipt = await fxAlias.Client.TransferAsync(fxAlias.Alias, fxAccount.Record.Address, (long)transferAmount, fxAlias.PrivateKey);
        var accountEndingBalance = await fxAlias.Client.GetAccountBalanceAsync(fxAccount);
        Assert.Equal(accountStartingBalance + (ulong)transferAmount, accountEndingBalance);

        var aliasEndingBalance = await fxAlias.Client.GetAccountBalanceAsync(fxAlias.Alias);
        Assert.Equal(aliasStartingBalance - (ulong)transferAmount, aliasEndingBalance);
    }
    // PRESENTLY NOT SUPPORTED
    // Since this is not yet support on the hedera network the PAYER property of the context
    // has been left as an Address instead of an AddressOrAlias.  The SDK will only generate
    // transaction IDs using the Address form.
    //
    //[Fact(DisplayName = "Transfer: Can Send From Paying Alias Account")]
    //public async Task CanSendFromPayingAliasAccount()
    //{
    //    await using var fxAccount = await TestAccount.CreateAsync(_network);
    //    await using var fxAlias = await TestAliasAccount.CreateAsync(_network, fx => fx.InitialTransfer = 5_00_000_000);

    //    var aliasStartingBalance = await fxAlias.Client.GetAccountBalanceAsync(fxAlias.Alias);
    //    var transferAmount = (aliasStartingBalance) / 2 + 1;

    //    var accountStartingBalance = await fxAlias.Client.GetAccountBalanceAsync(fxAccount);
    //    var receipt = await fxAlias.Client.TransferAsync(fxAlias.Alias, fxAccount.Record.Address, (long)transferAmount, ctx => ctx.Payer = fxAlias.Alias);
    //    var accountEndingBalance = await fxAlias.Client.GetAccountBalanceAsync(fxAccount);
    //    Assert.Equal(accountStartingBalance + (ulong)transferAmount, accountEndingBalance);

    //    var aliasEndingBalance = await fxAlias.Client.GetAccountBalanceAsync(fxAlias.Alias);
    //    Assert.Equal(aliasStartingBalance - (ulong)transferAmount, aliasEndingBalance);
    //}
    [Fact(DisplayName = "Transfer: Can Not Send From Paying Alias Account (API Marker Test)")]
    public void CanSendFromPayingAliasAccount()
    {
        Assert.Equal(typeof(Address), typeof(IContext).GetProperty("Payer").PropertyType);
    }
}