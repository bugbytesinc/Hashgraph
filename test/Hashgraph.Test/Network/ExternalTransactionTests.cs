namespace Hashgraph.Test.System;

[Collection(nameof(NetworkCredentials))]
public class ExternalTransactionTests
{
    private readonly NetworkCredentials _network;
    public ExternalTransactionTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "External Transactions: Can Transfer Crypoto via External Transaction With No Signatories")]
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
            Memo = "External Test",
            CryptoTransfer = new Proto.CryptoTransferTransactionBody { Transfers = transfers }
        };
        var invoice = new Invoice(body, 6);
        var senderSignatory = new Signatory(fxSender.PrivateKey) as ISignatory;
        await senderSignatory.SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures().ToByteString().Memory;

        var receipt = await client.SubmitExternalTransactionAsync(signedTransaction);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(txid, receipt.Id);

        var senderFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxSender);
        var receiverFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxReceiver);

        Assert.True(senderFinalBalance < (ulong)((long)senderInitialBalance - xferAmount));
        Assert.Equal(receiverInitialBalance + (ulong)xferAmount, receiverFinalBalance);
    }

    [Fact(DisplayName = "External Transactions: Can Transfer Crypoto via External Unsigned Transaction Local Payer")]
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
            Memo = "External Test",
            CryptoTransfer = new Proto.CryptoTransferTransactionBody { Transfers = transfers }
        };
        var signedTransaction = new SignedTransaction
        {
            BodyBytes = body.ToByteString()
        };

        var receipt = await client.SubmitExternalTransactionAsync(signedTransaction.ToByteArray());
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(txid, receipt.Id);

        var senderFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxSender);
        var receiverFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxReceiver);

        Assert.True(senderFinalBalance < (ulong)((long)senderInitialBalance - xferAmount));
        Assert.Equal(receiverInitialBalance + (ulong)xferAmount, receiverFinalBalance);
    }

    [Fact(DisplayName = "External Transactions: Can Transfer Crypoto via Signed External Transaction with local Payer")]
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
            Memo = "External Test",
            CryptoTransfer = new CryptoTransferTransactionBody { Transfers = transfers }
        };
        var invoice = new Invoice(body, 32);
        var senderSignatory = new Signatory(fxSender.PrivateKey) as ISignatory;
        await senderSignatory.SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures().ToByteString().Memory;

        var receipt = await fxSender.Client.SubmitExternalTransactionAsync(signedTransaction);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(txid, receipt.Id);

        var senderFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxSender);
        var receiverFinalBalance = await fxSender.Client.GetAccountBalanceAsync(fxReceiver);

        Assert.Equal(senderInitialBalance - (ulong)xferAmount, senderFinalBalance);
        Assert.Equal(receiverInitialBalance + (ulong)xferAmount, receiverFinalBalance);
    }

    [Fact(DisplayName = "External Transactions: Empty Protobuf Array Raises Error")]
    public async Task EmptyProtobufArrayRaisesError()
    {
        var client = _network.NewClient();

        var ae = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await client.SubmitExternalTransactionAsync(ReadOnlyMemory<byte>.Empty);
        });
        Assert.Equal("signedTransactionBytes", ae.ParamName);
        Assert.StartsWith("Missing Signed Transaction Bytes (was empty).", ae.Message);
    }

    [Fact(DisplayName = "External Transactions: Empty Body Bytes Raises Error")]
    public async Task EmptyBodyBytesRaisesError()
    {
        var client = _network.NewClient();
        var signedTx = new SignedTransaction();

        var ae = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await client.SubmitExternalTransactionAsync(signedTx.ToByteArray());
        });
        Assert.Equal("signedTransactionBytes", ae.ParamName);
        Assert.StartsWith("Missing Signed Transaction Bytes (was empty).", ae.Message);
    }
    [Fact(DisplayName = "External Transactions: Unknown Transaction Body Type Raises Error")]
    public async Task UnknownTransactionBodyTypeRaisesError()
    {
        var client = _network.NewClient();
        var signedTx = new SignedTransaction
        {
            BodyBytes = Generator.TransactionID().ToByteString()
        };

        var ae = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await client.SubmitExternalTransactionAsync(signedTx.ToByteArray());
        });
        Assert.Equal("signedTransactionBytes", ae.ParamName);
        Assert.StartsWith("Unrecognized Transaction Type, unable to determine which Hedera Network Service Type should process transaction.", ae.Message);
    }
    [Fact(DisplayName = "External Transactions: Invalid Gateway Raises Error")]
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
            Memo = "External Test",
            CryptoTransfer = new CryptoTransferTransactionBody { Transfers = transfers }
        };
        var invoice = new Invoice(body, 10);
        var senderSignatory = new Signatory(fxSender.PrivateKey) as ISignatory;
        await senderSignatory.SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures().ToByteString().Memory;

        var receipt = await fxSender.Client.SubmitExternalTransactionAsync(signedTransaction, ctx => ctx.SignaturePrefixTrimLimit = 10);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(txid, receipt.Id);

        var ioe = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await client.SubmitExternalTransactionAsync(signedTransaction);
        });
        Assert.StartsWith("The Network Gateway Node has not been configured. Please check that 'Gateway' is set in the Client context and is compatible with this external transaction.", ioe.Message);
    }
    [Fact(DisplayName = "External Transactions: Gateway Mismatch Raises Error")]
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
            Memo = "External Test",
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
            await client.SubmitExternalTransactionAsync(signedTransaction);
        });
        Assert.Equal("signedTransactionBytes", ae.ParamName);
        Assert.StartsWith("The configured Gateway is not compatible with the Node Account ID of this transaction.", ae.Message);
    }
    [Fact(DisplayName = "External Transactions: Scheduling External Transaction Raises Error")]
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
            Memo = "External Test",
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
            await client.SubmitExternalTransactionAsync(signedTransaction, new PendingParams { PendingPayer = fxReceiver });
        });
        Assert.Equal("signedTransactionBytes", ae.ParamName);
        Assert.StartsWith("Scheduling the submission of an external transaction is not supported (one or more signatories in the context were created as pending signatories).  However, the external transaction itself can be a scheduled transaction.", ae.Message);
    }
    [Fact(DisplayName = "External Transactions: Submitting with no signatures fails")]
    public async Task SubmittingWithNoSignaturesFails()
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
            Memo = "External Test",
            CryptoTransfer = new CryptoTransferTransactionBody { Transfers = transfers }
        };
        var signedTransaction = new SignedTransaction
        {
            BodyBytes = body.ToByteString()
        };
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxSender.Client.SubmitExternalTransactionAsync(signedTransaction.ToByteArray());
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: InvalidSignature", tex.Message);
    }
    [Fact(DisplayName = "External Transactions: Bogus Protobuf Bytes Raises Error")]
    public async Task BogusProtobufBytesRaisesError()
    {
        var client = _network.NewClient();
        var signedTx = Generator.SHA384Hash();

        var ae = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await client.SubmitExternalTransactionAsync(signedTx);
        });
        Assert.Equal("signedTransactionBytes", ae.ParamName);
        Assert.StartsWith("Signed Transaction Bytes not recognized as valid Protobuf.", ae.Message);
    }
}