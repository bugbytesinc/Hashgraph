using NSec.Cryptography;
using System;

namespace Hashgraph.Implementation;

/// <summary>
/// Internal helper class that provides conversion services 
/// between raw bytes and Ed25519 public and private keys.
/// </summary>
internal static class TestKeys
{
    internal static readonly byte[] privateKeyPrefix = Hex.ToBytes("302e020100300506032b6570").ToArray();
    internal static readonly byte[] publicKeyPrefix = Hex.ToBytes("302a300506032b6570032100").ToArray();
    internal static Key ImportPrivateEd25519KeyFromBytes(ReadOnlyMemory<byte> privateKey)
    {
        try
        {
            return Key.Import(SignatureAlgorithm.Ed25519, privateKey.Span, KeyBlobFormat.PkixPrivateKey);
        }
        catch (FormatException fe)
        {
            if (privateKey.Length == 0)
            {
                throw new ArgumentOutOfRangeException("Private Key cannot be empty.", fe);
            }
            if (privateKey.Length == 36)
            {
                throw new ArgumentOutOfRangeException("The private key was not provided in a recognizable Ed25519 format. Is it missing the encoding format prefix 0x302e020100300506032b6570?", fe);
            }
            else if (!privateKey.Span.StartsWith(privateKeyPrefix) || privateKey.Length != 48)
            {
                throw new ArgumentOutOfRangeException("The private key was not provided in a recognizable Ed25519 format. Was expecting 48 bytes starting with the prefix 0x302e020100300506032b6570.", fe);
            }
            throw new ArgumentOutOfRangeException("The private key does not appear to be encoded as a recognizable Ed25519 format.", fe);
        }
    }
}