using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using NSec.Cryptography;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Signature;

[Collection(nameof(NetworkCredentials))]
public class SignatureTests
{
    private readonly NetworkCredentials _network;
    public SignatureTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Signature: Can Sign with Extra Signature from Unrelated Key")]
    public async Task CanSignTransactionWithExtraSignature()
    {
        var (_, privateKey) = Generator.KeyPair();
        await using var fx = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await AssertHg.CryptoBalanceAsync(fx, 0);

        var transferAmount = Generator.Integer(10, 100);
        var receipt = await fx.Client.TransferAsync(_network.Payer, fx.Record.Address, transferAmount, ctx =>
        {
            ctx.Signatory = new Signatory(_network.PrivateKey, privateKey);
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);
        await AssertHg.CryptoBalanceAsync(fx, transferAmount);
    }
    [Fact(DisplayName = "Signature: Unrelated Public Keys can Sign Unrelated Message")]
    public async Task UnrelatedPublicKeysCanSignUnrelatedMessage()
    {
        var (_, privateKey) = Generator.Ed25519KeyPair();
        await using var fx = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await AssertHg.CryptoBalanceAsync(fx, 0);

        var transferAmount = Generator.Integer(10, 100);
        var receipt = await fx.Client.TransferAsync(_network.Payer, fx.Record.Address, transferAmount, ctx =>
        {
            ctx.Signatory = new Signatory(_network.PrivateKey, new Signatory(CustomSigner));
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);
        await AssertHg.CryptoBalanceAsync(fx, transferAmount);

        Task CustomSigner(IInvoice invoice)
        {
            var randomBytes = Generator.SHA384Hash();
            var signingKey = TestKeys.ImportPrivateEd25519KeyFromBytes(privateKey);
            var prefix = signingKey.PublicKey.Export(KeyBlobFormat.PkixPublicKey).TakeLast(32).Take(6).ToArray();
            var signature = SignatureAlgorithm.Ed25519.Sign(signingKey, randomBytes.Span);
            invoice.AddSignature(KeyType.Ed25519, prefix, signature);
            return Task.CompletedTask;
        }
    }
    [Fact(DisplayName = "Signature: Can Embed Messages in the Signature Map")]
    public async Task CanEmbedMessagesInTheSignatureMap()
    {
        var (_, privateKey) = Generator.Ed25519KeyPair();
        var randomBytes = Generator.SHA384Hash();
        await using var fx = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await AssertHg.CryptoBalanceAsync(fx, 0);

        var transferAmount = Generator.Integer(10, 100);
        var receipt = await fx.Client.TransferAsync(_network.Payer, fx.Record.Address, transferAmount, ctx =>
        {
            ctx.Signatory = new Signatory(_network.PrivateKey, new Signatory(CustomSigner));
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);
        await AssertHg.CryptoBalanceAsync(fx, transferAmount);

        Task CustomSigner(IInvoice invoice)
        {
            var message = Encoding.ASCII.GetBytes("This is an Embedded Message");
            var signingKey = TestKeys.ImportPrivateEd25519KeyFromBytes(privateKey);
            var signature = SignatureAlgorithm.Ed25519.Sign(signingKey, message);
            invoice.AddSignature(KeyType.Ed25519, message, signature);
            return Task.CompletedTask;
        }
    }

    [Fact(DisplayName = "Signature: Can Embed Messages in the Signature Itself")]
    public async Task CanEmbedMessagesInTheSignatureItself()
    {
        var (_, privateKey) = Generator.Ed25519KeyPair();
        var randomBytes = Generator.SHA384Hash();
        await using var fx = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 0);
        await AssertHg.CryptoBalanceAsync(fx, 0);

        var transferAmount = Generator.Integer(10, 100);
        var receipt = await fx.Client.TransferAsync(_network.Payer, fx.Record.Address, transferAmount, ctx =>
        {
            ctx.Signatory = new Signatory(_network.PrivateKey, new Signatory(CustomSigner));
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);
        await AssertHg.CryptoBalanceAsync(fx, transferAmount);

        Task CustomSigner(IInvoice invoice)
        {
            var message = Encoding.ASCII.GetBytes("This is an Embedded Message");
            var signingKey = TestKeys.ImportPrivateEd25519KeyFromBytes(privateKey);
            invoice.AddSignature(KeyType.Ed25519, message, message);
            return Task.CompletedTask;
        }
    }
    [Fact(DisplayName = "Signature: Signature Map No Prefix With Trim of Zero And One Signature")]
    public async Task SignatureMapNoPrefixWithTrimOfZeroAndOneSignature()
    {
        await using var client = _network.NewClient();
        var (_, privateKey) = Generator.Ed25519KeyPair();
        var invoice = new Invoice(new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(client.CreateNewTxId()),
            Memo = Generator.String(20, 30)
        }, 0);
        var signatory = new Signatory(CustomSigner);
        await (signatory as ISignatory).SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures();
        var signatureMap = signedTransaction.SigMap;
        Assert.Single(signatureMap.SigPair);
        Assert.Empty(signatureMap.SigPair[0].PubKeyPrefix);

        Task CustomSigner(IInvoice invoice)
        {
            var signingKey = TestKeys.ImportPrivateEd25519KeyFromBytes(privateKey);
            var prefix = signingKey.PublicKey.Export(KeyBlobFormat.PkixPublicKey).ToArray();
            var signature = SignatureAlgorithm.Ed25519.Sign(signingKey, invoice.TxBytes.Span);
            invoice.AddSignature(KeyType.Ed25519, prefix, signature);
            return Task.CompletedTask;
        }
    }
    [Fact(DisplayName = "Signature: Signature Map No With One Signature and Trim Limit Inlucdes Prefix")]
    public async Task SignatureMapNoWithOneSignatureAndTrimLimitInlucdesPrefix()
    {
        await using var client = _network.NewClient();
        var (_, privateKey) = Generator.Ed25519KeyPair();
        var trimLimit = Generator.Integer(5, 10);
        var invoice = new Invoice(new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(client.CreateNewTxId()),
            Memo = Generator.String(20, 30)
        }, trimLimit);
        var signatory = new Signatory(CustomSigner);
        await (signatory as ISignatory).SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures();
        var signatureMap = signedTransaction.SigMap;
        Assert.Single(signatureMap.SigPair);
        Assert.Equal(trimLimit, signatureMap.SigPair[0].PubKeyPrefix.Length);

        Task CustomSigner(IInvoice invoice)
        {
            var signingKey = TestKeys.ImportPrivateEd25519KeyFromBytes(privateKey);
            var prefix = signingKey.PublicKey.Export(KeyBlobFormat.PkixPublicKey).ToArray();
            var signature = SignatureAlgorithm.Ed25519.Sign(signingKey, invoice.TxBytes.Span);
            invoice.AddSignature(KeyType.Ed25519, prefix, signature);
            return Task.CompletedTask;
        }
    }
    [Fact(DisplayName = "Signature: Multiple Keys With Similar Starting Prefixes are Still Unique")]
    public async Task MultipleKeysWithSimilarStartingPrefixesStillUnique()
    {
        var prefix = Encoding.ASCII.GetBytes(Generator.String(10, 20));
        var sigCount = Generator.Integer(5, 10);
        await using var client = _network.NewClient();
        var invoice = new Invoice(new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(client.CreateNewTxId()),
            Memo = Generator.String(20, 30)
        }, 0);
        await (new Signatory(CustomSigner) as ISignatory).SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures();
        var signatureMap = signedTransaction.SigMap;
        Assert.Equal(sigCount, signatureMap.SigPair.Count);
        foreach (var sig in signatureMap.SigPair)
        {
            Assert.Equal(prefix.Length, sig.PubKeyPrefix.Length);
        }

        Task CustomSigner(IInvoice invoice)
        {
            var signingKey = TestKeys.ImportPrivateEd25519KeyFromBytes(Generator.Ed25519KeyPair().privateKey);
            var signature = SignatureAlgorithm.Ed25519.Sign(signingKey, invoice.TxBytes.Span);
            for (int i = 0; i < sigCount; i++)
            {
                var thumbprint = prefix.Clone() as byte[];
                thumbprint[thumbprint.Length - 1] = (byte)i;
                invoice.AddSignature(KeyType.Ed25519, thumbprint, signature);
            }
            return Task.CompletedTask;
        }
    }
    [Fact(DisplayName = "Signature: Prefix Trim Limit is Respected")]
    public async Task PrefixTrimLimitIsRespected()
    {
        var prefix = Encoding.ASCII.GetBytes(Generator.String(10, 20));
        var sigCount = Generator.Integer(5, 10);
        await using var client = _network.NewClient();
        var invoice = new Invoice(new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(client.CreateNewTxId()),
            Memo = Generator.String(20, 30)
        }, prefix.Length + 10);
        await (new Signatory(CustomSigner) as ISignatory).SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures();
        var signatureMap = signedTransaction.SigMap;
        Assert.Equal(sigCount, signatureMap.SigPair.Count);
        foreach (var sig in signatureMap.SigPair)
        {
            Assert.Equal(prefix.Length, sig.PubKeyPrefix.Length);
        }

