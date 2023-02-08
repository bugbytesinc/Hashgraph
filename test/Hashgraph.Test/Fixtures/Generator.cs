using Hashgraph.Implementation;
using NSec.Cryptography;
using Org.BouncyCastle.Asn1.Sec;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using System;
using System.Security.Cryptography;

namespace Hashgraph.Test.Fixtures;

public static class Generator
{
    private static readonly Random _random = new Random();
    private static readonly char[] _sample = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-*&%$#@!".ToCharArray();
    private static readonly char[] _alphaSample = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

    public static Int32 Integer(Int32 minValueInclusive, Int32 maxValueInclusive)
    {
        return _random.Next(minValueInclusive, maxValueInclusive + 1);
    }

    public static Double Double(Double minValueInclusive, Double maxValueInclusive)
    {
        return (_random.NextDouble() * (maxValueInclusive - minValueInclusive)) + minValueInclusive;
    }

    public static String Memo(Int32 minLengthInclusive, Int32 maxLengthInclusive = 0)
    {
        var length = maxLengthInclusive > 0 ? Integer(minLengthInclusive, maxLengthInclusive) : minLengthInclusive;
        return length > 15 ? ".NET SDK Test: " + Code(length - 15) : ".NET SDK Test: ".Substring(0, length);
    }

    public static String String(Int32 minLengthInclusive, Int32 maxLengthInclusive)
    {
        return Code(Integer(minLengthInclusive, maxLengthInclusive));
    }

    public static string Code(Int32 length)
    {
        var buffer = new char[length];
        for (int i = 0; i < length; i++)
        {
            buffer[i] = _sample[_random.Next(1, _sample.Length)];
        }
        return new string(buffer);
    }
    public static String[] ArrayOfStrings(Int32 minCount, Int32 maxCount, Int32 minLength, Int32 maxLength)
    {
        String[] result = new String[Integer(minCount, maxCount)];
        for (Int32 index = 0; index < result.Length; index++)
        {
            result[index] = String(minLength, maxLength);
        }
        return result;
    }

    public static ConsensusTimeStamp TruncatedFutureDate(Int32 minHoursAhead, Int32 maxHoursAhead)
    {
        var date = DateTime.UtcNow.AddHours(Double(minHoursAhead, maxHoursAhead));
        return new ConsensusTimeStamp(new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc));
    }

    public static ConsensusTimeStamp TruncateToSeconds(DateTime date)
    {
        return new ConsensusTimeStamp(new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, DateTimeKind.Utc));
    }

    public static (ReadOnlyMemory<byte> publicKey, ReadOnlyMemory<byte> privateKey) KeyPair()
    {
        return _random.Next(0, 1) == 1 ? Ed25519KeyPair() : Secp256k1KeyPair();
    }
    public static (ReadOnlyMemory<byte> publicKey, ReadOnlyMemory<byte> privateKey) Ed25519KeyPair()
    {
        // public prefix:  302a300506032b6570032100 
        // private prefix: 302e020100300506032b6570

        /*
         * From Discord: Greg Scullard (Hedera Hashgraph)05/01/2019
         * 
         Check the public key and private key are the right way round (I've done it wrong myself a few times). The private key is the longest of the two.
         If that isn't the issue, try with the encoded keys (they start with 302), there may be a bug with non encoded keys
         If you don't have them to hand, you can prefix the public key with 302a300506032b6570032100 and the private key with 302e020100300506032b6570
         this is common to all keys, it's ASN.1 Encoding that specifies the format of the data in the string.
         so 20aa4780f.... becomes 302a300506032b657003210020aa4780f... for a public key
         * */
        using var key = Key.Create(SignatureAlgorithm.Ed25519, new KeyCreationParameters { ExportPolicy = KeyExportPolicies.AllowPlaintextExport });
        var publicKey = key.Export(KeyBlobFormat.PkixPublicKey);
        var privateKey = key.Export(KeyBlobFormat.PkixPrivateKey);
        return (publicKey, privateKey);
    }

    private static X9ECParameters curve = SecNamedCurves.GetByName("secp256k1");
    private static ECDomainParameters domain = new ECDomainParameters(curve.Curve, curve.G, curve.N, curve.H);
    public static (ReadOnlyMemory<byte> publicKey, ReadOnlyMemory<byte> privateKey) Secp256k1KeyPair()
    {
        // Todo: consider native .net implementation to ensure interoperability
        ECKeyPairGenerator generator = new ECKeyPairGenerator();
        var ecGenerationParameters = new ECKeyGenerationParameters(domain, new SecureRandom());
        generator.Init(ecGenerationParameters);
        var keypair = generator.GenerateKeyPair();
        var privateKeyParameters = (ECPrivateKeyParameters)keypair.Private;
        var publicKeyParameters = (ECPublicKeyParameters)keypair.Public;
        var privateKeyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(privateKeyParameters);
        var publicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(publicKeyParameters);
        return (publicKeyInfo.GetDerEncoded(), privateKeyInfo.GetDerEncoded());
    }
    public static ReadOnlyMemory<byte> SHA384Hash()
    {
        return SHA384.Create().ComputeHash(KeyPair().publicKey.ToArray());
    }
    public static Proto.TransactionID TransactionID()
    {
        var (seconds, nanos) = Epoch.UniqueSecondsAndNanos(false);
        var shardNum = Generator.Integer(0, 200);
        var realmNum = Generator.Integer(0, 200);
        var accountNum = Generator.Integer(0, 200);
        return new Proto.TransactionID
        {
            AccountID = new Proto.AccountID
            {
                ShardNum = shardNum,
                RealmNum = realmNum,
                AccountNum = accountNum
            },
            TransactionValidStart = new Proto.Timestamp
            {
                Seconds = seconds,
                Nanos = nanos,
            }
        };

    }
}