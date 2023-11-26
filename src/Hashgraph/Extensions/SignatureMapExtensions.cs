using Google.Protobuf;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hashgraph.Extensions;

public static class SignatureMapExtensions
{
    /// <summary>
    /// Adds one or more signatures from the given <see cref="Signatory"/> to the
    /// existing <see cref="SignatureMap"/> instance.
    /// </summary>
    /// <remarks>
    /// This method does not examine the message being signed nor provide any
    /// validation of the results or de-duplication of identicial signatures.
    /// It is meant to support edge cases outside the normal usage patterns of 
    /// this SDK, therefore use sparingly and with care.
    /// </remarks>
    /// <param name="signatureMap">
    /// The signature map which may or may not contain pre-existing signatures.
    /// </param>
    /// <param name="data">
    /// The message data to sign, does not necessarily need to be a transaction,
    /// and as a result, the TxId property will always be <code>None</code> and
    /// the message will be an empty string.
    /// </param>
    /// <param name="signatory">
    /// The signatory that is asked to sign the message.
    /// </param>
    public static async Task AddSignatureAsync(this SignatureMap signatureMap, ReadOnlyMemory<byte> data, Signatory signatory)
    {
        await ((ISignatory)signatory).SignAsync(new UncheckedSignatureMapInvoice(signatureMap, data));
    }
    /// <summary>
    /// Lightweight <see cref="IInvoice"/> implemenation over an existing
    /// protobuf <see cref="SignatureMap"/> object, performs no validation
    /// checking, use with care.
    /// </summary>
    private class UncheckedSignatureMapInvoice : IInvoice
    {
        private readonly SignatureMap _signatureMap;
        private readonly ReadOnlyMemory<byte> _data;

        public TxId TxId => TxId.None;

        public string Memo => string.Empty;

        public ReadOnlyMemory<byte> TxBytes => _data;

        public int MinimumDesiredPrefixSize => int.MaxValue;

        public UncheckedSignatureMapInvoice(SignatureMap signatureMap, ReadOnlyMemory<byte> data)
        {
            _signatureMap = signatureMap;
            _data = data;
        }

        public void AddSignature(KeyType type, ReadOnlyMemory<byte> publicPrefix, ReadOnlyMemory<byte> signature)
        {
            var pair = new SignaturePair { PubKeyPrefix = ByteString.CopyFrom(publicPrefix.Span) };
            var value = ByteString.CopyFrom(signature.Span);
            switch (type)
            {
                case KeyType.Ed25519:
                    pair.Ed25519 = value;
                    break;
                case KeyType.ECDSASecp256K1:
                    pair.ECDSASecp256K1 = value;
                    break;
                case KeyType.Contract:
                    pair.Contract = value;
                    break;
            }
            _signatureMap.SigPair.Add(pair);
        }
    }
    /// <summary>
    /// Returns <code>True</code> if the given signature map contains
    /// enough correct signatures to satisfy the key signing requirements
    /// of the given <see cref="Endorsement"/>.
    /// </summary>
    /// <remarks>
    /// Note: this method does not return errors when it discovers invalid
    /// signatures.  It may still return <code>true</code> if sufficient
    /// correct signatures exist in the signature map to satisfy the 
    /// requrements described in the target <see cref="Endorsement"/>.
    /// </remarks>
    /// <param name="signatureMap">
    /// A signature map containing one or more signatures.
    /// </param>
    /// <param name="data">
    /// The message that was signed.
    /// </param>
    /// <param name="endorsement">
    /// The key signing requirements that must be met.
    /// </param>
    /// <returns>
    /// <code>True</code> if the signature map contains enough valid
    /// signatures for the data to satisfy the <see cref="Endorsement"/>
    /// key signing requirements.
    /// </returns>
    public static bool Satisfies(this SignatureMap signatureMap, ReadOnlyMemory<byte> data, Endorsement endorsement)
    {
        return isEndorsementSatisfied(endorsement);

        bool isEndorsementSatisfied(Endorsement endorsement)
        {
            return endorsement.Type switch
            {
                KeyType.Ed25519 => isKeySatisfied(endorsement),
                KeyType.ECDSASecp256K1 => isKeySatisfied(endorsement),
                KeyType.List => isListSatisfied(endorsement.RequiredCount, endorsement.List),
                _ => false
            };
        }

        bool isKeySatisfied(Endorsement endorsement)
        {
            foreach (var signature in findCandidateSignatures(endorsement.Type, endorsement.ToBytes(KeyFormat.Raw)))
            {
                if (endorsement.Verify(data, signature))
                {
                    return true;
                }
            }
            return false;
        }

        bool isListSatisfied(uint threshold, Endorsement[] list)
        {
            uint satisfied = 0;
            foreach (var endorsement in list)
            {
                if (isEndorsementSatisfied(endorsement))
                {
                    satisfied++;
                }
                if (satisfied >= threshold)
                {
                    return true;
                }
            }
            return false;
        }

        IEnumerable<ReadOnlyMemory<byte>> findCandidateSignatures(KeyType keyType, ReadOnlyMemory<byte> fullPublicKey)
        {
            switch (keyType)
            {
                case KeyType.Ed25519:
                    return signatureMap
                        .SigPair
                        .Where(p =>
                            p.SignatureCase == SignaturePair.SignatureOneofCase.Ed25519 &&
                            partialKeyPrefixMatches(fullPublicKey.Span, p.PubKeyPrefix.Span))
                        .Select(p => p.Ed25519.Memory);
                case KeyType.ECDSASecp256K1:
                    return signatureMap
                        .SigPair
                        .Where(p =>
                            p.SignatureCase == SignaturePair.SignatureOneofCase.ECDSASecp256K1 &&
                            partialKeyPrefixMatches(fullPublicKey.Span, p.PubKeyPrefix.Span))
                        .Select(p => p.ECDSASecp256K1.Memory);
                case KeyType.Contract:
                    return signatureMap
                        .SigPair
                        .Where(p =>
                            p.SignatureCase == SignaturePair.SignatureOneofCase.Contract &&
                            partialKeyPrefixMatches(fullPublicKey.Span, p.PubKeyPrefix.Span))
                        .Select(p => p.Contract.Memory);
            }
            return Array.Empty<ReadOnlyMemory<byte>>();
        }

        bool partialKeyPrefixMatches(ReadOnlySpan<byte> fullKey, ReadOnlySpan<byte> partialKeyPrefix)
        {
            // Have to try signatures with no partial key id.
            if (partialKeyPrefix.Length > 0)
            {
                if (partialKeyPrefix.Length > fullKey.Length)
                {
                    return false;
                }
                for (var i = 0; i < partialKeyPrefix.Length; i++)
                {
                    if (fullKey[i] != partialKeyPrefix[i])
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
