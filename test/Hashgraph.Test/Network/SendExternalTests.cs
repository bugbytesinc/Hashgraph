namespace Hashgraph.Test.System;

[Collection(nameof(NetworkCredentials))]
public class SendExternalTests
{
    private readonly NetworkCredentials _network;
    public SendExternalTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Send External: Can Transfer Crypoto via External Transaction With No Signatories")]
    public async Task CanTransferCryptoViaExternalTransactionWithNoSignatories()
    {
        await using var fxSender = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
        await using var fxReceiver = await TestAccount.CreateAsync(_network);

        var senderInitialBalance = await fxSender.Client.GetAccountBalanceAsync(fxSender);
        var receiverInitialBalance = await fxSender.Client.GetAccountBalanceAsync(fxReceiver);

        var xferAmount = (long)(fxSender.CreateParams.InitialBalance / 2);

        var client = fxSender.Client.Clone(ctx =>
        {
            ctx.Payer = null;
            ctx.Signatory = null;
        });

        var txid = fxSender.Client.CreateNewTxId(ctx => ctx.Payer = fxSender);
        var transfers = new Proto.TransferList();
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxSender.Record.Address),
            Amount = -xferAmount
        });
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxReceiver.Record.Address),
            Amount = xferAmount
        });
        var body = new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(txid),
            NodeAccountID = new Proto.AccountID(_network.Gateway),
            TransactionFee = 5_00_000_000,
            TransactionValidDuration = new Proto.Duration { Seconds = 180 },
            Memo = "Send Test",
            CryptoTransfer = new Proto.CryptoTransferTransactionBody { Transfers = transfers }
        };
        var invoice = new Invoice(body, 6);
        var senderSignatory = new Signatory(fxSender.PrivateKey) as ISignatory;
        await senderSignatory.SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures().ToByteString().Memory;

        var precheck = await client.SendExternalTransactionAsync(signedTransaction);
        Assert.Equal(ResponseCode.Ok, precheck);

        var receipt = await client.GetReceiptAsync(txid);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(txid, receipt.Id);

        var senderFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxSender);
        var receiverFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxReceiver);

        Assert.True(senderFinalBalance < (ulong)((long)senderInitialBalance - xferAmount));
        Assert.Equal(receiverInitialBalance + (ulong)xferAmount, receiverFinalBalance);
    }

    [Fact(DisplayName = "Send External: Can Transfer Crypoto via External Unsigned Transaction Local Payer")]
    public async Task CanTransferCrypotoViaExternalUnsignedTransactionLocalPayer()
    {
        await using var fxSender = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
        await using var fxReceiver = await TestAccount.CreateAsync(_network);

        var senderInitialBalance = await fxSender.Client.GetAccountBalanceAsync(fxSender);
        var receiverInitialBalance = await fxSender.Client.GetAccountBalanceAsync(fxReceiver);

        var xferAmount = (long)(fxSender.CreateParams.InitialBalance / 2);

        var client = fxSender.Client.Clone(ctx =>
        {
            ctx.Payer = fxSender;
            ctx.Signatory = fxSender.PrivateKey;
        });

        var txid = client.CreateNewTxId();
        var transfers = new Proto.TransferList();
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxSender.Record.Address),
            Amount = -xferAmount
        });
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxReceiver.Record.Address),
            Amount = xferAmount
        });
        var body = new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(txid),
            NodeAccountID = new Proto.AccountID(_network.Gateway),
            TransactionFee = 5_00_000_000,
            TransactionValidDuration = new Proto.Duration { Seconds = 180 },
            Memo = "Send Test",
            CryptoTransfer = new Proto.CryptoTransferTransactionBody { Transfers = transfers }
        };
        var signedTransaction = new SignedTransaction
        {
            BodyBytes = body.ToByteString()
        };

        var precheck = await client.SendExternalTransactionAsync(signedTransaction.ToByteArray());
        Assert.Equal(ResponseCode.Ok, precheck);

        var receipt = await client.GetReceiptAsync(txid);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(txid, receipt.Id);

        var senderFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxSender);
        var receiverFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxReceiver);

        Assert.True(senderFinalBalance < (ulong)((long)senderInitialBalance - xferAmount));
        Assert.Equal(receiverInitialBalance + (ulong)xferAmount, receiverFinalBalance);
    }

    [Fact(DisplayName = "Send External: Can Transfer Crypoto via Signed External Transaction with local Payer")]
    public async Task CanTransferCrypotoViaSignedExternalTransactionWithLocalPayer()
    {
        await using var fxSender = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
        await using var fxReceiver = await TestAccount.CreateAsync(_network);

        var senderInitialBalance = await fxSender.Client.GetAccountBalanceAsync(fxSender);
        var receiverInitialBalance = await fxSender.Client.GetAccountBalanceAsync(fxReceiver);

        var xferAmount = (long)(fxSender.CreateParams.InitialBalance / 2);

        var txid = fxSender.Client.CreateNewTxId();
        var transfers = new Proto.TransferList();
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxSender.Record.Address),
            Amount = -xferAmount
        });
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxReceiver.Record.Address),
            Amount = xferAmount
        });
        var body = new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(txid),
            NodeAccountID = new Proto.AccountID(_network.Gateway),
            TransactionFee = 5_00_000_000,
            TransactionValidDuration = new Proto.Duration { Seconds = 180 },
            Memo = "Send Test",
            CryptoTransfer = new CryptoTransferTransactionBody { Transfers = transfers }
        };
        var invoice = new Invoice(body, 32);
        var senderSignatory = new Signatory(fxSender.PrivateKey) as ISignatory;
        await senderSignatory.SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures().ToByteString().Memory;

        var precheck = await fxSender.Client.SendExternalTransactionAsync(signedTransaction);
        Assert.Equal(ResponseCode.Ok, precheck);

        var receipt = await fxSender.Client.GetReceiptAsync(txid);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(txid, receipt.Id);

        var senderFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxSender);
        var receiverFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxReceiver);

        Assert.Equal(senderInitialBalance - (ulong)xferAmount, senderFinalBalance);
        Assert.Equal(receiverInitialBalance + (ulong)xferAmount, receiverFinalBalance);
    }

    [Fact(DisplayName = "Send External: Empty Protobuf Array Raises Error")]
    public async Task EmptyProtobufArrayRaisesError()
    {
        var client = _network.NewClient();

        var ae = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await client.SendExternalTransactionAsync(ReadOnlyMemory<byte>.Empty);
        });
        Assert.Equal("signedTransactionBytes", ae.ParamName);
        Assert.StartsWith("Missing Signed Transaction Bytes (was empty).", ae.Message);
    }

    [Fact(DisplayName = "Send External: Empty Body Bytes Raises Error")]
    public async Task EmptyBodyBytesRaisesError()
    {
        var client = _network.NewClient();
        var signedTx = new SignedTransaction();

        var ae = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await client.SendExternalTransactionAsync(signedTx.ToByteArray());
        });
        Assert.Equal("signedTransactionBytes", ae.ParamName);
        Assert.StartsWith("Missing Signed Transaction Bytes (was empty).", ae.Message);
    }
    [Fact(DisplayName = "Send External: Unknown Transaction Body Type Raises Error")]
    public async Task UnknownTransactionBodyTypeRaisesError()
    {
        var client = _network.NewClient();
        var signedTx = new SignedTransaction
        {
            BodyBytes = Generator.TransactionID().ToByteString()
        };

        var ae = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await client.SendExternalTransactionAsync(signedTx.ToByteArray());
        });
        Assert.Equal("signedTransactionBytes", ae.ParamName);
        Assert.StartsWith("Unrecognized Transaction Type, unable to determine which Hedera Network Service Type should process transaction.", ae.Message);
    }
    [Fact(DisplayName = "Send External: Invalid Gateway Raises Error")]
    public async Task InvalidGatewayRaisesError()
    {
        await using var fxSender = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
        await using var fxReceiver = await TestAccount.CreateAsync(_network);

        var client = _network.NewClient().Clone(ctx => ctx.Gateway = null);

        var txid = fxSender.Client.CreateNewTxId();
        var transfers = new TransferList();
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxSender.Record.Address),
            Amount = -1
        });
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxReceiver.Record.Address),
            Amount = 1
        });
        var body = new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(txid),
            NodeAccountID = new Proto.AccountID(_network.Gateway),
            TransactionFee = 5_00_000_000,
            TransactionValidDuration = new Proto.Duration { Seconds = 180 },
            Memo = "Send Test",
            CryptoTransfer = new CryptoTransferTransactionBody { Transfers = transfers }
        };
        var invoice = new Invoice(body, 10);
        var senderSignatory = new Signatory(fxSender.PrivateKey) as ISignatory;
        await senderSignatory.SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures().ToByteString().Memory;

        var ioe = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await client.SendExternalTransactionAsync(signedTransaction);
        });
        Assert.StartsWith("The Network Gateway Node has not been configured. Please check that 'Gateway' is set in the Client context and is compatible with this external transaction.", ioe.Message);
    }
    [Fact(DisplayName = "Send External: Gateway Mismatch Raises Error")]
    public async Task GatewayMismatchRaisesError()
    {
        await using var fxSender = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
        await using var fxReceiver = await TestAccount.CreateAsync(_network);

        var client = _network.NewClient().Clone(ctx =>
        {
            var old = ctx.Gateway;
            ctx.Gateway = new Gateway(old.Uri, old.ShardNum, old.RealmNum, old.AccountNum + 1);
        });

        var txid = fxSender.Client.CreateNewTxId();
        var transfers = new TransferList();
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxSender.Record.Address),
            Amount = -1
        });
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxReceiver.Record.Address),
            Amount = 1
        });
        var body = new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(txid),
            NodeAccountID = new Proto.AccountID(_network.Gateway),
            TransactionFee = 5_00_000_000,
            TransactionValidDuration = new Proto.Duration { Seconds = 180 },
            Memo = "Send Test",
            CryptoTransfer = new CryptoTransferTransactionBody { Transfers = transfers }
        };
        var invoice = new Invoice(body, 10);
        var senderSignatory = new Signatory(fxSender.PrivateKey) as ISignatory;
        await senderSignatory.SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures().ToByteString().Memory;

        var ae = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await client.SendExternalTransactionAsync(signedTransaction);
        });
        Assert.Equal("signedTransactionBytes", ae.ParamName);
        Assert.StartsWith("The configured Gateway is not compatible with the Node Account ID of this transaction.", ae.Message);
    }
    [Fact(DisplayName = "Send External: Scheduling External Transaction Raises Error")]
    public async Task SchedulingExternalTransactionRaisesError()
    {
        await using var fxSender = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
        await using var fxReceiver = await TestAccount.CreateAsync(_network);

        var client = _network.NewClient();
        var txid = fxSender.Client.CreateNewTxId();
        var transfers = new TransferList();
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxSender.Record.Address),
            Amount = -1
        });
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxReceiver.Record.Address),
            Amount = 1
        });
        var body = new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(txid),
            NodeAccountID = new Proto.AccountID(_network.Gateway),
            TransactionFee = 5_00_000_000,
            TransactionValidDuration = new Proto.Duration { Seconds = 180 },
            Memo = "Send Test",
            CryptoTransfer = new CryptoTransferTransactionBody { Transfers = transfers }
        };
        var invoice = new Invoice(body, 10);
        var senderSignatory = new Signatory(fxSender.PrivateKey) as ISignatory;
        await senderSignatory.SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures().ToByteString().Memory;

        var receipt = await fxSender.Client.SubmitExternalTransactionAsync(signedTransaction, ctx => ctx.SignaturePrefixTrimLimit = 10);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(txid, receipt.Id);

        var ae = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await client.SendExternalTransactionAsync(signedTransaction, new PendingParams { PendingPayer = fxReceiver });
        });
        Assert.Equal("signedTransactionBytes", ae.ParamName);
        Assert.StartsWith("Scheduling the submission of an external transaction is not supported (one or more signatories in the context were created as pending signatories).  However, the external transaction itself can be a scheduled transaction.", ae.Message);
    }
    [Fact(DisplayName = "Send External: Submitting with no signatures is still accepted")]
    public async Task SubmittingWithNoSignaturesIsStillAccepted()
    {
        await using var fxSender = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
        await using var fxReceiver = await TestAccount.CreateAsync(_network);

        var client = _network.NewClient().Clone(fx =>
        {
            fx.Payer = fxSender;
            fx.Signatory = null;
        });
        var txid = fxSender.Client.CreateNewTxId();
        var transfers = new TransferList();
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxSender.Record.Address),
            Amount = -1
        });
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxReceiver.Record.Address),
            Amount = 1
        });
        var body = new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(txid),
            NodeAccountID = new Proto.AccountID(_network.Gateway),
            TransactionFee = 5_00_000_000,
            TransactionValidDuration = new Proto.Duration { Seconds = 180 },
            Memo = "Send Test",
            CryptoTransfer = new CryptoTransferTransactionBody { Transfers = transfers }
        };
        var signedTransaction = new SignedTransaction
        {
            BodyBytes = body.ToByteString()
        };
        var precheck = await fxSender.Client.SendExternalTransactionAsync(signedTransaction.ToByteArray());
        Assert.Equal(ResponseCode.Ok, precheck);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxSender.Client.GetReceiptAsync(txid);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.Equal(txid, tex.TxId);
        Assert.StartsWith("Unable to retreive receipt, status: InvalidSignature", tex.Message);
    }
    [Fact(DisplayName = "Send External: Bogus Protobuf Bytes Raises Error")]
    public async Task BogusProtobufBytesRaisesError()
    {
        var client = _network.NewClient();
        var signedTx = Generator.SHA384Hash();

        var ae = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await client.SendExternalTransactionAsync(signedTx);
        });
        Assert.Equal("signedTransactionBytes", ae.ParamName);
        Assert.StartsWith("Signed Transaction Bytes not recognized as valid Protobuf.", ae.Message);
    }
    [Fact(DisplayName = "Send External: Invlalid Transfer List Returns Error Code Without Throwing Exception")]
    public async Task InvlalidTransferListReturnsErrorCodeWithoutThrowingException()
    {
        await using var fxSender = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);
        await using var fxReceiver = await TestAccount.CreateAsync(_network);

        var senderInitialBalance = await fxSender.Client.GetAccountBalanceAsync(fxSender);
        var receiverInitialBalance = await fxSender.Client.GetAccountBalanceAsync(fxReceiver);

        var xferAmount = (long)(fxSender.CreateParams.InitialBalance / 3);

        var client = fxSender.Client.Clone(ctx =>
        {
            ctx.Payer = null;
            ctx.Signatory = null;
        });

        var txid = fxSender.Client.CreateNewTxId(ctx => ctx.Payer = fxSender);
        var transfers = new Proto.TransferList();
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxSender.Record.Address),
            Amount = -xferAmount
        });
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxReceiver.Record.Address),
            Amount = 2 * xferAmount
        });
        transfers.AccountAmounts.Add(new Proto.AccountAmount
        {
            AccountID = new Proto.AccountID(fxSender.Record.Address),
            Amount = -xferAmount
        });
        var body = new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(txid),
            NodeAccountID = new Proto.AccountID(_network.Gateway),
            TransactionFee = 5_00_000_000,
            TransactionValidDuration = new Proto.Duration { Seconds = 180 },
            Memo = "Send Test",
            CryptoTransfer = new Proto.CryptoTransferTransactionBody { Transfers = transfers }
        };
        var invoice = new Invoice(body, 6);
        var senderSignatory = new Signatory(fxSender.PrivateKey) as ISignatory;
        await senderSignatory.SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures().ToByteString().Memory;

        var precheck = await client.SendExternalTransactionAsync(signedTransaction);
        Assert.Equal(ResponseCode.AccountRepeatedInAccountAmounts, precheck);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxSender.Client.GetReceiptAsync(txid);
        });
        Assert.Equal(ResponseCode.ReceiptNotFound, tex.Status);
        Assert.Equal(txid, tex.TxId);
        Assert.StartsWith("Network failed to return a transaction receipt, Status Code Returned: ReceiptNotFound", tex.Message);

        var senderFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxSender);
        var receiverFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxReceiver);

        Assert.Equal(senderFinalBalance, senderInitialBalance);
        Assert.Equal(receiverInitialBalance, receiverFinalBalance);
    }
}