        Task CustomSigner(IInvoice invoice)
        {
            var signingKey = TestKeys.ImportPrivateEd25519KeyFromBytes(Generator.Ed25519KeyPair().privateKey);
            var signature = SignatureAlgorithm.Ed25519.Sign(signingKey, invoice.TxBytes.Span);
            for (int i = 0; i < sigCount; i++)
            {
                var thumbprint = prefix.Clone() as byte[];
                thumbprint[thumbprint.Length - 1] = (byte)i;
                invoice.AddSignature(KeyType.Ed25519, thumbprint, signature);
            }
            return Task.CompletedTask;
        }
    }

    [Fact(DisplayName = "Signature: Prefix Trim Accounts for Short Prefixes")]
    public async Task PrefixTrimAccountsForShortPrefixes()
    {
        var sigCount = Generator.Integer(5, 10);
        var prefix = Encoding.ASCII.GetBytes(Generator.Code(sigCount + 10));
        await using var client = _network.NewClient();
        var invoice = new Invoice(new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(client.CreateNewTxId()),
            Memo = Generator.String(20, 30)
        }, sigCount - 3);
        await (new Signatory(CustomSigner) as ISignatory).SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures();
        var signatureMap = signedTransaction.SigMap;
        Assert.Equal(sigCount, signatureMap.SigPair.Count);
        for (int i = 0; i < signatureMap.SigPair.Count; i++)
        {
            Assert.Equal(i + 1, signatureMap.SigPair[i].PubKeyPrefix.Length);
        }
        Task CustomSigner(IInvoice invoice)
        {
            var signingKey = TestKeys.ImportPrivateEd25519KeyFromBytes(Generator.Ed25519KeyPair().privateKey);
            var signature = SignatureAlgorithm.Ed25519.Sign(signingKey, invoice.TxBytes.Span);
            for (int i = 0; i < sigCount; i++)
            {
                var thumbprint = prefix.Take(1 + i).ToArray();
                invoice.AddSignature(KeyType.Ed25519, thumbprint, signature);
            }
            return Task.CompletedTask;
        }
    }
    [Fact(DisplayName = "Signature: Duplicate Signatures are Reduced")]
    public async Task DuplicateSignaturesAreReduced()
    {
        await using var client = _network.NewClient();
        var (_, privateKey) = Generator.Ed25519KeyPair();
        var invoice = new Invoice(new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(client.CreateNewTxId()),
            Memo = Generator.String(20, 30)
        }, 0);
        var signatory = new Signatory(CustomSigner);
        await (signatory as ISignatory).SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures();
        var signatureMap = signedTransaction.SigMap;
        Assert.Single(signatureMap.SigPair);
        Assert.Empty(signatureMap.SigPair[0].PubKeyPrefix);

        Task CustomSigner(IInvoice invoice)
        {
            for (int i = 0; i < Generator.Integer(3, 5); i++)
            {
                var signingKey = TestKeys.ImportPrivateEd25519KeyFromBytes(privateKey);
                var prefix = signingKey.PublicKey.Export(KeyBlobFormat.PkixPublicKey).ToArray();
                var signature = SignatureAlgorithm.Ed25519.Sign(signingKey, invoice.TxBytes.Span);
                invoice.AddSignature(KeyType.Ed25519, prefix, signature);
            }
            return Task.CompletedTask;
        }
    }
    [Fact(DisplayName = "Signature: Some Duplicate Signatures are Reduced")]
    public async Task SomeDuplicateSignaturesAreReduced()
    {
        await using var client = _network.NewClient();
        var (_, privateKey1) = Generator.Ed25519KeyPair();
        var (_, privateKey2) = Generator.Ed25519KeyPair();
        var invoice = new Invoice(new Proto.TransactionBody
        {
            TransactionID = new Proto.TransactionID(client.CreateNewTxId()),
            Memo = Generator.String(20, 30)
        }, 0);
        var signatory = new Signatory(CustomSigner);
        await (signatory as ISignatory).SignAsync(invoice);
        var signedTransaction = invoice.GenerateSignedTransactionFromSignatures();
        var signatureMap = signedTransaction.SigMap;
        Assert.Equal(2, signatureMap.SigPair.Count);

        Task CustomSigner(IInvoice invoice)
        {
            for (int i = 0; i < Generator.Integer(3, 5); i++)
            {
                var signingKey = TestKeys.ImportPrivateEd25519KeyFromBytes(privateKey1);
                var prefix = signingKey.PublicKey.Export(KeyBlobFormat.PkixPublicKey).ToArray();
                var signature = SignatureAlgorithm.Ed25519.Sign(signingKey, invoice.TxBytes.Span);
                invoice.AddSignature(KeyType.Ed25519, prefix, signature);
            }
            for (int i = 0; i < Generator.Integer(3, 5); i++)
            {
                var signingKey = TestKeys.ImportPrivateEd25519KeyFromBytes(privateKey2);
                var prefix = signingKey.PublicKey.Export(KeyBlobFormat.PkixPublicKey).ToArray();
                var signature = SignatureAlgorithm.Ed25519.Sign(signingKey, invoice.TxBytes.Span);
                invoice.AddSignature(KeyType.Ed25519, prefix, signature);
            }
            return Task.CompletedTask;
        }
    }
}