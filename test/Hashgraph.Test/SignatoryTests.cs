using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Proto;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Hashgraph.Tests;

public class SignatoriesTests
{
    [Fact(DisplayName = "Signatories: Can Create Valid Signatories Object")]
    public void CreateValidSignatoriesObject()
    {
        var (_, privateKey1) = Generator.KeyPair();
        var (_, privateKey2) = Generator.KeyPair();

        new Signatory(privateKey1);
        new Signatory(privateKey1, privateKey2);
        new Signatory(new Signatory(privateKey1, privateKey2), new Signatory(privateKey1, privateKey2));
    }
    [Fact(DisplayName = "Signatories: Can Create Explicit Ed25519 Signatories Object")]
    public void CanCreateExplicitEd25519SignatoriesObject()
    {
        var (_, privateKey) = Generator.Ed25519KeyPair();

        var sig1 = new Signatory(KeyType.Ed25519, privateKey[^32..]);
        var sig2 = new Signatory(KeyType.Ed25519, privateKey);
        Assert.Equal(sig1, sig2);
    }
    [Fact(DisplayName = "Signatories: Implicit and Explicit Ed25519 Signatories Are Same")]
    public void ImplicitAndExplicitEd25519SignatoriesAreSame()
    {
        var (_, privateKey) = Generator.Ed25519KeyPair();

        var sig1 = new Signatory(KeyType.Ed25519, privateKey[^32..]);
        var sig2 = new Signatory(KeyType.Ed25519, privateKey);
        var sig3 = new Signatory(privateKey);
        Assert.Equal(sig1, sig2);
        Assert.Equal(sig1, sig3);
        Assert.Equal(sig2, sig3);
    }
    [Fact(DisplayName = "Signatories: Can Create Explicit ECDSASecp256K1 Signatories Object")]
    public void CanCreateExplicitECDSASecp256K1SignatoriesObject()
    {
        var (_, privateKey) = Generator.Secp256k1KeyPair();
        var unencoded = ((ECPrivateKeyParameters)PrivateKeyFactory.CreateKey(privateKey.ToArray())).D.ToByteArray();

        var sig1 = new Signatory(KeyType.ECDSASecp256K1, Hex.ToBytes(Hex.FromBytes(unencoded)));
        var sig2 = new Signatory(KeyType.ECDSASecp256K1, Hex.ToBytes(Hex.FromBytes(privateKey)));
        Assert.Equal(sig1, sig2);
    }
    [Fact(DisplayName = "Signatories: Empty Private key throws Exception")]
    public void EmptyValueForKeyThrowsError()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Signatory();
        });
        Assert.Equal("signatories", exception.ParamName);
        Assert.StartsWith("At least one Signatory in a list is required.", exception.Message);
    }
    [Fact(DisplayName = "Signatories: Invalid Bytes in Private key throws Exception")]
    public void InvalidBytesForValueForKeyThrowsError()
    {
        var (_, originalKey) = Generator.KeyPair();
        var invalidKey = originalKey.ToArray();
        invalidKey[0] = 0;
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Signatory(KeyType.Ed25519, invalidKey);
        });
        Assert.StartsWith("The private key does not appear to be encoded as a recognizable Ed25519 format.", exception.Message);

        exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Signatory(KeyType.ECDSASecp256K1, invalidKey);
        });
        Assert.StartsWith("The private key was not provided in a recognizable ECDSA Secp256K1 format.", exception.Message);
    }
    [Fact(DisplayName = "Signatories: Invalid Byte Length in Private key throws Exception")]
    public void InvalidByteLengthForValueForKeyThrowsError()
    {
        var (_, originalKey) = Generator.KeyPair();
        var invalidKey = originalKey.ToArray().Take(30).ToArray();
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Signatory(KeyType.Ed25519, invalidKey);
        });
        Assert.StartsWith("The private key does not appear to be encoded as a recognizable Ed25519 format.", exception.Message);
        exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Signatory(KeyType.ECDSASecp256K1, invalidKey);
        });
        Assert.StartsWith("The private key was not provided in a recognizable ECDSA Secp256K1 format.", exception.Message);
    }
    [Fact(DisplayName = "Signatories: Equivalent Signatories are considered Equal")]
    public void EquivalentSignatoriesAreConsideredEqual()
    {
        var (_, privateKey1) = Generator.KeyPair();
        var (_, privateKey2) = Generator.KeyPair();
        var signatory1 = new Signatory(privateKey1);
        var signatory2 = new Signatory(privateKey1);
        Assert.Equal(signatory1, signatory2);
        Assert.True(signatory1 == signatory2);
        Assert.False(signatory1 != signatory2);

        signatory1 = new Signatory(privateKey1, privateKey2);
        signatory2 = new Signatory(privateKey1, privateKey2);
        Assert.Equal(signatory1, signatory2);
        Assert.True(signatory1 == signatory2);
        Assert.False(signatory1 != signatory2);
    }
    [Fact(DisplayName = "Signatories: Disimilar Signatories are not considered Equal")]
    public void DisimilarSignatoriesAreNotConsideredEqual()
    {
        var (_, privateKey1) = Generator.KeyPair();
        var (_, privateKey2) = Generator.KeyPair();
        var signatory1 = new Signatory(privateKey1);
        var signatory2 = new Signatory(privateKey2);
        Assert.NotEqual(signatory1, signatory2);
        Assert.False(signatory1 == signatory2);
        Assert.True(signatory1 != signatory2);

        signatory1 = new Signatory(privateKey1);
        signatory2 = new Signatory(privateKey1, privateKey2);
        Assert.NotEqual(signatory1, signatory2);
        Assert.False(signatory1 == signatory2);
        Assert.True(signatory1 != signatory2);

        signatory1 = new Signatory(privateKey1, privateKey2);
        signatory2 = new Signatory(privateKey2, privateKey1);
        Assert.NotEqual(signatory1, signatory2);
        Assert.False(signatory1 == signatory2);
        Assert.True(signatory1 != signatory2);
    }

    [Fact(DisplayName = "Signatories: Disimilar Multi-Key Signatories are not considered Equal")]
    public void DisimilarMultiKeySignatoriesAreNotConsideredEqual()
    {
        var (_, privateKey1) = Generator.KeyPair();
        var (_, privateKey2) = Generator.KeyPair();
        var (_, privateKey3) = Generator.KeyPair();
        var signatories1 = new Signatory(privateKey1, privateKey2);
        var signatories2 = new Signatory(privateKey2, privateKey3);
        Assert.NotEqual(signatories1, signatories2);
        Assert.False(signatories1 == signatories2);
        Assert.True(signatories1 != signatories2);

        signatories1 = new Signatory(privateKey1, privateKey2, privateKey3);
        signatories2 = new Signatory(privateKey1, privateKey2);
        Assert.NotEqual(signatories1, signatories2);
        Assert.False(signatories1 == signatories2);
        Assert.True(signatories1 != signatories2);

        signatories1 = new Signatory(privateKey2, privateKey3, privateKey1);
        signatories2 = new Signatory(privateKey1, privateKey2, privateKey3);
        Assert.NotEqual(signatories1, signatories2);
        Assert.False(signatories1 == signatories2);
        Assert.True(signatories1 != signatories2);
    }
    [Fact(DisplayName = "Signatories: Equivalent Complex Signatories are considered Equal")]
    public void EquivalentComplexSignatoriesAreConsideredEqual()
    {
        Func<IInvoice, Task> callback = ctx => { return Task.FromResult(0); };
        var (_, privateKey1) = Generator.KeyPair();
        var (_, privateKey2) = Generator.KeyPair();
        var (_, privateKey3) = Generator.KeyPair();
        var signatory1 = new Signatory(callback);
        var signatory2 = new Signatory(callback);
        Assert.Equal(signatory1, signatory2);
        Assert.True(signatory1 == signatory2);
        Assert.False(signatory1 != signatory2);

        signatory1 = new Signatory(privateKey1, new Signatory(callback));
        signatory2 = new Signatory(privateKey1, callback);
        Assert.Equal(signatory1, signatory2);
        Assert.True(signatory1 == signatory2);
        Assert.False(signatory1 != signatory2);
        signatory1 = new Signatory(privateKey1, callback, new Signatory(privateKey2, privateKey3));
        signatory2 = new Signatory(privateKey1, callback, new Signatory(privateKey2, privateKey3));
        Assert.Equal(signatory1, signatory2);
        Assert.True(signatory1 == signatory2);
        Assert.False(signatory1 != signatory2);
    }
    [Fact(DisplayName = "Signatories: Only Reference Equal Callbacks are considered Equal")]
    public void CallbackSignatoriesAreOnlyReferenceEqual()
    {
        static Task callback1(IInvoice ctx) { return Task.FromResult(0); }
        static Task callback2(IInvoice ctx) { return Task.FromResult(0); }

        var signatory1 = new Signatory(callback1);
        var signatory2 = new Signatory(callback2);
        Assert.NotEqual(signatory1, signatory2);
        Assert.False(signatory1 == signatory2);
        Assert.True(signatory1 != signatory2);
    }
    [Fact(DisplayName = "Signatories: Null PendingParams raises an Error")]
    public void NullPendingParamsRaisesAnError()
    {
        var exception = Assert.Throws<ArgumentNullException>(() =>
        {
            new Signatory((PendingParams)null);
        });
        Assert.Equal("pendingParams", exception.ParamName);
        Assert.StartsWith("Pending Parameters object cannot be null.", exception.Message);
    }
    [Fact(DisplayName = "Signatories: Empty Scheduled Signatories Are Considered Equal")]
    public void EmptyScheduledSignatoriesAreConsideredEqual()
    {
        var schedule1 = new PendingParams();
        var schedule2 = new PendingParams();

        var signatory1 = new Signatory(schedule1);
        var signatory2 = new Signatory(schedule2);
        Assert.Equal(signatory1, signatory2);
        Assert.True(signatory1 == signatory2);
        Assert.False(signatory1 != signatory2);
    }
    [Fact(DisplayName = "Signatories: Dissimilar Signatory Schedules Are Considered Not Equal")]
    public void DissimilarSignatorySchedulesAreConsideredNotEqual()
    {
        var schedule1 = new PendingParams
        {
            Memo = "memo 1"
        };
        var schedule2 = new PendingParams
        {
            Memo = "Memo 2"
        };

        var signatory1 = new Signatory(schedule1);
        var signatory2 = new Signatory(schedule2);
        Assert.NotEqual(signatory1, signatory2);
        Assert.False(signatory1 == signatory2);
        Assert.True(signatory1 != signatory2);
    }
    [Fact(DisplayName = "Signatories: Similar Signatory Schedules Are Considered Not Equal")]
    public void SimilarSignatorySchedulesAreConsideredNotEqual()
    {
        var schedule1 = new PendingParams
        {
            Memo = "memo 1"
        };
        var schedule2 = new PendingParams
        {
            Memo = "memo 1"
        };

        var signatory1 = new Signatory(schedule1);
        var signatory2 = new Signatory(schedule2);
        Assert.Equal(signatory1, signatory2);
        Assert.True(signatory1 == signatory2);
        Assert.False(signatory1 != signatory2);
    }
    [Fact(DisplayName = "Signatories: Can Retrieve Schedule Params From Signatory")]
    public void CanRetrieveScheduleParamsFromSignatory()
    {
        var schedule = new PendingParams
        {
            Memo = Generator.Memo(20)
        };
        var signatory = new Signatory(schedule);
        var retrieved = ((ISignatory)signatory).GetSchedule();
        Assert.Equal(schedule, retrieved);
    }
    [Fact(DisplayName = "Signatories: Can Retrieve Nested Schedule Params From Signatory")]
    public void CanRetrieveNestedScheduleParamsFromSignatory()
    {
        var (_, randomKey) = Generator.KeyPair();
        var schedule = new PendingParams
        {
            Memo = Generator.Memo(20)
        };
        var signatory = new Signatory(new Signatory(new Signatory(new Signatory(schedule))), randomKey);
        var retrieved = ((ISignatory)signatory).GetSchedule();
        Assert.Equal(schedule, retrieved);
    }
    [Fact(DisplayName = "Signatories: Multiple Schedules Accepted when Identical")]
    public void MultipleSchedulesAcceptedWhenIdentical()
    {
        var memo = Generator.Memo(50);
        var (_, randomKey) = Generator.KeyPair();
        var schedule1 = new PendingParams { Memo = memo };
        var schedule2 = new PendingParams { Memo = memo };
        var signatory = new Signatory(new Signatory(new Signatory(new Signatory(schedule1)), schedule2), randomKey);
        var retrieved = ((ISignatory)signatory).GetSchedule();
        Assert.Equal(schedule1, retrieved);
        Assert.Equal(schedule1, schedule2);
    }
    [Fact(DisplayName = "Signatories: Multiple Dissimilar Schedules Raises an Error On Retrieval")]
    public void MultipleDissimilarSchedulesRaisesAnErrorOnRetrieval()
    {
        var (_, randomKey) = Generator.KeyPair();
        var schedule1 = new PendingParams { Memo = Generator.Memo(50) };
        var schedule2 = new PendingParams { Memo = Generator.Memo(50) };
        var signatory = new Signatory(new Signatory(new Signatory(new Signatory(schedule1)), schedule2), randomKey);
        var exception = Assert.Throws<InvalidOperationException>(() =>
        {
            ((ISignatory)signatory).GetSchedule();
        });
        Assert.Equal("Found Multiple Pending Signatories, do not know which one to choose.", exception.Message);
    }

    [Fact(DisplayName = "Signatories: Can Parse Ed25519 From Der Encoding")]
    public async Task CanParseEd25519FromDerEncoding()
    {
        var body = new TransactionBody
        {
            TransactionID = new TransactionID(),
            NodeAccountID = new AccountID(),
            TransactionFee = 5_00_000_000,
            TransactionValidDuration = new Proto.Duration { Seconds = 180 },
            Memo = "External Test",
            CryptoTransfer = new CryptoTransferTransactionBody()
        };
        var derPrivateKey = Hex.ToBytes("302e020100300506032b657004220420a89f2eecc02118bc7f6205b11315e0e0a185a4170fa88f28990b5db93154055a");

        var signatory = new Signatory(derPrivateKey);
        var invoice = new Invoice(body, int.MaxValue);
        await ((ISignatory)signatory).SignAsync(invoice);

        var sigPair = invoice.TryGenerateMapFromCollectedSignatures().SigPair[0];
        Assert.NotNull(sigPair);
        Assert.Equal(SignaturePair.SignatureOneofCase.Ed25519, sigPair.SignatureCase);
        Assert.Equal("b9732ad628cb6c28da0c52a3123af7f2725e7a4df53c36a7fc357334ff6dba37", Hex.FromBytes(sigPair.PubKeyPrefix.Memory));
    }

    [Fact(DisplayName = "Signatories: Can Not Parse Ed25519 Raw 32 bit key")]
    public void CanNotParseEd25519Raw32BitKey()
    {
        var derPrivateKey = Hex.ToBytes("302e020100300506032b657004220420a89f2eecc02118bc7f6205b11315e0e0a185a4170fa88f28990b5db93154055a");
        var rawPrivateKey = derPrivateKey[^32..];
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Signatory(rawPrivateKey);
        });
        Assert.StartsWith("The private key byte length of 32 is ambiguous, unable to determine which type of key this refers to.", exception.Message);
    }

    [Fact(DisplayName = "Signatories: Can Explicitly Parse Ed25519 Raw 32 bit key")]
    public async Task CanExplicitlyParseEd25519Raw32BitKey()
    {
        var body = new TransactionBody
        {
            TransactionID = new TransactionID(),
            NodeAccountID = new AccountID(),
            TransactionFee = 5_00_000_000,
            TransactionValidDuration = new Proto.Duration { Seconds = 180 },
            Memo = "External Test",
            CryptoTransfer = new CryptoTransferTransactionBody()
        };

        var derPrivateKey = Hex.ToBytes("302e020100300506032b657004220420a89f2eecc02118bc7f6205b11315e0e0a185a4170fa88f28990b5db93154055a");
        var rawPrivateKey = derPrivateKey[^32..];

        var signatory = new Signatory(KeyType.Ed25519, rawPrivateKey);
        var invoice = new Invoice(body, int.MaxValue);
        await ((ISignatory)signatory).SignAsync(invoice);

        var sigPair = invoice.TryGenerateMapFromCollectedSignatures().SigPair[0];
        Assert.NotNull(sigPair);
        Assert.Equal(SignaturePair.SignatureOneofCase.Ed25519, sigPair.SignatureCase);
        Assert.Equal("b9732ad628cb6c28da0c52a3123af7f2725e7a4df53c36a7fc357334ff6dba37", Hex.FromBytes(sigPair.PubKeyPrefix.Memory));
    }

    [Fact(DisplayName = "Signatories: Can Parse Secp256K1 From Der Encoding")]
    public async Task CanParseSecp256K1FromDerEncoding()
    {
        var body = new TransactionBody
        {
            TransactionID = new TransactionID(),
            NodeAccountID = new AccountID(),
            TransactionFee = 5_00_000_000,
            TransactionValidDuration = new Proto.Duration { Seconds = 180 },
            Memo = "External Test",
            CryptoTransfer = new CryptoTransferTransactionBody()
        };
        var (derPublicKey, derPrivateKey) = Generator.Secp256k1KeyPair();

        var signatory = new Signatory(derPrivateKey);
        var compressed = (PublicKeyFactory.CreateKey(derPublicKey.ToArray()) as ECPublicKeyParameters).Q.GetEncoded(true);
        var invoice = new Invoice(body, int.MaxValue);
        await ((ISignatory)signatory).SignAsync(invoice);

        var sigPair = invoice.TryGenerateMapFromCollectedSignatures().SigPair[0];
        Assert.NotNull(sigPair);
        Assert.Equal(SignaturePair.SignatureOneofCase.ECDSASecp256K1, sigPair.SignatureCase);
        Assert.Equal(Hex.FromBytes(compressed), Hex.FromBytes(sigPair.PubKeyPrefix.Memory));
    }

    [Fact(DisplayName = "Signatories: Can Parse Secp256K1 From Hedera Der Encoding")]
    public async Task CanParseSecp256K1FromHederaDerEncoding()
    {
        var body = new TransactionBody
        {
            TransactionID = new TransactionID(),
            NodeAccountID = new AccountID(),
            TransactionFee = 5_00_000_000,
            TransactionValidDuration = new Proto.Duration { Seconds = 180 },
            Memo = "External Test",
            CryptoTransfer = new CryptoTransferTransactionBody()
        };

        var derPrivateKey = Hex.ToBytes("3030020100300706052b8104000a042204200ea81572b0fd122cc9cb90cc57506a2723a2fe1fd7e69c0f26b3c6b6917c60c3");
        var compressed = Hex.ToBytes("02cd51c7f285ffc6c158a4aa866eb6827a61cbe178288df850f26283103a23cc1e");

        var signatory = new Signatory(derPrivateKey);
        var invoice = new Invoice(body, int.MaxValue);
        await ((ISignatory)signatory).SignAsync(invoice);

        var sigPair = invoice.TryGenerateMapFromCollectedSignatures().SigPair[0];
        Assert.NotNull(sigPair);
        Assert.Equal(SignaturePair.SignatureOneofCase.ECDSASecp256K1, sigPair.SignatureCase);
        Assert.Equal(Hex.FromBytes(compressed), Hex.FromBytes(sigPair.PubKeyPrefix.Memory));
    }

    [Fact(DisplayName = "Signatories: Can Not Parse Secp256K1 Raw 32 bit key")]
    public void CanNotParseSecp256K1Raw32BitKey()
    {
        var derPrivateKey = Hex.ToBytes("aa55060f559d5454f596c4b5676e61840add416a49fddab7b7676f8e6899f3e7");
        var rawPrivateKey = derPrivateKey[^32..];
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            new Signatory(rawPrivateKey);
        });
        Assert.StartsWith("The private key byte length of 32 is ambiguous, unable to determine which type of key this refers to.", exception.Message);
    }

    [Fact(DisplayName = "Signatories: Can Explicitly Parse Secp256K1 Raw 32 bit key")]
    public async Task CanExplicitlyParseSecp256K1Raw32BitKey()
    {
        var body = new TransactionBody
        {
            TransactionID = new TransactionID(),
            NodeAccountID = new AccountID(),
            TransactionFee = 5_00_000_000,
            TransactionValidDuration = new Proto.Duration { Seconds = 180 },
            Memo = "External Test",
            CryptoTransfer = new CryptoTransferTransactionBody()
        };

        var rawPrivateKey = Hex.ToBytes("7696d163713ef671481340aa17c825738753fd67b81f9f7e42e4a95c59431cb7");

        var signatory = new Signatory(KeyType.ECDSASecp256K1, rawPrivateKey);
        var invoice = new Invoice(body, int.MaxValue);
        await ((ISignatory)signatory).SignAsync(invoice);

        var sigPair = invoice.TryGenerateMapFromCollectedSignatures().SigPair[0];
        Assert.NotNull(sigPair);
        Assert.Equal(SignaturePair.SignatureOneofCase.ECDSASecp256K1, sigPair.SignatureCase);
        Assert.Equal("032ac21b3fb74a014c3473c51153c590c75fbd969b4b007830bccc7a99c489ab88", Hex.FromBytes(sigPair.PubKeyPrefix.Memory));
    }
}